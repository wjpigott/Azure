# Azure Samples
## AUM_Scripts Sample.
1. The StartVMs.json/StopVMs.json are used with Azure Update Manager Pre/Post Scripts for Azure Virtual Machines. This use case is to start machines that are stopped prior to patching. Utilize Tags to determine if the machine should only be started, or also stopped at the end of the patching using a post event.
2. The AUM_Set_Tags_True_Submit_PatchAssessment.ps1 script could be used with a Automation Account to schedule monthly patching the day before a maintenance schedule. This would set a tag to PatchMonthly = True. A dynamic scope on a Maintenance schedule would look for machines only with PatchMonthly = True to patch during that month. After the Tag is set, a request is made to access to check for updates.
3. The AUM_Set_Tags_False.ps1 script could be used as Post event script. This script sets the tag PatchMonthly = False after the machines were patched. This would "remove" the machine from a dynamic scope only looking to patch machines that have the tag PatchMontly = True. 

## SQLArcWithEntraIDSample Sample
The SQLArcWithEntraIDSample Visual Studio project is a Winform example of how to use EntraID to connect to an Arc Enabled SQL Server. This form has options to cache the EntraID user token into memory or disk. It uses the Adventure Works database to pull up a small dataset of users to display in a datagrid. 

## 📁 Downloading Specific Folders

This repository contains multiple Azure-related samples organized by subfolder. If you only want to download a specific folder (e.g., `AUM_Scripts`, `SQLArcWithEntraIDSample`, etc.), here are a few easy options:

---

### ✅ Option 1: Use DownGit (No Git Required)

You can download any folder as a ZIP file using [DownGit]
- [Download the AUM_Scripts folder](https://minhaskamal.github.io/DownGit/#/home?url=https://github.com/wjpigott/Azure/tree/main/AUM_Scripts) the path to the folder you want.

- [Download the SQLArcWithEntraIDSample folder](https://minhaskamal.github.io/DownGit/#/home?url=https://github.com/wjpigott/Azure/tree/main/SQLArcWithEntraIDSample) the path to the folder you want.

---

### ✅ Option 2: Use Git Sparse Checkout (Advanced Git Users)

If you're comfortable with Git and want to clone only a specific folder:

```bash
git clone --filter=blob:none --no-checkout https://github.com/wjpigott/Azure.git
cd Azure
git sparse-checkout init --cone
git sparse-checkout set SQLArcWithEntraIDSample  # Replace 'SQLArcWithEntraIDSample' with the folder you want
