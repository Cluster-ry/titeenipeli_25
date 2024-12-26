package main

import (
	dbforpostgresql "github.com/pulumi/pulumi-azure-native-sdk/dbforpostgresql/v2/v20240801"
	network "github.com/pulumi/pulumi-azure-native-sdk/network/v2"
	"github.com/pulumi/pulumi/sdk/v3/go/pulumi"
	"github.com/sethvargo/go-password/password"
)

func createCosmosDB(ctx *pulumi.Context, k8sCluster *ClusterInfo) error {

	pw, err := password.Generate(20, 5, 5, false, false)
	if err != nil {
		return err
	}
	ctx.Export("adminDBPass", pulumi.ToSecret(pw))

	postgresServer, err := dbforpostgresql.NewServer(ctx, "titeeniDB", &dbforpostgresql.ServerArgs{
		ResourceGroupName: k8sCluster.ResourceGroup.Name,
		Location:          k8sCluster.ResourceGroup.Location,
		ServerName:        pulumi.String("titeenipelidb"),
		Sku: &dbforpostgresql.SkuArgs{
			Name: pulumi.String("Standard_B1ms"),
			Tier: pulumi.String(dbforpostgresql.SkuTierBurstable),
		},
		AdministratorLogin:         pulumi.String("myadmin"),
		AdministratorLoginPassword: pulumi.String(pw),
		AvailabilityZone:           pulumi.String("1"),
		Backup: &dbforpostgresql.BackupTypeArgs{
			BackupRetentionDays: pulumi.Int(7),
			GeoRedundantBackup:  pulumi.String(dbforpostgresql.GeoRedundantBackupEnumDisabled),
		},
		Storage: &dbforpostgresql.StorageArgs{
			StorageSizeGB: pulumi.Int(32),
		},
		HighAvailability: &dbforpostgresql.HighAvailabilityArgs{
			Mode: pulumi.String(dbforpostgresql.HighAvailabilityModeDisabled),
		},
		Version:    pulumi.String(dbforpostgresql.ServerVersion_16),
		CreateMode: pulumi.String(dbforpostgresql.CreateModeCreate),
	})
	if err != nil {
		return err
	}

	/*
		TODO: This private endpoint still needs to be accepted via portal as this pulumi resource  does not provide the
		PrivateEndpointConnectionName for the azure-native.dbforpostgresql.PrivateEndpointConnection resource that could
		automatically accept it automatically.
	*/
	_, err = network.NewPrivateEndpoint(ctx, "dbPrivateEndpoint", &network.PrivateEndpointArgs{
		ResourceGroupName:   k8sCluster.ResourceGroup.Name,
		Location:            k8sCluster.ResourceGroup.Location,
		PrivateEndpointName: pulumi.String("titeenipelidbprivateendpoint"),
		Subnet: &network.SubnetTypeArgs{
			Id: k8sCluster.NodeSubnetID,
		},
		ManualPrivateLinkServiceConnections: network.PrivateLinkServiceConnectionArray{
			&network.PrivateLinkServiceConnectionArgs{
				GroupIds: pulumi.StringArray{
					pulumi.String("postgresqlServer"),
				},
				PrivateLinkServiceId: postgresServer.ID(),
				Name:                 pulumi.String("titeenipelidbprivateendpoint"),
			},
		},
	})
	if err != nil {
		return err
	}

	return nil

}
