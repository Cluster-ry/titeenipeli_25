package main

import (
	"github.com/pulumi/pulumi-kubernetes/sdk/v4/go/kubernetes"
	v1 "github.com/pulumi/pulumi-kubernetes/sdk/v4/go/kubernetes/core/v1"
	"github.com/pulumi/pulumi-kubernetes/sdk/v4/go/kubernetes/helm/v3"
	metav1 "github.com/pulumi/pulumi-kubernetes/sdk/v4/go/kubernetes/meta/v1"
	"github.com/pulumi/pulumi/sdk/v3/go/pulumi"
)

func buildCharts(ctx *pulumi.Context, k8sProvider *kubernetes.Provider) error {

	v1.NewNamespace(ctx, "cert-manager", &v1.NamespaceArgs{
		Metadata: &metav1.ObjectMetaArgs{
			Name: pulumi.String("cert-manager"),
		}},
		pulumi.Providers(k8sProvider))

	certManagerChartArgs := helm.ChartArgs{
		Chart:   pulumi.String("cert-manager"),
		Version: pulumi.String("v1.15.3"),
		FetchArgs: helm.FetchArgs{
			Repo: pulumi.String("https://charts.jetstack.io"),
		},
		Namespace: pulumi.String("cert-manager"),
		Values: pulumi.Map{
			"crds": pulumi.Map{
				"enabled": pulumi.Bool(true),
			},
		},
	}

	helm.NewChart(ctx, "cert-manager", certManagerChartArgs,
		pulumi.Providers(k8sProvider))

	return nil
}
