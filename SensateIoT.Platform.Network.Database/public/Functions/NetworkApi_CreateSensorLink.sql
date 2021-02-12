CREATE FUNCTION networkapi_createsensorlink(sensorid VARCHAR(24), userid UUID)
    RETURNS TABLE(
        "SensorID" VARCHAR(24),
        "UserID" UUID
                 )
    LANGUAGE plpgsql
AS $$
BEGIN
	RETURN QUERY
    INSERT INTO "SensorLinks" ("SensorId",
                              "UserId")
    VALUES (sensorid, userid::TEXT)
    RETURNING
        "SensorLinks"."SensorId"::VARCHAR(24),
        "SensorLinks"."UserId"::UUID;
END
$$
