CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

CREATE TABLE "AspNetRoles" (
    "Id" text NOT NULL,
    "ConcurrencyStamp" text NULL,
    "Name" character varying(256) NULL,
    "NormalizedName" character varying(256) NULL,
    CONSTRAINT "PK_AspNetRoles" PRIMARY KEY ("Id")
);

CREATE TABLE "AspNetUsers" (
    "Id" text NOT NULL,
    "AccessFailedCount" integer NOT NULL,
    "ConcurrencyStamp" text NULL,
    "Discriminator" text NOT NULL,
    "Email" character varying(256) NULL,
    "EmailConfirmed" boolean NOT NULL,
    "LockoutEnabled" boolean NOT NULL,
    "LockoutEnd" timestamp with time zone NULL,
    "NormalizedEmail" character varying(256) NULL,
    "NormalizedUserName" character varying(256) NULL,
    "PasswordHash" text NULL,
    "PhoneNumber" text NULL,
    "PhoneNumberConfirmed" boolean NOT NULL,
    "SecurityStamp" text NULL,
    "TwoFactorEnabled" boolean NOT NULL,
    "UserName" character varying(256) NULL,
    "FirstName" text NULL,
    "LastName" text NULL,
    CONSTRAINT "PK_AspNetUsers" PRIMARY KEY ("Id")
);

CREATE TABLE "AspNetRoleClaims" (
    "Id" serial NOT NULL,
    "ClaimType" text NULL,
    "ClaimValue" text NULL,
    "RoleId" text NOT NULL,
    CONSTRAINT "PK_AspNetRoleClaims" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_AspNetRoleClaims_AspNetRoles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "AspNetRoles" ("Id") ON DELETE CASCADE
);

CREATE TABLE "AspNetUserClaims" (
    "Id" serial NOT NULL,
    "ClaimType" text NULL,
    "ClaimValue" text NULL,
    "UserId" text NOT NULL,
    CONSTRAINT "PK_AspNetUserClaims" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_AspNetUserClaims_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
);

CREATE TABLE "AspNetUserLogins" (
    "LoginProvider" text NOT NULL,
    "ProviderKey" text NOT NULL,
    "ProviderDisplayName" text NULL,
    "UserId" text NOT NULL,
    CONSTRAINT "PK_AspNetUserLogins" PRIMARY KEY ("LoginProvider", "ProviderKey"),
    CONSTRAINT "FK_AspNetUserLogins_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
);

CREATE TABLE "AspNetUserRoles" (
    "UserId" text NOT NULL,
    "RoleId" text NOT NULL,
    CONSTRAINT "PK_AspNetUserRoles" PRIMARY KEY ("UserId", "RoleId"),
    CONSTRAINT "FK_AspNetUserRoles_AspNetRoles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "AspNetRoles" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_AspNetUserRoles_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
);

