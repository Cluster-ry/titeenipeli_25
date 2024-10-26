# Note: you need to be logged in via az login before using this script
$rgName="pulumi_infra"
$location="swedencentral"
$key='infra'
$key2='kube'
$storageAccountName='titeenipelifoundation' # change this to unique 3-23 lower character string

# Create resource group where bicep resources are going to be deployed
az group create --name $rgName --location $location
$tenantID = az account show --query tenantId --output tsv
# Getting current users ID to set access rights to storageblob container
$email = az account show --query user.name --output tsv
$userID = az ad user show --id $email --query id --output tsv
# Deploy bicep file resources
az deployment group create --resource-group $rgName --template-file .\pulumi_setup.bicep --mode Complete -p location=$location -p storageAccountName=$storageAccountName -p tenantId=$tenantID -p key=$key -p key2=$key2 -p uid=$userID
