
CREATE FUNCTION networkapi_deletetriggeraction(triggerid BIGINT, channel INT)
    RETURNS TABLE(
        "ID" BIGINT,
        "TriggerID" BIGINT,
        "Channel" INT,
        "Target" VARCHAR(24),
        "Message" TEXT
    )
    LANGUAGE plpgsql
AS $$
BEGIN
	RETURN QUERY

    DELETE FROM "TriggerActions"
    WHERE "TriggerActions"."TriggerID" = triggerid AND "TriggerActions"."Channel" = channel
	RETURNING
        "TriggerActions"."ID",
	    "TriggerActions"."TriggerID",
	    "TriggerActions"."Channel",
	    "TriggerActions"."Target",
	    "TriggerActions"."Message";
END
$$
