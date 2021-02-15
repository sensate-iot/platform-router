CREATE FUNCTION networkapi_deletesensorlink(sensorid VARCHAR(24), userid UUID)
    RETURNS TABLE(
        "SensorID" VARCHAR(24),
        "UserID" UUID
                 )
    LANGUAGE plpgsql
AS $$
BEGIN
	RETURN QUERY
	DELETE FROM "SensorLinks"
    WHERE "SensorLinks"."UserId" = userid::TEXT AND
          "SensorLinks"."SensorId" = sensorid::TEXT
    RETURNING
        "SensorLinks"."SensorId"::VARCHAR(24),
        "SensorLinks"."UserId"::UUID;
END
$$