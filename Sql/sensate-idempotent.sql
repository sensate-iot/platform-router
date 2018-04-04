CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" varchar(150) NOT NULL,
    "ProductVersion" varchar(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);


DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20180306105643_CreateIdentityUser') THEN
    CREATE TABLE "AspNetRoles" (
        "Id" text NOT NULL,
        "ConcurrencyStamp" text NULL,
        "Name" varchar(256) NULL,
        "NormalizedName" varchar(256) NULL,
        CONSTRAINT "PK_AspNetRoles" PRIMARY KEY ("Id")
    );
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20180306105643_CreateIdentityUser') THEN
    CREATE TABLE "AspNetUsers" (
        "Id" text NOT NULL,
        "AccessFailedCount" int4 NOT NULL,
        "ConcurrencyStamp" text NULL,
        "Discriminator" text NOT NULL,
        "Email" varchar(256) NULL,
        "EmailConfirmed" bool NOT NULL,
        "LockoutEnabled" bool NOT NULL,
        "LockoutEnd" timestamptz NULL,
        "NormalizedEmail" varchar(256) NULL,
        "NormalizedUserName" varchar(256) NULL,
        "PasswordHash" text NULL,
        "PhoneNumber" text NULL,
        "PhoneNumberConfirmed" bool NOT NULL,
        "SecurityStamp" text NULL,
        "TwoFactorEnabled" bool NOT NULL,
        "UserName" varchar(256) NULL,
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
    VALUES ('20180306105643_CreateIdentityUser', '2.0.2-rtm-10011');
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20180308104550_CreateAuditLog') THEN
    CREATE TABLE "AuditLogs" (
        "Id" bigserial NOT NULL,
        "AuthorId" text NULL,
        "Route" text NOT NULL,
        "Timestamp" timestamp NOT NULL,
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
    VALUES ('20180308104550_CreateAuditLog', '2.0.2-rtm-10011');
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
    VALUES ('20180309192000_AddIdentityRole', '2.0.2-rtm-10011');
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
    VALUES ('20180312130835_CreatePasswordResetToken', '2.0.2-rtm-10011');
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
    VALUES ('20180319171541_AddChangeEmailToken', '2.0.2-rtm-10011');
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
    ALTER TABLE "AspNetUserTokens" ADD "CreatedAt" timestamp NULL;
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20180321085901_AddSensateUserToken') THEN
    ALTER TABLE "AspNetUserTokens" ADD "ExpiresAt" timestamp NULL;
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20180321085901_AddSensateUserToken') THEN
    ALTER TABLE "AspNetUserTokens" ADD "Valid" bool NULL;
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20180321085901_AddSensateUserToken') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20180321085901_AddSensateUserToken', '2.0.2-rtm-10011');
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
    VALUES ('20180321092407_AlterSensateUserTokenPK', '2.0.2-rtm-10011');
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
        "CreatedAt" timestamp NOT NULL,
        "ExpiresAt" timestamp NOT NULL,
        "LoginProvider" text NULL,
        "Valid" bool NOT NULL,
        CONSTRAINT "PK_AspNetAuthTokens" PRIMARY KEY ("UserId", "Value"),
        CONSTRAINT "FK_AspNetAuthTokens_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
    );
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20180321094920_AlterSensateUserTokenTableName') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20180321094920_AlterSensateUserTokenTableName', '2.0.2-rtm-10011');
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20180324232304_AddMethodToAuditLog') THEN
    ALTER TABLE "AuditLogs" ADD "Method" int4 NOT NULL DEFAULT 0;
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20180324232304_AddMethodToAuditLog') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20180324232304_AddMethodToAuditLog', '2.0.2-rtm-10011');
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
    VALUES ('20180324232847_RenameAuditLogToAspNetAuditLogs', '2.0.2-rtm-10011');
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
    VALUES ('20180325192036_AddRemoteAddressToAuditLog', '2.0.2-rtm-10011');
    END IF;
END $$;
