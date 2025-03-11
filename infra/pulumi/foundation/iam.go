package main

import (
	"fmt"

	authorization "github.com/pulumi/pulumi-azure-native-sdk/authorization/v2"
	"github.com/pulumi/pulumi-azure-native-sdk/resources/v2"
	"github.com/pulumi/pulumi-azure/sdk/v5/go/azure/core"
	"github.com/pulumi/pulumi-azure/sdk/v5/go/azure/dns"
	"github.com/pulumi/pulumi/sdk/v3/go/pulumi"
)

func addDNSZoneContributorRoleToId(ctx *pulumi.Context, domain *dns.Zone, id *workloadIdentities, name pulumi.String) error {
	primary, err := core.LookupSubscription(ctx, nil, nil)
	if err != nil {
		return err
	}

	// Role for DNS Zone Contributor
	roleDefinitionId := pulumi.String(fmt.Sprintf("%s/providers/Microsoft.Authorization/roleDefinitions/befefa01-2a29-4197-83a8-272ff33ce314", primary.Id))

	_, err = authorization.NewRoleAssignment(ctx, fmt.Sprintf("dnsZoneContributorAssignment_%s", name), &authorization.RoleAssignmentArgs{
		PrincipalId:      id.UserAssignedIdentity.PrincipalId,
		PrincipalType:    pulumi.String("ServicePrincipal"),
		RoleDefinitionId: roleDefinitionId,
		Scope:            domain.ID(),
	})
	if err != nil {
		return err
	}

	return nil
}

func addResourceGroupReaderRoleToId(ctx *pulumi.Context, rg *resources.ResourceGroup, id *workloadIdentities, name pulumi.String) error {
	primary, err := core.LookupSubscription(ctx, nil, nil)
	if err != nil {
		return err
	}

	// Role for DNS Zone Contributor
	roleDefinitionId := pulumi.String(fmt.Sprintf("%s/providers/Microsoft.Authorization/roleDefinitions/acdd72a7-3385-48ef-bd42-f606fba81ae7", primary.Id))

	_, err = authorization.NewRoleAssignment(ctx, fmt.Sprintf("resourceGroupReaderAssignment_%s", name), &authorization.RoleAssignmentArgs{
		PrincipalId:      id.UserAssignedIdentity.PrincipalId,
		PrincipalType:    pulumi.String("ServicePrincipal"),
		RoleDefinitionId: roleDefinitionId,
		Scope:            rg.ID(),
	})
	if err != nil {
		return err
	}

	return nil
}

func addAcrPullRoleToId(ctx *pulumi.Context, principalId pulumi.StringOutput, name pulumi.String) error {
	primary, err := core.LookupSubscription(ctx, nil, nil)
	if err != nil {
		return err
	}

	referencedStack, err := pulumi.NewStackReference(ctx, "organization/essentials/dev", nil)
	if err != nil {
		return fmt.Errorf("error referencing stack: %w", err)
	}
	registryID := referencedStack.GetOutput(pulumi.String("registryID")).AsStringOutput()

	// Role for AcrPull
	roleDefinitionId := pulumi.String(fmt.Sprintf("%s/providers/Microsoft.Authorization/roleDefinitions/7f951dda-4ed3-4680-a7ca-43fe172d538d", primary.Id))

	_, err = authorization.NewRoleAssignment(ctx, fmt.Sprintf("resourceGroupReaderAssignment_%s", name), &authorization.RoleAssignmentArgs{
		PrincipalId:      principalId,
		PrincipalType:    pulumi.String("ServicePrincipal"),
		RoleDefinitionId: roleDefinitionId,
		Scope:            registryID,
	})
	if err != nil {
		return err
	}

	return nil
}

func addStorageBlobDataContributorToId(ctx *pulumi.Context, id *workloadIdentities, name pulumi.String, scope pulumi.IDOutput) error {
	primary, err := core.LookupSubscription(ctx, nil, nil)
	if err != nil {
		return err
	}

	// Role for Storage Blob Data Contributor
	roleDefinitionId := pulumi.String(fmt.Sprintf("%s/providers/Microsoft.Authorization/roleDefinitions/ba92f5b4-2d11-453d-a403-e96b0029c9fe", primary.Id))

	_, err = authorization.NewRoleAssignment(ctx, fmt.Sprintf("storageBlobAssignment_%s", name), &authorization.RoleAssignmentArgs{
		PrincipalId:      id.UserAssignedIdentity.PrincipalId,
		PrincipalType:    pulumi.String("ServicePrincipal"),
		RoleDefinitionId: roleDefinitionId,
		Scope:            scope,
	})
	if err != nil {
		return err
	}

	return nil
}
