# Note: you need to be logged in via az login before using this script
$rgName="pulumi_infra"
$location="swedencentral"
$key='infra'
$key2='kube'
$key3='essentials'
$storageAccountName='titeenipelifoundation' # change this to unique 3-23 lower character string
$githubOrg='Cluster-ry'
$githubRepo='titeenipeli_25'

# Create resource group where bicep resources are going to be deployed
Write-Host "Creating/Updating resource group $rgName..."

az group create --name $rgName --location $location
$tenantID = az account show --query tenantId --output tsv
# Getting current users ID to set access rights to storageblob container
$email = az account show --query user.name --output tsv
$userID = az ad user show --id $email --query id --output tsv
# Deploy bicep file resources
Write-Host "Starting Pulumi setup..."

$outputs = az deployment group create --resource-group $rgName --template-file .\pulumi_setup.bicep --mode Complete `
  -p location=$location `
  -p storageAccountName=$storageAccountName `
  -p tenantId=$tenantID `
  -p key=$key `
  -p key2=$key2 `
  -p key3=$key3 `
  -p uid=$userID `
  -p githubOrg=$githubOrg `
  -p githubRepo=$githubRepo `
  --query "properties.outputs" `
  | ConvertFrom-Json

Write-Host "Pulumi setup, Done."
$roleAssignmentPreviewId = $outputs.roleAssignmentPreviewPrincipalId.value
$roleAssignmentApplyID = $outputs.roleAssignmentApplyPrincipalID.value

Write-Host "Starting Role Assignment..."
az deployment sub create `
  --name $storageAccountName `
  --location $location `
  --template-file .\roleAssignment.bicep `
  -p roleAssignmentPreviewId=$roleAssignmentPreviewId roleAssignmentApplyID=$roleAssignmentApplyID
Write-Host "Role Assignment, Done."