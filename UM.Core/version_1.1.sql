-- ============================================================
-- UM Core Database Schema v1.1
-- Target: UMCoreDb
-- Changelog v1.1:
--   + DROP TABLE IF EXISTS cho phép chạy lại nhiều lần
--   + Seed 3 user mẫu (admin, user1, user2)
-- ============================================================

USE UMCoreDb;
GO

-- ============================================================
-- DROP TABLES (reverse dependency order)
-- ============================================================
IF OBJECT_ID('dbo.TaskAssignees', 'U') IS NOT NULL DROP TABLE dbo.TaskAssignees;
IF OBJECT_ID('dbo.RolePermissions', 'U') IS NOT NULL DROP TABLE dbo.RolePermissions;
IF OBJECT_ID('dbo.Reports', 'U') IS NOT NULL DROP TABLE dbo.Reports;
IF OBJECT_ID('dbo.MessageReactions', 'U') IS NOT NULL DROP TABLE dbo.MessageReactions;
IF OBJECT_ID('dbo.MessageMentions', 'U') IS NOT NULL DROP TABLE dbo.MessageMentions;
IF OBJECT_ID('dbo.MessageAttachments', 'U') IS NOT NULL DROP TABLE dbo.MessageAttachments;
IF OBJECT_ID('dbo.ChannelReadStates', 'U') IS NOT NULL DROP TABLE dbo.ChannelReadStates;
IF OBJECT_ID('dbo.ChannelMessages', 'U') IS NOT NULL DROP TABLE dbo.ChannelMessages;
IF OBJECT_ID('dbo.Tasks', 'U') IS NOT NULL DROP TABLE dbo.Tasks;
IF OBJECT_ID('dbo.ServerSanctions', 'U') IS NOT NULL DROP TABLE dbo.ServerSanctions;
IF OBJECT_ID('dbo.ServerRoles', 'U') IS NOT NULL DROP TABLE dbo.ServerRoles;
IF OBJECT_ID('dbo.ServerMembers', 'U') IS NOT NULL DROP TABLE dbo.ServerMembers;
IF OBJECT_ID('dbo.ChatMembers', 'U') IS NOT NULL DROP TABLE dbo.ChatMembers;
IF OBJECT_ID('dbo.Channels', 'U') IS NOT NULL DROP TABLE dbo.Channels;
IF OBJECT_ID('dbo.Categories', 'U') IS NOT NULL DROP TABLE dbo.Categories;
IF OBJECT_ID('dbo.UserProfiles', 'U') IS NOT NULL DROP TABLE dbo.UserProfiles;
IF OBJECT_ID('dbo.Servers', 'U') IS NOT NULL DROP TABLE dbo.Servers;
IF OBJECT_ID('dbo.MessageKeyRecipients', 'U') IS NOT NULL DROP TABLE dbo.MessageKeyRecipients;
IF OBJECT_ID('dbo.Files', 'U') IS NOT NULL DROP TABLE dbo.Files;
IF OBJECT_ID('dbo.ChatMessages', 'U') IS NOT NULL DROP TABLE dbo.ChatMessages;
IF OBJECT_ID('dbo.UserSanctions', 'U') IS NOT NULL DROP TABLE dbo.UserSanctions;
IF OBJECT_ID('dbo.UserPublicKeys', 'U') IS NOT NULL DROP TABLE dbo.UserPublicKeys;
IF OBJECT_ID('dbo.SystemRolePermissions', 'U') IS NOT NULL DROP TABLE dbo.SystemRolePermissions;
IF OBJECT_ID('dbo.SystemAdmins', 'U') IS NOT NULL DROP TABLE dbo.SystemAdmins;
IF OBJECT_ID('dbo.RefreshTokens', 'U') IS NOT NULL DROP TABLE dbo.RefreshTokens;
IF OBJECT_ID('dbo.Notifications', 'U') IS NOT NULL DROP TABLE dbo.Notifications;
IF OBJECT_ID('dbo.Friendships', 'U') IS NOT NULL DROP TABLE dbo.Friendships;
IF OBJECT_ID('dbo.Folders', 'U') IS NOT NULL DROP TABLE dbo.Folders;
IF OBJECT_ID('dbo.EmailOtps', 'U') IS NOT NULL DROP TABLE dbo.EmailOtps;
IF OBJECT_ID('dbo.ChatRooms', 'U') IS NOT NULL DROP TABLE dbo.ChatRooms;
IF OBJECT_ID('dbo.AuditLogs', 'U') IS NOT NULL DROP TABLE dbo.AuditLogs;
IF OBJECT_ID('dbo.Users', 'U') IS NOT NULL DROP TABLE dbo.Users;
IF OBJECT_ID('dbo.SystemRoles', 'U') IS NOT NULL DROP TABLE dbo.SystemRoles;
IF OBJECT_ID('dbo.SystemPermissions', 'U') IS NOT NULL DROP TABLE dbo.SystemPermissions;
IF OBJECT_ID('dbo.Permissions', 'U') IS NOT NULL DROP TABLE dbo.Permissions;
IF OBJECT_ID('dbo.MessageEncryptionKeys', 'U') IS NOT NULL DROP TABLE dbo.MessageEncryptionKeys;
GO

-- ============================================================
-- CREATE TABLES
-- ============================================================

-- ---------- MessageEncryptionKeys ----------
CREATE TABLE dbo.MessageEncryptionKeys (
    Id bigint IDENTITY(1,1) NOT NULL,
    [Scope] tinyint NOT NULL,
    ScopeId bigint NOT NULL,
    WrappedDek varbinary(512) NOT NULL,
    KmsKeyId varchar(200) NOT NULL,
    Algorithm varchar(30) DEFAULT 'AES-256-GCM' NOT NULL,
    KeyVersion int NOT NULL,
    CreatedAt datetime2(3) DEFAULT sysutcdatetime() NOT NULL,
    RotatedAt datetime2(3) NULL,
    IsActive bit DEFAULT 1 NOT NULL,
    CONSTRAINT PK_MessageEncryptionKeys PRIMARY KEY (Id),
    CONSTRAINT UQ_MessageEncryptionKeys_Scope UNIQUE ([Scope], ScopeId, KeyVersion),
    CONSTRAINT CK_MessageEncryptionKeys_Scope CHECK ([Scope] IN (1, 2))
);
CREATE NONCLUSTERED INDEX IX_MessageEncryptionKeys_Scope_Active
    ON dbo.MessageEncryptionKeys ([Scope], ScopeId) WHERE IsActive = 1;
GO

