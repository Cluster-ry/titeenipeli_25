param location string
param storageAccountName string
param tenantId string
param key string
param key2 string
param uid string

var storageBlobDataContributorID = subscriptionResourceId('Microsoft.Authorization/roleDefinitions', 'ba92f5b4-2d11-453d-a403-e96b0029c9fe')
var keyVaultCryptoUserID = subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '12338af0-0e69-4776-bea7-57ae8d297424')
var keyVaultSecretsUserID = subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '4633458b-17de-408a-b874-0445c86b69e6')

resource storageAccount 'Microsoft.Storage/storageAccounts@2023-05-01' = {
  name: storageAccountName
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    accessTier: 'Hot'
    supportsHttpsTrafficOnly: true
  }
}

resource storageAccountBlob 'Microsoft.Storage/storageAccounts/blobServices@2023-05-01' = {
  name: 'default'
  parent: storageAccount
}

resource iacContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-05-01' = {
  name: 'iacstate'
  parent: storageAccountBlob
  properties: {
    publicAccess: 'None'
  }
}

resource roleAssingmentStorageAccount 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: storageAccount
  name: guid(storageAccount.id, uid, storageBlobDataContributorID)
  properties: {
    roleDefinitionId: storageBlobDataContributorID
    principalType: 'User'
    principalId: uid
  }
}

resource keyvault 'Microsoft.KeyVault/vaults@2023-07-01' = {
  name: storageAccountName
  location: location
  properties: {
    sku: {
      family: 'A'
      name: 'standard'
    }
    tenantId: tenantId
    enableRbacAuthorization: true
  }
}

resource keyvaultKey 'Microsoft.KeyVault/vaults/keys@2023-07-01' = {
  name: key
  parent: keyvault
  properties: {
    attributes: {
      enabled: true
      exportable: false
    }
    kty: 'RSA'
  }
}

resource keyvaultKey2 'Microsoft.KeyVault/vaults/keys@2023-07-01' = {
  name: key2
  parent: keyvault
  properties: {
    attributes: {
      enabled: true
      exportable: false
    }
    kty: 'RSA'
  }
}

resource roleAssingmentKeyVaultCrypto 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: keyvault
  name: guid(keyvault.id, uid, keyVaultCryptoUserID)
  properties: {
    roleDefinitionId: keyVaultCryptoUserID
    principalType: 'User'
    principalId: uid
  }
}

resource roleAssingmentKeyVaultSecrets 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: keyvault
  name: guid(keyvault.id, uid, keyVaultSecretsUserID)
  properties: {
    roleDefinitionId: keyVaultSecretsUserID
    principalType: 'User'
    principalId: uid
  }
}
