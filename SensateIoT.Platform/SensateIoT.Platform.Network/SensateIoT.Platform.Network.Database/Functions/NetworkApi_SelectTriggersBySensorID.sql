drop function networkapi_selecttriggerbysensorid;

CREATE FUNCTION networkapi_selecttriggerbysensorid(sensorid VARCHAR(24))
    RETURNS TABLE(
        "ID" BIGINT,
        "SensorID" VARCHAR(24),
        "KeyValue" VARCHAR(32),
        "LowerEdge" NUMERIC,
        "UpperEdge" NUMERIC,
        "FormalLanguage" TEXT,
        "Type" INT,
        "ActionID" BIGINT,
        "Channel" INT,
        "Target" VARCHAR(255),
        "Message" TEXT
                 )
    LANGUAGE plpgsql
AS $$
BEGIN
	RETURN QUERY
    SELECT
        "Triggers"."ID",
        "Triggers"."SensorID",
        "Triggers"."KeyValue",
        "Triggers"."LowerEdge",
        "Triggers"."UpperEdge",
        "Triggers"."FormalLanguage",
        "Triggers"."Type",
        "ta"."ID" AS "ActionID",
        "ta"."Channel",
        "ta"."Target",
        "ta"."Message"
    FROM "Triggers"
    LEFT JOIN "TriggerActions" AS ta ON ta."TriggerID" = "Triggers"."ID"
	WHERE "Triggers"."SensorID" = sensorid
    ORDER BY "Triggers"."ID", ta."ID";
END
$$