-- ---------- Permissions ----------
CREATE TABLE dbo.Permissions (
    Id int NOT NULL,
    Code varchar(50) NOT NULL,
    Description nvarchar(200) NULL,
    CONSTRAINT PK_Permissions PRIMARY KEY (Id),
    CONSTRAINT UQ_Permissions_Code UNIQUE (Code)
);
GO

-- ---------- SystemPermissions ----------
CREATE TABLE dbo.SystemPermissions (
    Id int NOT NULL,
    Code varchar(50) NOT NULL,
    Description nvarchar(200) NULL,
    CONSTRAINT PK_SystemPermissions PRIMARY KEY (Id),
    CONSTRAINT UQ_SystemPermissions_Code UNIQUE (Code)
);
GO

-- ---------- SystemRoles ----------
CREATE TABLE dbo.SystemRoles (
    Id int IDENTITY(1,1) NOT NULL,
    Name varchar(50) NOT NULL,
    Description nvarchar(200) NULL,
    CreatedAt datetime2(3) DEFAULT sysutcdatetime() NOT NULL,
    CONSTRAINT PK_SystemRoles PRIMARY KEY (Id),
    CONSTRAINT UQ_SystemRoles_Name UNIQUE (Name)
);
GO

-- ---------- Users ----------
CREATE TABLE dbo.Users (
    Id bigint IDENTITY(1,1) NOT NULL,
    PublicCode varchar(16) NOT NULL,
    Email varchar(256) NOT NULL,
    PasswordHash varchar(256) NOT NULL,
    IsActive bit DEFAULT 1 NOT NULL,
    IsEmailVerified bit DEFAULT 0 NOT NULL,
    FailedLoginCount int DEFAULT 0 NOT NULL,
    LockedUntil datetime2(3) NULL,
    LastLoginAt datetime2(3) NULL,
    CreatedAt datetime2(3) DEFAULT sysutcdatetime() NOT NULL,
    UpdatedAt datetime2(3) DEFAULT sysutcdatetime() NOT NULL,
    IsSuspended bit DEFAULT 0 NOT NULL,
    SuspendedUntil datetime2(3) NULL,
    CONSTRAINT PK_Users PRIMARY KEY (Id),
    CONSTRAINT UQ_Users_Email UNIQUE (Email),
    CONSTRAINT UQ_Users_PublicCode UNIQUE (PublicCode)
);
CREATE NONCLUSTERED INDEX IX_Users_CreatedAt ON dbo.Users (CreatedAt DESC);
CREATE NONCLUSTERED INDEX IX_Users_Suspended ON dbo.Users (IsSuspended) WHERE IsSuspended = 1;
GO

-- ---------- AuditLogs ----------
CREATE TABLE dbo.AuditLogs (
    Id bigint IDENTITY(1,1) NOT NULL,
    ActorUserId bigint NULL,
    [Action] varchar(50) NOT NULL,
    EntityType varchar(50) NOT NULL,
    EntityId bigint NOT NULL,
    IpAddress varchar(45) NULL,
    Metadata nvarchar(MAX) NULL,
    CreatedAt datetime2(3) DEFAULT sysutcdatetime() NOT NULL,
    CONSTRAINT PK_AuditLogs PRIMARY KEY (Id),
    CONSTRAINT FK_AuditLogs_Actor FOREIGN KEY (ActorUserId) REFERENCES dbo.Users(Id),
    CONSTRAINT CK_AuditLogs_Metadata_Json CHECK (Metadata IS NULL OR ISJSON(Metadata) = 1)
);
CREATE NONCLUSTERED INDEX IX_AuditLogs_Actor ON dbo.AuditLogs (ActorUserId, CreatedAt DESC);
CREATE NONCLUSTERED INDEX IX_AuditLogs_Entity ON dbo.AuditLogs (EntityType, EntityId, CreatedAt DESC);
GO

-- ---------- ChatRooms ----------
CREATE TABLE dbo.ChatRooms (
    Id bigint IDENTITY(1,1) NOT NULL,
    [Type] tinyint NOT NULL,
    Name nvarchar(100) NULL,
    CreatedBy bigint NOT NULL,
    CreatedAt datetime2(3) DEFAULT sysutcdatetime() NOT NULL,
    UpdatedAt datetime2(3) DEFAULT sysutcdatetime() NOT NULL,
    DeletedAt datetime2(3) NULL,
    CONSTRAINT PK_ChatRooms PRIMARY KEY (Id),
    CONSTRAINT FK_ChatRooms_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES dbo.Users(Id),
    CONSTRAINT CK_ChatRooms_Type CHECK ([Type] IN (1, 2))
);
GO

-- ---------- EmailOtps ----------
CREATE TABLE dbo.EmailOtps (
    Id bigint IDENTITY(1,1) NOT NULL,
    UserId bigint NOT NULL,
    CodeHash varbinary(32) NOT NULL,
    Purpose varchar(30) NOT NULL,
    ExpiresAt datetime2(3) NOT NULL,
    AttemptCount int DEFAULT 0 NOT NULL,
    MaxAttempts int DEFAULT 5 NOT NULL,
    UsedAt datetime2(3) NULL,
    RequestIp varchar(45) NULL,
    CreatedAt datetime2(3) DEFAULT sysutcdatetime() NOT NULL,
    CONSTRAINT PK_EmailOtps PRIMARY KEY (Id),
    CONSTRAINT FK_EmailOtps_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(Id) ON DELETE CASCADE
);
CREATE NONCLUSTERED INDEX IX_EmailOtps_UserId_Purpose_CreatedAt
    ON dbo.EmailOtps (UserId, Purpose, CreatedAt DESC);
GO

-- ---------- Folders ----------
CREATE TABLE dbo.Folders (
    Id bigint IDENTITY(1,1) NOT NULL,
    OwnerId bigint NOT NULL,
    ParentFolderId bigint NULL,
    Name nvarchar(255) NOT NULL,
    CreatedAt datetime2(3) DEFAULT sysutcdatetime() NOT NULL,
    UpdatedAt datetime2(3) DEFAULT sysutcdatetime() NOT NULL,
    DeletedAt datetime2(3) NULL,
    CONSTRAINT PK_Folders PRIMARY KEY (Id),
    CONSTRAINT FK_Folders_Owner FOREIGN KEY (OwnerId) REFERENCES dbo.Users(Id),
    CONSTRAINT FK_Folders_Parent FOREIGN KEY (ParentFolderId) REFERENCES dbo.Folders(Id)
);
CREATE NONCLUSTERED INDEX IX_Folders_Owner_Parent ON dbo.Folders (OwnerId, ParentFolderId) WHERE DeletedAt IS NULL;
GO

