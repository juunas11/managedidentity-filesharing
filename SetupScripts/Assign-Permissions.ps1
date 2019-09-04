$tenantId = ''
$subscriptionId = ''
$resourceGroupName = ''
$appServiceName = ''
$storageAccountName = ''
$storageContainerName = 'files'

Add-AzAccount -Tenant $tenantId -Subscription $subscriptionId

$app = Get-AzWebApp -ResourceGroupName $rg -Name $appServiceName
$servicePrincipalId = $app.Identity.PrincipalId

$blobRole = Get-AzRoleDefinition -Name 'Storage Blob Data Contributor'
New-AzRoleAssignment -RoleDefinitionId $blobRole.Id -ObjectId $servicePrincipalId `
    -Scope "/subscriptions/$subscriptionId/resourcegroups/$resourceGroupName/providers/Microsoft.Storage/storageAccounts/$storageAccountName/blobServices/default/containers/$storageContainerName"