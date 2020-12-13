CREATE FUNCTION networkapi_selectusersbyid(userid TEXT)
    RETURNS TABLE(
        "ID" UUID,
        "Firstname" TEXT,
        "Lastname" TEXT,
        "Email" VARCHAR(256),
        "EmailConfirmed" BOOLEAN,
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
           "Users"."EmailConfirmed",
           "Users"."PhoneNumber",
           "Users"."BillingLockout",
           "Roles"."NormalizedName" AS "Role"
    FROM "Users"
    INNER JOIN "UserRoles" ON "Users"."Id" = "UserRoles"."UserId"
    JOIN "Roles" ON "UserRoles"."RoleId" = "Roles"."Id"
    WHERE "Users"."Id" = userid;
END;
$$
