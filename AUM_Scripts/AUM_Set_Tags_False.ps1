# Post Event script to change tag "PatchMonthly" on machines that are patched back to False to remove machine from dynamic scope schedule.
# 
# Make sure that we are using eventGridEvent for parameter binding in Azure function.
param($eventGridEvent, $TriggerMetadata)

if (-not (Get-AzContext)) {
    Connect-AzAccount -Identity
}
 Install-Module Az.ConnectedMachine -Force
#Install the Resource Graph module from PowerShell Gallery
if (-not (Get-Module -Name Az.ResourceGraph -ListAvailable)) {
    Install-Module -Name Az.ResourceGraph -Force 
}

$maintenanceRunId = $eventGridEvent.data.CorrelationId
$resourceSubscriptionIds = $eventGridEvent.data.ResourceSubscriptionIds

if ($resourceSubscriptionIds.Count -eq 0) {
    Write-Output "Resource subscriptions are not present."
    break
}

Start-Sleep -Seconds 30
Write-Output "Querying ARG to get machine details [MaintenanceRunId=$maintenanceRunId][ResourceSubscriptionIdsCount=$($resourceSubscriptionIds.Count)]"

$argQuery = @"
    maintenanceresources 
    | where type =~ 'microsoft.maintenance/applyupdates'
    | where properties.correlationId =~ '$($maintenanceRunId)'
    | where id has '/providers/Microsoft.HybridCompute/machines/'
    | project id, resourceId = tostring(properties.resourceId)
    | order by id asc
"@

Write-Output "Arg Query Used: $argQuery"

$allMachines = [System.Collections.ArrayList]@()
$skipToken = $null

do
{
    $res = Search-AzGraph -Query $argQuery -First 1000 -SkipToken $skipToken -Subscription $resourceSubscriptionIds
    $skipToken = $res.SkipToken
    $allMachines.AddRange($res.Data)
} while ($skipToken -ne $null -and $skipToken.Length -ne 0)

if ($allMachines.Count -eq 0) {
    Write-Output "No Machines were found."
    break
}

    $jobIDs= New-Object System.Collections.Generic.List[System.Object]
    $allMachines | ForEach-Object {
    $vmId =  $_.resourceId

    $split = $vmId -split "/";
    $subscriptionId = $split[2]; 
    $rg = $split[4];
    $name = $split[8];

    Write-Output ("Subscription Id: " + $subscriptionId)
    Write-Output ("Resource Group: " + $rg)
     Write-Output ("Name: " + $name)
# Define variables
$resourceGroupName = $rg
$machineName = $name
$tagKey = "PatchMonthly"
$tagValue = "False"
# Get the Arc-enabled machine
$machine = Get-AzConnectedMachine -ResourceGroupName $resourceGroupName -Name $machineName
# Retrieve existing tags and convert to hashtable
$currentTags = @{}
foreach ($key in $machine.Tags.Keys) {
    $currentTags[$key] = $machine.Tags[$key]
}
# Add or update the tag
$currentTags[$tagKey] = $tagValue
# Update the machine with the new tags using Update-AzTag
$resourceId = $machine.Id
Update-AzTag -ResourceId $resourceId -Tag $currentTags -Operation Merge
}
    $jobsList = $jobIDs.ToArray()
    if ($jobsList)
    {
    Write-Output "Waiting for Update Tags..."
    Wait-Job -Id $jobsList
    }

    foreach($id in $jobsList)
    {
    $job = Get-Job -Id $id
    if ($job.Error)
    {
    Write-Output $job.Error
    }
}
