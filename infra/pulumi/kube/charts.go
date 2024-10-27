package main

import (
	"github.com/pulumi/pulumi-azure/sdk/v5/go/azure/core"
	"github.com/pulumi/pulumi-kubernetes/sdk/v4/go/kubernetes"
	v1 "github.com/pulumi/pulumi-kubernetes/sdk/v4/go/kubernetes/core/v1"
	"github.com/pulumi/pulumi-kubernetes/sdk/v4/go/kubernetes/helm/v3"
	metav1 "github.com/pulumi/pulumi-kubernetes/sdk/v4/go/kubernetes/meta/v1"
	"github.com/pulumi/pulumi/sdk/v3/go/pulumi/config"

	"github.com/pulumi/pulumi/sdk/v3/go/pulumi"
)

func buildCharts(
	ctx *pulumi.Context,
	k8sProvider *kubernetes.Provider,
	domainName pulumi.StringOutput,
	certManagerClientId pulumi.StringOutput,
	externalDnsClientId pulumi.StringOutput,
	titeenipeliRG pulumi.StringOutput) error {

	subscription, err := core.LookupSubscription(ctx, nil)
	if err != nil {
		return err
	}

	conf := config.New(ctx, "")
	email := conf.RequireSecret("email")

	cmNs, err := v1.NewNamespace(ctx, "cert-manager", &v1.NamespaceArgs{
		Metadata: &metav1.ObjectMetaArgs{
			Name: pulumi.String("cert-manager"),
		}},
		pulumi.Providers(k8sProvider))
	if err != nil {
		return err
	}

	certManagerChartArgs := helm.ChartArgs{
		Chart:   pulumi.String("cert-manager"),
		Version: pulumi.String("v1.15.3"),
		FetchArgs: helm.FetchArgs{
			Repo: pulumi.String("https://charts.jetstack.io"),
		},
		Namespace: pulumi.String("cert-manager"),
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
				"enabled": pulumi.String("true"),
			},
		},
	}

	helm.NewChart(ctx, "cert-manager", certManagerChartArgs,
		pulumi.Providers(k8sProvider), pulumi.DependsOn([]pulumi.Resource{cmNs}))

	helm.NewChart(ctx, "certmanager-certs", helm.ChartArgs{
		Path: pulumi.String("./helm/certmanager-certs"),
		Values: pulumi.Map{
			"hostedZoneName": domainName,
			"clientID":       certManagerClientId,
			"subscriptionID": pulumi.String(subscription.SubscriptionId),
			"titeenipeliRG":  titeenipeliRG,
			"email":          email,
		},
	}, pulumi.Provider(k8sProvider))

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
