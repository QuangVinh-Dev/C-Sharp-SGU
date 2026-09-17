-- DROP SCHEMA dbo;

CREATE SCHEMA dbo;
-- um_database.dbo.MessageEncryptionKeys definition
-- Drop table
-- DROP TABLE um_database.dbo.MessageEncryptionKeys;

CREATE TABLE um_database.dbo.MessageEncryptionKeys ( Id bigint IDENTITY(1, 1) NOT NULL,
[Scope] tinyint NOT NULL,
ScopeId bigint NOT NULL,
WrappedDek varbinary(512) NOT NULL,
KmsKeyId varchar(200) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
Algorithm varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS DEFAULT 'AES-256-GCM' NOT NULL,
KeyVersion int NOT NULL,
CreatedAt datetime2(3) DEFAULT sysutcdatetime() NOT NULL,
RotatedAt datetime2(3) NULL,
IsActive bit DEFAULT 1 NOT NULL,
CONSTRAINT PK_MessageEncryptionKeys PRIMARY KEY (Id),
CONSTRAINT UQ_MessageEncryptionKeys_Scope UNIQUE ([Scope],
ScopeId,
KeyVersion));

CREATE NONCLUSTERED INDEX IX_MessageEncryptionKeys_Scope_Active ON
um_database.dbo.MessageEncryptionKeys ( Scope ASC ,
ScopeId ASC )
WHERE
([IsActive] =(1))
	 WITH ( PAD_INDEX = OFF ,
FILLFACTOR = 100 ,
SORT_IN_TEMPDB = OFF ,
IGNORE_DUP_KEY = OFF ,
STATISTICS_NORECOMPUTE = OFF ,
ONLINE = OFF ,
ALLOW_ROW_LOCKS = ON
,
ALLOW_PAGE_LOCKS = ON
)
	 ON
[PRIMARY ] ;

ALTER TABLE um_database.dbo.MessageEncryptionKeys WITH NOCHECK ADD CONSTRAINT CK_MessageEncryptionKeys_Scope CHECK (([Scope] =(2)
    OR [Scope] =(1)));
-- um_database.dbo.Permissions definition
-- Drop table
-- DROP TABLE um_database.dbo.Permissions;

CREATE TABLE um_database.dbo.Permissions ( Id int NOT NULL,
Code varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
Description nvarchar(200) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
CONSTRAINT PK_Permissions PRIMARY KEY (Id),
CONSTRAINT UQ_Permissions_Code UNIQUE (Code));
-- um_database.dbo.SystemPermissions definition
-- Drop table
-- DROP TABLE um_database.dbo.SystemPermissions;

CREATE TABLE um_database.dbo.SystemPermissions ( Id int NOT NULL,
Code varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
Description nvarchar(200) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
CONSTRAINT PK_SystemPermissions PRIMARY KEY (Id),
CONSTRAINT UQ_SystemPermissions_Code UNIQUE (Code));
-- um_database.dbo.SystemRoles definition
-- Drop table
-- DROP TABLE um_database.dbo.SystemRoles;

CREATE TABLE um_database.dbo.SystemRoles ( Id int IDENTITY(1, 1) NOT NULL,
Name varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
Description nvarchar(200) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
CreatedAt datetime2(3) DEFAULT sysutcdatetime() NOT NULL,
CONSTRAINT PK_SystemRoles PRIMARY KEY (Id),
CONSTRAINT UQ_SystemRoles_Name UNIQUE (Name));
-- um_database.dbo.Users definition
-- Drop table
-- DROP TABLE um_database.dbo.Users;

CREATE TABLE um_database.dbo.Users ( Id bigint IDENTITY(1, 1) NOT NULL,
PublicCode varchar(16) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
Email varchar(256) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
PasswordHash varchar(256) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
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
CONSTRAINT UQ_Users_PublicCode UNIQUE (PublicCode));

CREATE NONCLUSTERED INDEX IX_Users_CreatedAt ON
um_database.dbo.Users ( CreatedAt DESC )  
	 WITH ( PAD_INDEX = OFF ,
FILLFACTOR = 100 ,
SORT_IN_TEMPDB = OFF ,
IGNORE_DUP_KEY = OFF ,
STATISTICS_NORECOMPUTE = OFF ,
ONLINE = OFF ,
ALLOW_ROW_LOCKS = ON
,
ALLOW_PAGE_LOCKS = ON
)
	 ON
[PRIMARY ] ;

CREATE NONCLUSTERED INDEX IX_Users_Suspended ON
um_database.dbo.Users ( IsSuspended ASC )
WHERE
([IsSuspended] =(1))
	 WITH ( PAD_INDEX = OFF ,
FILLFACTOR = 100 ,
SORT_IN_TEMPDB = OFF ,
IGNORE_DUP_KEY = OFF ,
STATISTICS_NORECOMPUTE = OFF ,
ONLINE = OFF ,
ALLOW_ROW_LOCKS = ON
,
ALLOW_PAGE_LOCKS = ON
)
	 ON
[PRIMARY ] ;
-- um_database.dbo.AuditLogs definition
-- Drop table
-- DROP TABLE um_database.dbo.AuditLogs;

CREATE TABLE um_database.dbo.AuditLogs ( Id bigint IDENTITY(1, 1) NOT NULL,
ActorUserId bigint NULL,
[Action] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
EntityType varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
EntityId bigint NOT NULL,
IpAddress varchar(45) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
Metadata nvarchar(MAX) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
CreatedAt datetime2(3) DEFAULT sysutcdatetime() NOT NULL,
CONSTRAINT PK_AuditLogs PRIMARY KEY (Id),
CONSTRAINT FK_AuditLogs_Actor FOREIGN KEY (ActorUserId) REFERENCES um_database.dbo.Users(Id));

CREATE NONCLUSTERED INDEX IX_AuditLogs_Actor ON
um_database.dbo.AuditLogs ( ActorUserId ASC ,
CreatedAt DESC )  
	 WITH ( PAD_INDEX = OFF ,
FILLFACTOR = 100 ,
SORT_IN_TEMPDB = OFF ,
IGNORE_DUP_KEY = OFF ,
STATISTICS_NORECOMPUTE = OFF ,
ONLINE = OFF ,
ALLOW_ROW_LOCKS = ON
,
ALLOW_PAGE_LOCKS = ON
)
	 ON
[PRIMARY ] ;

CREATE NONCLUSTERED INDEX IX_AuditLogs_Entity ON
um_database.dbo.AuditLogs ( EntityType ASC ,
EntityId ASC ,
CreatedAt DESC )  
	 WITH ( PAD_INDEX = OFF ,
FILLFACTOR = 100 ,
SORT_IN_TEMPDB = OFF ,
IGNORE_DUP_KEY = OFF ,
STATISTICS_NORECOMPUTE = OFF ,
ONLINE = OFF ,
ALLOW_ROW_LOCKS = ON
,
ALLOW_PAGE_LOCKS = ON
)
	 ON
[PRIMARY ] ;

ALTER TABLE um_database.dbo.AuditLogs WITH NOCHECK ADD CONSTRAINT CK_AuditLogs_Metadata_Json CHECK (([Metadata] IS NULL
    OR isjson([Metadata])=(1)));
-- um_database.dbo.ChatRooms definition
-- Drop table
-- DROP TABLE um_database.dbo.ChatRooms;

CREATE TABLE um_database.dbo.ChatRooms ( Id bigint IDENTITY(1, 1) NOT NULL,
[Type] tinyint NOT NULL,
Name nvarchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
CreatedBy bigint NOT NULL,
CreatedAt datetime2(3) DEFAULT sysutcdatetime() NOT NULL,
UpdatedAt datetime2(3) DEFAULT sysutcdatetime() NOT NULL,
DeletedAt datetime2(3) NULL,
CONSTRAINT PK_ChatRooms PRIMARY KEY (Id),
CONSTRAINT FK_ChatRooms_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES um_database.dbo.Users(Id));

ALTER TABLE um_database.dbo.ChatRooms WITH NOCHECK ADD CONSTRAINT CK_ChatRooms_Type CHECK (([Type] =(2)
    OR [Type] =(1)));
