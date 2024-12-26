package main

import (
	"github.com/pulumi/pulumi-azure/sdk/v5/go/azure/core"
	"github.com/pulumi/pulumi-kubernetes/sdk/v4/go/kubernetes"
	v1 "github.com/pulumi/pulumi-kubernetes/sdk/v4/go/kubernetes/core/v1"
	"github.com/pulumi/pulumi-kubernetes/sdk/v4/go/kubernetes/helm/v3"
	metav1 "github.com/pulumi/pulumi-kubernetes/sdk/v4/go/kubernetes/meta/v1"
	"github.com/pulumi/pulumi/sdk/v3/go/pulumi/config"
	"github.com/sethvargo/go-password/password"

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

	traefikNS, err := v1.NewNamespace(ctx, "traefik", &v1.NamespaceArgs{
		Metadata: &metav1.ObjectMetaArgs{
			Name: pulumi.String("traefik"),
		}},
		pulumi.Providers(k8sProvider))
	if err != nil {
		return err
	}

	pw, err := password.Generate(20, 5, 5, false, false)
	if err != nil {
		return err
	}
	ctx.Export("traefikPass", pulumi.ToSecret(pw))

	traefikChartArgs := helm.ChartArgs{
		Chart:   pulumi.String("traefik"),
		Version: pulumi.String("32.1.1"),
		FetchArgs: helm.FetchArgs{
			Repo: pulumi.String("https://traefik.github.io/charts"),
		},
		Namespace: pulumi.String("traefik"),
		Values: pulumi.Map{
			"persistence": pulumi.Map{
				"enabled": pulumi.Bool(true),
				"size":    pulumi.String("128Mi"),
			},
			"serviceAccountAnnotations": pulumi.Map{
				"azure.workload.identity/tenant-id": pulumi.String(current.TenantId),
				"azure.workload.identity/client-id": traefikIdentityClientId,
			}, /*
				"certificatesResolvers": pulumi.Map{
					"letsencrypt": pulumi.Map{
						"acme": pulumi.Map{
							"email":    email,
							"caServer": pulumi.String("https://acme-staging-v02.api.letsencrypt.org/directory"),
							"dnsChallenge": pulumi.Map{
								"provider": pulumi.String("azuredns"),
							},
							"storage": pulumi.String("/data/acme.json"),
						},
					},
				},*/
			"deployment": pulumi.Map{
				"annotations": pulumi.Map{
					"azure.workload.identity/use": pulumi.String("true"),
				}, /*
					"initContainers": pulumi.Array{
						pulumi.Map{
							"name":  pulumi.String("volume-permissions"),
							"image": pulumi.String("busybox:latest"),
							"command": pulumi.Array{
								pulumi.String("sh"),
								pulumi.String("-c"),
								pulumi.String("ls -la /; touch /data/acme.json; chmod -v 600 /data/acme.json"),
							},
							"volumeMounts": pulumi.Array{
								pulumi.Map{
									"mountPath": pulumi.String("/data"),
									"name":      pulumi.String("data"),
								},
							},
						},
					},*/
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
					"external-dns.alpha.kubernetes.io/hostname": pulumi.String("traefik.test.cluster2017.fi"), // dynamic domain
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
						}, /*
							pulumi.Map{
								"name": pulumi.String("https-redirect"),
							},*/
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
						"password": pulumi.String(pw),
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
				}, /*
					pulumi.Map{
						"apiVersion": pulumi.String("traefik.io/v1alpha1"),
						"kind":       pulumi.String("Middleware"),
						"metadata": pulumi.Map{
							"name": pulumi.String("https-redirect"),
						},
						"spec": pulumi.Map{
							"redirectScheme": pulumi.Map{
								"scheme": pulumi.String("https"),
							},
							"permanent": pulumi.Map{
								"scheme": pulumi.Bool(true),
							},
						},
					},*/
			},
		},
	}

	helm.NewChart(ctx, "traefik", traefikChartArgs,
		pulumi.Providers(k8sProvider), pulumi.DependsOn([]pulumi.Resource{traefikNS, certs}))

	return nil
}

/* removed as no values are read from yaml
func mapValues(path string) (pulumi.Map, error) {
	valuesData, err := os.ReadFile(path)
	if err != nil {
		return nil, fmt.Errorf("unable to read values.yaml: %w", err)
	}

	var values map[interface{}]interface{}
	if err := yaml.Unmarshal(valuesData, &values); err != nil {
		return nil, fmt.Errorf("unable to parse values.yaml: %w", err)
	}

	pulumiMap := make(pulumi.Map)
	for k, v := range convertMap(values) {
		pulumiMap[k] = pulumi.Any(v)
	}

	return pulumiMap, nil
}

func convertMap(in map[interface{}]interface{}) map[string]interface{} {
	out := make(map[string]interface{})
	for k, v := range in {
		key, ok := k.(string)
		if !ok {
			continue
		}
		out[key] = convertValue(v)
	}
	return out
}

func convertValue(v interface{}) interface{} {
	switch val := v.(type) {
	case map[interface{}]interface{}:
		return convertMap(val)
	case []interface{}:
		for i, elem := range val {
			val[i] = convertValue(elem)
		}
	}
	return v
}
*/
