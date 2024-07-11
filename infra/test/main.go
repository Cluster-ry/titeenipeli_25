package main

import (
	"github.com/pulumi/pulumi-azure-native-sdk/resources/v2"
	"github.com/pulumi/pulumi/sdk/v3/go/pulumi"
)

func main() {
	pulumi.Run(func(ctx *pulumi.Context) error {
		// Create an Azure Resource Group
		_, err := resources.NewResourceGroup(ctx, "testingRG", nil)
		if err != nil {
			return err
		}
		return nil
	})
}