-- um_database.dbo.EmailOtps definition
-- Drop table
-- DROP TABLE um_database.dbo.EmailOtps;

CREATE TABLE um_database.dbo.EmailOtps ( Id bigint IDENTITY(1, 1) NOT NULL,
UserId bigint NOT NULL,
CodeHash varbinary(32) NOT NULL,
Purpose varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
ExpiresAt datetime2(3) NOT NULL,
AttemptCount int DEFAULT 0 NOT NULL,
MaxAttempts int DEFAULT 5 NOT NULL,
UsedAt datetime2(3) NULL,
RequestIp varchar(45) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
CreatedAt datetime2(3) DEFAULT sysutcdatetime() NOT NULL,
CONSTRAINT PK_EmailOtps PRIMARY KEY (Id),
CONSTRAINT FK_EmailOtps_Users FOREIGN KEY (UserId) REFERENCES um_database.dbo.Users(Id) ON
DELETE
    CASCADE);

CREATE NONCLUSTERED INDEX IX_EmailOtps_UserId_Purpose_CreatedAt ON
um_database.dbo.EmailOtps ( UserId ASC ,
Purpose ASC ,
CreatedAt DESC )  
	 WITH ( PAD_INDEX = OFF ,
FILLFACTOR = 100 ,
SORT_IN_TEMPDB = OFF ,
IGNORE_DUP_KEY = OFF ,
STATISTICS_NORECOMPUTE = OFF ,
ONLINE = OFF ,
ALLOW_ROW_LOCKS = ON
,
ALLOW_PAGE_LOCKS = ON
)
	 ON
[PRIMARY ] ;
-- um_database.dbo.Folders definition
-- Drop table
-- DROP TABLE um_database.dbo.Folders;

CREATE TABLE um_database.dbo.Folders ( Id bigint IDENTITY(1, 1) NOT NULL,
OwnerId bigint NOT NULL,
ParentFolderId bigint NULL,
Name nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
CreatedAt datetime2(3) DEFAULT sysutcdatetime() NOT NULL,
UpdatedAt datetime2(3) DEFAULT sysutcdatetime() NOT NULL,
DeletedAt datetime2(3) NULL,
CONSTRAINT PK_Folders PRIMARY KEY (Id),
CONSTRAINT FK_Folders_Owner FOREIGN KEY (OwnerId) REFERENCES um_database.dbo.Users(Id),
CONSTRAINT FK_Folders_Parent FOREIGN KEY (ParentFolderId) REFERENCES um_database.dbo.Folders(Id));

CREATE NONCLUSTERED INDEX IX_Folders_Owner_Parent ON
um_database.dbo.Folders ( OwnerId ASC ,
ParentFolderId ASC )
WHERE
([DeletedAt] IS NULL)
	 WITH ( PAD_INDEX = OFF ,
FILLFACTOR = 100 ,
SORT_IN_TEMPDB = OFF ,
IGNORE_DUP_KEY = OFF ,
STATISTICS_NORECOMPUTE = OFF ,
ONLINE = OFF ,
ALLOW_ROW_LOCKS = ON
,
ALLOW_PAGE_LOCKS = ON
)
	 ON
[PRIMARY ] ;
-- um_database.dbo.Friendships definition
-- Drop table
-- DROP TABLE um_database.dbo.Friendships;

CREATE TABLE um_database.dbo.Friendships ( Id bigint IDENTITY(1, 1) NOT NULL,
RequesterId bigint NOT NULL,
AddresseeId bigint NOT NULL,
Status tinyint DEFAULT 1 NOT NULL,
UserLowId AS (case
    when [RequesterId]<[AddresseeId] then [RequesterId]
    else [AddresseeId]
end) PERSISTED NOT NULL,
UserHighId AS (case
    when [RequesterId]<[AddresseeId] then [AddresseeId]
    else [RequesterId]
end) PERSISTED NOT NULL,
CreatedAt datetime2(3) DEFAULT sysutcdatetime() NOT NULL,
UpdatedAt datetime2(3) DEFAULT sysutcdatetime() NOT NULL,
RespondedAt datetime2(3) NULL,
CONSTRAINT PK_Friendships PRIMARY KEY (Id),
CONSTRAINT FK_Friendships_Addressee FOREIGN KEY (AddresseeId) REFERENCES um_database.dbo.Users(Id),
CONSTRAINT FK_Friendships_Requester FOREIGN KEY (RequesterId) REFERENCES um_database.dbo.Users(Id));

CREATE UNIQUE NONCLUSTERED INDEX UQ_Friendships_Pair ON
um_database.dbo.Friendships ( UserLowId ASC ,
UserHighId ASC )  
	 WITH ( PAD_INDEX = OFF ,
FILLFACTOR = 100 ,
SORT_IN_TEMPDB = OFF ,
IGNORE_DUP_KEY = OFF ,
STATISTICS_NORECOMPUTE = OFF ,
ONLINE = OFF ,
ALLOW_ROW_LOCKS = ON
,
ALLOW_PAGE_LOCKS = ON
)
	 ON
[PRIMARY ] ;

ALTER TABLE um_database.dbo.Friendships WITH NOCHECK ADD CONSTRAINT CK_Friendships_NotSelf CHECK (([RequesterId] <> [AddresseeId]));

ALTER TABLE um_database.dbo.Friendships WITH NOCHECK ADD CONSTRAINT CK_Friendships_Status CHECK (([Status] >=(1)
    AND [Status] <=(4)));
-- um_database.dbo.Notifications definition
-- Drop table
-- DROP TABLE um_database.dbo.Notifications;

CREATE TABLE um_database.dbo.Notifications ( Id bigint IDENTITY(1, 1) NOT NULL,
UserId bigint NOT NULL,
[Type] varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
Title nvarchar(200) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
Content nvarchar(1000) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
ReferenceType varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
ReferenceId bigint NULL,
IsRead bit DEFAULT 0 NOT NULL,
CreatedAt datetime2(3) DEFAULT sysutcdatetime() NOT NULL,
ReadAt datetime2(3) NULL,
CONSTRAINT PK_Notifications PRIMARY KEY (Id),
CONSTRAINT FK_Notifications_Users FOREIGN KEY (UserId) REFERENCES um_database.dbo.Users(Id) ON
DELETE
    CASCADE);

CREATE NONCLUSTERED INDEX IX_Notifications_Unread ON
um_database.dbo.Notifications ( UserId ASC ,
CreatedAt DESC )
WHERE
([IsRead] =(0))
	 WITH ( PAD_INDEX = OFF ,
FILLFACTOR = 100 ,
SORT_IN_TEMPDB = OFF ,
IGNORE_DUP_KEY = OFF ,
STATISTICS_NORECOMPUTE = OFF ,
ONLINE = OFF ,
ALLOW_ROW_LOCKS = ON
,
ALLOW_PAGE_LOCKS = ON
)
	 ON
[PRIMARY ] ;
-- um_database.dbo.RefreshTokens definition
-- Drop table
-- DROP TABLE um_database.dbo.RefreshTokens;

CREATE TABLE um_database.dbo.RefreshTokens ( Id uniqueidentifier DEFAULT newid() NOT NULL,
UserId bigint NOT NULL,
TokenHash varbinary(64) NOT NULL,
ExpiresAt datetime2(3) NOT NULL,
CreatedAt datetime2(3) DEFAULT sysutcdatetime() NOT NULL,
CreatedByIp varchar(45) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
RevokedAt datetime2(3) NULL,
RevokedByIp varchar(45) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
ReplacedByTokenId uniqueidentifier NULL,
CONSTRAINT PK_RefreshTokens PRIMARY KEY (Id),
CONSTRAINT FK_RefreshTokens_Users FOREIGN KEY (UserId) REFERENCES um_database.dbo.Users(Id) ON
DELETE
    CASCADE);

CREATE NONCLUSTERED INDEX IX_RefreshTokens_UserId_ExpiresAt ON
um_database.dbo.RefreshTokens ( UserId ASC ,
ExpiresAt ASC )  
	 INCLUDE (RevokedAt) 
	 WITH ( PAD_INDEX = OFF ,
FILLFACTOR = 100 ,
SORT_IN_TEMPDB = OFF ,
IGNORE_DUP_KEY = OFF ,
STATISTICS_NORECOMPUTE = OFF ,
ONLINE = OFF ,
ALLOW_ROW_LOCKS = ON
,
ALLOW_PAGE_LOCKS = ON
)
	 ON
