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
