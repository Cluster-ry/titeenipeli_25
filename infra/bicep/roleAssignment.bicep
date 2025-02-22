targetScope = 'subscription'

param roleAssignmentPreviewId string
param roleAssignmentApplyID string

// Add reader role
resource roleAssignmentPreview 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(roleAssignmentPreviewId, 'Reader', subscription().id)
  properties: {
    principalId: roleAssignmentPreviewId
    roleDefinitionId: subscriptionResourceId(
      'Microsoft.Authorization/roleDefinitions',
      'acdd72a7-3385-48ef-bd42-f606fba81ae7'
    )
  }
}

// Add contributor role
resource roleAssignmentApply 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(roleAssignmentApplyID, 'Contributor', subscription().id)
  properties: {
    principalId: roleAssignmentApplyID
    roleDefinitionId: subscriptionResourceId(
      'Microsoft.Authorization/roleDefinitions',
      'b24988ac-6180-42a0-ab88-20f7382dd24c'
    )
  }
}
