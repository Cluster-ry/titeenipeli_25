package main

import (
	"fmt"

	authorization "github.com/pulumi/pulumi-azure-native-sdk/authorization/v2"
	"github.com/pulumi/pulumi-azure-native-sdk/containerregistry/v2"
	"github.com/pulumi/pulumi-azure-native-sdk/resources/v2"
	"github.com/pulumi/pulumi-azure/sdk/v5/go/azure/core"
	"github.com/pulumi/pulumi-azuread/sdk/v5/go/azuread"
	"github.com/pulumi/pulumi/sdk/v3/go/pulumi"
	"github.com/pulumiverse/pulumi-time/sdk/go/time"
)

func main() {
	pulumi.Run(func(ctx *pulumi.Context) error {
		primary, err := core.LookupSubscription(ctx, nil, nil)
		if err != nil {
			return err
		}

		resourceGroupName := "essentials"
		resourceGroup, err := resources.NewResourceGroup(ctx, resourceGroupName, &resources.ResourceGroupArgs{
			ResourceGroupName: pulumi.String(resourceGroupName),
		})
		if err != nil {
			return err
		}

		containerRegistryName := "titeenipelitACR"
		registry, err := containerregistry.NewRegistry(ctx, containerRegistryName, &containerregistry.RegistryArgs{
			ResourceGroupName: resourceGroup.Name,
			RegistryName:      pulumi.String(containerRegistryName),
			Sku: &containerregistry.SkuArgs{
				Name: pulumi.String("Basic"),
			},
			Location:         resourceGroup.Location,
			AdminUserEnabled: pulumi.Bool(true),
		})
		if err != nil {
			return err
		}

		application, err := azuread.NewApplication(ctx, "titeeni-docker", &azuread.ApplicationArgs{
			DisplayName: pulumi.String("titeeni-docker"),
		})
		if err != nil {
			return err
		}

		servicePrincipal, err := azuread.NewServicePrincipal(ctx, "titeeni-docker-sp", &azuread.ServicePrincipalArgs{
			ClientId: application.ClientId,
		})
		if err != nil {
			return err
		}

		password, err := azuread.NewServicePrincipalPassword(ctx, "sp-password", &azuread.ServicePrincipalPasswordArgs{
			ServicePrincipalId: servicePrincipal.ID(),
			EndDate:            pulumi.String("2099-01-01T00:00:00Z"),
			DisplayName:        pulumi.String("sp-password"),
		})
		if err != nil {
			return err
		}

		// Added sleep for servicePrincipal to be created before role assignment
		sleep, err := time.NewSleep(ctx, "wait30Seconds", &time.SleepArgs{
			CreateDuration: pulumi.String("30s"),
		}, pulumi.DependsOn([]pulumi.Resource{
			servicePrincipal,
		}))
		if err != nil {
			return nil
		}

		// Role for AcrPush
		roleDefinitionId := pulumi.String(fmt.Sprintf("%s/providers/Microsoft.Authorization/roleDefinitions/8311e382-0749-4cb8-b61a-304f252e45ec", primary.Id))

		_, err = authorization.NewRoleAssignment(ctx, fmt.Sprintf("resourceGroupReaderAssignment_%s", "acrpush"), &authorization.RoleAssignmentArgs{
			PrincipalId:      servicePrincipal.ID(),
			RoleDefinitionId: roleDefinitionId,
			PrincipalType:    pulumi.String("ServicePrincipal"),
			Scope:            registry.ID(),
		}, pulumi.DependsOn([]pulumi.Resource{
			sleep, application, servicePrincipal,
		}))
		if err != nil {
			return err
		}

		ctx.Export("registryLoginServer", registry.LoginServer)
		ctx.Export("registryID", registry.ID())
		ctx.Export("servicePrincipalClientId", servicePrincipal.ClientId)
		ctx.Export("servicePrincipalPassword", password.Value)

		return nil
	})
}
