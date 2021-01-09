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