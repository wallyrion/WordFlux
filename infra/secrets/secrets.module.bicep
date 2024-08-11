targetScope = 'resourceGroup'

@description('')
param location string = resourceGroup().location

@description('')
param principalId string

@description('')
param principalType string


resource keyVault_hph4bC5pB 'Microsoft.KeyVault/vaults@2022-07-01' = {
  name: toLower(take('secrets${uniqueString(resourceGroup().id)}', 24))
  location: location
  tags: {
    'aspire-resource-name': 'secrets'
  }
  properties: {
    tenantId: tenant().tenantId
    sku: {
      family: 'A'
      name: 'standard'
    }
    enableRbacAuthorization: true
  }
}

resource roleAssignment_3vQayc4Sc 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: keyVault_hph4bC5pB
  name: guid(keyVault_hph4bC5pB.id, principalId, subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '00482a5a-887f-4fb3-b363-3b7fe8e74483'))
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '00482a5a-887f-4fb3-b363-3b7fe8e74483')
    principalId: principalId
    principalType: principalType
  }
}

output vaultUri string = keyVault_hph4bC5pB.properties.vaultUri
