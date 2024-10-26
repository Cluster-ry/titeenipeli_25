package main

import (
	"github.com/pulumi/pulumi-azure-native-sdk/resources/v2"
	"github.com/pulumi/pulumi-azuread/sdk/v5/go/azuread"
	"github.com/pulumi/pulumi/sdk/v3/go/pulumi"
)

type EntraInfo struct {
	ResourceGroup             *resources.ResourceGroup
	Application               *azuread.Application
	ServicePrincipal          *azuread.ServicePrincipal
	ServicerPrincipalPassword *azuread.ServicePrincipalPassword
}

func buildEntra(ctx *pulumi.Context) (*EntraInfo, error) {
	resourceGroup, err := resources.NewResourceGroup(ctx, "titeenipeli-rg", nil)
	if err != nil {
		return nil, err
	}

	adApp, err := azuread.NewApplication(ctx, "titeenipeli-app",
		&azuread.ApplicationArgs{
			DisplayName: pulumi.String("titeenipeli-app"),
		})
	if err != nil {
		return nil, err
	}

	adSp, err := azuread.NewServicePrincipal(ctx, "service-principal",
		&azuread.ServicePrincipalArgs{
			ClientId: adApp.ClientId,
		})
	if err != nil {
		return nil, err
	}

	adSpPassword, err := azuread.NewServicePrincipalPassword(ctx, "sp-password",
		&azuread.ServicePrincipalPasswordArgs{
			ServicePrincipalId: adSp.ID(),
			EndDate:            pulumi.String("2099-01-01T00:00:00Z"),
		})
	if err != nil {
		return nil, err
	}

	return &EntraInfo{
		ResourceGroup:             resourceGroup,
		Application:               adApp,
		ServicePrincipal:          adSp,
		ServicerPrincipalPassword: adSpPassword,
	}, nil
}
