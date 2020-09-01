create function authorizationctx_getuseraccounts()
    returns TABLE(id text, billinglockout boolean, banned boolean)
    language plpgsql
as
$$
BEGIN
    return query
    SELECT "Users"."Id", "BillingLockout", "Roles"."NormalizedName" = 'BANNED' as "Banned"
    FROM "Users"
    INNER JOIN "UserRoles" ON "Users"."Id" = "UserRoles"."UserId"
    INNER JOIN "Roles" ON "UserRoles"."RoleId" = "Roles"."Id"
    GROUP BY "Users"."Id", "BillingLockout", "Roles"."NormalizedName" = 'BANNED';
END
$$;
