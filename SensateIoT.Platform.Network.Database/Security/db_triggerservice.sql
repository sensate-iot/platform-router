CREATE ROLE db_triggerservice;

GRANT EXECUTE ON FUNCTION triggerservice_gettriggersbysensorid(TEXT) TO db_triggerservice;
GRANT EXECUTE ON FUNCTION triggerservice_createinvocation(bigint,bigint,timestamp without time zone) TO db_triggerservice;

GRANT SELECT ON TABLE "TriggerActions" TO db_triggerservice;
GRANT SELECT ON TABLE "Triggers" TO db_triggerservice;
GRANT SELECT, INSERT, UPDATE, DELETE ON TABLE "TriggerInvocations" TO db_triggerservice;
