
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