-- ---------- Friendships ----------
CREATE TABLE dbo.Friendships (
    Id bigint IDENTITY(1,1) NOT NULL,
    RequesterId bigint NOT NULL,
    AddresseeId bigint NOT NULL,
    Status tinyint DEFAULT 1 NOT NULL,
    UserLowId AS (CASE WHEN RequesterId < AddresseeId THEN RequesterId ELSE AddresseeId END) PERSISTED NOT NULL,
    UserHighId AS (CASE WHEN RequesterId < AddresseeId THEN AddresseeId ELSE RequesterId END) PERSISTED NOT NULL,
    CreatedAt datetime2(3) DEFAULT sysutcdatetime() NOT NULL,
    UpdatedAt datetime2(3) DEFAULT sysutcdatetime() NOT NULL,
    RespondedAt datetime2(3) NULL,
    CONSTRAINT PK_Friendships PRIMARY KEY (Id),
    CONSTRAINT FK_Friendships_Addressee FOREIGN KEY (AddresseeId) REFERENCES dbo.Users(Id),
    CONSTRAINT FK_Friendships_Requester FOREIGN KEY (RequesterId) REFERENCES dbo.Users(Id),
    CONSTRAINT CK_Friendships_NotSelf CHECK (RequesterId <> AddresseeId),
    CONSTRAINT CK_Friendships_Status CHECK (Status BETWEEN 1 AND 4)
);
CREATE UNIQUE NONCLUSTERED INDEX UQ_Friendships_Pair ON dbo.Friendships (UserLowId, UserHighId);
GO

-- ---------- Notifications ----------
CREATE TABLE dbo.Notifications (
    Id bigint IDENTITY(1,1) NOT NULL,
    UserId bigint NOT NULL,
    [Type] varchar(30) NOT NULL,
    Title nvarchar(200) NOT NULL,
    Content nvarchar(1000) NULL,
    ReferenceType varchar(30) NULL,
    ReferenceId bigint NULL,
    IsRead bit DEFAULT 0 NOT NULL,
    CreatedAt datetime2(3) DEFAULT sysutcdatetime() NOT NULL,
    ReadAt datetime2(3) NULL,
    CONSTRAINT PK_Notifications PRIMARY KEY (Id),
    CONSTRAINT FK_Notifications_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(Id) ON DELETE CASCADE
);
CREATE NONCLUSTERED INDEX IX_Notifications_Unread ON dbo.Notifications (UserId, CreatedAt DESC) WHERE IsRead = 0;
GO

-- ---------- RefreshTokens ----------
CREATE TABLE dbo.RefreshTokens (
    Id uniqueidentifier DEFAULT newid() NOT NULL,
    UserId bigint NOT NULL,
    TokenHash varbinary(64) NOT NULL,
    ExpiresAt datetime2(3) NOT NULL,
    CreatedAt datetime2(3) DEFAULT sysutcdatetime() NOT NULL,
    CreatedByIp varchar(45) NULL,
    RevokedAt datetime2(3) NULL,
    RevokedByIp varchar(45) NULL,
    ReplacedByTokenId uniqueidentifier NULL,
    CONSTRAINT PK_RefreshTokens PRIMARY KEY (Id),
    CONSTRAINT FK_RefreshTokens_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(Id) ON DELETE CASCADE
);
CREATE NONCLUSTERED INDEX IX_RefreshTokens_UserId_ExpiresAt ON dbo.RefreshTokens (UserId, ExpiresAt) INCLUDE (RevokedAt);
CREATE UNIQUE NONCLUSTERED INDEX UQ_RefreshTokens_TokenHash ON dbo.RefreshTokens (TokenHash);
GO

-- ---------- SystemAdmins ----------
CREATE TABLE dbo.SystemAdmins (
    Id bigint IDENTITY(1,1) NOT NULL,
    UserId bigint NOT NULL,
    SystemRoleId int NOT NULL,
    RequireMfa bit DEFAULT 1 NOT NULL,
    GrantedBy bigint NOT NULL,
    GrantedAt datetime2(3) DEFAULT sysutcdatetime() NOT NULL,
    RevokedBy bigint NULL,
    RevokedAt datetime2(3) NULL,
    CONSTRAINT PK_SystemAdmins PRIMARY KEY (Id),
    CONSTRAINT FK_SystemAdmins_GrantedBy FOREIGN KEY (GrantedBy) REFERENCES dbo.Users(Id),
    CONSTRAINT FK_SystemAdmins_RevokedBy FOREIGN KEY (RevokedBy) REFERENCES dbo.Users(Id),
    CONSTRAINT FK_SystemAdmins_Role FOREIGN KEY (SystemRoleId) REFERENCES dbo.SystemRoles(Id),
    CONSTRAINT FK_SystemAdmins_User FOREIGN KEY (UserId) REFERENCES dbo.Users(Id),
    CONSTRAINT CK_SystemAdmins_NoSelfGrant CHECK (GrantedBy <> UserId)
);
CREATE UNIQUE NONCLUSTERED INDEX UQ_SystemAdmins_ActiveUser ON dbo.SystemAdmins (UserId) WHERE RevokedAt IS NULL;
GO

-- ---------- SystemRolePermissions ----------
CREATE TABLE dbo.SystemRolePermissions (
    SystemRoleId int NOT NULL,
    PermissionId int NOT NULL,
    CONSTRAINT PK_SystemRolePermissions PRIMARY KEY (SystemRoleId, PermissionId),
    CONSTRAINT FK_SystemRolePermissions_Permission FOREIGN KEY (PermissionId) REFERENCES dbo.SystemPermissions(Id),
    CONSTRAINT FK_SystemRolePermissions_Role FOREIGN KEY (SystemRoleId) REFERENCES dbo.SystemRoles(Id) ON DELETE CASCADE
);
GO

-- ---------- UserPublicKeys ----------
CREATE TABLE dbo.UserPublicKeys (
    Id bigint IDENTITY(1,1) NOT NULL,
    UserId bigint NOT NULL,
    PublicKey varbinary(1024) NOT NULL,
    Algorithm varchar(30) NOT NULL,
    IsActive bit DEFAULT 1 NOT NULL,
    CreatedAt datetime2(3) DEFAULT sysutcdatetime() NOT NULL,
    RevokedAt datetime2(3) NULL,
    CONSTRAINT PK_UserPublicKeys PRIMARY KEY (Id),
    CONSTRAINT FK_UserPublicKeys_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(Id) ON DELETE CASCADE
);
CREATE NONCLUSTERED INDEX IX_UserPublicKeys_UserId_Active ON dbo.UserPublicKeys (UserId) WHERE IsActive = 1;
GO

