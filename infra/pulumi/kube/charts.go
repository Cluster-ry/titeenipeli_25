package main

import (
	"github.com/pulumi/pulumi-azure/sdk/v5/go/azure/core"
	"github.com/pulumi/pulumi-kubernetes/sdk/v4/go/kubernetes"
	v1 "github.com/pulumi/pulumi-kubernetes/sdk/v4/go/kubernetes/core/v1"
	"github.com/pulumi/pulumi-kubernetes/sdk/v4/go/kubernetes/helm/v3"
	metav1 "github.com/pulumi/pulumi-kubernetes/sdk/v4/go/kubernetes/meta/v1"
	"github.com/pulumi/pulumi-random/sdk/v4/go/random"
	"github.com/pulumi/pulumi/sdk/v3/go/pulumi/config"

	"github.com/pulumi/pulumi/sdk/v3/go/pulumi"
)

func buildCharts(
	ctx *pulumi.Context,
	k8sProvider *kubernetes.Provider,
	domainName pulumi.StringOutput,
	configDomainName pulumi.StringOutput,
	certManagerClientId pulumi.StringOutput,
	externalDnsClientId pulumi.StringOutput,
	traefikIdentityClientId pulumi.StringOutput,
	titeenipeliClusterIdentity pulumi.StringOutput,
	titeenipeliRG pulumi.StringOutput,
	publicIpName pulumi.StringOutput, dbBackupContainer pulumi.StringOutput) error {

	subscription, err := core.LookupSubscription(ctx, nil)
	if err != nil {
		return err
	}

	conf := config.New(ctx, "")
	email := conf.RequireSecret("email")

	edNS, err := v1.NewNamespace(ctx, "external-dns", &v1.NamespaceArgs{
		Metadata: &metav1.ObjectMetaArgs{
			Name: pulumi.String("external-dns"),
		}},
		pulumi.Providers(k8sProvider))
	if err != nil {
		return err
	}

	gameNS, err := v1.NewNamespace(ctx, "titeenipeli", &v1.NamespaceArgs{
		Metadata: &metav1.ObjectMetaArgs{
			Name: pulumi.String("titeenipeli"),
			Labels: pulumi.StringMap{
				"prometheus": pulumi.String("watch"),
			},
		}},
		pulumi.Providers(k8sProvider))
	if err != nil {
		return err
	}

	current, err := core.LookupSubscription(ctx, &core.LookupSubscriptionArgs{}, nil)
	if err != nil {
		return err
	}

	helm.NewChart(ctx, "external-dns-secrets", helm.ChartArgs{
		Path: pulumi.String("./helm/external-dns-secret"),
		Values: pulumi.Map{
			"tenantId":       pulumi.String(current.TenantId),
			"subscriptionID": pulumi.String(subscription.SubscriptionId),
			"resourceGroup":  titeenipeliRG,
		},
		Namespace: pulumi.String("external-dns"),
	}, pulumi.Provider(k8sProvider), pulumi.DependsOn([]pulumi.Resource{edNS}))

	_, err = helm.NewRelease(ctx, "external-dns", &helm.ReleaseArgs{
		Chart:     pulumi.String("external-dns"),
		Name:      pulumi.String("external-dns"),
		Version:   pulumi.String("1.15.0"),
		Namespace: pulumi.String("external-dns"),
		RepositoryOpts: helm.RepositoryOptsArgs{
			Repo: pulumi.String("https://kubernetes-sigs.github.io/external-dns/"),
		},
		ValueYamlFiles: pulumi.AssetOrArchiveArray{
			pulumi.NewFileAsset("./helm/external-dns/values.yaml"),
		},
		Values: pulumi.Map{
			"serviceAccount": pulumi.Map{
				"annotations": pulumi.Map{
					"azure.workload.identity/client-id": externalDnsClientId,
				},
			},
		},
	}, pulumi.Providers(k8sProvider), pulumi.DependsOn([]pulumi.Resource{edNS}))
	if err != nil {
		return err
	}

	traefikNS, err := v1.NewNamespace(ctx, "traefik", &v1.NamespaceArgs{
		Metadata: &metav1.ObjectMetaArgs{
			Name: pulumi.String("traefik"),
			Labels: pulumi.StringMap{
				"prometheus": pulumi.String("watch"),
			},
		}},
		pulumi.Providers(k8sProvider))
	if err != nil {
		return err
	}

	certs, err := helm.NewChart(ctx, "certmanager-certs", helm.ChartArgs{
		Path: pulumi.String("./helm/certmanager-certs"),
		Values: pulumi.Map{
			"hostedZoneName":         domainName,
			"hostedConfigZoneName":   configDomainName,
			"clientID":               certManagerClientId,
			"subscriptionID":         pulumi.String(subscription.SubscriptionId),
			"titeenipeliRG":          titeenipeliRG,
			"email":                  email,
			"titeenipeliNS":          pulumi.String("titeenipeli"),
			"titeenipeliBackendName": pulumi.String("titeenipeli-back"),
		},
	}, pulumi.Provider(k8sProvider), pulumi.DependsOn([]pulumi.Resource{traefikNS, gameNS}))
	if err != nil {
		return err
	}

	pw, err := random.NewRandomPassword(ctx, "traefik-pw", &random.RandomPasswordArgs{
		Length:  pulumi.Int(20),
		Lower:   pulumi.Bool(true),
		Upper:   pulumi.Bool(true),
		Number:  pulumi.Bool(true),
		Special: pulumi.Bool(false),
		Keepers: pulumi.StringMap{
			"key": pulumi.String("static-value"),
		},
	})
	if err != nil {
		return err
	}
	ctx.Export("traefikPass", pulumi.ToSecret(pw.Result))

	traefik, err := helm.NewRelease(ctx, "traefik", &helm.ReleaseArgs{
		Chart:     pulumi.String("traefik"),
		Name:      pulumi.String("traefik"),
		Version:   pulumi.String("34.3.0"),
		Namespace: pulumi.String("traefik"),
		RepositoryOpts: helm.RepositoryOptsArgs{
			Repo: pulumi.String("https://traefik.github.io/charts"),
		},
		ValueYamlFiles: pulumi.AssetOrArchiveArray{
			pulumi.NewFileAsset("./helm/traefik/values.yaml"),
		},
		Values: pulumi.Map{
			"serviceAccountAnnotations": pulumi.Map{
				"azure.workload.identity/tenant-id": pulumi.String(current.TenantId),
				"azure.workload.identity/client-id": traefikIdentityClientId,
			},
			"service": pulumi.Map{
				"annotations": pulumi.Map{
					"service.beta.kubernetes.io/azure-pip-name": publicIpName,
					"external-dns.alpha.kubernetes.io/hostname": pulumi.String("traefik.peli.cluster2017.fi,grafana.peli.cluster2017.fi,peli.titeen.it"),
				},
			},
			"ingressRoute": pulumi.Map{
				"dashboard": pulumi.Map{
					"matchRule": pulumi.String("Host(`traefik.peli.cluster2017.fi`)"),
				},
			},
		},
	}, pulumi.Providers(k8sProvider), pulumi.DependsOn([]pulumi.Resource{traefikNS, certs}))
	if err != nil {
		return err
	}

	_, err = helm.NewChart(ctx, "traefik-resources", helm.ChartArgs{
		Path:      pulumi.String("./helm/traefik-resources"),
		Namespace: pulumi.String("traefik"),
		Values: pulumi.Map{
			"password": pw.Result,
		},
	}, pulumi.Provider(k8sProvider), pulumi.DependsOn([]pulumi.Resource{traefikNS, traefik}))
	if err != nil {
		return err
	}

	cnpgNS, err := v1.NewNamespace(ctx, "cnpg-system", &v1.NamespaceArgs{
		Metadata: &metav1.ObjectMetaArgs{
			Name: pulumi.String("cnpg-system"),
			Labels: pulumi.StringMap{
				"prometheus": pulumi.String("watch"),
			},
		}},
		pulumi.Providers(k8sProvider))
	if err != nil {
		return err
	}

	cloudnativePg, err := helm.NewRelease(ctx, "cloudnative-pg", &helm.ReleaseArgs{
		Chart:     pulumi.String("cloudnative-pg"),
		Name:      pulumi.String("cloudnative-pg"),
		Version:   pulumi.String("0.23.0"),
		Namespace: pulumi.String("cnpg-system"),
		RepositoryOpts: helm.RepositoryOptsArgs{
			Repo: pulumi.String("https://cloudnative-pg.github.io/charts"),
		},
		ValueYamlFiles: pulumi.AssetOrArchiveArray{
			pulumi.NewFileAsset("./helm/cloudnative-pg/values.yaml"),
		},
	}, pulumi.Providers(k8sProvider), pulumi.DependsOn([]pulumi.Resource{cnpgNS}))
	if err != nil {
		return err
	}

	jwtSecret, err := random.NewRandomPassword(ctx, "jwt-secret", &random.RandomPasswordArgs{
		Length:  pulumi.Int(256),
		Lower:   pulumi.Bool(true),
		Upper:   pulumi.Bool(true),
		Number:  pulumi.Bool(true),
		Special: pulumi.Bool(false),
		Keepers: pulumi.StringMap{
			"key": pulumi.String("static-value"),
		},
	})
	if err != nil {
		return err
	}

	jwtEncryption, err := random.NewRandomPassword(ctx, "jwt-encryption", &random.RandomPasswordArgs{
		Length:  pulumi.Int(32),
		Lower:   pulumi.Bool(true),
		Upper:   pulumi.Bool(true),
		Number:  pulumi.Bool(true),
		Special: pulumi.Bool(false),
		Keepers: pulumi.StringMap{
			"key": pulumi.String("static-value"),
		},
	})
	if err != nil {
		return err
	}

	botToken, err := random.NewRandomPassword(ctx, "bot-token", &random.RandomPasswordArgs{
		Length:  pulumi.Int(32),
		Lower:   pulumi.Bool(true),
		Upper:   pulumi.Bool(true),
		Number:  pulumi.Bool(true),
		Special: pulumi.Bool(false),
		Keepers: pulumi.StringMap{
			"key": pulumi.String("static-value"),
		},
	})
	if err != nil {
		return err
	}

	tgToken := conf.RequireSecret("tgToken")

	helm.NewChart(ctx, "titeenipeli-chart", helm.ChartArgs{
		Path: pulumi.String("./helm/titeenipeli"),
		Values: pulumi.Map{
			"databaseName":      pulumi.String("titepelidb"),
			"databaseUserName":  pulumi.String("tite"),
			"hostedZoneName":    pulumi.String("peli.titeen.it"),
			"gameUrl":           pulumi.String("peli.titeen.it"),
			"botToken":          botToken.Result,
			"jwtSecret":         jwtSecret.Result,
			"jwtEncryption":     jwtEncryption.Result,
			"tgToken":           tgToken,
			"dbBackupContainer": dbBackupContainer,
			"clientId":          titeenipeliClusterIdentity,
		},
		Namespace: pulumi.String("titeenipeli"),
	}, pulumi.Provider(k8sProvider), pulumi.DependsOn([]pulumi.Resource{gameNS, cloudnativePg}))

	return nil
}
