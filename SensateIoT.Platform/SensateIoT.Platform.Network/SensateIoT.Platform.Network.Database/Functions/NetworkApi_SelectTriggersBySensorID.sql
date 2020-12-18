CREATE FUNCTION networkapi_selecttriggersbysensorid(
    sensorid VARCHAR(24)
)
    RETURNS TABLE(
        "ID" BIGINT,
        "SensorID" VARCHAR(24),
        "KeyValue" VARCHAR(32),
        "LowerEdge" NUMERIC,
        "UpperEdge" NUMERIC,
        "FormalLanguage" TEXT,
        "Type" INT
    )
    LANGUAGE plpgsql
AS $$
BEGIN
	RETURN QUERY

    SELECT "Triggers"."ID",
           "Triggers"."SensorID",
           "Triggers"."KeyValue",
           "Triggers"."LowerEdge",
           "Triggers"."UpperEdge",
           "Triggers"."FormalLanguage",
           "Triggers"."Type"
    FROM "Triggers"
    WHERE "Triggers"."SensorID" = sensorid;
END
$$