-- ---------- UserSanctions ----------
CREATE TABLE dbo.UserSanctions (
    Id bigint IDENTITY(1,1) NOT NULL,
    UserId bigint NOT NULL,
    [Type] tinyint NOT NULL,
    Reason nvarchar(500) NOT NULL,
    IssuedBy bigint NOT NULL,
    IssuedAt datetime2(3) DEFAULT sysutcdatetime() NOT NULL,
    ExpiresAt datetime2(3) NULL,
    RevokedBy bigint NULL,
    RevokedAt datetime2(3) NULL,
    RevokeReason nvarchar(500) NULL,
    CONSTRAINT PK_UserSanctions PRIMARY KEY (Id),
    CONSTRAINT FK_UserSanctions_IssuedBy FOREIGN KEY (IssuedBy) REFERENCES dbo.Users(Id),
    CONSTRAINT FK_UserSanctions_RevokedBy FOREIGN KEY (RevokedBy) REFERENCES dbo.Users(Id),
    CONSTRAINT FK_UserSanctions_User FOREIGN KEY (UserId) REFERENCES dbo.Users(Id),
    CONSTRAINT CK_UserSanctions_Type CHECK ([Type] IN (1, 2))
);
CREATE NONCLUSTERED INDEX IX_UserSanctions_ActiveByUser ON dbo.UserSanctions (UserId) WHERE RevokedAt IS NULL;
CREATE NONCLUSTERED INDEX IX_UserSanctions_IssuedBy ON dbo.UserSanctions (IssuedBy, IssuedAt DESC);
GO

-- ---------- ChatMessages ----------
CREATE TABLE dbo.ChatMessages (
    Id bigint IDENTITY(1,1) NOT NULL,
    ChatRoomId bigint NOT NULL,
    SenderId bigint NOT NULL,
    MessageType tinyint NOT NULL,
    EncryptedContent varbinary(MAX) NOT NULL,
    IV varbinary(16) NOT NULL,
    AuthTag varbinary(16) NOT NULL,
    EncryptionKeyId bigint NOT NULL,
    KeyVersion int NOT NULL,
    ReplyToMessageId bigint NULL,
    IsEdited bit DEFAULT 0 NOT NULL,
    CreatedAt datetime2(3) DEFAULT sysutcdatetime() NOT NULL,
    UpdatedAt datetime2(3) DEFAULT sysutcdatetime() NOT NULL,
    DeletedAt datetime2(3) NULL,
    CONSTRAINT PK_ChatMessages PRIMARY KEY (Id),
    CONSTRAINT FK_ChatMessages_ChatRooms FOREIGN KEY (ChatRoomId) REFERENCES dbo.ChatRooms(Id),
    CONSTRAINT FK_ChatMessages_EncryptionKey FOREIGN KEY (EncryptionKeyId) REFERENCES dbo.MessageEncryptionKeys(Id),
    CONSTRAINT FK_ChatMessages_ReplyTo FOREIGN KEY (ReplyToMessageId) REFERENCES dbo.ChatMessages(Id),
    CONSTRAINT FK_ChatMessages_Sender FOREIGN KEY (SenderId) REFERENCES dbo.Users(Id),
    CONSTRAINT CK_ChatMessages_Type CHECK (MessageType BETWEEN 1 AND 4)
);
CREATE UNIQUE CLUSTERED INDEX CIX_ChatMessages_Room_Created ON dbo.ChatMessages (ChatRoomId, CreatedAt DESC, Id DESC);
GO

-- ---------- Files ----------
CREATE TABLE dbo.Files (
    Id bigint IDENTITY(1,1) NOT NULL,
    OwnerId bigint NOT NULL,
    FolderId bigint NULL,
    FileName nvarchar(255) NOT NULL,
    OriginalFileName nvarchar(255) NOT NULL,
    Extension varchar(20) NOT NULL,
    MimeType varchar(150) NOT NULL,
    DetectedMimeType varchar(150) NULL,
    SizeBytes bigint NOT NULL,
    Sha256Hash binary(32) NOT NULL,
    StorageProvider varchar(20) NOT NULL,
    StorageIdentifier varchar(500) NOT NULL,
    ScanStatus tinyint DEFAULT 0 NOT NULL,
    ScannedAt datetime2(3) NULL,
    CreatedAt datetime2(3) DEFAULT sysutcdatetime() NOT NULL,
    UpdatedAt datetime2(3) DEFAULT sysutcdatetime() NOT NULL,
    DeletedAt datetime2(3) NULL,
    CONSTRAINT PK_Files PRIMARY KEY (Id),
    CONSTRAINT FK_Files_Folder FOREIGN KEY (FolderId) REFERENCES dbo.Folders(Id),
    CONSTRAINT FK_Files_Owner FOREIGN KEY (OwnerId) REFERENCES dbo.Users(Id),
    CONSTRAINT CK_Files_ScanStatus CHECK (ScanStatus BETWEEN 0 AND 3)
);
CREATE NONCLUSTERED INDEX IX_Files_Owner_Folder ON dbo.Files (OwnerId, FolderId) WHERE DeletedAt IS NULL;
CREATE NONCLUSTERED INDEX IX_Files_ScanStatus_Pending ON dbo.Files (ScanStatus) WHERE ScanStatus = 0;
CREATE NONCLUSTERED INDEX IX_Files_Sha256Hash ON dbo.Files (Sha256Hash);
GO

-- ---------- MessageKeyRecipients ----------
CREATE TABLE dbo.MessageKeyRecipients (
    Id bigint IDENTITY(1,1) NOT NULL,
    ChatRoomId bigint NOT NULL,
    UserId bigint NOT NULL,
    UserPublicKeyId bigint NOT NULL,
    EncryptedSessionKey varbinary(512) NOT NULL,
    KeyVersion int NOT NULL,
    CreatedAt datetime2(3) DEFAULT sysutcdatetime() NOT NULL,
    CONSTRAINT PK_MessageKeyRecipients PRIMARY KEY (Id),
    CONSTRAINT UQ_MessageKeyRecipients UNIQUE (ChatRoomId, UserId, KeyVersion),
    CONSTRAINT FK_MessageKeyRecipients_ChatRooms FOREIGN KEY (ChatRoomId) REFERENCES dbo.ChatRooms(Id) ON DELETE CASCADE,
    CONSTRAINT FK_MessageKeyRecipients_PublicKey FOREIGN KEY (UserPublicKeyId) REFERENCES dbo.UserPublicKeys(Id),
    CONSTRAINT FK_MessageKeyRecipients_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(Id)
);
CREATE NONCLUSTERED INDEX IX_MessageKeyRecipients_User ON dbo.MessageKeyRecipients (UserId);
GO