[PRIMARY ] ;

CREATE UNIQUE NONCLUSTERED INDEX UQ_RefreshTokens_TokenHash ON
um_database.dbo.RefreshTokens ( TokenHash ASC )  
	 WITH ( PAD_INDEX = OFF ,
FILLFACTOR = 100 ,
SORT_IN_TEMPDB = OFF ,
IGNORE_DUP_KEY = OFF ,
STATISTICS_NORECOMPUTE = OFF ,
ONLINE = OFF ,
ALLOW_ROW_LOCKS = ON
,
ALLOW_PAGE_LOCKS = ON
)
	 ON
[PRIMARY ] ;
-- um_database.dbo.SystemAdmins definition
-- Drop table
-- DROP TABLE um_database.dbo.SystemAdmins;

CREATE TABLE um_database.dbo.SystemAdmins ( Id bigint IDENTITY(1, 1) NOT NULL,
UserId bigint NOT NULL,
SystemRoleId int NOT NULL,
RequireMfa bit DEFAULT 1 NOT NULL,
GrantedBy bigint NOT NULL,
GrantedAt datetime2(3) DEFAULT sysutcdatetime() NOT NULL,
RevokedBy bigint NULL,
RevokedAt datetime2(3) NULL,
CONSTRAINT PK_SystemAdmins PRIMARY KEY (Id),
CONSTRAINT FK_SystemAdmins_GrantedBy FOREIGN KEY (GrantedBy) REFERENCES um_database.dbo.Users(Id),
CONSTRAINT FK_SystemAdmins_RevokedBy FOREIGN KEY (RevokedBy) REFERENCES um_database.dbo.Users(Id),
CONSTRAINT FK_SystemAdmins_Role FOREIGN KEY (SystemRoleId) REFERENCES um_database.dbo.SystemRoles(Id),
CONSTRAINT FK_SystemAdmins_User FOREIGN KEY (UserId) REFERENCES um_database.dbo.Users(Id));

CREATE UNIQUE NONCLUSTERED INDEX UQ_SystemAdmins_ActiveUser ON
um_database.dbo.SystemAdmins ( UserId ASC )
WHERE
([RevokedAt] IS NULL)
	 WITH ( PAD_INDEX = OFF ,
FILLFACTOR = 100 ,
SORT_IN_TEMPDB = OFF ,
IGNORE_DUP_KEY = OFF ,
STATISTICS_NORECOMPUTE = OFF ,
ONLINE = OFF ,
ALLOW_ROW_LOCKS = ON
,
ALLOW_PAGE_LOCKS = ON
)
	 ON
[PRIMARY ] ;

ALTER TABLE um_database.dbo.SystemAdmins WITH NOCHECK ADD CONSTRAINT CK_SystemAdmins_NoSelfGrant CHECK (([GrantedBy] <> [UserId]));
-- um_database.dbo.SystemRolePermissions definition
-- Drop table
-- DROP TABLE um_database.dbo.SystemRolePermissions;

CREATE TABLE um_database.dbo.SystemRolePermissions ( SystemRoleId int NOT NULL,
PermissionId int NOT NULL,
CONSTRAINT PK_SystemRolePermissions PRIMARY KEY (SystemRoleId,
PermissionId),
CONSTRAINT FK_SystemRolePermissions_Permission FOREIGN KEY (PermissionId) REFERENCES um_database.dbo.SystemPermissions(Id),
CONSTRAINT FK_SystemRolePermissions_Role FOREIGN KEY (SystemRoleId) REFERENCES um_database.dbo.SystemRoles(Id) ON
DELETE
    CASCADE);
-- um_database.dbo.UserPublicKeys definition
-- Drop table
-- DROP TABLE um_database.dbo.UserPublicKeys;

CREATE TABLE um_database.dbo.UserPublicKeys ( Id bigint IDENTITY(1, 1) NOT NULL,
UserId bigint NOT NULL,
PublicKey varbinary(1024) NOT NULL,
Algorithm varchar(30) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
IsActive bit DEFAULT 1 NOT NULL,
CreatedAt datetime2(3) DEFAULT sysutcdatetime() NOT NULL,
RevokedAt datetime2(3) NULL,
CONSTRAINT PK_UserPublicKeys PRIMARY KEY (Id),
CONSTRAINT FK_UserPublicKeys_Users FOREIGN KEY (UserId) REFERENCES um_database.dbo.Users(Id) ON
DELETE
    CASCADE);

CREATE NONCLUSTERED INDEX IX_UserPublicKeys_UserId_Active ON
um_database.dbo.UserPublicKeys ( UserId ASC )
WHERE
([IsActive] =(1))
	 WITH ( PAD_INDEX = OFF ,
FILLFACTOR = 100 ,
SORT_IN_TEMPDB = OFF ,
IGNORE_DUP_KEY = OFF ,
STATISTICS_NORECOMPUTE = OFF ,
ONLINE = OFF ,
ALLOW_ROW_LOCKS = ON
,
ALLOW_PAGE_LOCKS = ON
)
	 ON
[PRIMARY ] ;
-- um_database.dbo.UserSanctions definition
-- Drop table
-- DROP TABLE um_database.dbo.UserSanctions;

CREATE TABLE um_database.dbo.UserSanctions ( Id bigint IDENTITY(1, 1) NOT NULL,
UserId bigint NOT NULL,
[Type] tinyint NOT NULL,
Reason nvarchar(500) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
IssuedBy bigint NOT NULL,
IssuedAt datetime2(3) DEFAULT sysutcdatetime() NOT NULL,
ExpiresAt datetime2(3) NULL,
RevokedBy bigint NULL,
RevokedAt datetime2(3) NULL,
RevokeReason nvarchar(500) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
CONSTRAINT PK_UserSanctions PRIMARY KEY (Id),
CONSTRAINT FK_UserSanctions_IssuedBy FOREIGN KEY (IssuedBy) REFERENCES um_database.dbo.Users(Id),
CONSTRAINT FK_UserSanctions_RevokedBy FOREIGN KEY (RevokedBy) REFERENCES um_database.dbo.Users(Id),
CONSTRAINT FK_UserSanctions_User FOREIGN KEY (UserId) REFERENCES um_database.dbo.Users(Id));

CREATE NONCLUSTERED INDEX IX_UserSanctions_ActiveByUser ON
um_database.dbo.UserSanctions ( UserId ASC )
WHERE
([RevokedAt] IS NULL)
	 WITH ( PAD_INDEX = OFF ,
FILLFACTOR = 100 ,
SORT_IN_TEMPDB = OFF ,
IGNORE_DUP_KEY = OFF ,
STATISTICS_NORECOMPUTE = OFF ,
ONLINE = OFF ,
ALLOW_ROW_LOCKS = ON
,
ALLOW_PAGE_LOCKS = ON
)
	 ON
[PRIMARY ] ;

CREATE NONCLUSTERED INDEX IX_UserSanctions_IssuedBy ON
um_database.dbo.UserSanctions ( IssuedBy ASC ,
IssuedAt DESC )  
	 WITH ( PAD_INDEX = OFF ,
FILLFACTOR = 100 ,
SORT_IN_TEMPDB = OFF ,
IGNORE_DUP_KEY = OFF ,
STATISTICS_NORECOMPUTE = OFF ,
ONLINE = OFF ,
ALLOW_ROW_LOCKS = ON
,
ALLOW_PAGE_LOCKS = ON
)
	 ON
[PRIMARY ] ;

ALTER TABLE um_database.dbo.UserSanctions WITH NOCHECK ADD CONSTRAINT CK_UserSanctions_Type CHECK (([Type] =(2)
    OR [Type] =(1)));
-- um_database.dbo.ChatMessages definition
-- Drop table
-- DROP TABLE um_database.dbo.ChatMessages;

