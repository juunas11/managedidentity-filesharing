# Demo app: File sharing app using Managed Identities for Azure Resources

This app showcases using Azure Storage and Azure SQL Database through Managed Identities.
Users can sign in to the app using their personal Microsoft account or an organizational Azure AD/Office 365 account.
They can then upload files which are stored in Azure Storage.
Personal account users can only see their own files.
Organizational account users can see files uploaded by anyone in their organization.

## Setup instructions

You'll need to have the .NET Core 2.2 SDK installed.
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

### Azure setup

There's no ARM template yet, it's on the todo list.
So you'll have to create the Azure resources manually:

- App Service
- SQL Database
- Storage account

1. Enable a System-assigned Managed Identity on the App Service
1. Assign connection string "DefaultConnection" to the App Service with Azure SQL type e.g. `Server=tcp:servernamehere.database.windows.net,1433;Initial Catalog=DatabaseNameHere;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;`
1. Assign app settings to the App Service:
   - Authentication__ClientId: your Azure AD application id / client id
   - Storage__AccountName: your Storage account name
   - Storage__FileContainerName: files (or another name if you made it with another name)
   - HTTPS_PORT: 443
1. Create the Storage container "files"
1. Assign the "Storage Blob Data Contributor" role to the App Service's identity on the created container
   - You can do this via the Portal or by running SetupScripts/Assign-Permissions.ps1 (set your own values in the script first though)
1. Enable AAD admin on the SQL server
1. Connect to the SQL server using the AAD admin
1. Run the SetupScripts/Migrations.sql on the database to create tables
1. Modify and run SetupScripts/CreateSqlUser.sql (change the user name to your App Service name)

Deploy the app to the App Service and all should work.
