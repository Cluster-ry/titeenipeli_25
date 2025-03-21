package main

import (
	"fmt"

	"github.com/pulumi/pulumi-azure/sdk/v5/go/azure/dns"
	"github.com/pulumi/pulumi/sdk/v3/go/pulumi"
)

func createSubDomainZone(ctx *pulumi.Context, entra *EntraInfo, domainName string, subdomain string) (*dns.Zone, error) {
	baseDomain, err := dns.LookupZone(ctx, &dns.LookupZoneArgs{
		Name: domainName,
	}, nil)
	if err != nil {
		return nil, err
	}

	domain, err := dns.NewZone(ctx, fmt.Sprintf("%s.%s", subdomain, domainName), &dns.ZoneArgs{
		Name:              pulumi.String(fmt.Sprintf("%s.%s", subdomain, domainName)),
		ResourceGroupName: entra.ResourceGroup.Name,
	})
	if err != nil {
		return nil, err
	}

	_, err = dns.NewNsRecord(ctx, fmt.Sprintf("%s.%s", subdomain, domainName), &dns.NsRecordArgs{
		Name:              pulumi.String(subdomain),
		ZoneName:          pulumi.String(baseDomain.Name),
		ResourceGroupName: pulumi.String(baseDomain.ResourceGroupName),
		Ttl:               pulumi.Int(300),
		Records:           domain.NameServers,
	})
	if err != nil {
		return nil, err
	}

	return domain, nil
}
