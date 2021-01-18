CREATE FUNCTION public.networkapi_incrementrequestcount(
	key text)
    RETURNS void
    LANGUAGE 'plpgsql'

    COST 100
    VOLATILE 
    
AS
$$
BEGIN
    UPDATE "ApiKeys"
        SET "RequestCount" = "RequestCount" + 1
    WHERE "ApiKey" = key;
END;
$$;

CREATE FUNCTION networkapi_deletesensorkey(key TEXT)
    RETURNS VOID
    LANGUAGE plpgsql
AS $$
BEGIN
    DELETE FROM "ApiKeys"
    WHERE "ApiKey" = key;
END;
$$;

CREATE FUNCTION networkapi_createsensorkey(key TEXT, userId TEXT)
    RETURNS TABLE(
        "Id" UUID,
        "UserId" UUID,
        "ApiKey" TEXT,
        "Revoked" BOOLEAN,
        "Type" INT,
        "ReadOnly" BOOLEAN
                 )
    LANGUAGE plpgsql
AS $$
BEGIN
    RETURN QUERY
    INSERT INTO "ApiKeys" (
                           "Id",
                           "UserId",
                           "ApiKey",
                           "Revoked",
                           "CreatedOn",
                           "Type"
                           )
    VALUES (
            uuid_generate_v4(),
            userId,
            key,
            FALSE,
            NOW(),
            0
    )
    RETURNING
        "ApiKeys"."Id"::UUID,
        "ApiKeys"."UserId"::UUID,
        "ApiKeys"."ApiKey",
        "ApiKeys"."Revoked",
        "ApiKeys"."Type",
        "ApiKeys"."ReadOnly";
END;
$$

CREATE FUNCTION networkapi_createauditlog(
	route text,
	method integer,
	address inet,
	author text)
    RETURNS void
    LANGUAGE 'plpgsql'

    COST 100
    VOLATILE 
    
AS
$$
BEGIN
    INSERT INTO "AuditLogs" ("Route",
                             "Method",
                             "Address",
                             "AuthorId",
                             "Timestamp")
    VALUES (route,
            method,
            address,
            author,
            NOW());
END;
$$

CREATE FUNCTION livedataservice_getapikey(userid TEXT, apikey TEXT)
    RETURNS TABLE("UserId" TEXT, "Type" INT)
    LANGUAGE plpgsql
AS $$
BEGIN
	RETURN QUERY
	
	SELECT key."UserId", key."Type"
	FROM public."ApiKeys" AS key
	WHERE key."UserId" = userid AND
	      key."ApiKey" = apikey AND
	      key."Type"   != 0 AND
	      key."Revoked" = False;
END;
$$;


CREATE FUNCTION router_getsensorkeys()
    RETURNS TABLE(apikey text, userid uuid, revoked boolean, readonly boolean)
	LANGUAGE plpgsql
AS
$$
BEGIN
RETURN query
	SELECT "ApiKey",
		   "UserId"::uuid,
		   "Revoked",
		   "ReadOnly"
	FROM "ApiKeys"
	WHERE "Type" = 0;
END;
$$;

CREATE FUNCTION router_getsensorkey(sensorkey text)
    RETURNS TABLE(apikey text, userid uuid, revoked boolean, readonly boolean)
	LANGUAGE plpgsql
AS
$$
BEGIN
RETURN query
	SELECT "ApiKey",
		   "UserId"::uuid,
		   "Revoked",
		   "ReadOnly"
	FROM "ApiKeys"
	WHERE "Type" = 0 AND "ApiKey" = sensorkey;
END;
$$;


CREATE FUNCTION router_getaccounts()
    RETURNS TABLE(id UUID, billinglockout BOOLEAN, banned BOOLEAN)
    LANGUAGE plpgsql
AS
$$
BEGIN
RETURN query
    SELECT "Users"."Id"::uuid,
           "BillingLockout",
           "Roles"."NormalizedName" = 'BANNED' AS "Banned"
    FROM "Users"
    INNER JOIN "UserRoles" ON "Users"."Id" = "UserRoles"."UserId"
    INNER JOIN "Roles" ON "UserRoles"."RoleId" = "Roles"."Id"
    GROUP BY "Users"."Id", "BillingLockout", "Roles"."NormalizedName" = 'BANNED';
END
$$;


CREATE FUNCTION router_getaccount(userid UUID)
    RETURNS TABLE(id UUID, billinglockout BOOLEAN, banned BOOLEAN)
    LANGUAGE plpgsql
AS
$$
BEGIN
RETURN query
    SELECT "Users"."Id"::uuid,
           "BillingLockout",
           "Roles"."NormalizedName" = 'BANNED' AS "Banned"
    FROM "Users"
    INNER JOIN "UserRoles" ON "Users"."Id" = "UserRoles"."UserId"
    INNER JOIN "Roles" ON "UserRoles"."RoleId" = "Roles"."Id"
	WHERE "Users"."Id" = userid::text
    GROUP BY "Users"."Id", "BillingLockout", "Roles"."NormalizedName" = 'BANNED';
	
END
$$;

