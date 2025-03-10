package main

import (
	storage "github.com/pulumi/pulumi-azure-native-sdk/storage/v2"
	"github.com/pulumi/pulumi/sdk/v3/go/pulumi"
)

func createStorageBlob(ctx *pulumi.Context, name pulumi.String, entraInfo *EntraInfo) (pulumi.IDOutput, error) {
	account, err := storage.NewStorageAccount(ctx, "storageaccount", &storage.StorageAccountArgs{
		ResourceGroupName: entraInfo.ResourceGroup.Name,
		Sku: &storage.SkuArgs{
			Name: pulumi.String("Standard_LRS"),
		},
		Kind:        pulumi.String("StorageV2"),
		AccountName: name,
	})
	if err != nil {
		return pulumi.IDOutput{}, err
	}

	container, err := storage.NewBlobContainer(ctx, "titeeni-backup", &storage.BlobContainerArgs{
		ResourceGroupName: entraInfo.ResourceGroup.Name,
		AccountName:       account.Name,
		ContainerName:     pulumi.String("titeeni-backup"),
	})
	if err != nil {
		return pulumi.IDOutput{}, err
	}

	ctx.Export("dbBackupContainer", pulumi.Sprintf("https://%s.blob.core.windows.net/%s", account.Name, container.Name))

	return container.ID(), nil
}
