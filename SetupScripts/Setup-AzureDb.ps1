param(
	[string]
	$serverName,
	[string]
	$databaseName,
	[string]
	$username,
	[string]
	$webAppName
)

# This script requires SQLCMD 15.0.1000.34 or later:
# https://docs.microsoft.com/en-us/sql/tools/sqlcmd-utility?view=sql-server-2017#download-the-latest-version-of-sqlcmd-utility
# I also needed to then modify the PATH so that this newer version was before the older version :\

# Run migrations
sqlcmd -S $serverName -d $databaseName -G -N -U $username -t 120 -b -i '.\Migrations.sql'

# Create Managed Identity user and grant permissions
$query = "CREATE USER [$webAppName] FROM EXTERNAL PROVIDER;"
$query = $query + "GRANT SELECT ON dbo.__EFMigrationsHistory TO [$webAppName];"
$query = $query + "GRANT SELECT,INSERT,DELETE ON dbo.StoredFiles TO [$webAppName];"

sqlcmd -S $serverName -d $databaseName -G -N -U $username -t 120 -b -Q $query
