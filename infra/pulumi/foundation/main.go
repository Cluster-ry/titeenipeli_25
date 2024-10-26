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

		identity, err := createNewIdentity(ctx, k8sCluster, "cert-manager", "cert-manager")
		if err != nil {
			return err
		}

		domain, err := createSubDomainZone(ctx, entraInfo, cfg, "test")
		if err != nil {
			return err
		}
		addDNSZoneContributorRoleToId(ctx, domain, identity)

		// Exports
		ctx.Export("domainName", domain.Name)
		ctx.Export("certManagerIdentityClientId", pulumi.ToSecret(identity.UserAssignedIdentity.ClientId))
		ctx.Export("kubeconfig", pulumi.ToSecret(kubeconfig))
		ctx.Export("clusterName", k8sCluster.ManagedCluster.Name)
		ctx.Export("titeenipeliRG", k8sCluster.ResourceGroup.Name)

		return nil
	})
}
