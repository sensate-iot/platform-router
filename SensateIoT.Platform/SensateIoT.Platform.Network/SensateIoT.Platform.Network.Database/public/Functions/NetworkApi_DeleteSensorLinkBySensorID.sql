CREATE FUNCTION networkapi_deletesensorlinkbysensorid(sensorid VARCHAR(24))
    RETURNS TABLE("SensorID" VARCHAR(24), "UserID" UUID)
    LANGUAGE plpgsql
AS $$
BEGIN
	RETURN QUERY
	DELETE FROM "SensorLinks"
    WHERE "SensorLinks"."SensorId" = sensorid::TEXT
    RETURNING
        "SensorLinks"."SensorId"::VARCHAR(24),
        "SensorLinks"."UserId"::UUID;
END
$$;
