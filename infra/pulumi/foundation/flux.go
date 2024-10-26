package main

import (
	"github.com/pulumi/pulumi-kubernetes/sdk/v4/go/kubernetes"
	v1 "github.com/pulumi/pulumi-kubernetes/sdk/v4/go/kubernetes/core/v1"
	metav1 "github.com/pulumi/pulumi-kubernetes/sdk/v4/go/kubernetes/meta/v1"

	"github.com/pulumi/pulumi/sdk/v3/go/pulumi"
)

func installFlux(ctx *pulumi.Context, cluster *ClusterInfo, k8sProvider *kubernetes.Provider) error {

	_, err := v1.NewNamespace(ctx, "flux-system", &v1.NamespaceArgs{
		Metadata: &metav1.ObjectMetaArgs{
			Name: pulumi.String("flux-systems"),
		}},
		pulumi.Providers(k8sProvider))
	if err != nil {
		return err
	}
	// TODO: this fails, so fix it.
	/*
		kubernetesconfiguration.NewFluxConfiguration(ctx, "flux-config", &kubernetesconfiguration.FluxConfigurationArgs{
			ResourceGroupName:   cluster.ResourceGroup.Name,
			ClusterName:         cluster.ManagedCluster.Name,
			ClusterResourceName: pulumi.String("managedClusters"),
			ClusterRp:           pulumi.String("Microsoft.ContainerService"),
			Namespace:           pulumi.String("flux-system"),
			Scope:               pulumi.String("cluster"),
			GitRepository: &kubernetesconfiguration.GitRepositoryDefinitionArgs{
				Url: pulumi.String("https://github.com/org/repo"),
				RepositoryRef: &kubernetesconfiguration.RepositoryRefDefinitionArgs{
					Branch: pulumi.String("main"),
				},
			},
		}, pulumi.DependsOn([]pulumi.Resource{
			ns,
		}))
	*/

	return nil
}
