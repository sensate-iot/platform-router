CREATE FUNCTION networkapi_createaction(
    triggerid BIGINT,
    channel INT,
    target VARCHAR(255),
    message TEXT
)
    RETURNS TABLE(
        "ID" BIGINT,
        "TriggerID" BIGINT,
        "Channel" INT,
        "Target" VARCHAR(255),
        "Message" TEXT
    )
    LANGUAGE plpgsql
AS $$
BEGIN
	RETURN QUERY

    INSERT INTO "TriggerActions" ("TriggerID" ,
                                  "Channel" ,
                                  "Target" ,
                                  "Message")
    VALUES (triggerid, channel, target, message)
    RETURNING
        "TriggerActions"."ID",
        "TriggerActions"."TriggerID",
        "TriggerActions"."Channel",
        "TriggerActions"."Target",
        "TriggerActions"."Message";
END
$$
