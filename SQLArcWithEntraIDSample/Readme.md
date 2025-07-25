# SQL Arc with Entra ID Sample
This sample uses Entra ID from an Arc Enabled SQL Server to query the SQL database in a Winform app. The requirement was to use .NET 4.6.2. 
This sample has a button for checking the current user, a button to connect with SQL using a cached Entra ID if it exists. If there is no cached token it will prompt the user for the user to use, and then cache this token. The final button connects to Adventureworks database. The checkbox allows the sample to cache the token to disk or keep it in memory. If the file is cached to disk, the location of the cached file is (C:\Users\\[useraccount]\AppData\Local).

Within the code there are only a few areas to make changes. Enter in your Application Registration Client ID, and the TenantID **(Line 60, 61 of Form1.cs)**. 

<img width="637" height="414" alt="image" src="https://github.com/user-attachments/assets/4415d794-d808-4a36-a54b-9689e80c5352" />



Requirements: Follow the documentation for enabling SQL Server authentication with Entra ID. This link describes how to add Entra ID to the Arc SQL Server, add a Key Vault with a self signed certificate that is used.  [Set up Microsoft Entra authentication for SQL Server](https://learn.microsoft.com/en-us/sql/relational-databases/security/authentication-access/azure-ad-authentication-sql-server-setup-tutorial?view=sql-server-ver17)
. 

This is a great place to start for testing and using Entra ID with SSMS and sets most of the configuration needed for this sample.

## API permissions.

## The application requires Entra ID permissions for Azure SQL Database to access from the Winform application
Select add a new permission, and select the tab "APIs my organization uses". Search for Azure SQL Database in the list.
![image](https://github.com/user-attachments/assets/4614b376-7000-4964-9305-960f8abf9102)

Choose Delegated permissions and check the option for user_impersonation.
![image](https://github.com/user-attachments/assets/8247abf5-4516-48a0-9f22-f7ba9507f7a2)

The above article mentions the other permisisons that are required.
Select Add a permission > Microsoft Graph > Application permissions
 - Check Application.Read.All
 - Check Group.Read.All
 - Check User.Read.All

