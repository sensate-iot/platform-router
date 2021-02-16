CREATE FUNCTION networkapi_deletetriggerbyid(id BIGINT)
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

    DELETE FROM "Triggers"
    WHERE "Triggers"."ID" = id
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