CREATE TABLE um_database.dbo.ChatMessages ( Id bigint IDENTITY(1, 1) NOT NULL,
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
CONSTRAINT FK_ChatMessages_ChatRooms FOREIGN KEY (ChatRoomId) REFERENCES um_database.dbo.ChatRooms(Id),
CONSTRAINT FK_ChatMessages_EncryptionKey FOREIGN KEY (EncryptionKeyId) REFERENCES um_database.dbo.MessageEncryptionKeys(Id),
CONSTRAINT FK_ChatMessages_ReplyTo FOREIGN KEY (ReplyToMessageId) REFERENCES um_database.dbo.ChatMessages(Id),
CONSTRAINT FK_ChatMessages_Sender FOREIGN KEY (SenderId) REFERENCES um_database.dbo.Users(Id));

CREATE UNIQUE CLUSTERED INDEX CIX_ChatMessages_Room_Created ON
um_database.dbo.ChatMessages ( ChatRoomId ASC ,
CreatedAt DESC ,
Id DESC )  
	 WITH ( PAD_INDEX = OFF ,
FILLFACTOR = 100 ,
SORT_IN_TEMPDB = OFF ,
IGNORE_DUP_KEY = OFF ,
STATISTICS_NORECOMPUTE = OFF ,
ONLINE = OFF ,
ALLOW_ROW_LOCKS = ON
,
ALLOW_PAGE_LOCKS = ON
)
	 ON
[PRIMARY ] ;

ALTER TABLE um_database.dbo.ChatMessages WITH NOCHECK ADD CONSTRAINT CK_ChatMessages_Type CHECK (([MessageType] >=(1)
    AND [MessageType] <=(4)));
-- um_database.dbo.Files definition
-- Drop table
-- DROP TABLE um_database.dbo.Files;

CREATE TABLE um_database.dbo.Files ( Id bigint IDENTITY(1, 1) NOT NULL,
OwnerId bigint NOT NULL,
FolderId bigint NULL,
FileName nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
OriginalFileName nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
Extension varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
MimeType varchar(150) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
DetectedMimeType varchar(150) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
SizeBytes bigint NOT NULL,
Sha256Hash binary(32) NOT NULL,
StorageProvider varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
StorageIdentifier varchar(500) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
ScanStatus tinyint DEFAULT 0 NOT NULL,
ScannedAt datetime2(3) NULL,
CreatedAt datetime2(3) DEFAULT sysutcdatetime() NOT NULL,
UpdatedAt datetime2(3) DEFAULT sysutcdatetime() NOT NULL,
DeletedAt datetime2(3) NULL,
CONSTRAINT PK_Files PRIMARY KEY (Id),
CONSTRAINT FK_Files_Folder FOREIGN KEY (FolderId) REFERENCES um_database.dbo.Folders(Id),
CONSTRAINT FK_Files_Owner FOREIGN KEY (OwnerId) REFERENCES um_database.dbo.Users(Id));

CREATE NONCLUSTERED INDEX IX_Files_Owner_Folder ON
um_database.dbo.Files ( OwnerId ASC ,
FolderId ASC )
WHERE
([DeletedAt] IS NULL)
	 WITH ( PAD_INDEX = OFF ,
FILLFACTOR = 100 ,
SORT_IN_TEMPDB = OFF ,
IGNORE_DUP_KEY = OFF ,
STATISTICS_NORECOMPUTE = OFF ,
ONLINE = OFF ,
ALLOW_ROW_LOCKS = ON
,
ALLOW_PAGE_LOCKS = ON
)
	 ON
[PRIMARY ] ;

CREATE NONCLUSTERED INDEX IX_Files_ScanStatus_Pending ON
um_database.dbo.Files ( ScanStatus ASC )
WHERE
([ScanStatus] =(0))
	 WITH ( PAD_INDEX = OFF ,
FILLFACTOR = 100 ,
SORT_IN_TEMPDB = OFF ,
IGNORE_DUP_KEY = OFF ,
STATISTICS_NORECOMPUTE = OFF ,
ONLINE = OFF ,
ALLOW_ROW_LOCKS = ON
,
ALLOW_PAGE_LOCKS = ON
)
	 ON
[PRIMARY ] ;

CREATE NONCLUSTERED INDEX IX_Files_Sha256Hash ON
um_database.dbo.Files ( Sha256Hash ASC )  
	 WITH ( PAD_INDEX = OFF ,
FILLFACTOR = 100 ,
SORT_IN_TEMPDB = OFF ,
IGNORE_DUP_KEY = OFF ,
STATISTICS_NORECOMPUTE = OFF ,
ONLINE = OFF ,
ALLOW_ROW_LOCKS = ON
,
ALLOW_PAGE_LOCKS = ON
)
	 ON
[PRIMARY ] ;

ALTER TABLE um_database.dbo.Files WITH NOCHECK ADD CONSTRAINT CK_Files_ScanStatus CHECK (([ScanStatus] >=(0)
    AND [ScanStatus] <=(3)));
-- um_database.dbo.MessageKeyRecipients definition
-- Drop table
-- DROP TABLE um_database.dbo.MessageKeyRecipients;

CREATE TABLE um_database.dbo.MessageKeyRecipients ( Id bigint IDENTITY(1, 1) NOT NULL,
ChatRoomId bigint NOT NULL,
UserId bigint NOT NULL,
UserPublicKeyId bigint NOT NULL,
EncryptedSessionKey varbinary(512) NOT NULL,
KeyVersion int NOT NULL,
CreatedAt datetime2(3) DEFAULT sysutcdatetime() NOT NULL,
CONSTRAINT PK_MessageKeyRecipients PRIMARY KEY (Id),
CONSTRAINT UQ_MessageKeyRecipients UNIQUE (ChatRoomId,
UserId,
KeyVersion),
CONSTRAINT FK_MessageKeyRecipients_ChatRooms FOREIGN KEY (ChatRoomId) REFERENCES um_database.dbo.ChatRooms(Id) ON
DELETE
    CASCADE,
    CONSTRAINT FK_MessageKeyRecipients_PublicKey FOREIGN KEY (UserPublicKeyId) REFERENCES um_database.dbo.UserPublicKeys(Id),
    CONSTRAINT FK_MessageKeyRecipients_Users FOREIGN KEY (UserId) REFERENCES um_database.dbo.Users(Id));

CREATE NONCLUSTERED INDEX IX_MessageKeyRecipients_User ON
um_database.dbo.MessageKeyRecipients ( UserId ASC )  
	 WITH ( PAD_INDEX = OFF ,
FILLFACTOR = 100 ,
SORT_IN_TEMPDB = OFF ,
IGNORE_DUP_KEY = OFF ,
STATISTICS_NORECOMPUTE = OFF ,
ONLINE = OFF ,
ALLOW_ROW_LOCKS = ON
,
ALLOW_PAGE_LOCKS = ON
)
	 ON
[PRIMARY ] ;
-- um_database.dbo.Servers definition
-- Drop table
-- DROP TABLE um_database.dbo.Servers;

CREATE TABLE um_database.dbo.Servers ( Id bigint IDENTITY(1, 1) NOT NULL,
Name nvarchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
OwnerId bigint NOT NULL,
IconFileId bigint NULL,
CreatedAt datetime2(3) DEFAULT sysutcdatetime() NOT NULL,
UpdatedAt datetime2(3) DEFAULT sysutcdatetime() NOT NULL,
DeletedAt datetime2(3) NULL,
ScheduledDeleteAt datetime2(3) NULL,
IsSuspended bit DEFAULT 0 NOT NULL,
SuspendedUntil datetime2(3) NULL,
CONSTRAINT PK_Servers PRIMARY KEY (Id),
CONSTRAINT FK_Servers_IconFile FOREIGN KEY (IconFileId) REFERENCES um_database.dbo.Files(Id),
CONSTRAINT FK_Servers_Owner FOREIGN KEY (OwnerId) REFERENCES um_database.dbo.Users(Id));