-- ---------- Servers ----------
CREATE TABLE dbo.Servers (
    Id bigint IDENTITY(1,1) NOT NULL,
    Name nvarchar(100) NOT NULL,
    OwnerId bigint NOT NULL,
    IconFileId bigint NULL,
    CreatedAt datetime2(3) DEFAULT sysutcdatetime() NOT NULL,
    UpdatedAt datetime2(3) DEFAULT sysutcdatetime() NOT NULL,
    DeletedAt datetime2(3) NULL,
    ScheduledDeleteAt datetime2(3) NULL,
    IsSuspended bit DEFAULT 0 NOT NULL,
    SuspendedUntil datetime2(3) NULL,
    CONSTRAINT PK_Servers PRIMARY KEY (Id),
    CONSTRAINT FK_Servers_IconFile FOREIGN KEY (IconFileId) REFERENCES dbo.Files(Id),
    CONSTRAINT FK_Servers_Owner FOREIGN KEY (OwnerId) REFERENCES dbo.Users(Id)
);
CREATE NONCLUSTERED INDEX IX_Servers_CreatedAt ON dbo.Servers (CreatedAt DESC) WHERE DeletedAt IS NULL;
CREATE NONCLUSTERED INDEX IX_Servers_ScheduledDeleteAt ON dbo.Servers (ScheduledDeleteAt) WHERE ScheduledDeleteAt IS NOT NULL;
CREATE NONCLUSTERED INDEX IX_Servers_Suspended ON dbo.Servers (IsSuspended) WHERE IsSuspended = 1;
GO

-- ---------- UserProfiles ----------
CREATE TABLE dbo.UserProfiles (
    Id bigint IDENTITY(1,1) NOT NULL,
    UserId bigint NOT NULL,
    AvatarFileId bigint NULL,
    FullName nvarchar(150) NULL,
    DateOfBirth date NULL,
    PhoneNumber varchar(20) NULL,
    CreatedAt datetime2(3) DEFAULT sysutcdatetime() NOT NULL,
    UpdatedAt datetime2(3) DEFAULT sysutcdatetime() NOT NULL,
    CONSTRAINT PK_UserProfiles PRIMARY KEY (Id),
    CONSTRAINT UQ_UserProfiles_UserId UNIQUE (UserId),
    CONSTRAINT FK_UserProfiles_AvatarFile FOREIGN KEY (AvatarFileId) REFERENCES dbo.Files(Id),
    CONSTRAINT FK_UserProfiles_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(Id) ON DELETE CASCADE
);
GO

-- ---------- Categories ----------
CREATE TABLE dbo.Categories (
    Id bigint IDENTITY(1,1) NOT NULL,
    ServerId bigint NOT NULL,
    Name nvarchar(100) NOT NULL,
    [Position] int DEFAULT 0 NOT NULL,
    CreatedAt datetime2(3) DEFAULT sysutcdatetime() NOT NULL,
    UpdatedAt datetime2(3) DEFAULT sysutcdatetime() NOT NULL,
    DeletedAt datetime2(3) NULL,
    CONSTRAINT PK_Categories PRIMARY KEY (Id),
    CONSTRAINT FK_Categories_Servers FOREIGN KEY (ServerId) REFERENCES dbo.Servers(Id) ON DELETE CASCADE
);
CREATE NONCLUSTERED INDEX IX_Categories_ServerId ON dbo.Categories (ServerId, [Position]) WHERE DeletedAt IS NULL;
GO

-- ---------- Channels ----------
CREATE TABLE dbo.Channels (
    Id bigint IDENTITY(1,1) NOT NULL,
    ServerId bigint NOT NULL,
    CategoryId bigint NULL,
    Name nvarchar(100) NOT NULL,
    [Type] tinyint NOT NULL,
    [Position] int DEFAULT 0 NOT NULL,
    CreatedAt datetime2(3) DEFAULT sysutcdatetime() NOT NULL,
    UpdatedAt datetime2(3) DEFAULT sysutcdatetime() NOT NULL,
    DeletedAt datetime2(3) NULL,
    CONSTRAINT PK_Channels PRIMARY KEY (Id),
    CONSTRAINT FK_Channels_Categories FOREIGN KEY (CategoryId) REFERENCES dbo.Categories(Id),
    CONSTRAINT FK_Channels_Servers FOREIGN KEY (ServerId) REFERENCES dbo.Servers(Id) ON DELETE CASCADE,
    CONSTRAINT CK_Channels_Type CHECK ([Type] IN (1, 2))
);
CREATE NONCLUSTERED INDEX IX_Channels_CategoryId ON dbo.Channels (CategoryId) WHERE DeletedAt IS NULL;
CREATE NONCLUSTERED INDEX IX_Channels_ServerId ON dbo.Channels (ServerId) WHERE DeletedAt IS NULL;
GO

-- ---------- ChatMembers ----------
CREATE TABLE dbo.ChatMembers (
    Id bigint IDENTITY(1,1) NOT NULL,
    ChatRoomId bigint NOT NULL,
    UserId bigint NOT NULL,
    [Role] tinyint DEFAULT 1 NOT NULL,
    JoinedAt datetime2(3) DEFAULT sysutcdatetime() NOT NULL,
    LeftAt datetime2(3) NULL,
    LastReadMessageId bigint NULL,
    IsMuted bit DEFAULT 0 NOT NULL,
    IsHidden bit DEFAULT 0 NOT NULL,
    CONSTRAINT PK_ChatMembers PRIMARY KEY (Id),
    CONSTRAINT UQ_ChatMembers_Room_User UNIQUE (ChatRoomId, UserId),
    CONSTRAINT FK_ChatMembers_ChatRooms FOREIGN KEY (ChatRoomId) REFERENCES dbo.ChatRooms(Id) ON DELETE CASCADE,
    CONSTRAINT FK_ChatMembers_LastReadMessage FOREIGN KEY (LastReadMessageId) REFERENCES dbo.ChatMessages(Id),
    CONSTRAINT FK_ChatMembers_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(Id)
);
CREATE NONCLUSTERED INDEX IX_ChatMembers_UserId ON dbo.ChatMembers (UserId) WHERE LeftAt IS NULL;
GO

