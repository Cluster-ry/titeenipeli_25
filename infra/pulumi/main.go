package main

import (
	"github.com/pulumi/pulumi/sdk/v3/go/pulumi"
)

func main() {
	pulumi.Run(func(ctx *pulumi.Context) error {
		cfg, err := configure(ctx)
		if err != nil {
			return err
		}

		entraInfo, err := buildEntra(ctx)
		if err != nil {
			return err
		}

		k8sCluster, err := buildCluster(ctx, cfg, *entraInfo)
		if err != nil {
			return err
		}

		kubeconfig := getKubeconfig(ctx, k8sCluster)

		k8sProvider, err := buildProvider(ctx, kubeconfig)
		if err != nil {
			return err
		}

		buildCharts(ctx, k8sProvider)

		ctx.Export("kubeconfig", pulumi.ToSecret(kubeconfig))
		ctx.Export("clusterName", k8sCluster.ManagedCluster.Name)

		return nil
	})
}
