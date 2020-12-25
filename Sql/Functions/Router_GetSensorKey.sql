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