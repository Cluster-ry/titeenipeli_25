package main

import (
	"github.com/pulumi/pulumi-kubernetes/sdk/v3/go/kubernetes/helm/v3"
	"github.com/pulumi/pulumi-kubernetes/sdk/v4/go/kubernetes"
	v1 "github.com/pulumi/pulumi-kubernetes/sdk/v4/go/kubernetes/core/v1"
	metav1 "github.com/pulumi/pulumi-kubernetes/sdk/v4/go/kubernetes/meta/v1"
	"github.com/pulumi/pulumi-random/sdk/v4/go/random"
	"github.com/pulumi/pulumi/sdk/v3/go/pulumi"
)

func installCSI(ctx *pulumi.Context, k8sProvider *kubernetes.Provider) error {

	_, err := helm.NewRelease(ctx, "csi-secrets-store-provider-azure", &helm.ReleaseArgs{
		Chart:           pulumi.String("csi-secrets-store-provider-azure"),
		Name:            pulumi.String("csi-secrets-store-provider-azure"),
		Version:         pulumi.String("1.6.0"),
		Namespace:       pulumi.String("kube-system"),
		CreateNamespace: pulumi.Bool(true),
		RepositoryOpts: helm.RepositoryOptsArgs{
			Repo: pulumi.String("https://azure.github.io/secrets-store-csi-driver-provider-azure/charts"),
		},
		ValueYamlFiles: pulumi.AssetOrArchiveArray{
			pulumi.NewFileAsset("./helm/csi-secrets-store-provider-azure/values.yaml"),
		},
	}, pulumi.Provider(k8sProvider))
	if err != nil {
		return err
	}

	return nil
}

func installCertManager(ctx *pulumi.Context, k8sProvider *kubernetes.Provider) error {

	_, err := helm.NewRelease(ctx, "cert-manager", &helm.ReleaseArgs{
		Chart:           pulumi.String("cert-manager"),
		Name:            pulumi.String("cert-manager"),
		Version:         pulumi.String("v1.17.1"),
		Namespace:       pulumi.String("cert-manager"),
		CreateNamespace: pulumi.Bool(true),
		RepositoryOpts: helm.RepositoryOptsArgs{
			Repo: pulumi.String("https://charts.jetstack.io"),
		},
		ValueYamlFiles: pulumi.AssetOrArchiveArray{
			pulumi.NewFileAsset("./helm/cert-manager/values.yaml"),
		},
	}, pulumi.Provider(k8sProvider))
	if err != nil {
		return err
	}

	return nil
}

func installMonitoring(ctx *pulumi.Context, k8sProvider *kubernetes.Provider) error {

	monNS, err := v1.NewNamespace(ctx, "monitoring", &v1.NamespaceArgs{
		Metadata: &metav1.ObjectMetaArgs{
			Name: pulumi.String("monitoring"),
			Labels: pulumi.StringMap{
				"prometheus": pulumi.String("watch"),
			},
		}},
		pulumi.Providers(k8sProvider))
	if err != nil {
		return err
	}

	grafanaPw, err := random.NewRandomPassword(ctx, "grafana-pw", &random.RandomPasswordArgs{
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
	ctx.Export("grafanaPass", pulumi.ToSecret(grafanaPw.Result))
	_, err = helm.NewRelease(ctx, "kube-prometheus-stack", &helm.ReleaseArgs{
		Chart:     pulumi.String("kube-prometheus-stack"),
		Name:      pulumi.String("kube-prometheus-stack"),
		Version:   pulumi.String("69.2.4"),
		Namespace: pulumi.String("monitoring"),
		RepositoryOpts: helm.RepositoryOptsArgs{
			Repo: pulumi.String("https://prometheus-community.github.io/helm-charts"),
		},
		ValueYamlFiles: pulumi.AssetOrArchiveArray{
			pulumi.NewFileAsset("./helm/kube-prometheus-stack/values.yaml"),
		},
	}, pulumi.Providers(k8sProvider), pulumi.DependsOn([]pulumi.Resource{monNS}))
	if err != nil {
		return err
	}

	_, err = helm.NewRelease(ctx, "promtail", &helm.ReleaseArgs{
		Chart:     pulumi.String("promtail"),
		Name:      pulumi.String("promtail"),
		Version:   pulumi.String("6.16.6"),
		Namespace: pulumi.String("monitoring"),
		RepositoryOpts: helm.RepositoryOptsArgs{
			Repo: pulumi.String("https://grafana.github.io/helm-charts"),
		},
		ValueYamlFiles: pulumi.AssetOrArchiveArray{
			pulumi.NewFileAsset("./helm/promtail/values.yaml"),
		},
	}, pulumi.Providers(k8sProvider), pulumi.DependsOn([]pulumi.Resource{monNS}))
	if err != nil {
		return err
	}

	_, err = helm.NewRelease(ctx, "lgtm-distributed", &helm.ReleaseArgs{
		Chart:     pulumi.String("lgtm-distributed"),
		Name:      pulumi.String("lgtm-distributed"),
		Version:   pulumi.String("2.1.0"),
		Namespace: pulumi.String("monitoring"),
		RepositoryOpts: helm.RepositoryOptsArgs{
			Repo: pulumi.String("https://grafana.github.io/helm-charts"),
		},
		ValueYamlFiles: pulumi.AssetOrArchiveArray{
			pulumi.NewFileAsset("./helm/lgtm-distributed/values.yaml"),
		},
		Values: pulumi.Map{
			"grafana": pulumi.Map{
				"adminPassword": grafanaPw.Result,
			},
		},
	}, pulumi.Providers(k8sProvider), pulumi.DependsOn([]pulumi.Resource{monNS}))
	if err != nil {
		return err
	}

	_, err = helm.NewRelease(ctx, "reflector", &helm.ReleaseArgs{
		Chart:     pulumi.String("reflector"),
		Name:      pulumi.String("reflector"),
		Version:   pulumi.String("7.1.288"),
		Namespace: pulumi.String("kube-system"),
		RepositoryOpts: helm.RepositoryOptsArgs{
			Repo: pulumi.String("https://emberstack.github.io/helm-charts"),
		},
	}, pulumi.Providers(k8sProvider))
	if err != nil {
		return err
	}

	_, err = helm.NewRelease(ctx, "opentelemetry-collector", &helm.ReleaseArgs{
		Chart:     pulumi.String("opentelemetry-collector"),
		Name:      pulumi.String("opentelemetry-collector"),
		Version:   pulumi.String("0.116.0"),
		Namespace: pulumi.String("kube-system"),
		RepositoryOpts: helm.RepositoryOptsArgs{
			Repo: pulumi.String("https://open-telemetry.github.io/opentelemetry-helm-charts"),
		},
		ValueYamlFiles: pulumi.AssetOrArchiveArray{
			pulumi.NewFileAsset("./helm/opentelemetry-collector/values.yaml"),
		},
	}, pulumi.Providers(k8sProvider))
	if err != nil {
		return err
	}

	return nil
}
