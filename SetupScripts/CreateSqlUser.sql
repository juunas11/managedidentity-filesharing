CREATE USER [managedidentitysharing] FROM EXTERNAL PROVIDER;
GRANT SELECT ON dbo.__EFMigrationsHistory TO [managedidentitysharing];
GRANT SELECT,INSERT ON dbo.StoredFiles TO [managedidentitysharing];