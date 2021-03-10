DROP FUNCTION networkapi_createinvocation(triggerid BIGINT, actionid BIGINT, timestmp TIMESTAMP);

CREATE FUNCTION triggerservice_createinvocation(
    triggerid BIGINT,
    actionid BIGINT,
    timestmp TIMESTAMP
)
    RETURNS TABLE(
        "ID" BIGINT,
        "TriggerID" BIGINT,
        "ActionID" BIGINT,
        "Timestamp" TIMESTAMP
    )
    LANGUAGE plpgsql
AS $$
BEGIN
	RETURN QUERY

    INSERT INTO "TriggerInvocations" ("TriggerID",
                                      "ActionID",
                                      "Timestamp")
    VALUES (triggerid,
            actionid,
            timestmp)
    RETURNING
        "TriggerInvocations"."ID",
        "TriggerInvocations"."TriggerID",
        "TriggerInvocations"."ActionID",
        "TriggerInvocations"."Timestamp";
END
$$;

GRANT EXECUTE ON FUNCTION triggerservice_createinvocation(bigint,bigint,timestamp without time zone) TO db_triggerservice;
