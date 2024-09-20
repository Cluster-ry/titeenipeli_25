package main

import (
	"fmt"

	authorization "github.com/pulumi/pulumi-azure-native-sdk/authorization/v2"
	"github.com/pulumi/pulumi-azure/sdk/v5/go/azure/core"
	"github.com/pulumi/pulumi-azure/sdk/v5/go/azure/dns"
	"github.com/pulumi/pulumi/sdk/v3/go/pulumi"
)

func addDNSZoneContributorRoleToId(ctx *pulumi.Context, domain *dns.Zone, id *workloadIdentities) error {
	primary, err := core.LookupSubscription(ctx, nil, nil)
	if err != nil {
		return err
	}

	// Role for DNS Zone Contributor
	roleDefinitionId := pulumi.String(fmt.Sprintf("%s/providers/Microsoft.Authorization/roleDefinitions/befefa01-2a29-4197-83a8-272ff33ce314", primary.Id))

	_, err = authorization.NewRoleAssignment(ctx, "dnsZoneContributorAssignment", &authorization.RoleAssignmentArgs{
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
