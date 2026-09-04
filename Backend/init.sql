IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [Users] (
    [Id] uniqueidentifier NOT NULL,
    [FullName] nvarchar(max) NOT NULL,
    [Email] nvarchar(450) NOT NULL,
    [Picture] nvarchar(max) NOT NULL,
    [ProviderKey] nvarchar(450) NOT NULL,
    [Role] nvarchar(10) NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [AIReadHistories] (
    [Id] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [Status] nvarchar(10) NOT NULL,
    [CardCount] nvarchar(10) NULL,
    [QuestionType] nvarchar(10) NULL,
    [Question] nvarchar(2000) NOT NULL,
    [Answer] nvarchar(max) NULL,
    [Cards] nvarchar(2000) NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_AIReadHistories] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AIReadHistories_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [ReadHistories] (
    [Id] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [CardCode] nvarchar(12) NOT NULL,
    [IsReversed] bit NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_ReadHistories] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ReadHistories_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [RefreshTokens] (
    [Id] uniqueidentifier NOT NULL,
    [Token] nvarchar(max) NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [ExpiresAt] datetimeoffset NOT NULL,
    [DeviceFingerprint] nvarchar(450) NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_RefreshTokens] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_RefreshTokens_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Wallets] (
    [Id] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [WhiteCoin] int NOT NULL DEFAULT 0,
    [RedCoin] int NOT NULL DEFAULT 0,
    [CreatedAt] datetimeoffset NOT NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_Wallets] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Wallets_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [ChatMessages] (
    [Id] uniqueidentifier NOT NULL,
    [HistoryId] uniqueidentifier NOT NULL,
    [Role] nvarchar(10) NOT NULL,
    [Text] nvarchar(max) NOT NULL,
    [Sequence] int NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_ChatMessages] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ChatMessages_AIReadHistories_HistoryId] FOREIGN KEY ([HistoryId]) REFERENCES [AIReadHistories] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_AIReadHistories_UserId] ON [AIReadHistories] ([UserId]);
GO

CREATE INDEX [IX_ChatMessages_HistoryId_Sequence] ON [ChatMessages] ([HistoryId], [Sequence]);
GO

CREATE INDEX [IX_ReadHistories_UserId] ON [ReadHistories] ([UserId]);
GO

CREATE INDEX [IX_RefreshTokens_DeviceFingerprint] ON [RefreshTokens] ([DeviceFingerprint]);
GO

CREATE INDEX [IX_RefreshTokens_UserId] ON [RefreshTokens] ([UserId]);
GO

CREATE UNIQUE INDEX [IX_Users_Email] ON [Users] ([Email]);
GO

CREATE UNIQUE INDEX [IX_Users_ProviderKey] ON [Users] ([ProviderKey]);
GO

CREATE UNIQUE INDEX [IX_Wallets_UserId] ON [Wallets] ([UserId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260904125701_Initial', N'8.0.11');
GO

COMMIT;
GO

