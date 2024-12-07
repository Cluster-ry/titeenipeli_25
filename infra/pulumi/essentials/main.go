package main

import (
	"github.com/pulumi/pulumi-azure-native-sdk/containerregistry/v2"
	"github.com/pulumi/pulumi-azure-native-sdk/resources/v2"
	"github.com/pulumi/pulumi/sdk/v3/go/pulumi"
)

func main() {
	pulumi.Run(func(ctx *pulumi.Context) error {
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

		ctx.Export("registryLoginServer", registry.LoginServer)

		return nil
	})
}
