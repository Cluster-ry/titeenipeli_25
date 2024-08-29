package main

import (
	v1 "github.com/pulumi/pulumi-kubernetes/sdk/v4/go/kubernetes/core/v1"
	"github.com/pulumi/pulumi-kubernetes/sdk/v4/go/kubernetes/helm/v3"
	metav1 "github.com/pulumi/pulumi-kubernetes/sdk/v4/go/kubernetes/meta/v1"
	"github.com/pulumi/pulumi/sdk/v3/go/pulumi"
)

func main() {
	pulumi.Run(func(ctx *pulumi.Context) error {
		cfg, err := configure(ctx)
		if err != nil {
			return err
		}

		k8sCluster, err := buildCluster(ctx, cfg)
		if err != nil {
			return err
		}

		kubeconfig := getKubeconfig(ctx, k8sCluster)

		k8sProvider, err := buildProvider(ctx, kubeconfig)
		if err != nil {
			return err
		}

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

		ctx.Export("kubeconfig", pulumi.ToSecret(kubeconfig))
		ctx.Export("clusterName", k8sCluster.ManagedCluster.Name)

		return nil
	})
}