CREATE NONCLUSTERED INDEX IX_Servers_CreatedAt ON
um_database.dbo.Servers ( CreatedAt DESC )
WHERE
([DeletedAt] IS NULL)
	 WITH ( PAD_INDEX = OFF ,
FILLFACTOR = 100 ,
SORT_IN_TEMPDB = OFF ,
IGNORE_DUP_KEY = OFF ,
STATISTICS_NORECOMPUTE = OFF ,
ONLINE = OFF ,
ALLOW_ROW_LOCKS = ON
,
ALLOW_PAGE_LOCKS = ON
)
	 ON
[PRIMARY ] ;

CREATE NONCLUSTERED INDEX IX_Servers_ScheduledDeleteAt ON
um_database.dbo.Servers ( ScheduledDeleteAt ASC )
WHERE
([ScheduledDeleteAt] IS NOT NULL)
	 WITH ( PAD_INDEX = OFF ,
FILLFACTOR = 100 ,
SORT_IN_TEMPDB = OFF ,
IGNORE_DUP_KEY = OFF ,
STATISTICS_NORECOMPUTE = OFF ,
ONLINE = OFF ,
ALLOW_ROW_LOCKS = ON
,
ALLOW_PAGE_LOCKS = ON
)
	 ON
[PRIMARY ] ;

CREATE NONCLUSTERED INDEX IX_Servers_Suspended ON
um_database.dbo.Servers ( IsSuspended ASC )
WHERE
([IsSuspended] =(1))
	 WITH ( PAD_INDEX = OFF ,
FILLFACTOR = 100 ,
SORT_IN_TEMPDB = OFF ,
IGNORE_DUP_KEY = OFF ,
STATISTICS_NORECOMPUTE = OFF ,
ONLINE = OFF ,
ALLOW_ROW_LOCKS = ON
,
ALLOW_PAGE_LOCKS = ON
)
	 ON
[PRIMARY ] ;
-- um_database.dbo.UserProfiles definition
-- Drop table
-- DROP TABLE um_database.dbo.UserProfiles;

CREATE TABLE um_database.dbo.UserProfiles ( Id bigint IDENTITY(1, 1) NOT NULL,
UserId bigint NOT NULL,
AvatarFileId bigint NULL,
FullName nvarchar(150) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
DateOfBirth date NULL,
PhoneNumber varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
CreatedAt datetime2(3) DEFAULT sysutcdatetime() NOT NULL,
UpdatedAt datetime2(3) DEFAULT sysutcdatetime() NOT NULL,
CONSTRAINT PK_UserProfiles PRIMARY KEY (Id),
CONSTRAINT UQ_UserProfiles_UserId UNIQUE (UserId),
CONSTRAINT FK_UserProfiles_AvatarFile FOREIGN KEY (AvatarFileId) REFERENCES um_database.dbo.Files(Id),
CONSTRAINT FK_UserProfiles_Users FOREIGN KEY (UserId) REFERENCES um_database.dbo.Users(Id) ON
DELETE
    CASCADE);
-- um_database.dbo.Categories definition
-- Drop table
-- DROP TABLE um_database.dbo.Categories;

CREATE TABLE um_database.dbo.Categories ( Id bigint IDENTITY(1, 1) NOT NULL,
ServerId bigint NOT NULL,
Name nvarchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[Position] int DEFAULT 0 NOT NULL,
CreatedAt datetime2(3) DEFAULT sysutcdatetime() NOT NULL,
UpdatedAt datetime2(3) DEFAULT sysutcdatetime() NOT NULL,
DeletedAt datetime2(3) NULL,
CONSTRAINT PK_Categories PRIMARY KEY (Id),
CONSTRAINT FK_Categories_Servers FOREIGN KEY (ServerId) REFERENCES um_database.dbo.Servers(Id) ON
DELETE
    CASCADE);

CREATE NONCLUSTERED INDEX IX_Categories_ServerId ON
um_database.dbo.Categories ( ServerId ASC ,
Position ASC )
WHERE
([DeletedAt] IS NULL)
	 WITH ( PAD_INDEX = OFF ,
FILLFACTOR = 100 ,
SORT_IN_TEMPDB = OFF ,
IGNORE_DUP_KEY = OFF ,
STATISTICS_NORECOMPUTE = OFF ,
ONLINE = OFF ,
ALLOW_ROW_LOCKS = ON
,
ALLOW_PAGE_LOCKS = ON
)
	 ON
[PRIMARY ] ;
-- um_database.dbo.Channels definition
-- Drop table
-- DROP TABLE um_database.dbo.Channels;

CREATE TABLE um_database.dbo.Channels ( Id bigint IDENTITY(1, 1) NOT NULL,
ServerId bigint NOT NULL,
CategoryId bigint NULL,
Name nvarchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[Type] tinyint NOT NULL,
[Position] int DEFAULT 0 NOT NULL,
CreatedAt datetime2(3) DEFAULT sysutcdatetime() NOT NULL,
UpdatedAt datetime2(3) DEFAULT sysutcdatetime() NOT NULL,
DeletedAt datetime2(3) NULL,
CONSTRAINT PK_Channels PRIMARY KEY (Id),
CONSTRAINT FK_Channels_Categories FOREIGN KEY (CategoryId) REFERENCES um_database.dbo.Categories(Id),
CONSTRAINT FK_Channels_Servers FOREIGN KEY (ServerId) REFERENCES um_database.dbo.Servers(Id) ON
DELETE
    CASCADE);

CREATE NONCLUSTERED INDEX IX_Channels_CategoryId ON
um_database.dbo.Channels ( CategoryId ASC )
WHERE
([DeletedAt] IS NULL)
	 WITH ( PAD_INDEX = OFF ,
FILLFACTOR = 100 ,
SORT_IN_TEMPDB = OFF ,
IGNORE_DUP_KEY = OFF ,
STATISTICS_NORECOMPUTE = OFF ,
ONLINE = OFF ,
ALLOW_ROW_LOCKS = ON
,
ALLOW_PAGE_LOCKS = ON
)
	 ON
[PRIMARY ] ;

CREATE NONCLUSTERED INDEX IX_Channels_ServerId ON
um_database.dbo.Channels ( ServerId ASC )
WHERE
([DeletedAt] IS NULL)
	 WITH ( PAD_INDEX = OFF ,
FILLFACTOR = 100 ,
SORT_IN_TEMPDB = OFF ,
IGNORE_DUP_KEY = OFF ,
STATISTICS_NORECOMPUTE = OFF ,
ONLINE = OFF ,
ALLOW_ROW_LOCKS = ON
,
ALLOW_PAGE_LOCKS = ON
)
	 ON
[PRIMARY ] ;

ALTER TABLE um_database.dbo.Channels WITH NOCHECK ADD CONSTRAINT CK_Channels_Type CHECK (([Type] =(2)
    OR [Type] =(1)));
-- um_database.dbo.ChatMembers definition
-- Drop table
-- DROP TABLE um_database.dbo.ChatMembers;

CREATE TABLE um_database.dbo.ChatMembers ( Id bigint IDENTITY(1, 1) NOT NULL,
ChatRoomId bigint NOT NULL,
UserId bigint NOT NULL,
[Role] tinyint DEFAULT 1 NOT NULL,
JoinedAt datetime2(3) DEFAULT sysutcdatetime() NOT NULL,
LeftAt datetime2(3) NULL,
LastReadMessageId bigint NULL,
IsMuted bit DEFAULT 0 NOT NULL,
IsHidden bit DEFAULT 0 NOT NULL,
CONSTRAINT PK_ChatMembers PRIMARY KEY (Id),
CONSTRAINT UQ_ChatMembers_Room_User UNIQUE (ChatRoomId,
UserId),
CONSTRAINT FK_ChatMembers_ChatRooms FOREIGN KEY (ChatRoomId) REFERENCES um_database.dbo.ChatRooms(Id) ON
DELETE
    CASCADE,
    CONSTRAINT FK_ChatMembers_LastReadMessage FOREIGN KEY (LastReadMessageId) REFERENCES um_database.dbo.ChatMessages(Id),
    CONSTRAINT FK_ChatMembers_Users FOREIGN KEY (UserId) REFERENCES um_database.dbo.Users(Id));