CREATE FUNCTION networkapi_updateapikey(old TEXT, new TEXT)
    RETURNS TABLE(
        "Id" UUID,
        "UserId" UUID,
        "ApiKey" TEXT,
        "Revoked" BOOLEAN,
        "Type" INT,
        "ReadOnly" BOOLEAN
                 )
    LANGUAGE plpgsql
AS $$
BEGIN
	RETURN QUERY
	
	UPDATE "ApiKeys"
	SET	"ApiKey" = new
	WHERE "ApiKeys"."ApiKey" = old
    RETURNING
        "ApiKeys"."Id"::UUID,
        "ApiKeys"."UserId"::UUID,
        "ApiKeys"."ApiKey",
        "ApiKeys"."Revoked",
        "ApiKeys"."Type",
        "ApiKeys"."ReadOnly";
END;
$$


CREATE FUNCTION networkapi_selectusersbyid(userids TEXT)
    RETURNS TABLE(
        "ID" UUID,
        "Firstname" TEXT,
        "Lastname" TEXT,
        "Email" VARCHAR(256),
        "RegisteredAt" TIMESTAMP,
        "PhoneNumber" TEXT,
        "BillingLockout" BOOLEAN,
        "Role" VARCHAR(256)
                 )
    LANGUAGE plpgsql
AS $$
DECLARE idlist VARCHAR(36)[];
BEGIN
    idlist = ARRAY(SELECT DISTINCT UNNEST(string_to_array(userids, ',')));
	RETURN QUERY

    SELECT "Users"."Id"::UUID AS "ID",
           "Users"."FirstName",
           "Users"."LastName",
           "Users"."Email",
           "Users"."RegisteredAt",
           "Users"."PhoneNumber",
           "Users"."BillingLockout",
           "Roles"."NormalizedName" AS "Role"
    FROM "Users"
    INNER JOIN "UserRoles" ON "Users"."Id" = "UserRoles"."UserId"
    JOIN "Roles" ON "UserRoles"."RoleId" = "Roles"."Id"
    WHERE "Users"."Id" = ANY(idlist);
END;
$$;

CREATE FUNCTION networkapi_selectuserbyid(userid UUID)
    RETURNS TABLE(
        "ID" UUID,
        "Firstname" TEXT,
        "Lastname" TEXT,
        "Email" VARCHAR(256),
        "RegisteredAt" TIMESTAMP,
        "PhoneNumber" TEXT,
        "BillingLockout" BOOLEAN,
        "Role" VARCHAR(256)
                 )
    LANGUAGE plpgsql
AS $$
BEGIN
	RETURN QUERY

    SELECT "Users"."Id"::UUID AS "ID",
           "Users"."FirstName",
           "Users"."LastName",
           "Users"."Email",
           "Users"."RegisteredAt",
           "Users"."PhoneNumber",
           "Users"."BillingLockout",
           "Roles"."NormalizedName" AS "Role"
    FROM "Users"
    INNER JOIN "UserRoles" ON "Users"."Id" = "UserRoles"."UserId"
    JOIN "Roles" ON "UserRoles"."RoleId" = "Roles"."Id"
    WHERE "Users"."Id" = userid::TEXT;
END;
$$;

CREATE FUNCTION networkapi_selectuserbyemail(email TEXT)
    RETURNS TABLE(
        "ID" UUID,
        "Firstname" TEXT,
        "Lastname" TEXT,
        "Email" VARCHAR(256),
        "RegisteredAt" TIMESTAMP,
        "PhoneNumber" TEXT,
        "BillingLockout" BOOLEAN,
        "Role" VARCHAR(256)
                 )
    LANGUAGE plpgsql
AS $$
BEGIN
	RETURN QUERY

    SELECT "Users"."Id"::UUID AS "ID",
           "Users"."FirstName",
           "Users"."LastName",
           "Users"."Email",
           "Users"."RegisteredAt",
           "Users"."PhoneNumber",
           "Users"."BillingLockout",
           "Roles"."NormalizedName" AS "Role"
    FROM "Users"
    INNER JOIN "UserRoles" ON "Users"."Id" = "UserRoles"."UserId"
    JOIN "Roles" ON "UserRoles"."RoleId" = "Roles"."Id"
    WHERE "Users"."Email" = email;
END;
$$;

CREATE FUNCTION networkapi_selectapikeybykey(key TEXT)
    RETURNS TABLE(
        "UserId" UUID,
        "Revoked" BOOLEAN,
        "Type" INT,
        "ReadOnly" BOOLEAN
                 )
    LANGUAGE plpgsql
AS $$
BEGIN
	RETURN QUERY

    SELECT "ApiKeys"."UserId"::UUID,
           "ApiKeys"."Revoked",
           "ApiKeys"."Type",
           "ApiKeys"."ReadOnly"
    FROM "ApiKeys"
    WHERE "ApiKey" = key;
END;
$$

CREATE FUNCTION public.livedataservice_createauditlog(
	route text,
	method integer,
	address inet,
	author text)
    RETURNS void
    LANGUAGE 'plpgsql'   
AS $BODY$
BEGIN
    INSERT INTO "AuditLogs" ("Route",
                             "Method",
                             "Address",
                             "AuthorId",
                             "Timestamp")
    VALUES (route,
            method,
            address,
            author,
            NOW());
END
$BODY$;
