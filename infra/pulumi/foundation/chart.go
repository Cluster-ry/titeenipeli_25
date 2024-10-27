package main

import (
	"github.com/pulumi/pulumi-kubernetes/sdk/v3/go/kubernetes/helm/v3"
	"github.com/pulumi/pulumi-kubernetes/sdk/v4/go/kubernetes"

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

	helm.NewChart(ctx, "cert-manager", chartArgs,
		pulumi.Providers(k8sProvider))

	return nil
}
