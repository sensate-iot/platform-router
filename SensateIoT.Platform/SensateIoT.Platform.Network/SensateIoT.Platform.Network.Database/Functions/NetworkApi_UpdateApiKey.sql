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
	SET
		"ApiKey" = 'ABCD'
	WHERE "ApiKey" = 'ABCD'
END;
$$
