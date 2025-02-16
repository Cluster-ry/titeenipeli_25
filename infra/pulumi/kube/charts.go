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
	certManagerClientId pulumi.StringOutput,
	externalDnsClientId pulumi.StringOutput,
	traefikIdentityClientId pulumi.StringOutput,
	titeenipeliRG pulumi.StringOutput,
	publicIpName pulumi.StringOutput) error {

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
		Values: pulumi.Map{
			"fullnameOverride": pulumi.String("external-dns"),
			"serviceAccount": pulumi.Map{
				"labels": pulumi.Map{
					"azure.workload.identity/use": pulumi.String("true"),
				},
				"annotations": pulumi.Map{
					"azure.workload.identity/client-id": externalDnsClientId,
				},
			},
			"podLabels": pulumi.Map{
				"azure.workload.identity/use": pulumi.String("true"),
			},
			"extraVolumes": pulumi.Array{
				pulumi.Map{
					"name": pulumi.String("azure-config-file"),
					"secret": pulumi.Map{
						"secretName": pulumi.String("external-dns-azure"),
					},
				},
			},
			"extraVolumeMounts": pulumi.Array{
				pulumi.Map{
					"name":      pulumi.String("azure-config-file"),
					"mountPath": pulumi.String("/etc/kubernetes"),
					"readOnly":  pulumi.Bool(true),
				},
			},
			"provider": pulumi.Map{
				"name": pulumi.String("azure"),
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

	_, err = helm.NewRelease(ctx, "traefik", &helm.ReleaseArgs{
		Chart:     pulumi.String("traefik"),
		Name:      pulumi.String("traefik"),
		Version:   pulumi.String("34.3.0"),
		Namespace: pulumi.String("traefik"),
		RepositoryOpts: helm.RepositoryOptsArgs{
			Repo: pulumi.String("https://traefik.github.io/charts"),
		},
		Values: pulumi.Map{
			"logs": pulumi.Map{
				"general": pulumi.Map{
					"level": pulumi.String("INFO"),
				},
			},

			"metrics": pulumi.Map{
				"prometheus": pulumi.Map{
					"service": pulumi.Map{
						"enabled": pulumi.Bool(true),
					},
					"disableAPICheck": pulumi.Bool(false),
					"serviceMonitor": pulumi.Map{
						"enabled": pulumi.Bool(true),
						"metricRelabelings": pulumi.Array{
							pulumi.Map{
								"sourceLabels": pulumi.StringArray{
									pulumi.String("__name__"),
								},
								"separator":   pulumi.String(";"),
								"regex":       pulumi.String("^fluentd_output_status_buffer_(oldest|newest)_.+"),
								"replacement": pulumi.String("$1"),
								"action":      pulumi.String("drop"),
							},
						},
						"relabelings": pulumi.Array{
							pulumi.Map{
								"sourceLabels": pulumi.StringArray{
									pulumi.String("__meta_kubernetes_pod_node_name"),
								},
								"separator":   pulumi.String(";"),
								"regex":       pulumi.String("^(.*)$"),
								"targetLabel": pulumi.String("nodename"),
								"replacement": pulumi.String("$1"),
								"action":      pulumi.String("replace"),
							},
						},
						"jobLabel":    pulumi.String("traefik"),
						"interval":    pulumi.String("30s"),
						"honorLabels": pulumi.Bool(true),
					},
					"prometheusRule": pulumi.Map{
						"enabled": pulumi.Bool(true),
						"rules": pulumi.Array{
							pulumi.Map{
								"alert": pulumi.String("TraefikDown"),
								"expr":  pulumi.String(`up{job="traefik"} == 0`),
								"for":   pulumi.String("5m"),
								"labels": pulumi.Map{
									"context":  pulumi.String("traefik"),
									"severity": pulumi.String("warning"),
								},
								"annotations": pulumi.Map{
									"summary":     pulumi.String("Traefik Down"),
									"description": pulumi.String("{{ $labels.pod }} on {{ $labels.nodename }} is down"),
								},
							},
						},
					},
				},
			},
			"tracing": pulumi.Map{
				"otlp": pulumi.Map{
					"http": pulumi.Map{
						"endpoint": pulumi.String("http://lgtm-distributed-tempo-distributor.monitoring.svc:4318/v1/traces"),
					},
				},
			},
			"persistence": pulumi.Map{
				"enabled": pulumi.Bool(true),
				"size":    pulumi.String("128Mi"),
			},
			"serviceAccountAnnotations": pulumi.Map{
				"azure.workload.identity/tenant-id": pulumi.String(current.TenantId),
				"azure.workload.identity/client-id": traefikIdentityClientId,
			},
			"deployment": pulumi.Map{
				"annotations": pulumi.Map{
					"azure.workload.identity/use": pulumi.String("true"),
				},
			},
			"podSecurityContext": pulumi.Map{
				"fsGroup":             pulumi.Int(65532),
				"fsGroupChangePolicy": pulumi.String("OnRootMismatch"),
			},
			"service": pulumi.Map{
				"spec": pulumi.Map{
					"type": pulumi.String("LoadBalancer"),
				},
				"annotations": pulumi.Map{
					"service.beta.kubernetes.io/azure-pip-name": publicIpName,
					"external-dns.alpha.kubernetes.io/hostname": pulumi.String("traefik.test.cluster2017.fi,grafana.test.cluster2017.fi,peli.test.cluster2017.fi,grcp.peli.test.cluster2017.fi"), // dynamic domain
				},
			},
			"ports": pulumi.Map{
				"web": pulumi.Map{
					"redirections": pulumi.Map{
						"entryPoint": pulumi.Map{
							"to":        pulumi.String("websecure"),
							"scheme":    pulumi.String("https"),
							"permanent": pulumi.Bool(true),
						},
					},
				},
			},
			"tlsStore": pulumi.Map{
				"default": pulumi.Map{
					"defaultCertificate": pulumi.Map{
						"secretName": pulumi.String("wildcard-tls"),
					},
				},
			},
			"ingressRoute": pulumi.Map{
				"dashboard": pulumi.Map{
					"enabled":   pulumi.Bool(true),
					"matchRule": pulumi.String("Host(`traefik.test.cluster2017.fi`)"),
					"entryPoints": pulumi.StringArray{
						pulumi.String("websecure"),
					},
					"middlewares": pulumi.Array{
						pulumi.Map{
							"name": pulumi.String("traefik-dashboard-auth"),
						},
					},
				},
			},
			"extraObjects": pulumi.Array{
				pulumi.Map{
					"apiVersion": pulumi.String("v1"),
					"kind":       pulumi.String("Secret"),
					"metadata": pulumi.Map{
						"name": pulumi.String("traefik-dashboard-auth-secret"),
					},
					"type": pulumi.String("kubernetes.io/basic-auth"),
					"stringData": pulumi.Map{
						"username": pulumi.String("admin"),
						"password": pw.Result,
					},
				},
				pulumi.Map{
					"apiVersion": pulumi.String("traefik.io/v1alpha1"),
					"kind":       pulumi.String("Middleware"),
					"metadata": pulumi.Map{
						"name": pulumi.String("traefik-dashboard-auth"),
					},
					"spec": pulumi.Map{
						"basicAuth": pulumi.Map{
							"secret": pulumi.String("traefik-dashboard-auth-secret"),
						},
					},
				},
				pulumi.Map{
					"apiVersion": pulumi.String("traefik.io/v1alpha1"),
					"kind":       pulumi.String("IngressRoute"),
					"metadata": pulumi.Map{
						"labels": pulumi.Map{
							"app.kubernetes.io/name": pulumi.String("grafana"),
						},
						"name":      pulumi.String("grafana-dashboard"),
						"namespace": pulumi.String("monitoring"),
					},
					"spec": pulumi.Map{
						"entryPoints": pulumi.Array{
							pulumi.String("websecure"),
						},
						"routes": pulumi.Array{
							pulumi.Map{
								"kind":  pulumi.String("Rule"),
								"match": pulumi.String("Host(`grafana.test.cluster2017.fi`)"),
								"services": pulumi.Array{
									pulumi.Map{
										"name": pulumi.String("lgtm-distributed-grafana"),
										"port": pulumi.Int(80),
									},
								},
							},
						},
					},
				},
			},
		},
	}, pulumi.Providers(k8sProvider), pulumi.DependsOn([]pulumi.Resource{traefikNS, certs}))
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
		Values: pulumi.Map{
			"replicaCount": pulumi.Int(1),
			"monitoring": pulumi.Map{
				"podMonitorEnabled": pulumi.Bool(true),
				"grafanaDashboard": pulumi.Map{
					"create": pulumi.Bool(true),
				},
			},
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
			"databaseName":     pulumi.String("titepelidb"),
			"databaseUserName": pulumi.String("tite"),
			"hostedZoneName":   pulumi.String("test.cluster2017.fi"),
			"gameUrl":          pulumi.String("peli.test.cluster2017.fi"),
			"botToken":         botToken.Result,
			"jwtSecret":        jwtSecret.Result,
			"jwtEncryption":    jwtEncryption.Result,
			"tgToken":          tgToken,
		},
		Namespace: pulumi.String("titeenipeli"),
	}, pulumi.Provider(k8sProvider), pulumi.DependsOn([]pulumi.Resource{gameNS, cloudnativePg}))

	return nil
}
