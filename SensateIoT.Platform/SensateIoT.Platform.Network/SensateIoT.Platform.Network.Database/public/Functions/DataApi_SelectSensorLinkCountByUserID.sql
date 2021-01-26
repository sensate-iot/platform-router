CREATE FUNCTION dataapi_selectsensorlinkcountbyuserid(userid UUID)
    RETURNS TABLE("Count" BIGINT)
    LANGUAGE plpgsql
AS $$
BEGIN
	RETURN QUERY

    SELECT COUNT(*) AS "Count"
    FROM "SensorLinks"
    WHERE "UserId" = "UserId"::TEXT;
END
$$;