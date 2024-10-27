package main

import (
	"encoding/base64"
	"fmt"

	cs "github.com/pulumi/pulumi-azure-native-sdk/containerservice/v2"
	managedidentity "github.com/pulumi/pulumi-azure-native-sdk/managedidentity/v2"
	"github.com/pulumi/pulumi-azure-native-sdk/resources/v2"
	"github.com/pulumi/pulumi-kubernetes/sdk/v4/go/kubernetes"
	"github.com/pulumi/pulumi-null/sdk/go/null"
	"github.com/pulumiverse/pulumi-time/sdk/go/time"

	"github.com/pulumi/pulumi/sdk/v3/go/pulumi"
)

type ClusterInfo struct {
	ManagedCluster *cs.ManagedCluster
	ResourceGroup  *resources.ResourceGroup
}

type workloadIdentities struct {
	UserAssignedIdentity        *managedidentity.UserAssignedIdentity
	FederatedIdentityCredential *managedidentity.FederatedIdentityCredential
}

func buildCluster(ctx *pulumi.Context, cfg Config, entra EntraInfo) (*ClusterInfo, error) {
	// These are here as there seems to be some kind of bug with the readiness
	// of the service principal that causes pulumi to try and create k8s cluster
	// before service principal can be found. This 30s delay should remedy it.
	previous, err := null.NewResource(ctx, "waiting", nil,
		pulumi.DependsOn([]pulumi.Resource{
			entra.ServicePrincipal,
		}))
	if err != nil {
		return nil, err
	}
	sleep, err := time.NewSleep(ctx, "wait30Seconds", &time.SleepArgs{
		CreateDuration: pulumi.String("30s"),
	}, pulumi.DependsOn([]pulumi.Resource{
		previous,
	}))
	if err != nil {
		return nil, err
	}

	k8sCluster, _ := cs.NewManagedCluster(ctx, "cluster",
		&cs.ManagedClusterArgs{
			ResourceGroupName: entra.ResourceGroup.Name,
			ResourceName:      pulumi.String(cfg.ClusterName),
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
			NetworkProfile: cs.ContainerServiceNetworkProfileArgs{
				LoadBalancerProfile: cs.ManagedClusterLoadBalancerProfileArgs{
					ManagedOutboundIPs: cs.ManagedClusterLoadBalancerProfileManagedOutboundIPsArgs{
						Count: pulumi.Int(1),
					},
				},
				LoadBalancerSku: pulumi.String(cs.LoadBalancerSkuStandard),
				OutboundType:    pulumi.String(cs.OutboundTypeLoadBalancer),
			},
			NodeResourceGroup: pulumi.String("node-resource-group"),
			ServicePrincipalProfile: cs.ManagedClusterServicePrincipalProfileArgs{
				ClientId: entra.Application.ClientId,
				Secret:   entra.ServicerPrincipalPassword.Value,
			},
		},
		pulumi.DependsOn([]pulumi.Resource{
			sleep,
		}))

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

func createNewIdentity(ctx *pulumi.Context, info *ClusterInfo, name pulumi.String, namespace pulumi.String) (*workloadIdentities, error) {
	mgIdent, err := managedidentity.NewUserAssignedIdentity(ctx, fmt.Sprintf("userAssignedIdentity_%s", name), &managedidentity.UserAssignedIdentityArgs{
		ResourceGroupName: info.ResourceGroup.Name,
		ResourceName:      name,
	})
	if err != nil {
		return nil, err
	}

	fic, err := managedidentity.NewFederatedIdentityCredential(ctx, fmt.Sprintf("federatedIdentityCredential_%s", name), &managedidentity.FederatedIdentityCredentialArgs{
		Audiences: pulumi.StringArray{
			pulumi.String("api://AzureADTokenExchange"),
		},
		FederatedIdentityCredentialResourceName: mgIdent.Name,
		Issuer:                                  info.ManagedCluster.OidcIssuerProfile.Elem().IssuerURL(),
		ResourceGroupName:                       info.ResourceGroup.Name,
		ResourceName:                            name,
		Subject:                                 pulumi.String(fmt.Sprintf("system:serviceaccount:%s:%s", namespace, name)),
	})
	if err != nil {
		return nil, err
	}

	return &workloadIdentities{
		UserAssignedIdentity:        mgIdent,
		FederatedIdentityCredential: fic,
	}, nil

}