CREATE TABLE "AspNetUserTokens" (
    "UserId" text NOT NULL,
    "LoginProvider" text NOT NULL,
    "Name" text NOT NULL,
    "Value" text NULL,
    CONSTRAINT "PK_AspNetUserTokens" PRIMARY KEY ("UserId", "LoginProvider", "Name"),
    CONSTRAINT "FK_AspNetUserTokens_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_AspNetRoleClaims_RoleId" ON "AspNetRoleClaims" ("RoleId");

CREATE UNIQUE INDEX "RoleNameIndex" ON "AspNetRoles" ("NormalizedName");

CREATE INDEX "IX_AspNetUserClaims_UserId" ON "AspNetUserClaims" ("UserId");

CREATE INDEX "IX_AspNetUserLogins_UserId" ON "AspNetUserLogins" ("UserId");

CREATE INDEX "IX_AspNetUserRoles_RoleId" ON "AspNetUserRoles" ("RoleId");

CREATE INDEX "EmailIndex" ON "AspNetUsers" ("NormalizedEmail");

CREATE UNIQUE INDEX "UserNameIndex" ON "AspNetUsers" ("NormalizedUserName");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20180306105643_CreateIdentityUser', '3.1.3');

CREATE TABLE "AuditLogs" (
    "Id" bigserial NOT NULL,
    "AuthorId" text NULL,
    "Route" text NOT NULL,
    "Timestamp" timestamp without time zone NOT NULL,
    CONSTRAINT "PK_AuditLogs" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_AuditLogs_AspNetUsers_AuthorId" FOREIGN KEY ("AuthorId") REFERENCES "AspNetUsers" ("Id") ON DELETE RESTRICT
);

CREATE INDEX "IX_AuditLogs_AuthorId" ON "AuditLogs" ("AuthorId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20180308104550_CreateAuditLog', '3.1.3');

ALTER TABLE "AspNetUsers" DROP COLUMN "Discriminator";

ALTER TABLE "AspNetRoles" ADD "Description" text NULL;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20180309192000_AddIdentityRole', '3.1.3');

CREATE TABLE "PasswordResetTokens" (
    "UserToken" text NOT NULL,
    "IdentityToken" text NULL,
    CONSTRAINT "PK_PasswordResetTokens" PRIMARY KEY ("UserToken")
);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20180312130835_CreatePasswordResetToken', '3.1.3');

CREATE TABLE "ChangeEmailTokens" (
    "IdentityToken" text NOT NULL,
    "Email" text NULL,
    "UserToken" text NULL,
    CONSTRAINT "PK_ChangeEmailTokens" PRIMARY KEY ("IdentityToken")
);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20180319171541_AddChangeEmailToken', '3.1.3');

ALTER TABLE "AspNetUserTokens" ADD "Discriminator" text NOT NULL DEFAULT '';

ALTER TABLE "AspNetUserTokens" ADD "CreatedAt" timestamp without time zone NULL;

ALTER TABLE "AspNetUserTokens" ADD "ExpiresAt" timestamp without time zone NULL;

ALTER TABLE "AspNetUserTokens" ADD "Valid" boolean NULL;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20180321085901_AddSensateUserToken', '3.1.3');

ALTER TABLE "AspNetUserTokens" DROP CONSTRAINT "PK_AspNetUserTokens";

ALTER TABLE "AspNetUserTokens" ALTER COLUMN "Value" TYPE text;
ALTER TABLE "AspNetUserTokens" ALTER COLUMN "Value" SET NOT NULL;
ALTER TABLE "AspNetUserTokens" ALTER COLUMN "Value" DROP DEFAULT;

ALTER TABLE "AspNetUserTokens" ADD CONSTRAINT "PK_AspNetUserTokens" PRIMARY KEY ("Value", "UserId");

ALTER TABLE "AspNetUserTokens" ADD CONSTRAINT "AK_AspNetUserTokens_UserId_LoginProvider_Name" UNIQUE ("UserId", "LoginProvider", "Name");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20180321092407_AlterSensateUserTokenPK', '3.1.3');

ALTER TABLE "AspNetUserTokens" DROP CONSTRAINT "PK_AspNetUserTokens";

ALTER TABLE "AspNetUserTokens" DROP CONSTRAINT "AK_AspNetUserTokens_UserId_LoginProvider_Name";

ALTER TABLE "AspNetUserTokens" DROP COLUMN "Discriminator";

ALTER TABLE "AspNetUserTokens" DROP COLUMN "CreatedAt";

ALTER TABLE "AspNetUserTokens" DROP COLUMN "ExpiresAt";

ALTER TABLE "AspNetUserTokens" DROP COLUMN "Valid";

ALTER TABLE "AspNetUserTokens" ALTER COLUMN "Value" TYPE text;
ALTER TABLE "AspNetUserTokens" ALTER COLUMN "Value" DROP NOT NULL;
ALTER TABLE "AspNetUserTokens" ALTER COLUMN "Value" DROP DEFAULT;

ALTER TABLE "AspNetUserTokens" ADD CONSTRAINT "PK_AspNetUserTokens" PRIMARY KEY ("UserId", "LoginProvider", "Name");

CREATE TABLE "AspNetAuthTokens" (
    "UserId" text NOT NULL,
    "Value" text NOT NULL,
    "CreatedAt" timestamp without time zone NOT NULL,
    "ExpiresAt" timestamp without time zone NOT NULL,
    "LoginProvider" text NULL,
    "Valid" boolean NOT NULL,
    CONSTRAINT "PK_AspNetAuthTokens" PRIMARY KEY ("UserId", "Value"),
    CONSTRAINT "FK_AspNetAuthTokens_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20180321094920_AlterSensateUserTokenTableName', '3.1.3');

ALTER TABLE "AuditLogs" ADD "Method" integer NOT NULL DEFAULT 0;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20180324232304_AddMethodToAuditLog', '3.1.3');

ALTER TABLE "AuditLogs" DROP CONSTRAINT "FK_AuditLogs_AspNetUsers_AuthorId";

ALTER TABLE "AuditLogs" DROP CONSTRAINT "PK_AuditLogs";

ALTER TABLE "AuditLogs" RENAME TO "AspNetAuditLogs";

ALTER INDEX "IX_AuditLogs_AuthorId" RENAME TO "IX_AspNetAuditLogs_AuthorId";

ALTER TABLE "AspNetAuditLogs" ADD CONSTRAINT "PK_AspNetAuditLogs" PRIMARY KEY ("Id");

ALTER TABLE "AspNetAuditLogs" ADD CONSTRAINT "FK_AspNetAuditLogs_AspNetUsers_AuthorId" FOREIGN KEY ("AuthorId") REFERENCES "AspNetUsers" ("Id") ON DELETE RESTRICT;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20180324232847_RenameAuditLogToAspNetAuditLogs', '3.1.3');

ALTER TABLE "AspNetAuditLogs" ADD "Address" inet NOT NULL;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20180325192036_AddRemoteAddressToAuditLog', '3.1.3');

ALTER TABLE "AspNetUsers" ALTER COLUMN "LastName" TYPE text;
ALTER TABLE "AspNetUsers" ALTER COLUMN "LastName" SET NOT NULL;
ALTER TABLE "AspNetUsers" ALTER COLUMN "LastName" DROP DEFAULT;

ALTER TABLE "AspNetUsers" ALTER COLUMN "FirstName" TYPE text;
ALTER TABLE "AspNetUsers" ALTER COLUMN "FirstName" SET NOT NULL;
ALTER TABLE "AspNetUsers" ALTER COLUMN "FirstName" DROP DEFAULT;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20180416111034_FirstLastNameNotNullable', '3.1.3');

CREATE TABLE "ChangePhoneNumberTokens" (
    "IdentityToken" text NOT NULL,
    "PhoneNumber" text NULL,
    "UserToken" text NOT NULL,
    CONSTRAINT "PK_ChangePhoneNumberTokens" PRIMARY KEY ("IdentityToken"),
    CONSTRAINT "AlternateKey_UserToken" UNIQUE ("UserToken")
);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20180927135439_AddChangePhoneNumberTokenModel', '3.1.3');

ALTER TABLE "ChangePhoneNumberTokens" DROP CONSTRAINT "PK_ChangePhoneNumberTokens";

ALTER TABLE "ChangePhoneNumberTokens" ALTER COLUMN "PhoneNumber" TYPE text;
ALTER TABLE "ChangePhoneNumberTokens" ALTER COLUMN "PhoneNumber" SET NOT NULL;
ALTER TABLE "ChangePhoneNumberTokens" ALTER COLUMN "PhoneNumber" DROP DEFAULT;

ALTER TABLE "ChangePhoneNumberTokens" ADD "Timestamp" timestamp without time zone NOT NULL DEFAULT TIMESTAMP '0001-01-01 00:00:00';

ALTER TABLE "ChangePhoneNumberTokens" ADD "UserId" text NULL;

ALTER TABLE "ChangePhoneNumberTokens" ADD CONSTRAINT "PK_ChangePhoneNumberTokens" PRIMARY KEY ("IdentityToken", "PhoneNumber");

CREATE INDEX "IX_ChangePhoneNumberTokens_UserId" ON "ChangePhoneNumberTokens" ("UserId");

ALTER TABLE "ChangePhoneNumberTokens" ADD CONSTRAINT "FK_ChangePhoneNumberTokens_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE RESTRICT;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20181017081823_AddUserAndTimestampToChangePhoneNumberTokens', '3.1.3');

ALTER TABLE "AspNetUsers" ADD "UnconfirmedPhoneNumber" text NULL;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20181017155214_AddUnconfirmedPhoneNumberToSensateUser', '3.1.3');

ALTER TABLE "AspNetUsers" ADD "RegisteredAt" timestamp without time zone NOT NULL DEFAULT TIMESTAMP '0001-01-01 00:00:00';

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20181104162220_AddRegistrationDateToUsers', '3.1.3');

DROP TABLE "AspNetAuditLogs";

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20190304204245_RemoveAuditLogTable', '3.1.3');

ALTER TABLE "ChangePhoneNumberTokens" DROP CONSTRAINT "FK_ChangePhoneNumberTokens_AspNetUsers_UserId";

ALTER TABLE "ChangePhoneNumberTokens" DROP CONSTRAINT "PK_ChangePhoneNumberTokens";

ALTER TABLE "ChangeEmailTokens" DROP CONSTRAINT "PK_ChangeEmailTokens";

ALTER TABLE "ChangePhoneNumberTokens" RENAME TO "AspNetPhoneNumberTokens";

ALTER TABLE "ChangeEmailTokens" RENAME TO "AspNetEmailTokens";

ALTER INDEX "IX_ChangePhoneNumberTokens_UserId" RENAME TO "IX_AspNetPhoneNumberTokens_UserId";

ALTER TABLE "AspNetPhoneNumberTokens" ADD CONSTRAINT "PK_AspNetPhoneNumberTokens" PRIMARY KEY ("IdentityToken", "PhoneNumber");

ALTER TABLE "AspNetEmailTokens" ADD CONSTRAINT "PK_AspNetEmailTokens" PRIMARY KEY ("IdentityToken");

ALTER TABLE "AspNetPhoneNumberTokens" ADD CONSTRAINT "FK_AspNetPhoneNumberTokens_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE RESTRICT;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20190305131708_StandardizeTableNames', '3.1.3');

ALTER TABLE "PasswordResetTokens" DROP CONSTRAINT "PK_PasswordResetTokens";

ALTER TABLE "PasswordResetTokens" RENAME TO "AspNetPasswordResetTokens";

ALTER TABLE "AspNetPasswordResetTokens" ADD CONSTRAINT "PK_AspNetPasswordResetTokens" PRIMARY KEY ("UserToken");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20190305154458_StandardizePasswordResetTokenTable', '3.1.3');

CREATE TABLE "AspNetApiKeys" (
    "Id" text NOT NULL,
    "UserId" text NOT NULL,
    "ApiKey" text NOT NULL,
    "Revoked" boolean NOT NULL,
    "CreatedOn" timestamp without time zone NOT NULL,
    "Type" integer NOT NULL,
    CONSTRAINT "PK_AspNetApiKeys" PRIMARY KEY ("Id"),
    CONSTRAINT "AK_AspNetApiKeys_ApiKey" UNIQUE ("ApiKey"),
    CONSTRAINT "FK_AspNetApiKeys_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_AspNetApiKeys_UserId" ON "AspNetApiKeys" ("UserId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20190313090012_AddApiKeyModel', '3.1.3');

ALTER TABLE "AspNetApiKeys" DROP CONSTRAINT "AK_AspNetApiKeys_ApiKey";

CREATE UNIQUE INDEX "IX_AspNetApiKeys_ApiKey" ON "AspNetApiKeys" ("ApiKey");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20190313133755_SetApiKeyToUnique', '3.1.3');

ALTER TABLE "AspNetApiKeys" ADD "Name" text NOT NULL DEFAULT '';

ALTER TABLE "AspNetApiKeys" ADD "ReadOnly" boolean NOT NULL DEFAULT FALSE;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20190313153152_AddNameAndReadOnlyToApiKey', '3.1.3');

CREATE SEQUENCE "Id_sequence" START WITH 1 INCREMENT BY 1 NO MINVALUE NO MAXVALUE NO CYCLE;

CREATE TABLE "AuditLogs" (
    "Id" bigint NOT NULL DEFAULT (nextval('"Id_sequence"')),
    "Route" text NOT NULL,
    "Method" integer NOT NULL,
    "Address" inet NOT NULL,
    "AuthorId" text NULL,
    "Timestamp" timestamp without time zone NOT NULL,
    CONSTRAINT "PK_AuditLogs" PRIMARY KEY ("Id")
);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20190911123858_CreateAuditLogModel', '3.1.3');

ALTER TABLE "AuditLogs" ALTER COLUMN "Id" DROP DEFAULT

ALTER TABLE "AuditLogs" ALTER COLUMN "Id" TYPE bigint;
ALTER TABLE "AuditLogs" ALTER COLUMN "Id" SET NOT NULL;
ALTER TABLE "AuditLogs" ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY;

DROP SEQUENCE "Id_sequence";

ALTER TABLE "AspNetRoleClaims" ALTER COLUMN "Id" DROP DEFAULT

ALTER TABLE "AspNetRoleClaims" ALTER COLUMN "Id" SET NOT NULL

ALTER SEQUENCE "AspNetRoleClaims_Id_seq" RENAME TO "AspNetRoleClaims_Id_old_seq"

ALTER TABLE "AspNetRoleClaims" ALTER COLUMN "Id" DROP DEFAULT

ALTER TABLE "AspNetRoleClaims" ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY

DROP SEQUENCE "AspNetRoleClaims_Id_old_seq"

ALTER TABLE "AspNetUserClaims" ALTER COLUMN "Id" DROP DEFAULT

ALTER TABLE "AspNetUserClaims" ALTER COLUMN "Id" SET NOT NULL

ALTER SEQUENCE "AspNetUserClaims_Id_seq" RENAME TO "AspNetUserClaims_Id_old_seq"

ALTER TABLE "AspNetUserClaims" ALTER COLUMN "Id" DROP DEFAULT

ALTER TABLE "AspNetUserClaims" ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY

DROP SEQUENCE "AspNetUserClaims_Id_old_seq"

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20200115124443_MigrateToIdentityColumns', '3.1.3');

ALTER TABLE "AspNetApiKeys" DROP CONSTRAINT "FK_AspNetApiKeys_AspNetUsers_UserId";

ALTER TABLE "AspNetAuthTokens" DROP CONSTRAINT "FK_AspNetAuthTokens_AspNetUsers_UserId";

ALTER TABLE "AspNetPhoneNumberTokens" DROP CONSTRAINT "FK_AspNetPhoneNumberTokens_AspNetUsers_UserId";

ALTER TABLE "AspNetRoleClaims" DROP CONSTRAINT "FK_AspNetRoleClaims_AspNetRoles_RoleId";

ALTER TABLE "AspNetUserClaims" DROP CONSTRAINT "FK_AspNetUserClaims_AspNetUsers_UserId";

ALTER TABLE "AspNetUserLogins" DROP CONSTRAINT "FK_AspNetUserLogins_AspNetUsers_UserId";

ALTER TABLE "AspNetUserRoles" DROP CONSTRAINT "FK_AspNetUserRoles_AspNetRoles_RoleId";

ALTER TABLE "AspNetUserRoles" DROP CONSTRAINT "FK_AspNetUserRoles_AspNetUsers_UserId";

ALTER TABLE "AspNetUserTokens" DROP CONSTRAINT "FK_AspNetUserTokens_AspNetUsers_UserId";

ALTER TABLE "AspNetUserTokens" DROP CONSTRAINT "PK_AspNetUserTokens";

ALTER TABLE "AspNetUsers" DROP CONSTRAINT "PK_AspNetUsers";

ALTER TABLE "AspNetUserRoles" DROP CONSTRAINT "PK_AspNetUserRoles";

ALTER TABLE "AspNetUserLogins" DROP CONSTRAINT "PK_AspNetUserLogins";

ALTER TABLE "AspNetUserClaims" DROP CONSTRAINT "PK_AspNetUserClaims";

ALTER TABLE "AspNetRoles" DROP CONSTRAINT "PK_AspNetRoles";

ALTER TABLE "AspNetRoleClaims" DROP CONSTRAINT "PK_AspNetRoleClaims";

ALTER TABLE "AspNetPhoneNumberTokens" DROP CONSTRAINT "PK_AspNetPhoneNumberTokens";

ALTER TABLE "AspNetPasswordResetTokens" DROP CONSTRAINT "PK_AspNetPasswordResetTokens";

ALTER TABLE "AspNetEmailTokens" DROP CONSTRAINT "PK_AspNetEmailTokens";

ALTER TABLE "AspNetAuthTokens" DROP CONSTRAINT "PK_AspNetAuthTokens";

ALTER TABLE "AspNetApiKeys" DROP CONSTRAINT "PK_AspNetApiKeys";

ALTER TABLE "AspNetUserTokens" RENAME TO "UserTokens";

ALTER TABLE "AspNetUsers" RENAME TO "Users";

ALTER TABLE "AspNetUserRoles" RENAME TO "UserRoles";

ALTER TABLE "AspNetUserLogins" RENAME TO "UserLogins";

ALTER TABLE "AspNetUserClaims" RENAME TO "UserClaims";

ALTER TABLE "AspNetRoles" RENAME TO "Roles";

ALTER TABLE "AspNetRoleClaims" RENAME TO "RoleClaims";

ALTER TABLE "AspNetPhoneNumberTokens" RENAME TO "PhoneNumberTokens";

ALTER TABLE "AspNetPasswordResetTokens" RENAME TO "PasswordResetTokens";

ALTER TABLE "AspNetEmailTokens" RENAME TO "EmailTokens";

ALTER TABLE "AspNetAuthTokens" RENAME TO "AuthTokens";

ALTER TABLE "AspNetApiKeys" RENAME TO "ApiKeys";

ALTER INDEX "IX_AspNetUserRoles_RoleId" RENAME TO "IX_UserRoles_RoleId";

ALTER INDEX "IX_AspNetUserLogins_UserId" RENAME TO "IX_UserLogins_UserId";

ALTER INDEX "IX_AspNetUserClaims_UserId" RENAME TO "IX_UserClaims_UserId";

ALTER INDEX "IX_AspNetRoleClaims_RoleId" RENAME TO "IX_RoleClaims_RoleId";

ALTER INDEX "IX_AspNetPhoneNumberTokens_UserId" RENAME TO "IX_PhoneNumberTokens_UserId";

ALTER INDEX "IX_AspNetApiKeys_UserId" RENAME TO "IX_ApiKeys_UserId";

ALTER INDEX "IX_AspNetApiKeys_ApiKey" RENAME TO "IX_ApiKeys_ApiKey";

ALTER TABLE "UserTokens" ADD CONSTRAINT "PK_UserTokens" PRIMARY KEY ("UserId", "LoginProvider", "Name");

ALTER TABLE "Users" ADD CONSTRAINT "PK_Users" PRIMARY KEY ("Id");

ALTER TABLE "UserRoles" ADD CONSTRAINT "PK_UserRoles" PRIMARY KEY ("UserId", "RoleId");

ALTER TABLE "UserLogins" ADD CONSTRAINT "PK_UserLogins" PRIMARY KEY ("LoginProvider", "ProviderKey");

ALTER TABLE "UserClaims" ADD CONSTRAINT "PK_UserClaims" PRIMARY KEY ("Id");

ALTER TABLE "Roles" ADD CONSTRAINT "PK_Roles" PRIMARY KEY ("Id");

ALTER TABLE "RoleClaims" ADD CONSTRAINT "PK_RoleClaims" PRIMARY KEY ("Id");

ALTER TABLE "PhoneNumberTokens" ADD CONSTRAINT "PK_PhoneNumberTokens" PRIMARY KEY ("IdentityToken", "PhoneNumber");

ALTER TABLE "PasswordResetTokens" ADD CONSTRAINT "PK_PasswordResetTokens" PRIMARY KEY ("UserToken");

ALTER TABLE "EmailTokens" ADD CONSTRAINT "PK_EmailTokens" PRIMARY KEY ("IdentityToken");

ALTER TABLE "AuthTokens" ADD CONSTRAINT "PK_AuthTokens" PRIMARY KEY ("UserId", "Value");

ALTER TABLE "ApiKeys" ADD CONSTRAINT "PK_ApiKeys" PRIMARY KEY ("Id");

ALTER TABLE "ApiKeys" ADD CONSTRAINT "FK_ApiKeys_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE;

ALTER TABLE "AuthTokens" ADD CONSTRAINT "FK_AuthTokens_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE;

ALTER TABLE "PhoneNumberTokens" ADD CONSTRAINT "FK_PhoneNumberTokens_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE RESTRICT;

ALTER TABLE "RoleClaims" ADD CONSTRAINT "FK_RoleClaims_Roles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "Roles" ("Id") ON DELETE CASCADE;

ALTER TABLE "UserClaims" ADD CONSTRAINT "FK_UserClaims_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE;

ALTER TABLE "UserLogins" ADD CONSTRAINT "FK_UserLogins_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE;

ALTER TABLE "UserRoles" ADD CONSTRAINT "FK_UserRoles_Roles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "Roles" ("Id") ON DELETE CASCADE;

ALTER TABLE "UserRoles" ADD CONSTRAINT "FK_UserRoles_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE;

ALTER TABLE "UserTokens" ADD CONSTRAINT "FK_UserTokens_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20200116182252_UpdateIdentityTableNames', '3.1.3');

CREATE INDEX "IX_AuditLogs_AuthorId" ON "AuditLogs" ("AuthorId");

CREATE INDEX "IX_AuditLogs_Method" ON "AuditLogs" ("Method");

ALTER TABLE "AuditLogs" ADD CONSTRAINT "FK_AuditLogs_Users_AuthorId" FOREIGN KEY ("AuthorId") REFERENCES "Users" ("Id") ON DELETE RESTRICT;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20200123111807_AddForeignKeyToAuditLogs', '3.1.3');

CREATE TABLE "Triggers" (
    "Id" bigint NOT NULL GENERATED BY DEFAULT AS IDENTITY,
    "KeyValue" text NOT NULL,
    "LowerEdge" numeric NULL,
    "UpperEdige" numeric NULL,
    "SensorId" character varying(24) NOT NULL,
    CONSTRAINT "PK_Triggers" PRIMARY KEY ("Id")
);

CREATE INDEX "IX_Triggers_SensorId" ON "Triggers" ("SensorId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20200123112621_CreateTriggerTable', '3.1.3');

ALTER TABLE "Triggers" DROP COLUMN "UpperEdige";

ALTER TABLE "Triggers" ADD "UpperEdge" numeric NULL;

CREATE TABLE "TriggerActions" (
    "TriggerId" bigint NOT NULL,
    "Channel" integer NOT NULL,
    CONSTRAINT "PK_TriggerActions" PRIMARY KEY ("TriggerId", "Channel"),
    CONSTRAINT "FK_TriggerActions_Triggers_TriggerId" FOREIGN KEY ("TriggerId") REFERENCES "Triggers" ("Id") ON DELETE CASCADE
);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20200123163640_CreateTriggerActionsTable', '3.1.3');

ALTER TABLE "Triggers" ADD "LastTriggered" timestamp without time zone NOT NULL DEFAULT TIMESTAMP '0001-01-01 00:00:00';

CREATE INDEX "IX_Triggers_LastTriggered" ON "Triggers" ("LastTriggered");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20200123175614_AddTimestampToTrigger', '3.1.3');

DROP INDEX "IX_Triggers_LastTriggered";

ALTER TABLE "Triggers" DROP COLUMN "LastTriggered";

ALTER TABLE "Triggers" ADD "Message" character varying(300) NOT NULL DEFAULT '';

CREATE TABLE "TriggerInvocations" (
    "Id" bigint NOT NULL GENERATED BY DEFAULT AS IDENTITY,
    "MeasurementBucketId" character varying(24) NULL,
    "MeasurementId" integer NOT NULL,
    "TriggerId" bigint NOT NULL,
    "Timestamp" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_TriggerInvocations" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_TriggerInvocations_Triggers_TriggerId" FOREIGN KEY ("TriggerId") REFERENCES "Triggers" ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_TriggerInvocations_TriggerId" ON "TriggerInvocations" ("TriggerId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20200125001637_AddTriggerInvocationsTable', '3.1.3');

ALTER TABLE "TriggerInvocations" ALTER COLUMN "MeasurementBucketId" TYPE character varying(24);
ALTER TABLE "TriggerInvocations" ALTER COLUMN "MeasurementBucketId" SET NOT NULL;
ALTER TABLE "TriggerInvocations" ALTER COLUMN "MeasurementBucketId" DROP DEFAULT;

ALTER TABLE "TriggerInvocations" ADD CONSTRAINT "AK_TriggerInvocations_MeasurementBucketId_MeasurementId_Trigge~" UNIQUE ("MeasurementBucketId", "MeasurementId", "TriggerId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20200125163420_AddAlternateKeyToTriggerInvocations', '3.1.3');

CREATE TABLE "Blobs" (
    "Id" bigint NOT NULL GENERATED BY DEFAULT AS IDENTITY,
    "SensorId" character varying(24) NOT NULL,
    "FileName" text NOT NULL,
    "Path" text NOT NULL,
    "StorageType" integer NOT NULL,
    CONSTRAINT "PK_Blobs" PRIMARY KEY ("Id")
);

CREATE INDEX "IX_Blobs_SensorId" ON "Blobs" ("SensorId");

CREATE UNIQUE INDEX "IX_Blobs_SensorId_FileName" ON "Blobs" ("SensorId", "FileName");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20200130161811_CreateBlobsTable', '3.1.3');

ALTER TABLE "TriggerActions" ADD "Target" text NULL;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20200203161025_AddTargetColumnToTriggerActions', '3.1.3');

ALTER TABLE "Triggers" DROP COLUMN "Message";

ALTER TABLE "TriggerActions" ADD "Message" character varying(255) NOT NULL DEFAULT '';

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20200214101035_MoveMessageFromTriggersToTriggerActions', '3.1.3');

ALTER TABLE "TriggerInvocations" DROP CONSTRAINT "AK_TriggerInvocations_MeasurementBucketId_MeasurementId_Trigge~";

ALTER TABLE "TriggerInvocations" DROP COLUMN "MeasurementBucketId";

ALTER TABLE "TriggerInvocations" DROP COLUMN "MeasurementId";

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20200214142510_RemoveMeasurementIndexFromTriggerInvocations', '3.1.3');

CREATE TABLE "SensorLinks" (
    "SensorId" text NOT NULL,
    "UserId" text NOT NULL,
    CONSTRAINT "PK_SensorLinks" PRIMARY KEY ("UserId", "SensorId")
);

CREATE INDEX "IX_SensorLinks_UserId" ON "SensorLinks" ("UserId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20200306135250_AddSensorLinksTable', '3.1.3');

ALTER TABLE "Triggers" ADD "FormalLanguage" text NULL;

ALTER TABLE "Triggers" ADD "Type" integer NOT NULL DEFAULT 0;

CREATE INDEX "IX_Triggers_Type" ON "Triggers" ("Type");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20200310225133_AddFormalLanguageToTriggersTable', '3.1.3');

ALTER TABLE "AuditLogs" DROP CONSTRAINT "FK_AuditLogs_Users_AuthorId";

ALTER TABLE "PhoneNumberTokens" DROP CONSTRAINT "FK_PhoneNumberTokens_Users_UserId";

ALTER TABLE "PhoneNumberTokens" ALTER COLUMN "UserId" TYPE text;
ALTER TABLE "PhoneNumberTokens" ALTER COLUMN "UserId" SET NOT NULL;
ALTER TABLE "PhoneNumberTokens" ALTER COLUMN "UserId" DROP DEFAULT;

ALTER TABLE "AuditLogs" ADD CONSTRAINT "FK_AuditLogs_Users_AuthorId" FOREIGN KEY ("AuthorId") REFERENCES "Users" ("Id") ON DELETE CASCADE;

ALTER TABLE "PhoneNumberTokens" ADD CONSTRAINT "FK_PhoneNumberTokens_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE;

ALTER TABLE "SensorLinks" ADD CONSTRAINT "FK_SensorLinks_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20200324234559_UpdateOnDeleteRules', '3.1.3');

ALTER TABLE "Blobs" ADD "Timestamp" timestamp without time zone NOT NULL DEFAULT TIMESTAMP '0001-01-01 00:00:00';

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20200325222614_AddTimestampToBlobs', '3.1.3');

CREATE TABLE "DataProtectionKeys" (
    "Id" integer NOT NULL GENERATED BY DEFAULT AS IDENTITY,
    "FriendlyName" text NULL,
    "Xml" text NULL,
    CONSTRAINT "PK_DataProtectionKeys" PRIMARY KEY ("Id")
);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20200326181831_AddDataProtectionKeyTable', '3.1.3');

ALTER TABLE "Blobs" ADD "FileSize" bigint NOT NULL DEFAULT 0;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20200402131550_AddFileSizeToBlobsTable', '3.1.3');

ALTER TABLE "Users" ADD "BillingLockout" boolean NOT NULL DEFAULT FALSE;

CREATE INDEX "IX_Users_BillingLockout" ON "Users" ("BillingLockout");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20200408213239_AddBillingLockoutToUsers', '3.1.3');

ALTER TABLE "ApiKeys" ADD "RequestCount" bigint NOT NULL DEFAULT 0;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20200410172420_AddRequestCountToApiKeyTable', '3.1.3');

