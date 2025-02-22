# Guide to setting up new pulumi stack for azure

Firstly edit the `deployment.ps1` file and change the `$storageAccountName` variable string to something unique. This deployment script will fail if this is not unique as it is used by azure to create unique URLs for accessing the storage account and key vault.

After that login to your azure account using `az login` command. The user should have `Contributor` and `Role Based Access Control Administrator` permissions to the subscription

Once you have confirmed the subscription you want to use for creating the pulumi stack use powershell to run the `deployment.ps1` script making sure that the `pulumi_setup.bicep` template file is in the same folder.

Then you should be able to login to pulumi using `pulumi login azblob://iacstate?storage_account={storageAccountName}` replacing the `{storageAccountName}` with the unique string you set. Note that it might take a minute or two for the RBAC permissions to sync.

With the successful login you can then create a new pulumi project with the command `pulumi new --secrets-provider="azurekeyvault://{storageAccountName}.vault.azure.net/keys/{key}"` of course replacing the `{storageAccountName}` with the unique string you set along with replacing `{key}` with the variable that is in the `deployment.ps1` script.