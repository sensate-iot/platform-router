
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
