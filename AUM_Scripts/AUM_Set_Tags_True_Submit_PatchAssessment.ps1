# Connect to your Azure account
Connect-AzAccount -Identity

# Define variables
# Example tag of PatchMonthly would be added with value of False on machines to be checked for assessments and patch the following day.
$resourceGroupName = "ResourceGroupName"
$tagKey = "PatchMonthly"
$tagValue = "True"
$searchTagKey = "PatchMonthly"
$subid = "SubscriptionId"

# Get all Arc-enabled machines in the resource group
$machines = Get-AzConnectedMachine -ResourceGroupName $resourceGroupName -SubscriptionId $subid
# Loop through each machine and check for the PatchMonthly tag
foreach ($machine in $machines) {
    if ($machine.Tags[$searchTagKey] -eq "False") {
        # Retrieve existing tags and convert to hashtable
        $currentTags = @{}
        foreach ($key in $machine.Tags.Keys) {
            $currentTags[$key] = $machine.Tags[$key]
        }
        Write-Output "Machine: " $machine.Name
        # Add or update the Patch tag
        $currentTags[$tagKey] = $tagValue
        # Update the machine with the new tags using Update-AzTag
        $resourceId = $machine.Id
        Update-AzTag -ResourceId $resourceId -Tag $currentTags -Operation Merge
        Invoke-AzConnectedAssessMachinePatch -Name $machine.Name -ResourceGroupName $resourceGroupName -nowait
    }
}