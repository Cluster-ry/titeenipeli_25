package main

import (
	"github.com/pulumi/pulumi-kubernetes/sdk/v3/go/kubernetes/helm/v3"
	"github.com/pulumi/pulumi-kubernetes/sdk/v4/go/kubernetes"
	v1 "github.com/pulumi/pulumi-kubernetes/sdk/v4/go/kubernetes/core/v1"
	metav1 "github.com/pulumi/pulumi-kubernetes/sdk/v4/go/kubernetes/meta/v1"
	"github.com/pulumi/pulumi/sdk/v3/go/pulumi"
)

func installCSI(ctx *pulumi.Context, k8sProvider *kubernetes.Provider) error {

	chartArgs := helm.ChartArgs{
		Chart:   pulumi.String("csi-secrets-store-provider-azure"),
		Version: pulumi.String("1.6.0"),
		FetchArgs: helm.FetchArgs{
			Repo: pulumi.String("https://azure.github.io/secrets-store-csi-driver-provider-azure/charts"),
		},
		Namespace: pulumi.String("kube-system"),
		Values: pulumi.Map{
			"secrets-store-csi-driver.syncSecret.enabled": pulumi.Bool(true),
		},
	}

	helm.NewChart(ctx, "csi-secrets-store-provider-azure", chartArgs,
		pulumi.Providers(k8sProvider))

	return nil
}

func installCertManager(ctx *pulumi.Context, k8sProvider *kubernetes.Provider) error {

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

	return nil
}
