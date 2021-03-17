CREATE ROLE db_triggerservice;

GRANT EXECUTE ON FUNCTION triggerservice_gettriggers() TO db_triggerservice;
GRANT EXECUTE ON FUNCTION triggerservice_gettriggersbysensorid(id VARCHAR(24)) TO db_triggerservice;

GRANT SELECT ON TABLE "TriggerActions" TO db_triggerservice;
GRANT SELECT ON TABLE "Triggers" TO db_triggerservice;