CREATE NONCLUSTERED INDEX IX_ChatMembers_UserId ON
um_database.dbo.ChatMembers ( UserId ASC )
WHERE
([LeftAt] IS NULL)
	 WITH ( PAD_INDEX = OFF ,
FILLFACTOR = 100 ,
SORT_IN_TEMPDB = OFF ,
IGNORE_DUP_KEY = OFF ,
STATISTICS_NORECOMPUTE = OFF ,
ONLINE = OFF ,
ALLOW_ROW_LOCKS = ON
,
ALLOW_PAGE_LOCKS = ON
)
	 ON
[PRIMARY ] ;
-- um_database.dbo.ServerMembers definition
-- Drop table
-- DROP TABLE um_database.dbo.ServerMembers;

CREATE TABLE um_database.dbo.ServerMembers ( Id bigint IDENTITY(1, 1) NOT NULL,
ServerId bigint NOT NULL,
UserId bigint NOT NULL,
Nickname nvarchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
JoinedAt datetime2(3) DEFAULT sysutcdatetime() NOT NULL,
IsBanned bit DEFAULT 0 NOT NULL,
BannedAt datetime2(3) NULL,
BannedBy bigint NULL,
BanReason nvarchar(500) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
LeftAt datetime2(3) NULL,
CONSTRAINT PK_ServerMembers PRIMARY KEY (Id),
CONSTRAINT UQ_ServerMembers_Server_User UNIQUE (ServerId,
UserId),
CONSTRAINT FK_ServerMembers_BannedBy FOREIGN KEY (BannedBy) REFERENCES um_database.dbo.Users(Id),
CONSTRAINT FK_ServerMembers_Servers FOREIGN KEY (ServerId) REFERENCES um_database.dbo.Servers(Id) ON
DELETE
    CASCADE,
    CONSTRAINT FK_ServerMembers_Users FOREIGN KEY (UserId) REFERENCES um_database.dbo.Users(Id));

CREATE UNIQUE NONCLUSTERED INDEX IX_ServerMembers_ServerId_UserId ON
um_database.dbo.ServerMembers ( ServerId ASC ,
UserId ASC )  
	 INCLUDE (IsBanned)
WHERE
([LeftAt] IS NULL)
	 WITH ( PAD_INDEX = OFF ,
FILLFACTOR = 100 ,
SORT_IN_TEMPDB = OFF ,
IGNORE_DUP_KEY = OFF ,
STATISTICS_NORECOMPUTE = OFF ,
ONLINE = OFF ,
ALLOW_ROW_LOCKS = ON
,
ALLOW_PAGE_LOCKS = ON
)
	 ON
[PRIMARY ] ;

CREATE NONCLUSTERED INDEX IX_ServerMembers_UserId ON
um_database.dbo.ServerMembers ( UserId ASC )
WHERE
([LeftAt] IS NULL)
	 WITH ( PAD_INDEX = OFF ,
FILLFACTOR = 100 ,
SORT_IN_TEMPDB = OFF ,
IGNORE_DUP_KEY = OFF ,
STATISTICS_NORECOMPUTE = OFF ,
ONLINE = OFF ,
ALLOW_ROW_LOCKS = ON
,
ALLOW_PAGE_LOCKS = ON
)
	 ON
[PRIMARY ] ;
-- um_database.dbo.ServerRoles definition
-- Drop table
-- DROP TABLE um_database.dbo.ServerRoles;

CREATE TABLE um_database.dbo.ServerRoles ( Id bigint IDENTITY(1, 1) NOT NULL,
ServerId bigint NOT NULL,
Name nvarchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
IsSystem bit DEFAULT 0 NOT NULL,
[Position] int DEFAULT 0 NOT NULL,
Color varchar(7) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
CreatedAt datetime2(3) DEFAULT sysutcdatetime() NOT NULL,
CONSTRAINT PK_ServerRoles PRIMARY KEY (Id),
CONSTRAINT UQ_ServerRoles_Server_Name UNIQUE (ServerId,
Name),
CONSTRAINT FK_ServerRoles_Servers FOREIGN KEY (ServerId) REFERENCES um_database.dbo.Servers(Id) ON
DELETE
    CASCADE);

CREATE NONCLUSTERED INDEX IX_ServerRoles_ServerId ON
um_database.dbo.ServerRoles ( ServerId ASC )  
	 WITH ( PAD_INDEX = OFF ,
FILLFACTOR = 100 ,
SORT_IN_TEMPDB = OFF ,
IGNORE_DUP_KEY = OFF ,
STATISTICS_NORECOMPUTE = OFF ,
ONLINE = OFF ,
ALLOW_ROW_LOCKS = ON
,
ALLOW_PAGE_LOCKS = ON
)
	 ON
[PRIMARY ] ;
-- um_database.dbo.ServerSanctions definition
-- Drop table
-- DROP TABLE um_database.dbo.ServerSanctions;

CREATE TABLE um_database.dbo.ServerSanctions ( Id bigint IDENTITY(1, 1) NOT NULL,
ServerId bigint NOT NULL,
[Type] tinyint DEFAULT 1 NOT NULL,
Reason nvarchar(500) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
IssuedBy bigint NOT NULL,
IssuedAt datetime2(3) DEFAULT sysutcdatetime() NOT NULL,
ExpiresAt datetime2(3) NULL,
RevokedBy bigint NULL,
RevokedAt datetime2(3) NULL,
RevokeReason nvarchar(500) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
CONSTRAINT PK_ServerSanctions PRIMARY KEY (Id),
CONSTRAINT FK_ServerSanctions_IssuedBy FOREIGN KEY (IssuedBy) REFERENCES um_database.dbo.Users(Id),
CONSTRAINT FK_ServerSanctions_RevokedBy FOREIGN KEY (RevokedBy) REFERENCES um_database.dbo.Users(Id),
CONSTRAINT FK_ServerSanctions_Server FOREIGN KEY (ServerId) REFERENCES um_database.dbo.Servers(Id));

CREATE NONCLUSTERED INDEX IX_ServerSanctions_ActiveByServer ON
um_database.dbo.ServerSanctions ( ServerId ASC )
WHERE
([RevokedAt] IS NULL)
	 WITH ( PAD_INDEX = OFF ,
FILLFACTOR = 100 ,
SORT_IN_TEMPDB = OFF ,
IGNORE_DUP_KEY = OFF ,
STATISTICS_NORECOMPUTE = OFF ,
ONLINE = OFF ,
ALLOW_ROW_LOCKS = ON
,
ALLOW_PAGE_LOCKS = ON
)
	 ON
[PRIMARY ] ;

ALTER TABLE um_database.dbo.ServerSanctions WITH NOCHECK ADD CONSTRAINT CK_ServerSanctions_Type CHECK (([Type] =(1)));
-- um_database.dbo.Tasks definition
-- Drop table
-- DROP TABLE um_database.dbo.Tasks;

CREATE TABLE um_database.dbo.Tasks ( Id bigint IDENTITY(1, 1) NOT NULL,
ServerId bigint NOT NULL,
ChannelId bigint NULL,
CreatorId bigint NOT NULL,
Title nvarchar(200) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
Description nvarchar(MAX) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
Status tinyint DEFAULT 1 NOT NULL,
Priority tinyint DEFAULT 2 NOT NULL,
DueDate datetime2(3) NULL,
ReminderAt datetime2(3) NULL,
CreatedAt datetime2(3) DEFAULT sysutcdatetime() NOT NULL,
UpdatedAt datetime2(3) DEFAULT sysutcdatetime() NOT NULL,
CompletedAt datetime2(3) NULL,
DeletedAt datetime2(3) NULL,
CONSTRAINT PK_Tasks PRIMARY KEY (Id),
CONSTRAINT FK_Tasks_Channels FOREIGN KEY (ChannelId) REFERENCES um_database.dbo.Channels(Id),
CONSTRAINT FK_Tasks_Creator FOREIGN KEY (CreatorId) REFERENCES um_database.dbo.Users(Id),
CONSTRAINT FK_Tasks_Servers FOREIGN KEY (ServerId) REFERENCES um_database.dbo.Servers(Id));

