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

		//createCosmosDB(ctx, k8sCluster)

		kubeconfig := getKubeconfig(ctx, k8sCluster)

		certManagerIdentity, err := createNewIdentity(ctx, k8sCluster, "cert-manager", "cert-manager")
		if err != nil {
			return err
		}

		externalDnsIdentity, err := createNewIdentity(ctx, k8sCluster, "external-dns", "external-dns")
		if err != nil {
			return err
		}

		traefikIdentity, err := createNewIdentity(ctx, k8sCluster, "traefik", "traefik")
		if err != nil {
			return err
		}

		domain, err := createSubDomainZone(ctx, entraInfo, cfg, "test")
		if err != nil {
			return err
		}
		addDNSZoneContributorRoleToId(ctx, domain, certManagerIdentity, "cert")
		addDNSZoneContributorRoleToId(ctx, domain, externalDnsIdentity, "dns")
		addDNSZoneContributorRoleToId(ctx, domain, traefikIdentity, "traefik")
		addResourceGroupReaderRoleToId(ctx, k8sCluster.ResourceGroup, externalDnsIdentity, "dns")

		k8sProvider, err := buildProvider(ctx, kubeconfig)
		if err != nil {
			return err
		}

		installCSI(ctx, k8sProvider)
		installCertManager(ctx, k8sProvider)

		// Exports
		ctx.Export("domainName", domain.Name)
		ctx.Export("certManagerIdentityClientId", pulumi.ToSecret(certManagerIdentity.UserAssignedIdentity.ClientId))
		ctx.Export("externalDnsIdentityClientId", pulumi.ToSecret(externalDnsIdentity.UserAssignedIdentity.ClientId))
		ctx.Export("traefikIdentityClientId", pulumi.ToSecret(traefikIdentity.UserAssignedIdentity.ClientId))
		ctx.Export("kubeconfig", pulumi.ToSecret(kubeconfig))
		ctx.Export("clusterName", k8sCluster.ManagedCluster.Name)
		ctx.Export("titeenipeliRG", k8sCluster.ResourceGroup.Name)
		ctx.Export("publicIpName", k8sCluster.PublicIpName)

		return nil
	})
}
