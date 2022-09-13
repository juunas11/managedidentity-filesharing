@description('Azure location for resources')
param location string = resourceGroup().location

@description('Name of Application Insights resource')
param appInsightsName string

@description('Name of logical SQL Server')
param sqlServerName string

@description('SQL Database name')
param sqlDatabaseName string

@description('Username for SQL server admin')
param sqlAdminUsername string

@description('Password for SQL server admin')
@secure()
param sqlAdminPassword string

@description('Username of Azure AD user who will be admin')
param sqlAadAdminUsername string

@description('Object id of Azure AD user who will be admin')
param sqlAadAdminObjectId string

@description('Name of App Service Plan')
param appServicePlanName string

@description('Pricing tier of App Service Plan')
@allowed([
  'F1'
  'D1'
  'B1'
  'B2'
  'B3'
  'S1'
  'S2'
  'S3'
  'P1'
  'P2'
  'P3'
  'P4'
])
param appServicePlanTier string = 'F1'

@description('# of instances in App Service Plan')
@minValue(1)
param appServicePlanCapacity int = 1

@description('Name of Web App that will host the app')
param webAppName string

@description('Name of Storage account')
@maxLength(24)
param storageAccountName string

@description('Name of blob container in Storage account')
param storageContainerName string

@description('Client id / application id of app in Azure AD used for login')
param aadAppClientId string

var storageBlobContributorRoleId = subscriptionResourceId('Microsoft.Authorization/roleDefinitions', 'ba92f5b4-2d11-453d-a403-e96b0029c9fe')

resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: appInsightsName
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
  }
}

resource sqlServer 'Microsoft.Sql/servers@2021-08-01-preview' = {
  name: sqlServerName
  location: location
  properties: {
    administratorLogin: sqlAdminUsername
    administratorLoginPassword: sqlAdminPassword
    version: '12.0'
  }
}

resource sqlServerAadAdmin 'Microsoft.Sql/servers/administrators@2021-08-01-preview' = {
  parent: sqlServer
  name: 'ActiveDirectory'
  properties: {
    administratorType: 'ActiveDirectory'
    login: sqlAadAdminUsername
    sid: sqlAadAdminObjectId
    tenantId: subscription().tenantId
  }
}

resource sqlDatabase 'Microsoft.Sql/servers/databases@2021-08-01-preview' = {
  parent: sqlServer
  name: sqlDatabaseName
  location: location
  sku: {
    name: 'Basic'
    tier: 'Basic'
  }
  properties: {
    collation: 'SQL_Latin1_General_CP1_CI_AS'
    maxSizeBytes: 2147483648
    catalogCollation: 'SQL_Latin1_General_CP1_CI_AS'
    zoneRedundant: false
  }
}

resource sqlServerFirewallAllowAzure 'Microsoft.Sql/servers/firewallRules@2021-08-01-preview' = {
  parent: sqlServer
  name: 'AllowAllWindowsAzureIps'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

resource storageAccount 'Microsoft.Storage/storageAccounts@2021-08-01' = {
  name: storageAccountName
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    networkAcls: {
      bypass: 'AzureServices'
      virtualNetworkRules: []
      ipRules: []
      defaultAction: 'Allow'
    }
    supportsHttpsTrafficOnly: true
    encryption: {
      services: {
        file: {
          enabled: true
        }
        blob: {
          enabled: true
        }
      }
      keySource: 'Microsoft.Storage'
    }
    accessTier: 'Hot'
  }
}

resource storageBlobService 'Microsoft.Storage/storageAccounts/blobServices@2021-08-01' = {
  parent: storageAccount
  name: 'default'
}

resource storageContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2021-08-01' = {
  parent: storageBlobService
  name: storageContainerName
  properties: {
    publicAccess: 'None'
  }
}

resource webAppFilesAccessRoleAssignment 'Microsoft.Authorization/roleAssignments@2020-10-01-preview' = {
  scope: storageContainer
  name: guid(resourceGroup().id, 'webAppFilesAccess')
  properties: {
    principalId: webApp.identity.principalId
    roleDefinitionId: storageBlobContributorRoleId
  }
}

resource appServicePlan 'Microsoft.Web/serverfarms@2021-03-01' = {
  name: appServicePlanName
  location: location
  sku: {
    name: appServicePlanTier
    capacity: appServicePlanCapacity
  }
  kind: 'app'
  properties: {}
}

resource webApp 'Microsoft.Web/sites@2021-03-01' = {
  name: webAppName
  location: location
  kind: 'app'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    enabled: true
    serverFarmId: appServicePlan.id
    clientAffinityEnabled: false
    httpsOnly: true
  }
}

resource webAppWebConfig 'Microsoft.Web/sites/config@2021-03-01' = {
  parent: webApp
  name: 'web'
  properties: {
    use32BitWorkerProcess: true
  }
}

resource webAppSettings 'Microsoft.Web/sites/config@2021-03-01' = {
  parent: webApp
  name: 'appsettings'
  properties: {
    Authentication__ClientId: aadAppClientId
    Storage__AccountName: storageAccountName
    Storage__FileContainerName: storageContainerName
    HTTPS_PORT: '443'
    APPINSIGHTS_CONNECTIONSTRING: appInsights.properties.ConnectionString
  }
}

resource webAppConnectionStrings 'Microsoft.Web/sites/config@2021-03-01' = {
  parent: webApp
  name: 'connectionstrings'
  properties: {
    DefaultConnection: {
      type: 'SQLAzure'
      value: 'Server=tcp:${sqlServerName}${environment().suffixes.sqlServerHostname},1433;Initial Catalog=${sqlDatabaseName};Authentication=Active Directory Managed Identity;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;'
    }
  }
}