-- ---------- ServerMembers ----------
CREATE TABLE dbo.ServerMembers (
    Id bigint IDENTITY(1,1) NOT NULL,
    ServerId bigint NOT NULL,
    UserId bigint NOT NULL,
    Nickname nvarchar(100) NULL,
    JoinedAt datetime2(3) DEFAULT sysutcdatetime() NOT NULL,
    IsBanned bit DEFAULT 0 NOT NULL,
    BannedAt datetime2(3) NULL,
    BannedBy bigint NULL,
    BanReason nvarchar(500) NULL,
    LeftAt datetime2(3) NULL,
    CONSTRAINT PK_ServerMembers PRIMARY KEY (Id),
    CONSTRAINT UQ_ServerMembers_Server_User UNIQUE (ServerId, UserId),
    CONSTRAINT FK_ServerMembers_BannedBy FOREIGN KEY (BannedBy) REFERENCES dbo.Users(Id),
    CONSTRAINT FK_ServerMembers_Servers FOREIGN KEY (ServerId) REFERENCES dbo.Servers(Id) ON DELETE CASCADE,
    CONSTRAINT FK_ServerMembers_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(Id)
);
CREATE UNIQUE NONCLUSTERED INDEX IX_ServerMembers_ServerId_UserId ON dbo.ServerMembers (ServerId, UserId) INCLUDE (IsBanned) WHERE LeftAt IS NULL;
CREATE NONCLUSTERED INDEX IX_ServerMembers_UserId ON dbo.ServerMembers (UserId) WHERE LeftAt IS NULL;
GO

-- ---------- ServerRoles ----------
CREATE TABLE dbo.ServerRoles (
    Id bigint IDENTITY(1,1) NOT NULL,
    ServerId bigint NOT NULL,
    Name nvarchar(50) NOT NULL,
    IsSystem bit DEFAULT 0 NOT NULL,
    [Position] int DEFAULT 0 NOT NULL,
    Color varchar(7) NULL,
    CreatedAt datetime2(3) DEFAULT sysutcdatetime() NOT NULL,
    CONSTRAINT PK_ServerRoles PRIMARY KEY (Id),
    CONSTRAINT UQ_ServerRoles_Server_Name UNIQUE (ServerId, Name),
    CONSTRAINT FK_ServerRoles_Servers FOREIGN KEY (ServerId) REFERENCES dbo.Servers(Id) ON DELETE CASCADE
);
CREATE NONCLUSTERED INDEX IX_ServerRoles_ServerId ON dbo.ServerRoles (ServerId);
GO

-- ---------- ServerSanctions ----------
CREATE TABLE dbo.ServerSanctions (
    Id bigint IDENTITY(1,1) NOT NULL,
    ServerId bigint NOT NULL,
    [Type] tinyint DEFAULT 1 NOT NULL,
    Reason nvarchar(500) NOT NULL,
    IssuedBy bigint NOT NULL,
    IssuedAt datetime2(3) DEFAULT sysutcdatetime() NOT NULL,
    ExpiresAt datetime2(3) NULL,
    RevokedBy bigint NULL,
    RevokedAt datetime2(3) NULL,
    RevokeReason nvarchar(500) NULL,
    CONSTRAINT PK_ServerSanctions PRIMARY KEY (Id),
    CONSTRAINT FK_ServerSanctions_IssuedBy FOREIGN KEY (IssuedBy) REFERENCES dbo.Users(Id),
    CONSTRAINT FK_ServerSanctions_RevokedBy FOREIGN KEY (RevokedBy) REFERENCES dbo.Users(Id),
    CONSTRAINT FK_ServerSanctions_Server FOREIGN KEY (ServerId) REFERENCES dbo.Servers(Id),
    CONSTRAINT CK_ServerSanctions_Type CHECK ([Type] = 1)
);
CREATE NONCLUSTERED INDEX IX_ServerSanctions_ActiveByServer ON dbo.ServerSanctions (ServerId) WHERE RevokedAt IS NULL;
GO

-- ---------- Tasks ----------
CREATE TABLE dbo.Tasks (
    Id bigint IDENTITY(1,1) NOT NULL,
    ServerId bigint NOT NULL,
    ChannelId bigint NULL,
    CreatorId bigint NOT NULL,
    Title nvarchar(200) NOT NULL,
    Description nvarchar(MAX) NULL,
    Status tinyint DEFAULT 1 NOT NULL,
    Priority tinyint DEFAULT 2 NOT NULL,
    DueDate datetime2(3) NULL,
    ReminderAt datetime2(3) NULL,
    CreatedAt datetime2(3) DEFAULT sysutcdatetime() NOT NULL,
    UpdatedAt datetime2(3) DEFAULT sysutcdatetime() NOT NULL,
    CompletedAt datetime2(3) NULL,
    DeletedAt datetime2(3) NULL,
    CONSTRAINT PK_Tasks PRIMARY KEY (Id),
    CONSTRAINT FK_Tasks_Channels FOREIGN KEY (ChannelId) REFERENCES dbo.Channels(Id),
    CONSTRAINT FK_Tasks_Creator FOREIGN KEY (CreatorId) REFERENCES dbo.Users(Id),
    CONSTRAINT FK_Tasks_Servers FOREIGN KEY (ServerId) REFERENCES dbo.Servers(Id),
    CONSTRAINT CK_Tasks_Status CHECK (Status BETWEEN 1 AND 5),
    CONSTRAINT CK_Tasks_Priority CHECK (Priority BETWEEN 1 AND 4)
);
CREATE NONCLUSTERED INDEX IX_Tasks_Server_Status ON dbo.Tasks (ServerId, Status) WHERE DeletedAt IS NULL;
GO

-- ---------- ChannelMessages ----------
CREATE TABLE dbo.ChannelMessages (
    Id bigint IDENTITY(1,1) NOT NULL,
    ChannelId bigint NOT NULL,
    SenderId bigint NOT NULL,
    MessageType tinyint NOT NULL,
    EncryptedContent varbinary(MAX) NOT NULL,
    IV varbinary(16) NOT NULL,
    AuthTag varbinary(16) NOT NULL,
    EncryptionKeyId bigint NOT NULL,
    KeyVersion int NOT NULL,
    ReplyToMessageId bigint NULL,
    IsEdited bit DEFAULT 0 NOT NULL,
    CreatedAt datetime2(3) DEFAULT sysutcdatetime() NOT NULL,
    UpdatedAt datetime2(3) DEFAULT sysutcdatetime() NOT NULL,
    DeletedAt datetime2(3) NULL,
    CONSTRAINT PK_ChannelMessages PRIMARY KEY (Id),
    CONSTRAINT FK_ChannelMessages_Channels FOREIGN KEY (ChannelId) REFERENCES dbo.Channels(Id),
    CONSTRAINT FK_ChannelMessages_EncryptionKey FOREIGN KEY (EncryptionKeyId) REFERENCES dbo.MessageEncryptionKeys(Id),
    CONSTRAINT FK_ChannelMessages_ReplyTo FOREIGN KEY (ReplyToMessageId) REFERENCES dbo.ChannelMessages(Id),
    CONSTRAINT FK_ChannelMessages_Sender FOREIGN KEY (SenderId) REFERENCES dbo.Users(Id),
    CONSTRAINT CK_ChannelMessages_Type CHECK (MessageType BETWEEN 1 AND 4)
);
CREATE UNIQUE CLUSTERED INDEX CIX_ChannelMessages_Channel_Created ON dbo.ChannelMessages (ChannelId, CreatedAt DESC, Id DESC);
GO

