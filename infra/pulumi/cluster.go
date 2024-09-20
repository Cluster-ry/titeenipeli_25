package main

import (
	"encoding/base64"

	cs "github.com/pulumi/pulumi-azure-native-sdk/containerservice/v2"
	managedidentity "github.com/pulumi/pulumi-azure-native-sdk/managedidentity/v2"
	"github.com/pulumi/pulumi-azure-native-sdk/resources/v2"
	"github.com/pulumi/pulumi-kubernetes/sdk/v4/go/kubernetes"
	"github.com/pulumi/pulumi/sdk/v3/go/pulumi"
)

type ClusterInfo struct {
	ManagedCluster *cs.ManagedCluster
	ResourceGroup  *resources.ResourceGroup
}

func buildCluster(ctx *pulumi.Context, cfg Config, entra EntraInfo) (*ClusterInfo, error) {
	k8sCluster, _ := cs.NewManagedCluster(ctx, "cluster",
		&cs.ManagedClusterArgs{
			ResourceGroupName: entra.ResourceGroup.Name,
			AgentPoolProfiles: cs.ManagedClusterAgentPoolProfileArray{
				cs.ManagedClusterAgentPoolProfileArgs{
					Count:        pulumi.Int(cfg.NodeCount),
					VmSize:       pulumi.String(cfg.NodeSize),
					MaxPods:      pulumi.Int(110),
					Mode:         pulumi.String("System"),
					Name:         pulumi.String("agentpool"),
					OsDiskSizeGB: pulumi.Int(30),
					OsType:       pulumi.String("Linux"),
					Type:         pulumi.String("VirtualMachineScaleSets"),
				},
			},
			DnsPrefix:  entra.ResourceGroup.Name,
			EnableRBAC: pulumi.Bool(true),
			OidcIssuerProfile: &cs.ManagedClusterOIDCIssuerProfileArgs{
				Enabled: pulumi.Bool(true),
			},
			SecurityProfile: &cs.ManagedClusterSecurityProfileArgs{
				WorkloadIdentity: &cs.ManagedClusterSecurityProfileWorkloadIdentityArgs{
					Enabled: pulumi.Bool(true),
				},
			},
			KubernetesVersion: pulumi.String(cfg.K8sVersion),
			LinuxProfile: cs.ContainerServiceLinuxProfileArgs{
				AdminUsername: pulumi.String(cfg.AdminUserName),
				Ssh: cs.ContainerServiceSshConfigurationArgs{
					PublicKeys: cs.ContainerServiceSshPublicKeyArray{
						cs.ContainerServiceSshPublicKeyArgs{
							KeyData: cfg.SshPublicKey,
						},
					},
				},
			},
			NodeResourceGroup: pulumi.String("node-resource-group"),
			ServicePrincipalProfile: cs.ManagedClusterServicePrincipalProfileArgs{
				ClientId: entra.Application.ClientId,
				Secret:   entra.ServicerPrincipalPassword.Value,
			},
		})

	mgIdent, err := managedidentity.NewUserAssignedIdentity(ctx, "userAssignedIdentity", &managedidentity.UserAssignedIdentityArgs{
		ResourceGroupName: entra.ResourceGroup.Name,
		ResourceName:      pulumi.String("AKS_service_account"),
	})
	if err != nil {
		return nil, err
	}

	managedidentity.NewFederatedIdentityCredential(ctx, "federatedIdentityCredential", &managedidentity.FederatedIdentityCredentialArgs{
		Audiences: pulumi.StringArray{
			pulumi.String("api://AzureADTokenExchange"),
		},
		FederatedIdentityCredentialResourceName: mgIdent.Name,
		Issuer:                                  k8sCluster.OidcIssuerProfile.Elem().IssuerURL(),
		ResourceGroupName:                       entra.ResourceGroup.Name,
		ResourceName:                            pulumi.String("AKS_service_account"),
		Subject:                                 pulumi.String("system:serviceaccount:titeenipeli:keyvault"),
	})

	return &ClusterInfo{
		ManagedCluster: k8sCluster,
		ResourceGroup:  entra.ResourceGroup,
	}, nil
}

func getKubeconfig(ctx *pulumi.Context, cluster *ClusterInfo) pulumi.StringOutput {
	creds := cs.ListManagedClusterUserCredentialsOutput(ctx,
		cs.ListManagedClusterUserCredentialsOutputArgs{
			ResourceGroupName: cluster.ResourceGroup.Name,
			ResourceName:      cluster.ManagedCluster.Name,
		},
	)
	kubeconfig := creds.Kubeconfigs().Index(pulumi.Int(0)).Value().
		ApplyT(func(arg string) string {
			kubeconfig, err := base64.StdEncoding.DecodeString(arg)
			if err != nil {
				return ""
			}
			return string(kubeconfig)
		}).(pulumi.StringOutput)
	return kubeconfig
}

func buildProvider(
	ctx *pulumi.Context,
	kubeConfig pulumi.StringOutput) (*kubernetes.Provider, error) {

	return kubernetes.NewProvider(ctx, "k8s-provider", &kubernetes.ProviderArgs{
		Kubeconfig: kubeConfig,
	})
}