CREATE NONCLUSTERED INDEX IX_Tasks_Server_Status ON
um_database.dbo.Tasks ( ServerId ASC ,
Status ASC )
WHERE
([DeletedAt] IS NULL)
	 WITH ( PAD_INDEX = OFF ,
FILLFACTOR = 100 ,
SORT_IN_TEMPDB = OFF ,
IGNORE_DUP_KEY = OFF ,
STATISTICS_NORECOMPUTE = OFF ,
ONLINE = OFF ,
ALLOW_ROW_LOCKS = ON
,
ALLOW_PAGE_LOCKS = ON
)
	 ON
[PRIMARY ] ;

ALTER TABLE um_database.dbo.Tasks WITH NOCHECK ADD CONSTRAINT CK_Tasks_Status CHECK (([Status] >=(1)
    AND [Status] <=(5)));

ALTER TABLE um_database.dbo.Tasks WITH NOCHECK ADD CONSTRAINT CK_Tasks_Priority CHECK (([Priority] >=(1)
    AND [Priority] <=(4)));
-- um_database.dbo.ChannelMessages definition
-- Drop table
-- DROP TABLE um_database.dbo.ChannelMessages;

CREATE TABLE um_database.dbo.ChannelMessages ( Id bigint IDENTITY(1, 1) NOT NULL,
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
CONSTRAINT FK_ChannelMessages_Channels FOREIGN KEY (ChannelId) REFERENCES um_database.dbo.Channels(Id),
CONSTRAINT FK_ChannelMessages_EncryptionKey FOREIGN KEY (EncryptionKeyId) REFERENCES um_database.dbo.MessageEncryptionKeys(Id),
CONSTRAINT FK_ChannelMessages_ReplyTo FOREIGN KEY (ReplyToMessageId) REFERENCES um_database.dbo.ChannelMessages(Id),
CONSTRAINT FK_ChannelMessages_Sender FOREIGN KEY (SenderId) REFERENCES um_database.dbo.Users(Id));

CREATE UNIQUE CLUSTERED INDEX CIX_ChannelMessages_Channel_Created ON
um_database.dbo.ChannelMessages ( ChannelId ASC ,
CreatedAt DESC ,
Id DESC )  
	 WITH ( PAD_INDEX = OFF ,
FILLFACTOR = 100 ,
SORT_IN_TEMPDB = OFF ,
IGNORE_DUP_KEY = OFF ,
STATISTICS_NORECOMPUTE = OFF ,
ONLINE = OFF ,
ALLOW_ROW_LOCKS = ON
,
ALLOW_PAGE_LOCKS = ON
)
	 ON
[PRIMARY ] ;

ALTER TABLE um_database.dbo.ChannelMessages WITH NOCHECK ADD CONSTRAINT CK_ChannelMessages_Type CHECK (([MessageType] >=(1)
    AND [MessageType] <=(4)));
-- um_database.dbo.ChannelReadStates definition
-- Drop table
-- DROP TABLE um_database.dbo.ChannelReadStates;

CREATE TABLE um_database.dbo.ChannelReadStates ( ChannelId bigint NOT NULL,
UserId bigint NOT NULL,
LastReadMessageId bigint NULL,
UpdatedAt datetime2(3) DEFAULT sysutcdatetime() NOT NULL,
CONSTRAINT PK_ChannelReadStates PRIMARY KEY (ChannelId,
UserId),
CONSTRAINT FK_ChannelReadStates_Channels FOREIGN KEY (ChannelId) REFERENCES um_database.dbo.Channels(Id) ON
DELETE
    CASCADE,
    CONSTRAINT FK_ChannelReadStates_LastRead FOREIGN KEY (LastReadMessageId) REFERENCES um_database.dbo.ChannelMessages(Id),
    CONSTRAINT FK_ChannelReadStates_Users FOREIGN KEY (UserId) REFERENCES um_database.dbo.Users(Id));
-- um_database.dbo.MessageAttachments definition
-- Drop table
-- DROP TABLE um_database.dbo.MessageAttachments;

CREATE TABLE um_database.dbo.MessageAttachments ( Id bigint IDENTITY(1, 1) NOT NULL,
FileId bigint NOT NULL,
ChannelMessageId bigint NULL,
ChatMessageId bigint NULL,
CreatedAt datetime2(3) DEFAULT sysutcdatetime() NOT NULL,
CONSTRAINT PK_MessageAttachments PRIMARY KEY (Id),
CONSTRAINT FK_MessageAttachments_ChannelMessage FOREIGN KEY (ChannelMessageId) REFERENCES um_database.dbo.ChannelMessages(Id) ON
DELETE
    CASCADE,
    CONSTRAINT FK_MessageAttachments_ChatMessage FOREIGN KEY (ChatMessageId) REFERENCES um_database.dbo.ChatMessages(Id) ON
    DELETE
        CASCADE,
        CONSTRAINT FK_MessageAttachments_Files FOREIGN KEY (FileId) REFERENCES um_database.dbo.Files(Id));

CREATE NONCLUSTERED INDEX IX_MessageAttachments_FileId ON
um_database.dbo.MessageAttachments ( FileId ASC )  
	 WITH ( PAD_INDEX = OFF ,
FILLFACTOR = 100 ,
SORT_IN_TEMPDB = OFF ,
IGNORE_DUP_KEY = OFF ,
STATISTICS_NORECOMPUTE = OFF ,
ONLINE = OFF ,
ALLOW_ROW_LOCKS = ON
,
ALLOW_PAGE_LOCKS = ON
)
	 ON
[PRIMARY ] ;

ALTER TABLE um_database.dbo.MessageAttachments WITH NOCHECK ADD CONSTRAINT CK_MessageAttachments_OneTarget CHECK (((case
    when [ChannelMessageId] IS NOT NULL then (1)
    else (0)
end + case
    when [ChatMessageId] IS NOT NULL then (1)
    else (0)
end)=(1)));
-- um_database.dbo.MessageMentions definition
-- Drop table
-- DROP TABLE um_database.dbo.MessageMentions;

CREATE TABLE um_database.dbo.MessageMentions ( Id bigint IDENTITY(1, 1) NOT NULL,
ChannelMessageId bigint NULL,
ChatMessageId bigint NULL,
MentionedUserId bigint NOT NULL,
CreatedAt datetime2(3) DEFAULT sysutcdatetime() NOT NULL,
CONSTRAINT PK_MessageMentions PRIMARY KEY (Id),
CONSTRAINT FK_MessageMentions_ChannelMessage FOREIGN KEY (ChannelMessageId) REFERENCES um_database.dbo.ChannelMessages(Id) ON
DELETE
    CASCADE,
    CONSTRAINT FK_MessageMentions_ChatMessage FOREIGN KEY (ChatMessageId) REFERENCES um_database.dbo.ChatMessages(Id) ON
    DELETE
        CASCADE,
        CONSTRAINT FK_MessageMentions_User FOREIGN KEY (MentionedUserId) REFERENCES um_database.dbo.Users(Id));

CREATE NONCLUSTERED INDEX IX_MessageMentions_MentionedUserId ON
um_database.dbo.MessageMentions ( MentionedUserId ASC ,
CreatedAt DESC )  
	 WITH ( PAD_INDEX = OFF ,
FILLFACTOR = 100 ,
SORT_IN_TEMPDB = OFF ,
IGNORE_DUP_KEY = OFF ,
STATISTICS_NORECOMPUTE = OFF ,
ONLINE = OFF ,
ALLOW_ROW_LOCKS = ON
,
ALLOW_PAGE_LOCKS = ON
)
	 ON
[PRIMARY ] ;

ALTER TABLE um_database.dbo.MessageMentions WITH NOCHECK ADD CONSTRAINT CK_MessageMentions_OneTarget CHECK (((case
    when [ChannelMessageId] IS NOT NULL then (1)
    else (0)
end + case
    when [ChatMessageId] IS NOT NULL then (1)
    else (0)
end)=(1)));
-- um_database.dbo.MessageReactions definition
-- Drop table
-- DROP TABLE um_database.dbo.MessageReactions;

