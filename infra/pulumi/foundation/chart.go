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
		Values: pulumi.Map{
			"secrets-store-csi-driver.syncSecret.enabled": pulumi.Bool(true),
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
		Values: pulumi.Map{
			"podLabels": pulumi.Map{
				"azure.workload.identity/use": pulumi.String("true"),
			},
			"serviceAccount": pulumi.Map{
				"labels": pulumi.Map{
					"azure.workload.identity/use": pulumi.String("true"),
				},
			},
			"crds": pulumi.Map{
				"enabled": pulumi.Bool(true),
			},
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
		Values: pulumi.Map{
			"enabled": pulumi.Bool(true),
			"kubeControllerManager": pulumi.Map{
				"enabled": pulumi.Bool(true),
			},
			"nodeExporter": pulumi.Map{
				"enabled": pulumi.Bool(true),
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
					"remoteWrite": pulumi.Array{
						pulumi.Map{
							"url": pulumi.String("http://lgtm-distributed-mimir-nginx.monitoring.svc:80/api/v1/push"),
						},
					},
					"externalLabels": pulumi.Map{
						"environment": pulumi.String("mimir"),
					},
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
				"enabled": pulumi.Bool(false),
			},
			"alertmanager": pulumi.Map{
				"enabled": pulumi.Bool(false),
			},
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
		Values: pulumi.Map{
			"config": pulumi.Map{
				"clients": pulumi.Array{
					pulumi.Map{
						"url": pulumi.String("http://lgtm-distributed-loki-distributor.monitoring.svc:3100/loki/api/v1/push"),
					},
				},
			},
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
		Values: pulumi.Map{
			"grafana": pulumi.Map{
				"enabled":                  pulumi.Bool(true),
				"adminPassword":            grafanaPw.Result,
				"defaultDashboardsEnabled": pulumi.Bool(true),
				"datasources": pulumi.Map{
					"datasources.yaml": pulumi.Map{
						"apiVersion": pulumi.Int(1),
						"datasources": pulumi.Array{
							pulumi.Map{
								"name":      pulumi.String("Loki"),
								"uid":       pulumi.String("loki"),
								"type":      pulumi.String("loki"),
								"url":       pulumi.String("http://{{ .Release.Name }}-loki-gateway"),
								"isDefault": pulumi.Bool(false),
							},
							pulumi.Map{
								"name":      pulumi.String("Mimir"),
								"uid":       pulumi.String("prom"),
								"type":      pulumi.String("prometheus"),
								"url":       pulumi.String("http://{{ .Release.Name }}-mimir-nginx/prometheus"),
								"isDefault": pulumi.Bool(true),
							},
							pulumi.Map{
								"name":      pulumi.String("Tempo"),
								"uid":       pulumi.String("tempo"),
								"type":      pulumi.String("tempo"),
								"url":       pulumi.String("http://{{ .Release.Name }}-tempo-query-frontend:3100"),
								"isDefault": pulumi.Bool(false),
								"jsonData": pulumi.Map{
									"tracesToLogsV2": pulumi.Map{
										"datasourceUid": pulumi.String("loki"),
									},
									"lokiSearch": pulumi.Map{
										"datasourceUid": pulumi.String("loki"),
									},
									"tracesToMetrics": pulumi.Map{
										"datasourceUid": pulumi.String("prom"),
									},
									"serviceMap": pulumi.Map{
										"datasourceUid": pulumi.String("prom"),
									},
								},
							},
						},
					},
				},
				"ingress": pulumi.Map{
					"enabled": pulumi.Bool(true),
				},
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
							"datasource": pulumi.String("Mimir"),
						},
						"traefik": pulumi.Map{
							"gnetId":     pulumi.Int(17346),
							"revision":   pulumi.Int(9),
							"datasource": pulumi.String("Mimir"),
						},
						"mimir-overview": pulumi.Map{
							"gnetId":     pulumi.Int(17607),
							"revision":   pulumi.Int(10),
							"datasource": pulumi.String("Mimir"),
						},
						"loki-promtail": pulumi.Map{
							"gnetId":     pulumi.Int(10880),
							"revision":   pulumi.Int(1),
							"datasource": pulumi.String("Mimir"),
						},
						"node-exporter": pulumi.Map{
							"gnetId":     pulumi.Int(1860),
							"revision":   pulumi.Int(37),
							"datasource": pulumi.String("Mimir"),
						},
						"logs": pulumi.Map{
							"gnetId":     pulumi.Int(13639),
							"revision":   pulumi.Int(2),
							"datasource": pulumi.String("Loki"),
						},
					},
				},
			},
			"mimir": pulumi.Map{
				"enabled": pulumi.Bool(true),
				"metaMonitoring": pulumi.Map{
					"dashboards": pulumi.Map{
						"enabled": pulumi.Bool(true),
					},
					"serviceMonitor": pulumi.Map{
						"enabled": pulumi.Bool(true),
						"labels": pulumi.Map{
							"release": pulumi.String("kube-prometheus-stack"),
						},
					},
				},
				"alertmanager": pulumi.Map{
					"resources": pulumi.Map{
						"requests": pulumi.Map{
							"cpu": pulumi.String("20m"),
						},
					},
				},
				"compactor": pulumi.Map{
					"resources": pulumi.Map{
						"requests": pulumi.Map{
							"cpu": pulumi.String("20m"),
						},
					},
				},
				"distributor": pulumi.Map{
					"resources": pulumi.Map{
						"requests": pulumi.Map{
							"cpu": pulumi.String("20m"),
						},
					},
				},
				"ingester": pulumi.Map{
					"replicas": pulumi.Int(2),
					"zoneAwareReplication": pulumi.Map{
						"enabled": pulumi.Bool(false),
					},
					"resources": pulumi.Map{
						"requests": pulumi.Map{
							"cpu": pulumi.String("20m"),
						},
					},
				},
				"overrides_exporter": pulumi.Map{
					"resources": pulumi.Map{
						"requests": pulumi.Map{
							"cpu": pulumi.String("20m"),
						},
					},
				},
				"querier": pulumi.Map{
					"replicas": pulumi.Int(1),
					"resources": pulumi.Map{
						"requests": pulumi.Map{
							"cpu": pulumi.String("20m"),
						},
					},
				},
				"query_frontend": pulumi.Map{
					"resources": pulumi.Map{
						"requests": pulumi.Map{
							"cpu": pulumi.String("20m"),
						},
					},
				},
				"query_scheduler": pulumi.Map{
					"replicas": pulumi.Int(1),
					"resources": pulumi.Map{
						"requests": pulumi.Map{
							"cpu": pulumi.String("20m"),
						},
					},
				},
				"ruler": pulumi.Map{
					"resources": pulumi.Map{
						"requests": pulumi.Map{
							"cpu": pulumi.String("20m"),
						},
					},
				},
				"store_gateway": pulumi.Map{
					"zoneAwareReplication": pulumi.Map{
						"enabled": pulumi.Bool(false),
					},
					"resources": pulumi.Map{
						"requests": pulumi.Map{
							"cpu": pulumi.String("20m"),
						},
					},
				},
				"minio": pulumi.Map{
					"resources": pulumi.Map{
						"requests": pulumi.Map{
							"cpu": pulumi.String("20m"),
						},
					},
				},
				"rollout_operator": pulumi.Map{
					"resources": pulumi.Map{
						"requests": pulumi.Map{
							"cpu": pulumi.String("20m"),
						},
					},
				},
			},
			"tempo": pulumi.Map{
				"enabled": pulumi.Bool(true),
				"ingester": pulumi.Map{
					"replicas": pulumi.Int(1),
					"config": pulumi.Map{
						"replication_factor": pulumi.Int(1),
					},
				},
				"metaMonitoring": pulumi.Map{
					"dashboards": pulumi.Map{
						"enabled": pulumi.Bool(true),
					},
					"serviceMonitor": pulumi.Map{
						"enabled": pulumi.Bool(true),
						"labels": pulumi.Map{
							"release": pulumi.String("kube-prometheus-stack"),
						},
					},
					"prometheusRule": pulumi.Map{
						"enabled": pulumi.Bool(true),
						"labels": pulumi.Map{
							"release": pulumi.String("kube-prometheus-stack"),
						},
					},
				},
				"metricsGenerator": pulumi.Map{
					"enabled": pulumi.Bool(true),
					"config": pulumi.Map{
						"storage": pulumi.Map{
							"remote_write": pulumi.Array{
								pulumi.Map{
									"url":            pulumi.String("http://lgtm-distributed-mimir-nginx.monitoring.svc:80/api/v1/push"),
									"send_exemplars": pulumi.Bool(true),
								},
							},
						},
					},
				},
				"traces": pulumi.Map{
					"otlp": pulumi.Map{
						"grpc": pulumi.Map{
							"enabled": pulumi.Bool(true),
						},
						"http": pulumi.Map{
							"enabled": pulumi.Bool(true),
						},
					},
				},
			},
			"loki": pulumi.Map{
				"enabled": pulumi.Bool(true),
				"serviceMonitor": pulumi.Map{
					"enabled": pulumi.Bool(true),
					"labels": pulumi.Map{
						"release": pulumi.String("kube-prometheus-stack"),
					},
				},
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

	return nil
}
