CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);


DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20180306105643_CreateIdentityUser') THEN
    CREATE TABLE "AspNetRoles" (
        "Id" text NOT NULL,
        "ConcurrencyStamp" text NULL,
        "Name" character varying(256) NULL,
        "NormalizedName" character varying(256) NULL,
        CONSTRAINT "PK_AspNetRoles" PRIMARY KEY ("Id")
    );
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20180306105643_CreateIdentityUser') THEN
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
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20180306105643_CreateIdentityUser') THEN
    CREATE TABLE "AspNetRoleClaims" (
        "Id" serial NOT NULL,
        "ClaimType" text NULL,
        "ClaimValue" text NULL,
        "RoleId" text NOT NULL,
        CONSTRAINT "PK_AspNetRoleClaims" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_AspNetRoleClaims_AspNetRoles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "AspNetRoles" ("Id") ON DELETE CASCADE
    );
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20180306105643_CreateIdentityUser') THEN
    CREATE TABLE "AspNetUserClaims" (
        "Id" serial NOT NULL,
        "ClaimType" text NULL,
        "ClaimValue" text NULL,
        "UserId" text NOT NULL,
        CONSTRAINT "PK_AspNetUserClaims" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_AspNetUserClaims_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
    );
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20180306105643_CreateIdentityUser') THEN
    CREATE TABLE "AspNetUserLogins" (
        "LoginProvider" text NOT NULL,
        "ProviderKey" text NOT NULL,
        "ProviderDisplayName" text NULL,
        "UserId" text NOT NULL,
        CONSTRAINT "PK_AspNetUserLogins" PRIMARY KEY ("LoginProvider", "ProviderKey"),
        CONSTRAINT "FK_AspNetUserLogins_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
    );
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20180306105643_CreateIdentityUser') THEN
    CREATE TABLE "AspNetUserRoles" (
        "UserId" text NOT NULL,
        "RoleId" text NOT NULL,
        CONSTRAINT "PK_AspNetUserRoles" PRIMARY KEY ("UserId", "RoleId"),
        CONSTRAINT "FK_AspNetUserRoles_AspNetRoles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "AspNetRoles" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_AspNetUserRoles_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
    );
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20180306105643_CreateIdentityUser') THEN
    CREATE TABLE "AspNetUserTokens" (
        "UserId" text NOT NULL,
        "LoginProvider" text NOT NULL,
        "Name" text NOT NULL,
        "Value" text NULL,
        CONSTRAINT "PK_AspNetUserTokens" PRIMARY KEY ("UserId", "LoginProvider", "Name"),
        CONSTRAINT "FK_AspNetUserTokens_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
    );
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20180306105643_CreateIdentityUser') THEN
    CREATE INDEX "IX_AspNetRoleClaims_RoleId" ON "AspNetRoleClaims" ("RoleId");
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20180306105643_CreateIdentityUser') THEN
    CREATE UNIQUE INDEX "RoleNameIndex" ON "AspNetRoles" ("NormalizedName");
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20180306105643_CreateIdentityUser') THEN
    CREATE INDEX "IX_AspNetUserClaims_UserId" ON "AspNetUserClaims" ("UserId");
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20180306105643_CreateIdentityUser') THEN
    CREATE INDEX "IX_AspNetUserLogins_UserId" ON "AspNetUserLogins" ("UserId");
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20180306105643_CreateIdentityUser') THEN
    CREATE INDEX "IX_AspNetUserRoles_RoleId" ON "AspNetUserRoles" ("RoleId");
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20180306105643_CreateIdentityUser') THEN
    CREATE INDEX "EmailIndex" ON "AspNetUsers" ("NormalizedEmail");
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20180306105643_CreateIdentityUser') THEN
    CREATE UNIQUE INDEX "UserNameIndex" ON "AspNetUsers" ("NormalizedUserName");
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20180306105643_CreateIdentityUser') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20180306105643_CreateIdentityUser', '3.1.3');
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20180308104550_CreateAuditLog') THEN
    CREATE TABLE "AuditLogs" (
        "Id" bigserial NOT NULL,
        "AuthorId" text NULL,
        "Route" text NOT NULL,
        "Timestamp" timestamp without time zone NOT NULL,
        CONSTRAINT "PK_AuditLogs" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_AuditLogs_AspNetUsers_AuthorId" FOREIGN KEY ("AuthorId") REFERENCES "AspNetUsers" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20180308104550_CreateAuditLog') THEN
    CREATE INDEX "IX_AuditLogs_AuthorId" ON "AuditLogs" ("AuthorId");
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20180308104550_CreateAuditLog') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20180308104550_CreateAuditLog', '3.1.3');
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20180309192000_AddIdentityRole') THEN
    ALTER TABLE "AspNetUsers" DROP COLUMN "Discriminator";
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20180309192000_AddIdentityRole') THEN
    ALTER TABLE "AspNetRoles" ADD "Description" text NULL;
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20180309192000_AddIdentityRole') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20180309192000_AddIdentityRole', '3.1.3');
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20180312130835_CreatePasswordResetToken') THEN
    CREATE TABLE "PasswordResetTokens" (
        "UserToken" text NOT NULL,
        "IdentityToken" text NULL,
        CONSTRAINT "PK_PasswordResetTokens" PRIMARY KEY ("UserToken")
    );
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20180312130835_CreatePasswordResetToken') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20180312130835_CreatePasswordResetToken', '3.1.3');
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20180319171541_AddChangeEmailToken') THEN
    CREATE TABLE "ChangeEmailTokens" (
        "IdentityToken" text NOT NULL,
        "Email" text NULL,
        "UserToken" text NULL,
        CONSTRAINT "PK_ChangeEmailTokens" PRIMARY KEY ("IdentityToken")
    );
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20180319171541_AddChangeEmailToken') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20180319171541_AddChangeEmailToken', '3.1.3');
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20180321085901_AddSensateUserToken') THEN
    ALTER TABLE "AspNetUserTokens" ADD "Discriminator" text NOT NULL DEFAULT '';
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20180321085901_AddSensateUserToken') THEN
    ALTER TABLE "AspNetUserTokens" ADD "CreatedAt" timestamp without time zone NULL;
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20180321085901_AddSensateUserToken') THEN
    ALTER TABLE "AspNetUserTokens" ADD "ExpiresAt" timestamp without time zone NULL;
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20180321085901_AddSensateUserToken') THEN
    ALTER TABLE "AspNetUserTokens" ADD "Valid" boolean NULL;
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20180321085901_AddSensateUserToken') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20180321085901_AddSensateUserToken', '3.1.3');
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20180321092407_AlterSensateUserTokenPK') THEN
    ALTER TABLE "AspNetUserTokens" DROP CONSTRAINT "PK_AspNetUserTokens";
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20180321092407_AlterSensateUserTokenPK') THEN
    ALTER TABLE "AspNetUserTokens" ALTER COLUMN "Value" TYPE text;
    ALTER TABLE "AspNetUserTokens" ALTER COLUMN "Value" SET NOT NULL;
    ALTER TABLE "AspNetUserTokens" ALTER COLUMN "Value" DROP DEFAULT;
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20180321092407_AlterSensateUserTokenPK') THEN
    ALTER TABLE "AspNetUserTokens" ADD CONSTRAINT "PK_AspNetUserTokens" PRIMARY KEY ("Value", "UserId");
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20180321092407_AlterSensateUserTokenPK') THEN
    ALTER TABLE "AspNetUserTokens" ADD CONSTRAINT "AK_AspNetUserTokens_UserId_LoginProvider_Name" UNIQUE ("UserId", "LoginProvider", "Name");
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20180321092407_AlterSensateUserTokenPK') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20180321092407_AlterSensateUserTokenPK', '3.1.3');
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20180321094920_AlterSensateUserTokenTableName') THEN
    ALTER TABLE "AspNetUserTokens" DROP CONSTRAINT "PK_AspNetUserTokens";
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20180321094920_AlterSensateUserTokenTableName') THEN
    ALTER TABLE "AspNetUserTokens" DROP CONSTRAINT "AK_AspNetUserTokens_UserId_LoginProvider_Name";
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20180321094920_AlterSensateUserTokenTableName') THEN
    ALTER TABLE "AspNetUserTokens" DROP COLUMN "Discriminator";
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20180321094920_AlterSensateUserTokenTableName') THEN
    ALTER TABLE "AspNetUserTokens" DROP COLUMN "CreatedAt";
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20180321094920_AlterSensateUserTokenTableName') THEN
    ALTER TABLE "AspNetUserTokens" DROP COLUMN "ExpiresAt";
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20180321094920_AlterSensateUserTokenTableName') THEN
    ALTER TABLE "AspNetUserTokens" DROP COLUMN "Valid";
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20180321094920_AlterSensateUserTokenTableName') THEN
    ALTER TABLE "AspNetUserTokens" ALTER COLUMN "Value" TYPE text;
    ALTER TABLE "AspNetUserTokens" ALTER COLUMN "Value" DROP NOT NULL;
    ALTER TABLE "AspNetUserTokens" ALTER COLUMN "Value" DROP DEFAULT;
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20180321094920_AlterSensateUserTokenTableName') THEN
    ALTER TABLE "AspNetUserTokens" ADD CONSTRAINT "PK_AspNetUserTokens" PRIMARY KEY ("UserId", "LoginProvider", "Name");
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20180321094920_AlterSensateUserTokenTableName') THEN
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
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20180321094920_AlterSensateUserTokenTableName') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20180321094920_AlterSensateUserTokenTableName', '3.1.3');
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20180324232304_AddMethodToAuditLog') THEN
    ALTER TABLE "AuditLogs" ADD "Method" integer NOT NULL DEFAULT 0;
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20180324232304_AddMethodToAuditLog') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20180324232304_AddMethodToAuditLog', '3.1.3');
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20180324232847_RenameAuditLogToAspNetAuditLogs') THEN
    ALTER TABLE "AuditLogs" DROP CONSTRAINT "FK_AuditLogs_AspNetUsers_AuthorId";
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20180324232847_RenameAuditLogToAspNetAuditLogs') THEN
    ALTER TABLE "AuditLogs" DROP CONSTRAINT "PK_AuditLogs";
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20180324232847_RenameAuditLogToAspNetAuditLogs') THEN
    ALTER TABLE "AuditLogs" RENAME TO "AspNetAuditLogs";
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20180324232847_RenameAuditLogToAspNetAuditLogs') THEN
    ALTER INDEX "IX_AuditLogs_AuthorId" RENAME TO "IX_AspNetAuditLogs_AuthorId";
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20180324232847_RenameAuditLogToAspNetAuditLogs') THEN
    ALTER TABLE "AspNetAuditLogs" ADD CONSTRAINT "PK_AspNetAuditLogs" PRIMARY KEY ("Id");
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20180324232847_RenameAuditLogToAspNetAuditLogs') THEN
    ALTER TABLE "AspNetAuditLogs" ADD CONSTRAINT "FK_AspNetAuditLogs_AspNetUsers_AuthorId" FOREIGN KEY ("AuthorId") REFERENCES "AspNetUsers" ("Id") ON DELETE RESTRICT;
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20180324232847_RenameAuditLogToAspNetAuditLogs') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20180324232847_RenameAuditLogToAspNetAuditLogs', '3.1.3');
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20180325192036_AddRemoteAddressToAuditLog') THEN
    ALTER TABLE "AspNetAuditLogs" ADD "Address" inet NOT NULL;
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20180325192036_AddRemoteAddressToAuditLog') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20180325192036_AddRemoteAddressToAuditLog', '3.1.3');
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20180416111034_FirstLastNameNotNullable') THEN
    ALTER TABLE "AspNetUsers" ALTER COLUMN "LastName" TYPE text;
    ALTER TABLE "AspNetUsers" ALTER COLUMN "LastName" SET NOT NULL;
    ALTER TABLE "AspNetUsers" ALTER COLUMN "LastName" DROP DEFAULT;
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20180416111034_FirstLastNameNotNullable') THEN
    ALTER TABLE "AspNetUsers" ALTER COLUMN "FirstName" TYPE text;
    ALTER TABLE "AspNetUsers" ALTER COLUMN "FirstName" SET NOT NULL;
    ALTER TABLE "AspNetUsers" ALTER COLUMN "FirstName" DROP DEFAULT;
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20180416111034_FirstLastNameNotNullable') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20180416111034_FirstLastNameNotNullable', '3.1.3');
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20180927135439_AddChangePhoneNumberTokenModel') THEN
    CREATE TABLE "ChangePhoneNumberTokens" (
        "IdentityToken" text NOT NULL,
        "PhoneNumber" text NULL,
        "UserToken" text NOT NULL,
        CONSTRAINT "PK_ChangePhoneNumberTokens" PRIMARY KEY ("IdentityToken"),
        CONSTRAINT "AlternateKey_UserToken" UNIQUE ("UserToken")
    );
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20180927135439_AddChangePhoneNumberTokenModel') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20180927135439_AddChangePhoneNumberTokenModel', '3.1.3');
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20181017081823_AddUserAndTimestampToChangePhoneNumberTokens') THEN
    ALTER TABLE "ChangePhoneNumberTokens" DROP CONSTRAINT "PK_ChangePhoneNumberTokens";
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20181017081823_AddUserAndTimestampToChangePhoneNumberTokens') THEN
    ALTER TABLE "ChangePhoneNumberTokens" ALTER COLUMN "PhoneNumber" TYPE text;
    ALTER TABLE "ChangePhoneNumberTokens" ALTER COLUMN "PhoneNumber" SET NOT NULL;
    ALTER TABLE "ChangePhoneNumberTokens" ALTER COLUMN "PhoneNumber" DROP DEFAULT;
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20181017081823_AddUserAndTimestampToChangePhoneNumberTokens') THEN
    ALTER TABLE "ChangePhoneNumberTokens" ADD "Timestamp" timestamp without time zone NOT NULL DEFAULT TIMESTAMP '0001-01-01 00:00:00';
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20181017081823_AddUserAndTimestampToChangePhoneNumberTokens') THEN
    ALTER TABLE "ChangePhoneNumberTokens" ADD "UserId" text NULL;
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20181017081823_AddUserAndTimestampToChangePhoneNumberTokens') THEN
    ALTER TABLE "ChangePhoneNumberTokens" ADD CONSTRAINT "PK_ChangePhoneNumberTokens" PRIMARY KEY ("IdentityToken", "PhoneNumber");
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20181017081823_AddUserAndTimestampToChangePhoneNumberTokens') THEN
    CREATE INDEX "IX_ChangePhoneNumberTokens_UserId" ON "ChangePhoneNumberTokens" ("UserId");
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20181017081823_AddUserAndTimestampToChangePhoneNumberTokens') THEN
    ALTER TABLE "ChangePhoneNumberTokens" ADD CONSTRAINT "FK_ChangePhoneNumberTokens_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE RESTRICT;
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20181017081823_AddUserAndTimestampToChangePhoneNumberTokens') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20181017081823_AddUserAndTimestampToChangePhoneNumberTokens', '3.1.3');
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20181017155214_AddUnconfirmedPhoneNumberToSensateUser') THEN
    ALTER TABLE "AspNetUsers" ADD "UnconfirmedPhoneNumber" text NULL;
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20181017155214_AddUnconfirmedPhoneNumberToSensateUser') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20181017155214_AddUnconfirmedPhoneNumberToSensateUser', '3.1.3');
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20181104162220_AddRegistrationDateToUsers') THEN
    ALTER TABLE "AspNetUsers" ADD "RegisteredAt" timestamp without time zone NOT NULL DEFAULT TIMESTAMP '0001-01-01 00:00:00';
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20181104162220_AddRegistrationDateToUsers') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20181104162220_AddRegistrationDateToUsers', '3.1.3');
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20190304204245_RemoveAuditLogTable') THEN
    DROP TABLE "AspNetAuditLogs";
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20190304204245_RemoveAuditLogTable') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20190304204245_RemoveAuditLogTable', '3.1.3');
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20190305131708_StandardizeTableNames') THEN
    ALTER TABLE "ChangePhoneNumberTokens" DROP CONSTRAINT "FK_ChangePhoneNumberTokens_AspNetUsers_UserId";
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20190305131708_StandardizeTableNames') THEN
    ALTER TABLE "ChangePhoneNumberTokens" DROP CONSTRAINT "PK_ChangePhoneNumberTokens";
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20190305131708_StandardizeTableNames') THEN
    ALTER TABLE "ChangeEmailTokens" DROP CONSTRAINT "PK_ChangeEmailTokens";
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20190305131708_StandardizeTableNames') THEN
    ALTER TABLE "ChangePhoneNumberTokens" RENAME TO "AspNetPhoneNumberTokens";
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20190305131708_StandardizeTableNames') THEN
    ALTER TABLE "ChangeEmailTokens" RENAME TO "AspNetEmailTokens";
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20190305131708_StandardizeTableNames') THEN
    ALTER INDEX "IX_ChangePhoneNumberTokens_UserId" RENAME TO "IX_AspNetPhoneNumberTokens_UserId";
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20190305131708_StandardizeTableNames') THEN
    ALTER TABLE "AspNetPhoneNumberTokens" ADD CONSTRAINT "PK_AspNetPhoneNumberTokens" PRIMARY KEY ("IdentityToken", "PhoneNumber");
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20190305131708_StandardizeTableNames') THEN
    ALTER TABLE "AspNetEmailTokens" ADD CONSTRAINT "PK_AspNetEmailTokens" PRIMARY KEY ("IdentityToken");
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20190305131708_StandardizeTableNames') THEN
    ALTER TABLE "AspNetPhoneNumberTokens" ADD CONSTRAINT "FK_AspNetPhoneNumberTokens_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE RESTRICT;
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20190305131708_StandardizeTableNames') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20190305131708_StandardizeTableNames', '3.1.3');
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20190305154458_StandardizePasswordResetTokenTable') THEN
    ALTER TABLE "PasswordResetTokens" DROP CONSTRAINT "PK_PasswordResetTokens";
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20190305154458_StandardizePasswordResetTokenTable') THEN
    ALTER TABLE "PasswordResetTokens" RENAME TO "AspNetPasswordResetTokens";
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20190305154458_StandardizePasswordResetTokenTable') THEN
    ALTER TABLE "AspNetPasswordResetTokens" ADD CONSTRAINT "PK_AspNetPasswordResetTokens" PRIMARY KEY ("UserToken");
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20190305154458_StandardizePasswordResetTokenTable') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20190305154458_StandardizePasswordResetTokenTable', '3.1.3');
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20190313090012_AddApiKeyModel') THEN
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
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20190313090012_AddApiKeyModel') THEN
    CREATE INDEX "IX_AspNetApiKeys_UserId" ON "AspNetApiKeys" ("UserId");
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20190313090012_AddApiKeyModel') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20190313090012_AddApiKeyModel', '3.1.3');
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20190313133755_SetApiKeyToUnique') THEN
    ALTER TABLE "AspNetApiKeys" DROP CONSTRAINT "AK_AspNetApiKeys_ApiKey";
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20190313133755_SetApiKeyToUnique') THEN
    CREATE UNIQUE INDEX "IX_AspNetApiKeys_ApiKey" ON "AspNetApiKeys" ("ApiKey");
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20190313133755_SetApiKeyToUnique') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20190313133755_SetApiKeyToUnique', '3.1.3');
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20190313153152_AddNameAndReadOnlyToApiKey') THEN
    ALTER TABLE "AspNetApiKeys" ADD "Name" text NOT NULL DEFAULT '';
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20190313153152_AddNameAndReadOnlyToApiKey') THEN
    ALTER TABLE "AspNetApiKeys" ADD "ReadOnly" boolean NOT NULL DEFAULT FALSE;
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20190313153152_AddNameAndReadOnlyToApiKey') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20190313153152_AddNameAndReadOnlyToApiKey', '3.1.3');
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20190911123858_CreateAuditLogModel') THEN
    CREATE SEQUENCE "Id_sequence" START WITH 1 INCREMENT BY 1 NO MINVALUE NO MAXVALUE NO CYCLE;
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20190911123858_CreateAuditLogModel') THEN
    CREATE TABLE "AuditLogs" (
        "Id" bigint NOT NULL DEFAULT (nextval('"Id_sequence"')),
        "Route" text NOT NULL,
        "Method" integer NOT NULL,
        "Address" inet NOT NULL,
        "AuthorId" text NULL,
        "Timestamp" timestamp without time zone NOT NULL,
        CONSTRAINT "PK_AuditLogs" PRIMARY KEY ("Id")
    );
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20190911123858_CreateAuditLogModel') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20190911123858_CreateAuditLogModel', '3.1.3');
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200115124443_MigrateToIdentityColumns') THEN
    ALTER TABLE "AuditLogs" ALTER COLUMN "Id" DROP DEFAULT;
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200115124443_MigrateToIdentityColumns') THEN
    ALTER TABLE "AuditLogs" ALTER COLUMN "Id" TYPE bigint;
    ALTER TABLE "AuditLogs" ALTER COLUMN "Id" SET NOT NULL;
    ALTER TABLE "AuditLogs" ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY;
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200115124443_MigrateToIdentityColumns') THEN
    DROP SEQUENCE "Id_sequence";
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200115124443_MigrateToIdentityColumns') THEN
    ALTER TABLE "AspNetRoleClaims" ALTER COLUMN "Id" DROP DEFAULT;
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200115124443_MigrateToIdentityColumns') THEN
    ALTER TABLE "AspNetRoleClaims" ALTER COLUMN "Id" SET NOT NULL;
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200115124443_MigrateToIdentityColumns') THEN
    ALTER SEQUENCE "AspNetRoleClaims_Id_seq" RENAME TO "AspNetRoleClaims_Id_old_seq";
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200115124443_MigrateToIdentityColumns') THEN
    ALTER TABLE "AspNetRoleClaims" ALTER COLUMN "Id" DROP DEFAULT;
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200115124443_MigrateToIdentityColumns') THEN
    ALTER TABLE "AspNetRoleClaims" ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY;
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200115124443_MigrateToIdentityColumns') THEN
    DROP SEQUENCE "AspNetRoleClaims_Id_old_seq";
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200115124443_MigrateToIdentityColumns') THEN
    ALTER TABLE "AspNetUserClaims" ALTER COLUMN "Id" DROP DEFAULT;
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200115124443_MigrateToIdentityColumns') THEN
    ALTER TABLE "AspNetUserClaims" ALTER COLUMN "Id" SET NOT NULL;
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200115124443_MigrateToIdentityColumns') THEN
    ALTER SEQUENCE "AspNetUserClaims_Id_seq" RENAME TO "AspNetUserClaims_Id_old_seq";
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200115124443_MigrateToIdentityColumns') THEN
    ALTER TABLE "AspNetUserClaims" ALTER COLUMN "Id" DROP DEFAULT;
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200115124443_MigrateToIdentityColumns') THEN
    ALTER TABLE "AspNetUserClaims" ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY;
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200115124443_MigrateToIdentityColumns') THEN
    DROP SEQUENCE "AspNetUserClaims_Id_old_seq";
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200115124443_MigrateToIdentityColumns') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20200115124443_MigrateToIdentityColumns', '3.1.3');
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200116182252_UpdateIdentityTableNames') THEN
    ALTER TABLE "AspNetApiKeys" DROP CONSTRAINT "FK_AspNetApiKeys_AspNetUsers_UserId";
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200116182252_UpdateIdentityTableNames') THEN
    ALTER TABLE "AspNetAuthTokens" DROP CONSTRAINT "FK_AspNetAuthTokens_AspNetUsers_UserId";
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200116182252_UpdateIdentityTableNames') THEN
    ALTER TABLE "AspNetPhoneNumberTokens" DROP CONSTRAINT "FK_AspNetPhoneNumberTokens_AspNetUsers_UserId";
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200116182252_UpdateIdentityTableNames') THEN
    ALTER TABLE "AspNetRoleClaims" DROP CONSTRAINT "FK_AspNetRoleClaims_AspNetRoles_RoleId";
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200116182252_UpdateIdentityTableNames') THEN
    ALTER TABLE "AspNetUserClaims" DROP CONSTRAINT "FK_AspNetUserClaims_AspNetUsers_UserId";
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200116182252_UpdateIdentityTableNames') THEN
    ALTER TABLE "AspNetUserLogins" DROP CONSTRAINT "FK_AspNetUserLogins_AspNetUsers_UserId";
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200116182252_UpdateIdentityTableNames') THEN
    ALTER TABLE "AspNetUserRoles" DROP CONSTRAINT "FK_AspNetUserRoles_AspNetRoles_RoleId";
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200116182252_UpdateIdentityTableNames') THEN
    ALTER TABLE "AspNetUserRoles" DROP CONSTRAINT "FK_AspNetUserRoles_AspNetUsers_UserId";
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200116182252_UpdateIdentityTableNames') THEN
    ALTER TABLE "AspNetUserTokens" DROP CONSTRAINT "FK_AspNetUserTokens_AspNetUsers_UserId";
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200116182252_UpdateIdentityTableNames') THEN
    ALTER TABLE "AspNetUserTokens" DROP CONSTRAINT "PK_AspNetUserTokens";
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200116182252_UpdateIdentityTableNames') THEN
    ALTER TABLE "AspNetUsers" DROP CONSTRAINT "PK_AspNetUsers";
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200116182252_UpdateIdentityTableNames') THEN
    ALTER TABLE "AspNetUserRoles" DROP CONSTRAINT "PK_AspNetUserRoles";
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200116182252_UpdateIdentityTableNames') THEN
    ALTER TABLE "AspNetUserLogins" DROP CONSTRAINT "PK_AspNetUserLogins";
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200116182252_UpdateIdentityTableNames') THEN
    ALTER TABLE "AspNetUserClaims" DROP CONSTRAINT "PK_AspNetUserClaims";
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200116182252_UpdateIdentityTableNames') THEN
    ALTER TABLE "AspNetRoles" DROP CONSTRAINT "PK_AspNetRoles";
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200116182252_UpdateIdentityTableNames') THEN
    ALTER TABLE "AspNetRoleClaims" DROP CONSTRAINT "PK_AspNetRoleClaims";
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200116182252_UpdateIdentityTableNames') THEN
    ALTER TABLE "AspNetPhoneNumberTokens" DROP CONSTRAINT "PK_AspNetPhoneNumberTokens";
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200116182252_UpdateIdentityTableNames') THEN
    ALTER TABLE "AspNetPasswordResetTokens" DROP CONSTRAINT "PK_AspNetPasswordResetTokens";
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200116182252_UpdateIdentityTableNames') THEN
    ALTER TABLE "AspNetEmailTokens" DROP CONSTRAINT "PK_AspNetEmailTokens";
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200116182252_UpdateIdentityTableNames') THEN
    ALTER TABLE "AspNetAuthTokens" DROP CONSTRAINT "PK_AspNetAuthTokens";
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200116182252_UpdateIdentityTableNames') THEN
    ALTER TABLE "AspNetApiKeys" DROP CONSTRAINT "PK_AspNetApiKeys";
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200116182252_UpdateIdentityTableNames') THEN
    ALTER TABLE "AspNetUserTokens" RENAME TO "UserTokens";
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200116182252_UpdateIdentityTableNames') THEN
    ALTER TABLE "AspNetUsers" RENAME TO "Users";
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200116182252_UpdateIdentityTableNames') THEN
    ALTER TABLE "AspNetUserRoles" RENAME TO "UserRoles";
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200116182252_UpdateIdentityTableNames') THEN
    ALTER TABLE "AspNetUserLogins" RENAME TO "UserLogins";
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200116182252_UpdateIdentityTableNames') THEN
    ALTER TABLE "AspNetUserClaims" RENAME TO "UserClaims";
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200116182252_UpdateIdentityTableNames') THEN
    ALTER TABLE "AspNetRoles" RENAME TO "Roles";
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200116182252_UpdateIdentityTableNames') THEN
    ALTER TABLE "AspNetRoleClaims" RENAME TO "RoleClaims";
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200116182252_UpdateIdentityTableNames') THEN
    ALTER TABLE "AspNetPhoneNumberTokens" RENAME TO "PhoneNumberTokens";
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200116182252_UpdateIdentityTableNames') THEN
    ALTER TABLE "AspNetPasswordResetTokens" RENAME TO "PasswordResetTokens";
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200116182252_UpdateIdentityTableNames') THEN
    ALTER TABLE "AspNetEmailTokens" RENAME TO "EmailTokens";
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200116182252_UpdateIdentityTableNames') THEN
    ALTER TABLE "AspNetAuthTokens" RENAME TO "AuthTokens";
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200116182252_UpdateIdentityTableNames') THEN
    ALTER TABLE "AspNetApiKeys" RENAME TO "ApiKeys";
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200116182252_UpdateIdentityTableNames') THEN
    ALTER INDEX "IX_AspNetUserRoles_RoleId" RENAME TO "IX_UserRoles_RoleId";
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200116182252_UpdateIdentityTableNames') THEN
    ALTER INDEX "IX_AspNetUserLogins_UserId" RENAME TO "IX_UserLogins_UserId";
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200116182252_UpdateIdentityTableNames') THEN
    ALTER INDEX "IX_AspNetUserClaims_UserId" RENAME TO "IX_UserClaims_UserId";
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200116182252_UpdateIdentityTableNames') THEN
    ALTER INDEX "IX_AspNetRoleClaims_RoleId" RENAME TO "IX_RoleClaims_RoleId";
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200116182252_UpdateIdentityTableNames') THEN
    ALTER INDEX "IX_AspNetPhoneNumberTokens_UserId" RENAME TO "IX_PhoneNumberTokens_UserId";
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200116182252_UpdateIdentityTableNames') THEN
    ALTER INDEX "IX_AspNetApiKeys_UserId" RENAME TO "IX_ApiKeys_UserId";
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200116182252_UpdateIdentityTableNames') THEN
    ALTER INDEX "IX_AspNetApiKeys_ApiKey" RENAME TO "IX_ApiKeys_ApiKey";
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200116182252_UpdateIdentityTableNames') THEN
    ALTER TABLE "UserTokens" ADD CONSTRAINT "PK_UserTokens" PRIMARY KEY ("UserId", "LoginProvider", "Name");
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200116182252_UpdateIdentityTableNames') THEN
    ALTER TABLE "Users" ADD CONSTRAINT "PK_Users" PRIMARY KEY ("Id");
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200116182252_UpdateIdentityTableNames') THEN
    ALTER TABLE "UserRoles" ADD CONSTRAINT "PK_UserRoles" PRIMARY KEY ("UserId", "RoleId");
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200116182252_UpdateIdentityTableNames') THEN
    ALTER TABLE "UserLogins" ADD CONSTRAINT "PK_UserLogins" PRIMARY KEY ("LoginProvider", "ProviderKey");
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200116182252_UpdateIdentityTableNames') THEN
    ALTER TABLE "UserClaims" ADD CONSTRAINT "PK_UserClaims" PRIMARY KEY ("Id");
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200116182252_UpdateIdentityTableNames') THEN
    ALTER TABLE "Roles" ADD CONSTRAINT "PK_Roles" PRIMARY KEY ("Id");
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200116182252_UpdateIdentityTableNames') THEN
    ALTER TABLE "RoleClaims" ADD CONSTRAINT "PK_RoleClaims" PRIMARY KEY ("Id");
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200116182252_UpdateIdentityTableNames') THEN
    ALTER TABLE "PhoneNumberTokens" ADD CONSTRAINT "PK_PhoneNumberTokens" PRIMARY KEY ("IdentityToken", "PhoneNumber");
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200116182252_UpdateIdentityTableNames') THEN
    ALTER TABLE "PasswordResetTokens" ADD CONSTRAINT "PK_PasswordResetTokens" PRIMARY KEY ("UserToken");
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200116182252_UpdateIdentityTableNames') THEN
    ALTER TABLE "EmailTokens" ADD CONSTRAINT "PK_EmailTokens" PRIMARY KEY ("IdentityToken");
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200116182252_UpdateIdentityTableNames') THEN
    ALTER TABLE "AuthTokens" ADD CONSTRAINT "PK_AuthTokens" PRIMARY KEY ("UserId", "Value");
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200116182252_UpdateIdentityTableNames') THEN
    ALTER TABLE "ApiKeys" ADD CONSTRAINT "PK_ApiKeys" PRIMARY KEY ("Id");
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200116182252_UpdateIdentityTableNames') THEN
    ALTER TABLE "ApiKeys" ADD CONSTRAINT "FK_ApiKeys_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE;
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200116182252_UpdateIdentityTableNames') THEN
    ALTER TABLE "AuthTokens" ADD CONSTRAINT "FK_AuthTokens_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE;
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200116182252_UpdateIdentityTableNames') THEN
    ALTER TABLE "PhoneNumberTokens" ADD CONSTRAINT "FK_PhoneNumberTokens_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE RESTRICT;
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200116182252_UpdateIdentityTableNames') THEN
    ALTER TABLE "RoleClaims" ADD CONSTRAINT "FK_RoleClaims_Roles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "Roles" ("Id") ON DELETE CASCADE;
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200116182252_UpdateIdentityTableNames') THEN
    ALTER TABLE "UserClaims" ADD CONSTRAINT "FK_UserClaims_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE;
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200116182252_UpdateIdentityTableNames') THEN
    ALTER TABLE "UserLogins" ADD CONSTRAINT "FK_UserLogins_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE;
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200116182252_UpdateIdentityTableNames') THEN
    ALTER TABLE "UserRoles" ADD CONSTRAINT "FK_UserRoles_Roles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "Roles" ("Id") ON DELETE CASCADE;
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200116182252_UpdateIdentityTableNames') THEN
    ALTER TABLE "UserRoles" ADD CONSTRAINT "FK_UserRoles_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE;
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200116182252_UpdateIdentityTableNames') THEN
    ALTER TABLE "UserTokens" ADD CONSTRAINT "FK_UserTokens_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE;
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200116182252_UpdateIdentityTableNames') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20200116182252_UpdateIdentityTableNames', '3.1.3');
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200123111807_AddForeignKeyToAuditLogs') THEN
    CREATE INDEX "IX_AuditLogs_AuthorId" ON "AuditLogs" ("AuthorId");
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200123111807_AddForeignKeyToAuditLogs') THEN
    CREATE INDEX "IX_AuditLogs_Method" ON "AuditLogs" ("Method");
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200123111807_AddForeignKeyToAuditLogs') THEN
    ALTER TABLE "AuditLogs" ADD CONSTRAINT "FK_AuditLogs_Users_AuthorId" FOREIGN KEY ("AuthorId") REFERENCES "Users" ("Id") ON DELETE RESTRICT;
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200123111807_AddForeignKeyToAuditLogs') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20200123111807_AddForeignKeyToAuditLogs', '3.1.3');
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200123112621_CreateTriggerTable') THEN
    CREATE TABLE "Triggers" (
        "Id" bigint NOT NULL GENERATED BY DEFAULT AS IDENTITY,
        "KeyValue" text NOT NULL,
        "LowerEdge" numeric NULL,
        "UpperEdige" numeric NULL,
        "SensorId" character varying(24) NOT NULL,
        CONSTRAINT "PK_Triggers" PRIMARY KEY ("Id")
    );
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200123112621_CreateTriggerTable') THEN
    CREATE INDEX "IX_Triggers_SensorId" ON "Triggers" ("SensorId");
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200123112621_CreateTriggerTable') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20200123112621_CreateTriggerTable', '3.1.3');
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200123163640_CreateTriggerActionsTable') THEN
    ALTER TABLE "Triggers" DROP COLUMN "UpperEdige";
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200123163640_CreateTriggerActionsTable') THEN
    ALTER TABLE "Triggers" ADD "UpperEdge" numeric NULL;
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200123163640_CreateTriggerActionsTable') THEN
    CREATE TABLE "TriggerActions" (
        "TriggerId" bigint NOT NULL,
        "Channel" integer NOT NULL,
        CONSTRAINT "PK_TriggerActions" PRIMARY KEY ("TriggerId", "Channel"),
        CONSTRAINT "FK_TriggerActions_Triggers_TriggerId" FOREIGN KEY ("TriggerId") REFERENCES "Triggers" ("Id") ON DELETE CASCADE
    );
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200123163640_CreateTriggerActionsTable') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20200123163640_CreateTriggerActionsTable', '3.1.3');
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200123175614_AddTimestampToTrigger') THEN
    ALTER TABLE "Triggers" ADD "LastTriggered" timestamp without time zone NOT NULL DEFAULT TIMESTAMP '0001-01-01 00:00:00';
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200123175614_AddTimestampToTrigger') THEN
    CREATE INDEX "IX_Triggers_LastTriggered" ON "Triggers" ("LastTriggered");
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200123175614_AddTimestampToTrigger') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20200123175614_AddTimestampToTrigger', '3.1.3');
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200125001637_AddTriggerInvocationsTable') THEN
    DROP INDEX "IX_Triggers_LastTriggered";
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200125001637_AddTriggerInvocationsTable') THEN
    ALTER TABLE "Triggers" DROP COLUMN "LastTriggered";
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200125001637_AddTriggerInvocationsTable') THEN
    ALTER TABLE "Triggers" ADD "Message" character varying(300) NOT NULL DEFAULT '';
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200125001637_AddTriggerInvocationsTable') THEN
    CREATE TABLE "TriggerInvocations" (
        "Id" bigint NOT NULL GENERATED BY DEFAULT AS IDENTITY,
        "MeasurementBucketId" character varying(24) NULL,
        "MeasurementId" integer NOT NULL,
        "TriggerId" bigint NOT NULL,
        "Timestamp" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_TriggerInvocations" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_TriggerInvocations_Triggers_TriggerId" FOREIGN KEY ("TriggerId") REFERENCES "Triggers" ("Id") ON DELETE CASCADE
    );
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200125001637_AddTriggerInvocationsTable') THEN
    CREATE INDEX "IX_TriggerInvocations_TriggerId" ON "TriggerInvocations" ("TriggerId");
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200125001637_AddTriggerInvocationsTable') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20200125001637_AddTriggerInvocationsTable', '3.1.3');
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200125163420_AddAlternateKeyToTriggerInvocations') THEN
    ALTER TABLE "TriggerInvocations" ALTER COLUMN "MeasurementBucketId" TYPE character varying(24);
    ALTER TABLE "TriggerInvocations" ALTER COLUMN "MeasurementBucketId" SET NOT NULL;
    ALTER TABLE "TriggerInvocations" ALTER COLUMN "MeasurementBucketId" DROP DEFAULT;
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200125163420_AddAlternateKeyToTriggerInvocations') THEN
    ALTER TABLE "TriggerInvocations" ADD CONSTRAINT "AK_TriggerInvocations_MeasurementBucketId_MeasurementId_Trigge~" UNIQUE ("MeasurementBucketId", "MeasurementId", "TriggerId");
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200125163420_AddAlternateKeyToTriggerInvocations') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20200125163420_AddAlternateKeyToTriggerInvocations', '3.1.3');
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200130161811_CreateBlobsTable') THEN
    CREATE TABLE "Blobs" (
        "Id" bigint NOT NULL GENERATED BY DEFAULT AS IDENTITY,
        "SensorId" character varying(24) NOT NULL,
        "FileName" text NOT NULL,
        "Path" text NOT NULL,
        "StorageType" integer NOT NULL,
        CONSTRAINT "PK_Blobs" PRIMARY KEY ("Id")
    );
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200130161811_CreateBlobsTable') THEN
    CREATE INDEX "IX_Blobs_SensorId" ON "Blobs" ("SensorId");
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200130161811_CreateBlobsTable') THEN
    CREATE UNIQUE INDEX "IX_Blobs_SensorId_FileName" ON "Blobs" ("SensorId", "FileName");
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200130161811_CreateBlobsTable') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20200130161811_CreateBlobsTable', '3.1.3');
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200203161025_AddTargetColumnToTriggerActions') THEN
    ALTER TABLE "TriggerActions" ADD "Target" text NULL;
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200203161025_AddTargetColumnToTriggerActions') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20200203161025_AddTargetColumnToTriggerActions', '3.1.3');
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200214101035_MoveMessageFromTriggersToTriggerActions') THEN
    ALTER TABLE "Triggers" DROP COLUMN "Message";
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200214101035_MoveMessageFromTriggersToTriggerActions') THEN
    ALTER TABLE "TriggerActions" ADD "Message" character varying(255) NOT NULL DEFAULT '';
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200214101035_MoveMessageFromTriggersToTriggerActions') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20200214101035_MoveMessageFromTriggersToTriggerActions', '3.1.3');
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200214142510_RemoveMeasurementIndexFromTriggerInvocations') THEN
    ALTER TABLE "TriggerInvocations" DROP CONSTRAINT "AK_TriggerInvocations_MeasurementBucketId_MeasurementId_Trigge~";
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200214142510_RemoveMeasurementIndexFromTriggerInvocations') THEN
    ALTER TABLE "TriggerInvocations" DROP COLUMN "MeasurementBucketId";
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200214142510_RemoveMeasurementIndexFromTriggerInvocations') THEN
    ALTER TABLE "TriggerInvocations" DROP COLUMN "MeasurementId";
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200214142510_RemoveMeasurementIndexFromTriggerInvocations') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20200214142510_RemoveMeasurementIndexFromTriggerInvocations', '3.1.3');
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200306135250_AddSensorLinksTable') THEN
    CREATE TABLE "SensorLinks" (
        "SensorId" text NOT NULL,
        "UserId" text NOT NULL,
        CONSTRAINT "PK_SensorLinks" PRIMARY KEY ("UserId", "SensorId")
    );
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200306135250_AddSensorLinksTable') THEN
    CREATE INDEX "IX_SensorLinks_UserId" ON "SensorLinks" ("UserId");
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200306135250_AddSensorLinksTable') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20200306135250_AddSensorLinksTable', '3.1.3');
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200310225133_AddFormalLanguageToTriggersTable') THEN
    ALTER TABLE "Triggers" ADD "FormalLanguage" text NULL;
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200310225133_AddFormalLanguageToTriggersTable') THEN
    ALTER TABLE "Triggers" ADD "Type" integer NOT NULL DEFAULT 0;
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200310225133_AddFormalLanguageToTriggersTable') THEN
    CREATE INDEX "IX_Triggers_Type" ON "Triggers" ("Type");
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200310225133_AddFormalLanguageToTriggersTable') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20200310225133_AddFormalLanguageToTriggersTable', '3.1.3');
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200324234559_UpdateOnDeleteRules') THEN
    ALTER TABLE "AuditLogs" DROP CONSTRAINT "FK_AuditLogs_Users_AuthorId";
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200324234559_UpdateOnDeleteRules') THEN
    ALTER TABLE "PhoneNumberTokens" DROP CONSTRAINT "FK_PhoneNumberTokens_Users_UserId";
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200324234559_UpdateOnDeleteRules') THEN
    ALTER TABLE "PhoneNumberTokens" ALTER COLUMN "UserId" TYPE text;
    ALTER TABLE "PhoneNumberTokens" ALTER COLUMN "UserId" SET NOT NULL;
    ALTER TABLE "PhoneNumberTokens" ALTER COLUMN "UserId" DROP DEFAULT;
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200324234559_UpdateOnDeleteRules') THEN
    ALTER TABLE "AuditLogs" ADD CONSTRAINT "FK_AuditLogs_Users_AuthorId" FOREIGN KEY ("AuthorId") REFERENCES "Users" ("Id") ON DELETE CASCADE;
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200324234559_UpdateOnDeleteRules') THEN
    ALTER TABLE "PhoneNumberTokens" ADD CONSTRAINT "FK_PhoneNumberTokens_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE;
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200324234559_UpdateOnDeleteRules') THEN
    ALTER TABLE "SensorLinks" ADD CONSTRAINT "FK_SensorLinks_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE;
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200324234559_UpdateOnDeleteRules') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20200324234559_UpdateOnDeleteRules', '3.1.3');
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200325222614_AddTimestampToBlobs') THEN
    ALTER TABLE "Blobs" ADD "Timestamp" timestamp without time zone NOT NULL DEFAULT TIMESTAMP '0001-01-01 00:00:00';
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200325222614_AddTimestampToBlobs') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20200325222614_AddTimestampToBlobs', '3.1.3');
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200326181831_AddDataProtectionKeyTable') THEN
    CREATE TABLE "DataProtectionKeys" (
        "Id" integer NOT NULL GENERATED BY DEFAULT AS IDENTITY,
        "FriendlyName" text NULL,
        "Xml" text NULL,
        CONSTRAINT "PK_DataProtectionKeys" PRIMARY KEY ("Id")
    );
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200326181831_AddDataProtectionKeyTable') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20200326181831_AddDataProtectionKeyTable', '3.1.3');
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200402131550_AddFileSizeToBlobsTable') THEN
    ALTER TABLE "Blobs" ADD "FileSize" bigint NOT NULL DEFAULT 0;
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20200402131550_AddFileSizeToBlobsTable') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20200402131550_AddFileSizeToBlobsTable', '3.1.3');
    END IF;
END $$;
