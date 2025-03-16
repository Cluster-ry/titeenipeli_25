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

		blobID, err := createStorageBlob(ctx, pulumi.String("titeenidbbackup"), entraInfo)
		if err != nil {
			return err
		}

		k8sCluster, err := buildCluster(ctx, cfg, *entraInfo)
		if err != nil {
			return err
		}

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

		titeenipeliClusterIdentity, err := createNewIdentity(ctx, k8sCluster, "titeenipeli-cluster", "titeenipeli")
		if err != nil {
			return err
		}

		baseDomain, err := createSubDomainZone(ctx, entraInfo, cfg.BaseDomain, "peli")
		if err != nil {
			return err
		}

		configDomain, err := createSubDomainZone(ctx, entraInfo, cfg.BaseConfigDomain, "peli")
		if err != nil {
			return err
		}

		addDNSZoneContributorRoleToId(ctx, baseDomain, certManagerIdentity, "cert", "1")
		addDNSZoneContributorRoleToId(ctx, baseDomain, externalDnsIdentity, "dns", "1")
		addDNSZoneContributorRoleToId(ctx, baseDomain, traefikIdentity, "traefik", "1")
		addDNSZoneContributorRoleToId(ctx, configDomain, certManagerIdentity, "cert", "2")
		addDNSZoneContributorRoleToId(ctx, configDomain, externalDnsIdentity, "dns", "2")
		addDNSZoneContributorRoleToId(ctx, configDomain, traefikIdentity, "traefik", "2")
		addStorageBlobDataContributorToId(ctx, titeenipeliClusterIdentity, "titeenipelicluster", blobID)
		addResourceGroupReaderRoleToId(ctx, k8sCluster.ResourceGroup, externalDnsIdentity, "dns")

		k8sProvider, err := buildProvider(ctx, kubeconfig)
		if err != nil {
			return err
		}

		installCSI(ctx, k8sProvider)
		installCertManager(ctx, k8sProvider)
		installMonitoring(ctx, k8sProvider)

		// Exports
		ctx.Export("domainName", baseDomain.Name)
		ctx.Export("configDomainName", configDomain.Name)
		ctx.Export("certManagerIdentityClientId", pulumi.ToSecret(certManagerIdentity.UserAssignedIdentity.ClientId))
		ctx.Export("titeenipeliClusterIdentity", pulumi.ToSecret(titeenipeliClusterIdentity.UserAssignedIdentity.ClientId))
		ctx.Export("externalDnsIdentityClientId", pulumi.ToSecret(externalDnsIdentity.UserAssignedIdentity.ClientId))
		ctx.Export("traefikIdentityClientId", pulumi.ToSecret(traefikIdentity.UserAssignedIdentity.ClientId))
		ctx.Export("kubeconfig", pulumi.ToSecret(kubeconfig))
		ctx.Export("clusterName", k8sCluster.ManagedCluster.Name)
		ctx.Export("titeenipeliRG", k8sCluster.ResourceGroup.Name)
		ctx.Export("publicIpName", k8sCluster.PublicIpName)

		return nil
	})
}
