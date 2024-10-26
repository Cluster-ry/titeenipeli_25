package main

import (
	"github.com/pulumi/pulumi-kubernetes/sdk/v4/go/kubernetes"

	"github.com/pulumi/pulumi/sdk/v3/go/pulumi"
)

func buildProvider(
	ctx *pulumi.Context,
	kubeConfig pulumi.StringOutput) (*kubernetes.Provider, error) {

	return kubernetes.NewProvider(ctx, "k8s-provider", &kubernetes.ProviderArgs{
		Kubeconfig: kubeConfig,
	})
}
