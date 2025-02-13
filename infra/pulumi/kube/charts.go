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

	certs, err := helm.NewChart(ctx, "certmanager-certs", helm.ChartArgs{
		Path: pulumi.String("./helm/certmanager-certs"),
		Values: pulumi.Map{
			"hostedZoneName": domainName,
			"clientID":       certManagerClientId,
			"subscriptionID": pulumi.String(subscription.SubscriptionId),
			"titeenipeliRG":  titeenipeliRG,
			"email":          email,
		},
	}, pulumi.Provider(k8sProvider))
	if err != nil {
		return err
	}

	edNS, err := v1.NewNamespace(ctx, "external-dns", &v1.NamespaceArgs{
		Metadata: &metav1.ObjectMetaArgs{
			Name: pulumi.String("external-dns"),
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

	externalDnsChartArgs := helm.ChartArgs{
		Chart:   pulumi.String("external-dns"),
		Version: pulumi.String("1.15.0"),
		FetchArgs: helm.FetchArgs{
			Repo: pulumi.String("https://kubernetes-sigs.github.io/external-dns/"),
		},
		Namespace: pulumi.String("external-dns"),
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
	}

	helm.NewChart(ctx, "external-dns", externalDnsChartArgs,
		pulumi.Providers(k8sProvider), pulumi.DependsOn([]pulumi.Resource{edNS}))

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

	kubePrometheusStack, err := helm.NewRelease(ctx, "kube-prometheus-stack", &helm.ReleaseArgs{
		Chart:     pulumi.String("kube-prometheus-stack"),
		Namespace: pulumi.String("monitoring"),
		RepositoryOpts: helm.RepositoryOptsArgs{
			Repo: pulumi.String("https://prometheus-community.github.io/helm-charts"),
		},
		Values: pulumi.Map{
			"nameOverride":     pulumi.String("kube-prometheus-stack-app"),
			"fullnameOverride": pulumi.String("kube-prometheus-stack"),
			"enabled":          pulumi.Bool(true),
			"kubeControllerManager": pulumi.Map{
				"enabled": pulumi.Bool(false),
			},
			"nodeExporter": pulumi.Map{
				"enabled": pulumi.Bool(false),
			},
			"defaultRules": pulumi.Map{
				"create": pulumi.Bool(true),
				"rules": pulumi.Map{
					"k8s":                         pulumi.Bool(true),
					"kubelet":                     pulumi.Bool(true),
					"node":                        pulumi.Bool(true),
					"nodeExporterRecording":       pulumi.Bool(true),
					"alertmanager":                pulumi.Bool(false),
					"etcd":                        pulumi.Bool(false),
					"configReloaders":             pulumi.Bool(false),
					"general":                     pulumi.Bool(false),
					"kubeApiserver":               pulumi.Bool(false),
					"kubeApiserverAvailability":   pulumi.Bool(false),
					"kubeApiserverSlos":           pulumi.Bool(false),
					"kubeProxy":                   pulumi.Bool(false),
					"kubePrometheusGeneral":       pulumi.Bool(false),
					"kubePrometheusNodeRecording": pulumi.Bool(false),
					"kubernetesApps":              pulumi.Bool(false),
					"kubernetesResources":         pulumi.Bool(false),
					"kubernetesStorage":           pulumi.Bool(false),
					"kubernetesSystem":            pulumi.Bool(false),
					"kubeScheduler":               pulumi.Bool(false),
					"kubeStateMetrics":            pulumi.Bool(false),
					"network":                     pulumi.Bool(false),
					"nodeExporterAlerting":        pulumi.Bool(false),
					"prometheus":                  pulumi.Bool(false),
					"prometheusOperator":          pulumi.Bool(false),
				},
			},
			"prometheus": pulumi.Map{
				"prometheusSpec": pulumi.Map{
					"podMonitorSelectorNilUsesHelmValues":     pulumi.Bool(false),
					"ruleSelectorNilUsesHelmValues":           pulumi.Bool(false),
					"serviceMonitorSelectorNilUsesHelmValues": pulumi.Bool(false),
					"probeSelectorNilUsesHelmValues":          pulumi.Bool(false),
					"serviceMonitorNamespaceSelector": pulumi.Map{
						"matchLabels": pulumi.StringMap{
							"prometheus": pulumi.String("watch"),
						},
					},
				},
			},
			"grafana": pulumi.Map{
				"enabled":                  pulumi.Bool(true),
				"adminPassword":            grafanaPw.Result,
				"defaultDashboardsEnabled": pulumi.Bool(true),
				"ingress": pulumi.Map{
					"enabled": pulumi.Bool(true),
				},
				"fullnameOverride": pulumi.String("grafana"),
				"sidecar": pulumi.Map{
					"dashboards": pulumi.Map{
						"enabled": pulumi.Bool(true),
					},
				},
				"dashboardProviders": pulumi.Map{
					"dashboardproviders.yaml": pulumi.Map{
						"apiVersion": pulumi.Int(1),
						"providers": pulumi.Array{
							pulumi.Map{
								"name":            pulumi.String("provider-site"),
								"orgId":           pulumi.Int(1),
								"folder":          pulumi.String(""),
								"type":            pulumi.String("file"),
								"disableDeletion": pulumi.Bool(false),
								"editable":        pulumi.Bool(true),
								"options": pulumi.Map{
									"path": pulumi.String("/var/lib/grafana/dashboards/provider-site"),
								},
							},
						},
					},
				},
				"dashboards": pulumi.Map{
					"provider-site": pulumi.Map{
						"cloudnative-pg": pulumi.Map{
							"gnetId":     pulumi.Int(20417),
							"revision":   pulumi.Int(3),
							"datasource": pulumi.String("Prometheus"),
						},
						"traefik": pulumi.Map{
							"gnetId":     pulumi.Int(17346),
							"revision":   pulumi.Int(9),
							"datasource": pulumi.String("Prometheus"),
						},
					},
				},
			},
			"alertmanager": pulumi.Map{
				"enabled": pulumi.Bool(false),
			},
		},
	}, pulumi.DependsOn([]pulumi.Resource{monNS}))
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
		pulumi.Providers(k8sProvider), pulumi.DependsOn([]pulumi.Resource{kubePrometheusStack}))
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

	traefikChartArgs := helm.ChartArgs{
		Chart:   pulumi.String("traefik"),
		Version: pulumi.String("32.1.1"),
		FetchArgs: helm.FetchArgs{
			Repo: pulumi.String("https://traefik.github.io/charts"),
		},
		Namespace: pulumi.String("traefik"),
		Values: pulumi.Map{
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
					"external-dns.alpha.kubernetes.io/hostname": pulumi.String("traefik.test.cluster2017.fi,grafana.test.cluster2017.fi"), // dynamic domain
				},
			},
			"ports": pulumi.Map{
				"web": pulumi.Map{
					"redirectTo": pulumi.Map{
						"port":     pulumi.String("websecure"),
						"priority": pulumi.Int(10),
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
										"name": pulumi.String("grafana"),
										"port": pulumi.Int(80),
									},
								},
							},
						},
					},
				},
			},
		},
	}

	helm.NewChart(ctx, "traefik", traefikChartArgs,
		pulumi.Providers(k8sProvider), pulumi.DependsOn([]pulumi.Resource{traefikNS, certs, kubePrometheusStack}))

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

	cnpgChartArgs := helm.ChartArgs{
		Chart:   pulumi.String("cloudnative-pg"),
		Version: pulumi.String("0.23.0"),
		FetchArgs: helm.FetchArgs{
			Repo: pulumi.String("https://cloudnative-pg.github.io/charts"),
		},
		Namespace: pulumi.String("cnpg-system"),
		Values: pulumi.Map{
			"replicaCount": pulumi.Int(1),
			"monitoring": pulumi.Map{
				"podMonitorEnabled": pulumi.Bool(true),
				"grafanaDashboard": pulumi.Map{
					"create": pulumi.Bool(true),
				},
			},
		},
	}

	helm.NewChart(ctx, "cloudnative-pg-chart", cnpgChartArgs,
		pulumi.Providers(k8sProvider), pulumi.DependsOn([]pulumi.Resource{cnpgNS}))

	return nil
}
