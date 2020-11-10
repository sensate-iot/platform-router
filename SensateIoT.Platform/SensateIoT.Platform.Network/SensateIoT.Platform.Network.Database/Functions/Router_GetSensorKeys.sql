
CREATE FUNCTION router_getsensorkeys()
    RETURNS TABLE(apikey text, userid text, revoked boolean, readonly boolean)
	LANGUAGE plpgsql
AS
$$
BEGIN
RETURN query
	SELECT "ApiKey",
		   "UserId",
		   "Revoked",
		   "ReadOnly"
	FROM "ApiKeys"
	WHERE "Type" = 0;
END;
$$;
