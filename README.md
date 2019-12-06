# Demo app: File sharing app using Managed Identities for Azure Resources

This app showcases using Azure Storage and Azure SQL Database through Managed Identities.
Users can sign in to the app using their personal Microsoft account or an organizational Azure AD/Office 365 account.
They can then upload files which are stored in Azure Storage.
Personal account users can only see their own files.
Organizational account users can see files uploaded by anyone in their organization.

## Setup instructions

You'll need to have the .NET Core 3.1 SDK installed.
You can use any editor with the project, though I used Visual Studio 2019.

To enable sign-ins, you need to register an app in your Azure AD tenant.
If you want to allow any account to sign in as originally intended,
make sure to select the following option when creating the app registration:

> Accounts in any organizational directory (Any Azure AD directory - Multitenant) and personal Microsoft accounts (e.g. Skype, Xbox)

You should also add `https://localhost:44318/aad-callback` as a reply URL to enable local development.
If you deploy the app in Azure App Service, you can also add `https://yourappservicename.azurewebsites.net/aad-callback` to the reply URLs.

### Local setup

To run the app locally, you need to have an SQL Server (Express should be fine) + Azure Storage Emulator.
Modify appsettings.Development.json:

- ConnectionStrings:DefaultConnection: Modify to point the app at your development database
- Authentication:ClientId: The application id / client id of the app you registered in AAD

You need to create the `files` container in your Storage Emulator.
The app does not create the container as it won't have the right to do that in Azure.

Create the database in your SQL Server and run the SetupScripts/Migrations.sql script in there.
You can also run `dotnet ef` commands to migrate your database/generate the script.

You should be able to run the app locally now.

Note that you may get an error saying the Storage Emulator does not support
the API version we are using.
In that case, you will need to update the Storage Emulator.

### Azure setup

Deploy the ARM template located in the Joonasw.ManagedIdentityFileSharingDemo.ARM project.
There is a parameter file that you can use,
but you can also specify all the parameters at deployment time.

Note you will have to know the Azure AD user who will be the "Active Directory admin" for the SQL server.
You will need the user's username and object id (within the tenant linked to the subscription).

After successfully deploying the ARM template, you can setup the Azure SQL database with
a script included in the SetupScripts folder.
In order to run the script,
*add your current client IP address to the SQL server firewall*.

Open PowerShell in the SetupScripts folder and run (replace values with yours):

```
.\Setup-AzureDb.ps1 -serverName 'azure-sql-server-name.database.windows.net' -databaseName 'azure-db-name' -username 'aad-admin-username' -webAppName 'web-app-name'
```

The script will execute pre-generated EF Core migrations and
allow access to the Managed Identity.
It will pop a login twice for your user,
one for the migrations file and one for the inline query to enable access.
