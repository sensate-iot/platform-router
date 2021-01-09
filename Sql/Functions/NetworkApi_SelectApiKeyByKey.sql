CREATE FUNCTION networkapi_selectapikeybykey(key TEXT)
    RETURNS TABLE(
        "UserId" UUID,
        "Revoked" BOOLEAN,
        "Type" INT,
        "ReadOnly" BOOLEAN
                 )
    LANGUAGE plpgsql
AS $$
BEGIN
	RETURN QUERY

    SELECT "ApiKeys"."UserId"::UUID,
           "ApiKeys"."Revoked",
           "ApiKeys"."Type",
           "ApiKeys"."ReadOnly"
    FROM "ApiKeys"
    WHERE "ApiKey" = key;
END;
$$