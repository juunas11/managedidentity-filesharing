IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20190901065517_Initial')
BEGIN
    CREATE TABLE [StoredFiles] (
        [Id] uniqueidentifier NOT NULL,
        [FileName] nvarchar(256) NULL,
        [Description] nvarchar(512) NULL,
        [StoredBlobId] uniqueidentifier NOT NULL,
        [CreatorTenantId] nvarchar(64) NULL,
        [CreatorObjectId] nvarchar(64) NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        CONSTRAINT [PK_StoredFiles] PRIMARY KEY ([Id])
    );
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20190901065517_Initial')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20190901065517_Initial', N'2.2.6-servicing-10079');
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20190901150912_RemoveDescription')
BEGIN
    DECLARE @var0 sysname;
    SELECT @var0 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[StoredFiles]') AND [c].[name] = N'Description');
    IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [StoredFiles] DROP CONSTRAINT [' + @var0 + '];');
    ALTER TABLE [StoredFiles] DROP COLUMN [Description];
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20190901150912_RemoveDescription')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20190901150912_RemoveDescription', N'2.2.6-servicing-10079');
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20190901152612_AddFileContentType')
BEGIN
    ALTER TABLE [StoredFiles] ADD [FileContentType] nvarchar(128) NULL;
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20190901152612_AddFileContentType')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20190901152612_AddFileContentType', N'2.2.6-servicing-10079');
END;

GO

