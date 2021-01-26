CREATE FUNCTION dataapi_selectsensorlinkbyuserid(userid UUID)
    RETURNS TABLE(
        "SensorID" VARCHAR(24),
        "UserID" UUID
                 )
    LANGUAGE plpgsql
AS $$
BEGIN
	RETURN QUERY
    SELECT "SensorLinks"."SensorId"::VARCHAR(24),
           "SensorLinks"."UserId"::UUID
    FROM "SensorLinks"
    WHERE "SensorLinks"."UserId" = userid::TEXT;
END
$$;
