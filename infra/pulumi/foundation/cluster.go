package main

import (
	"encoding/base64"
	"fmt"

	cs "github.com/pulumi/pulumi-azure-native-sdk/containerservice/v2/v20241001"
	managedidentity "github.com/pulumi/pulumi-azure-native-sdk/managedidentity/v2"
	"github.com/pulumi/pulumi-azure-native-sdk/network/v2"
	"github.com/pulumi/pulumi-azure-native-sdk/resources/v2"
	"github.com/pulumi/pulumi-kubernetes/sdk/v4/go/kubernetes"
	"github.com/pulumi/pulumi-null/sdk/go/null"
	"github.com/pulumiverse/pulumi-time/sdk/go/time"

	"github.com/pulumi/pulumi/sdk/v3/go/pulumi"
)

type ClusterInfo struct {
	ManagedCluster *cs.ManagedCluster
	ResourceGroup  *resources.ResourceGroup
	PublicIpName   pulumi.StringOutput
	NodeSubnetID   pulumi.IDOutput
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

	vnet, err := network.NewVirtualNetwork(ctx, "aks-vnet", &network.VirtualNetworkArgs{
		ResourceGroupName: entra.ResourceGroup.Name,
		AddressSpace: &network.AddressSpaceArgs{
			AddressPrefixes: pulumi.StringArray{
				pulumi.String("10.0.0.0/16"),
			},
		},
	})
	if err != nil {
		return nil, err
	}

	nodeSubnet, err := network.NewSubnet(ctx, "aks-node-subnet", &network.SubnetArgs{
		ResourceGroupName:                 entra.ResourceGroup.Name,
		VirtualNetworkName:                vnet.Name,
		AddressPrefix:                     pulumi.String("10.0.1.0/24"),
		PrivateEndpointNetworkPolicies:    pulumi.String("Disabled"),
		PrivateLinkServiceNetworkPolicies: pulumi.String("Disabled"),
	})
	if err != nil {
		return nil, err
	}
	podSubnet, err := network.NewSubnet(ctx, "aks-pod-subnet", &network.SubnetArgs{
		ResourceGroupName:                 entra.ResourceGroup.Name,
		VirtualNetworkName:                vnet.Name,
		AddressPrefix:                     pulumi.String("10.0.2.0/24"),
		PrivateEndpointNetworkPolicies:    pulumi.String("Disabled"),
		PrivateLinkServiceNetworkPolicies: pulumi.String("Disabled"),
	})
	if err != nil {
		return nil, err
	}

	k8sCluster, _ := cs.NewManagedCluster(ctx, "cluster",
		&cs.ManagedClusterArgs{
			ResourceGroupName: entra.ResourceGroup.Name,
			ResourceName:      pulumi.String(cfg.ClusterName),
			AgentPoolProfiles: cs.ManagedClusterAgentPoolProfileArray{
				cs.ManagedClusterAgentPoolProfileArgs{
					Count:             pulumi.Int(2),
					VmSize:            pulumi.String("Standard_D2ds_v5"),
					MaxPods:           pulumi.Int(110),
					Mode:              pulumi.String("System"),
					Name:              pulumi.String("agentpool"),
					OsType:            pulumi.String("Linux"),
					Type:              pulumi.String("VirtualMachineScaleSets"),
					VnetSubnetID:      nodeSubnet.ID(),
					PodSubnetID:       podSubnet.ID(),
					EnableAutoScaling: pulumi.Bool(true),
					MinCount:          pulumi.Int(1),
					MaxCount:          pulumi.Int(5),
				},
			},
			DnsPrefix:  entra.ResourceGroup.Name,
			EnableRBAC: pulumi.Bool(true),
			OidcIssuerProfile: &cs.ManagedClusterOIDCIssuerProfileArgs{
				Enabled: pulumi.Bool(true),
			},
			ServiceMeshProfile: cs.ServiceMeshProfileArgs{
				Istio: cs.IstioServiceMeshArgs{
					Revisions: pulumi.StringArray{
						pulumi.String("asm-1-23"),
					},
				},
				Mode: pulumi.String("Istio"),
			},
			SecurityProfile: &cs.ManagedClusterSecurityProfileArgs{
				WorkloadIdentity: &cs.ManagedClusterSecurityProfileWorkloadIdentityArgs{
					Enabled: pulumi.Bool(true),
				},
			},
			Sku: &cs.ManagedClusterSKUArgs{
				Name: pulumi.String("Base"),
				Tier: pulumi.String(cs.ManagedClusterSKUTierFree),
			},
			Identity: &cs.ManagedClusterIdentityArgs{
				Type: cs.ResourceIdentityTypeSystemAssigned,
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
				NetworkPlugin:   pulumi.String("azure"),
				ServiceCidr:     pulumi.String("10.0.0.0/24"),
			},
			NodeResourceGroup: entra.ResourceGroup.Name.ApplyT(func(name string) string {
				return name + "-nodepool"
			}).(pulumi.StringOutput),
			ServicePrincipalProfile: cs.ManagedClusterServicePrincipalProfileArgs{
				ClientId: entra.Application.ClientId,
				Secret:   entra.ServicerPrincipalPassword.Value,
			},
		},
		pulumi.DependsOn([]pulumi.Resource{
			sleep,
		}))

	objectId := k8sCluster.IdentityProfile.MapIndex(pulumi.String("kubeletidentity")).ObjectId()

	addAcrPullRoleToId(ctx, objectId.Elem(), "acrPull")

	// Create a Public IP
	publicIP, err := network.NewPublicIPAddress(ctx, "aks-pip", &network.PublicIPAddressArgs{
		ResourceGroupName: entra.ResourceGroup.Name.ApplyT(func(name string) string {
			return name + "-nodepool"
		}).(pulumi.StringOutput),
		PublicIPAllocationMethod: pulumi.String("Static"),
		Sku: &network.PublicIPAddressSkuArgs{
			Name: pulumi.String("Standard"),
		},
	}, pulumi.DependsOn([]pulumi.Resource{k8sCluster}))
	if err != nil {
		return nil, err
	}

	_, err = cs.NewAgentPool(ctx, "dedicatedAgentPool", &cs.AgentPoolArgs{
		ResourceGroupName: entra.ResourceGroup.Name,
		ResourceName:      k8sCluster.Name,
		AgentPoolName:     pulumi.String("dedicated"),
		Count:             pulumi.Int(1),
		EnableAutoScaling: pulumi.Bool(false),
		OsType:            pulumi.String("Linux"),
		VmSize:            pulumi.String("Standard_F4s_v2"),
		VnetSubnetID:      nodeSubnet.ID(),
		PodSubnetID:       podSubnet.ID(),
		NodeTaints: pulumi.StringArray{
			pulumi.String("dedicated=node:NoSchedule"),
		},
		NodeLabels: pulumi.StringMap{
			"dedicated": pulumi.String("titeenipeli"),
		},
	}, pulumi.DependsOn([]pulumi.Resource{k8sCluster}))
	if err != nil {
		return nil, err
	}

	return &ClusterInfo{
		ManagedCluster: k8sCluster,
		ResourceGroup:  entra.ResourceGroup,
		PublicIpName:   publicIP.Name,
		NodeSubnetID:   nodeSubnet.ID(),
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
