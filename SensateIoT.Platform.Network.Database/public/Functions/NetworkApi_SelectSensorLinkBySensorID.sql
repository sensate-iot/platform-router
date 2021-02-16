CREATE FUNCTION networkapi_selectsensorlinkbysensorid(sensorid VARCHAR(24))
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
    WHERE "SensorLinks"."SensorId" = sensorid;
END
$$