CREATE TABLE um_database.dbo.MessageReactions ( Id bigint IDENTITY(1, 1) NOT NULL,
ChannelMessageId bigint NULL,
ChatMessageId bigint NULL,
UserId bigint NOT NULL,
Emoji nvarchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
CreatedAt datetime2(3) DEFAULT sysutcdatetime() NOT NULL,
CONSTRAINT PK_MessageReactions PRIMARY KEY (Id),
CONSTRAINT UQ_MessageReactions_Channel UNIQUE (ChannelMessageId,
UserId,
Emoji),
CONSTRAINT UQ_MessageReactions_Chat UNIQUE (ChatMessageId,
UserId,
Emoji),
CONSTRAINT FK_MessageReactions_ChannelMessage FOREIGN KEY (ChannelMessageId) REFERENCES um_database.dbo.ChannelMessages(Id) ON
DELETE
    CASCADE,
    CONSTRAINT FK_MessageReactions_ChatMessage FOREIGN KEY (ChatMessageId) REFERENCES um_database.dbo.ChatMessages(Id) ON
    DELETE
        CASCADE,
        CONSTRAINT FK_MessageReactions_Users FOREIGN KEY (UserId) REFERENCES um_database.dbo.Users(Id));

ALTER TABLE um_database.dbo.MessageReactions WITH NOCHECK ADD CONSTRAINT CK_MessageReactions_OneTarget CHECK (((case
    when [ChannelMessageId] IS NOT NULL then (1)
    else (0)
end + case
    when [ChatMessageId] IS NOT NULL then (1)
    else (0)
end)=(1)));
-- um_database.dbo.Reports definition
-- Drop table
-- DROP TABLE um_database.dbo.Reports;

CREATE TABLE um_database.dbo.Reports ( Id bigint IDENTITY(1, 1) NOT NULL,
ReporterId bigint NOT NULL,
ReportedUserId bigint NULL,
ReportedServerId bigint NULL,
ReportedChannelMessageId bigint NULL,
ReportedChatMessageId bigint NULL,
Reason varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
Description nvarchar(1000) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
Status tinyint DEFAULT 1 NOT NULL,
ReviewedBy bigint NULL,
ReviewedAt datetime2(3) NULL,
ResolutionNote nvarchar(1000) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
CreatedAt datetime2(3) DEFAULT sysutcdatetime() NOT NULL,
CONSTRAINT PK_Reports PRIMARY KEY (Id),
CONSTRAINT FK_Reports_ReportedChannelMessage FOREIGN KEY (ReportedChannelMessageId) REFERENCES um_database.dbo.ChannelMessages(Id),
CONSTRAINT FK_Reports_ReportedChatMessage FOREIGN KEY (ReportedChatMessageId) REFERENCES um_database.dbo.ChatMessages(Id),
CONSTRAINT FK_Reports_ReportedServer FOREIGN KEY (ReportedServerId) REFERENCES um_database.dbo.Servers(Id),
CONSTRAINT FK_Reports_ReportedUser FOREIGN KEY (ReportedUserId) REFERENCES um_database.dbo.Users(Id),
CONSTRAINT FK_Reports_Reporter FOREIGN KEY (ReporterId) REFERENCES um_database.dbo.Users(Id),
CONSTRAINT FK_Reports_ReviewedBy FOREIGN KEY (ReviewedBy) REFERENCES um_database.dbo.Users(Id));

CREATE NONCLUSTERED INDEX IX_Reports_Queue ON
um_database.dbo.Reports ( Status ASC ,
CreatedAt DESC )
WHERE
([Status] IN ((1), (2)))
	 WITH ( PAD_INDEX = OFF ,
FILLFACTOR = 100 ,
SORT_IN_TEMPDB = OFF ,
IGNORE_DUP_KEY = OFF ,
STATISTICS_NORECOMPUTE = OFF ,
ONLINE = OFF ,
ALLOW_ROW_LOCKS = ON
,
ALLOW_PAGE_LOCKS = ON
)
	 ON
[PRIMARY ] ;

CREATE NONCLUSTERED INDEX IX_Reports_ReportedServer ON
um_database.dbo.Reports ( ReportedServerId ASC )
WHERE
([ReportedServerId] IS NOT NULL)
	 WITH ( PAD_INDEX = OFF ,
FILLFACTOR = 100 ,
SORT_IN_TEMPDB = OFF ,
IGNORE_DUP_KEY = OFF ,
STATISTICS_NORECOMPUTE = OFF ,
ONLINE = OFF ,
ALLOW_ROW_LOCKS = ON
,
ALLOW_PAGE_LOCKS = ON
)
	 ON
[PRIMARY ] ;

CREATE NONCLUSTERED INDEX IX_Reports_ReportedUser ON
um_database.dbo.Reports ( ReportedUserId ASC )
WHERE
([ReportedUserId] IS NOT NULL)
	 WITH ( PAD_INDEX = OFF ,
FILLFACTOR = 100 ,
SORT_IN_TEMPDB = OFF ,
IGNORE_DUP_KEY = OFF ,
STATISTICS_NORECOMPUTE = OFF ,
ONLINE = OFF ,
ALLOW_ROW_LOCKS = ON
,
ALLOW_PAGE_LOCKS = ON
)
	 ON
[PRIMARY ] ;

ALTER TABLE um_database.dbo.Reports WITH NOCHECK ADD CONSTRAINT CK_Reports_Status CHECK (([Status] >=(1)
    AND [Status] <=(4)));

ALTER TABLE um_database.dbo.Reports WITH NOCHECK ADD CONSTRAINT CK_Reports_OneTarget CHECK (((((case
    when [ReportedUserId] IS NOT NULL then (1)
    else (0)
end + case
    when [ReportedServerId] IS NOT NULL then (1)
    else (0)
end)+ case
    when [ReportedChannelMessageId] IS NOT NULL then (1)
    else (0)
end)+ case
    when [ReportedChatMessageId] IS NOT NULL then (1)
    else (0)
end)=(1)));
-- um_database.dbo.RolePermissions definition
-- Drop table
-- DROP TABLE um_database.dbo.RolePermissions;

CREATE TABLE um_database.dbo.RolePermissions ( ServerRoleId bigint NOT NULL,
PermissionId int NOT NULL,
CONSTRAINT PK_RolePermissions PRIMARY KEY (ServerRoleId,
PermissionId),
CONSTRAINT FK_RolePermissions_Permissions FOREIGN KEY (PermissionId) REFERENCES um_database.dbo.Permissions(Id),
CONSTRAINT FK_RolePermissions_Roles FOREIGN KEY (ServerRoleId) REFERENCES um_database.dbo.ServerRoles(Id) ON
DELETE
    CASCADE);
-- um_database.dbo.TaskAssignees definition
-- Drop table
-- DROP TABLE um_database.dbo.TaskAssignees;

CREATE TABLE um_database.dbo.TaskAssignees ( Id bigint IDENTITY(1, 1) NOT NULL,
TaskId bigint NOT NULL,
UserId bigint NOT NULL,
AssignedBy bigint NOT NULL,
AssignedAt datetime2(3) DEFAULT sysutcdatetime() NOT NULL,
CONSTRAINT PK_TaskAssignees PRIMARY KEY (Id),
CONSTRAINT UQ_TaskAssignees_Task_User UNIQUE (TaskId,
UserId),
CONSTRAINT FK_TaskAssignees_AssignedBy FOREIGN KEY (AssignedBy) REFERENCES um_database.dbo.Users(Id),
CONSTRAINT FK_TaskAssignees_Tasks FOREIGN KEY (TaskId) REFERENCES um_database.dbo.Tasks(Id) ON
DELETE
    CASCADE,
    CONSTRAINT FK_TaskAssignees_Users FOREIGN KEY (UserId) REFERENCES um_database.dbo.Users(Id));

CREATE NONCLUSTERED INDEX IX_TaskAssignees_UserId ON
um_database.dbo.TaskAssignees ( UserId ASC )  
	 INCLUDE (TaskId) 
	 WITH ( PAD_INDEX = OFF ,
FILLFACTOR = 100 ,
SORT_IN_TEMPDB = OFF ,
IGNORE_DUP_KEY = OFF ,
STATISTICS_NORECOMPUTE = OFF ,
ONLINE = OFF ,
ALLOW_ROW_LOCKS = ON
,
ALLOW_PAGE_LOCKS = ON
)
	 ON
[PRIMARY ] ;