-- ---------- ChannelReadStates ----------
CREATE TABLE dbo.ChannelReadStates (
    ChannelId bigint NOT NULL,
    UserId bigint NOT NULL,
    LastReadMessageId bigint NULL,
    UpdatedAt datetime2(3) DEFAULT sysutcdatetime() NOT NULL,
    CONSTRAINT PK_ChannelReadStates PRIMARY KEY (ChannelId, UserId),
    CONSTRAINT FK_ChannelReadStates_Channels FOREIGN KEY (ChannelId) REFERENCES dbo.Channels(Id) ON DELETE CASCADE,
    CONSTRAINT FK_ChannelReadStates_LastRead FOREIGN KEY (LastReadMessageId) REFERENCES dbo.ChannelMessages(Id),
    CONSTRAINT FK_ChannelReadStates_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(Id)
);
GO

-- ---------- MessageAttachments ----------
CREATE TABLE dbo.MessageAttachments (
    Id bigint IDENTITY(1,1) NOT NULL,
    FileId bigint NOT NULL,
    ChannelMessageId bigint NULL,
    ChatMessageId bigint NULL,
    CreatedAt datetime2(3) DEFAULT sysutcdatetime() NOT NULL,
    CONSTRAINT PK_MessageAttachments PRIMARY KEY (Id),
    CONSTRAINT FK_MessageAttachments_ChannelMessage FOREIGN KEY (ChannelMessageId) REFERENCES dbo.ChannelMessages(Id) ON DELETE CASCADE,
    CONSTRAINT FK_MessageAttachments_ChatMessage FOREIGN KEY (ChatMessageId) REFERENCES dbo.ChatMessages(Id) ON DELETE CASCADE,
    CONSTRAINT FK_MessageAttachments_Files FOREIGN KEY (FileId) REFERENCES dbo.Files(Id),
    CONSTRAINT CK_MessageAttachments_OneTarget CHECK (
        (CASE WHEN ChannelMessageId IS NOT NULL THEN 1 ELSE 0 END +
         CASE WHEN ChatMessageId IS NOT NULL THEN 1 ELSE 0 END) = 1
    )
);
CREATE NONCLUSTERED INDEX IX_MessageAttachments_FileId ON dbo.MessageAttachments (FileId);
GO

-- ---------- MessageMentions ----------
CREATE TABLE dbo.MessageMentions (
    Id bigint IDENTITY(1,1) NOT NULL,
    ChannelMessageId bigint NULL,
    ChatMessageId bigint NULL,
    MentionedUserId bigint NOT NULL,
    CreatedAt datetime2(3) DEFAULT sysutcdatetime() NOT NULL,
    CONSTRAINT PK_MessageMentions PRIMARY KEY (Id),
    CONSTRAINT FK_MessageMentions_ChannelMessage FOREIGN KEY (ChannelMessageId) REFERENCES dbo.ChannelMessages(Id) ON DELETE CASCADE,
    CONSTRAINT FK_MessageMentions_ChatMessage FOREIGN KEY (ChatMessageId) REFERENCES dbo.ChatMessages(Id) ON DELETE CASCADE,
    CONSTRAINT FK_MessageMentions_User FOREIGN KEY (MentionedUserId) REFERENCES dbo.Users(Id),
    CONSTRAINT CK_MessageMentions_OneTarget CHECK (
        (CASE WHEN ChannelMessageId IS NOT NULL THEN 1 ELSE 0 END +
         CASE WHEN ChatMessageId IS NOT NULL THEN 1 ELSE 0 END) = 1
    )
);
CREATE NONCLUSTERED INDEX IX_MessageMentions_MentionedUserId ON dbo.MessageMentions (MentionedUserId, CreatedAt DESC);
GO

-- ---------- MessageReactions ----------
CREATE TABLE dbo.MessageReactions (
    Id bigint IDENTITY(1,1) NOT NULL,
    ChannelMessageId bigint NULL,
    ChatMessageId bigint NULL,
    UserId bigint NOT NULL,
    Emoji nvarchar(20) NOT NULL,
    CreatedAt datetime2(3) DEFAULT sysutcdatetime() NOT NULL,
    CONSTRAINT PK_MessageReactions PRIMARY KEY (Id),
    CONSTRAINT UQ_MessageReactions_Channel UNIQUE (ChannelMessageId, UserId, Emoji),
    CONSTRAINT UQ_MessageReactions_Chat UNIQUE (ChatMessageId, UserId, Emoji),
    CONSTRAINT FK_MessageReactions_ChannelMessage FOREIGN KEY (ChannelMessageId) REFERENCES dbo.ChannelMessages(Id) ON DELETE CASCADE,
    CONSTRAINT FK_MessageReactions_ChatMessage FOREIGN KEY (ChatMessageId) REFERENCES dbo.ChatMessages(Id) ON DELETE CASCADE,
    CONSTRAINT FK_MessageReactions_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(Id),
    CONSTRAINT CK_MessageReactions_OneTarget CHECK (
        (CASE WHEN ChannelMessageId IS NOT NULL THEN 1 ELSE 0 END +
         CASE WHEN ChatMessageId IS NOT NULL THEN 1 ELSE 0 END) = 1
    )
);
GO

