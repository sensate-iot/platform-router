CREATE FUNCTION networkapi_createtrigger(
    sensorid VARCHAR(24),
    keyvalue VARCHAR(32),
    loweredge NUMERIC,
    upperedge NUMERIC,
    formallanguage TEXT,
    type INT
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

    INSERT INTO "Triggers" ("SensorID",
                           "KeyValue",
                           "LowerEdge",
                           "UpperEdge",
                           "FormalLanguage",
                           "Type")
    VALUES (sensorid,
            keyvalue,
            loweredge,
            upperedge,
            formallanguage,
            type)
    RETURNING
        "Triggers"."ID",
        "Triggers"."SensorID",
        "Triggers"."KeyValue",
        "Triggers"."LowerEdge",
        "Triggers"."UpperEdge",
        "Triggers"."FormalLanguage",
        "Triggers"."Type";
END
$$