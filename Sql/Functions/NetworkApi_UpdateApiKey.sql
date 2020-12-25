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