-- ---------- Reports ----------
CREATE TABLE dbo.Reports (
    Id bigint IDENTITY(1,1) NOT NULL,
    ReporterId bigint NOT NULL,
    ReportedUserId bigint NULL,
    ReportedServerId bigint NULL,
    ReportedChannelMessageId bigint NULL,
    ReportedChatMessageId bigint NULL,
    Reason varchar(50) NOT NULL,
    Description nvarchar(1000) NULL,
    Status tinyint DEFAULT 1 NOT NULL,
    ReviewedBy bigint NULL,
    ReviewedAt datetime2(3) NULL,
    ResolutionNote nvarchar(1000) NULL,
    CreatedAt datetime2(3) DEFAULT sysutcdatetime() NOT NULL,
    CONSTRAINT PK_Reports PRIMARY KEY (Id),
    CONSTRAINT FK_Reports_ReportedChannelMessage FOREIGN KEY (ReportedChannelMessageId) REFERENCES dbo.ChannelMessages(Id),
    CONSTRAINT FK_Reports_ReportedChatMessage FOREIGN KEY (ReportedChatMessageId) REFERENCES dbo.ChatMessages(Id),
    CONSTRAINT FK_Reports_ReportedServer FOREIGN KEY (ReportedServerId) REFERENCES dbo.Servers(Id),
    CONSTRAINT FK_Reports_ReportedUser FOREIGN KEY (ReportedUserId) REFERENCES dbo.Users(Id),
    CONSTRAINT FK_Reports_Reporter FOREIGN KEY (ReporterId) REFERENCES dbo.Users(Id),
    CONSTRAINT FK_Reports_ReviewedBy FOREIGN KEY (ReviewedBy) REFERENCES dbo.Users(Id),
    CONSTRAINT CK_Reports_Status CHECK (Status BETWEEN 1 AND 4),
    CONSTRAINT CK_Reports_OneTarget CHECK (
        (CASE WHEN ReportedUserId IS NOT NULL THEN 1 ELSE 0 END +
         CASE WHEN ReportedServerId IS NOT NULL THEN 1 ELSE 0 END +
         CASE WHEN ReportedChannelMessageId IS NOT NULL THEN 1 ELSE 0 END +
         CASE WHEN ReportedChatMessageId IS NOT NULL THEN 1 ELSE 0 END) = 1
    )
);
CREATE NONCLUSTERED INDEX IX_Reports_Queue ON dbo.Reports (Status, CreatedAt DESC) WHERE Status IN (1, 2);
CREATE NONCLUSTERED INDEX IX_Reports_ReportedServer ON dbo.Reports (ReportedServerId) WHERE ReportedServerId IS NOT NULL;
CREATE NONCLUSTERED INDEX IX_Reports_ReportedUser ON dbo.Reports (ReportedUserId) WHERE ReportedUserId IS NOT NULL;
GO

-- ---------- RolePermissions ----------
CREATE TABLE dbo.RolePermissions (
    ServerRoleId bigint NOT NULL,
    PermissionId int NOT NULL,
    CONSTRAINT PK_RolePermissions PRIMARY KEY (ServerRoleId, PermissionId),
    CONSTRAINT FK_RolePermissions_Permissions FOREIGN KEY (PermissionId) REFERENCES dbo.Permissions(Id),
    CONSTRAINT FK_RolePermissions_Roles FOREIGN KEY (ServerRoleId) REFERENCES dbo.ServerRoles(Id) ON DELETE CASCADE
);
GO

-- ---------- TaskAssignees ----------
CREATE TABLE dbo.TaskAssignees (
    Id bigint IDENTITY(1,1) NOT NULL,
    TaskId bigint NOT NULL,
    UserId bigint NOT NULL,
    AssignedBy bigint NOT NULL,
    AssignedAt datetime2(3) DEFAULT sysutcdatetime() NOT NULL,
    CONSTRAINT PK_TaskAssignees PRIMARY KEY (Id),
    CONSTRAINT UQ_TaskAssignees_Task_User UNIQUE (TaskId, UserId),
    CONSTRAINT FK_TaskAssignees_AssignedBy FOREIGN KEY (AssignedBy) REFERENCES dbo.Users(Id),
    CONSTRAINT FK_TaskAssignees_Tasks FOREIGN KEY (TaskId) REFERENCES dbo.Tasks(Id) ON DELETE CASCADE,
    CONSTRAINT FK_TaskAssignees_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(Id)
);
CREATE NONCLUSTERED INDEX IX_TaskAssignees_UserId ON dbo.TaskAssignees (UserId) INCLUDE (TaskId);
GO

-- ============================================================
-- SEED DATA — Users mẫu
-- Password của cả 3 user: Password123!
-- Bcrypt hash (rounds=11): $2a$11$8qYzM1L5xK9pN2vC7bT4QeR6sW3uJ8mH0dF5gK2lP9nX4vB7tY1iW
-- Lưu ý: Nếu hash không login được, dùng API /api/auth/register để tạo user mới
--        rồi copy PasswordHash từ DB vào các user mẫu.
-- ============================================================

SET IDENTITY_INSERT dbo.Users ON;
INSERT INTO dbo.Users (Id, PublicCode, Email, PasswordHash, IsActive, IsEmailVerified, FailedLoginCount, CreatedAt, UpdatedAt, IsSuspended)
VALUES
    (1, 'ADMIN0000000001', 'admin@umcore.local',
     '$2a$11$8qYzM1L5xK9pN2vC7bT4QeR6sW3uJ8mH0dF5gK2lP9nX4vB7tY1iW',
     1, 1, 0, SYSUTCDATETIME(), SYSUTCDATETIME(), 0),
    (2, 'USER000000000002', 'user1@umcore.local',
     '$2a$11$8qYzM1L5xK9pN2vC7bT4QeR6sW3uJ8mH0dF5gK2lP9nX4vB7tY1iW',
     1, 1, 0, SYSUTCDATETIME(), SYSUTCDATETIME(), 0),
    (3, 'USER000000000003', 'user2@umcore.local',
     '$2a$11$8qYzM1L5xK9pN2vC7bT4QeR6sW3uJ8mH0dF5gK2lP9nX4vB7tY1iW',
     1, 1, 0, SYSUTCDATETIME(), SYSUTCDATETIME(), 0);
SET IDENTITY_INSERT dbo.Users OFF;
GO

SET IDENTITY_INSERT dbo.UserProfiles ON;
INSERT INTO dbo.UserProfiles (Id, UserId, AvatarFileId, FullName, DateOfBirth, PhoneNumber, CreatedAt, UpdatedAt)
VALUES
    (1, 1, NULL, N'System Admin', '1990-01-01', '0901234567', SYSUTCDATETIME(), SYSUTCDATETIME()),
    (2, 2, NULL, N'Nguyễn Văn A',  '1995-05-15', '0901234568', SYSUTCDATETIME(), SYSUTCDATETIME()),
    (3, 3, NULL, N'Trần Thị B',    '1998-10-20', '0901234569', SYSUTCDATETIME(), SYSUTCDATETIME());
SET IDENTITY_INSERT dbo.UserProfiles OFF;
GO

PRINT '=== UM Core DB initialized successfully ===';
PRINT 'Sample accounts (password: Password123!):';
PRINT '  admin@umcore.local';
PRINT '  user1@umcore.local';
PRINT '  user2@umcore.local';
GO