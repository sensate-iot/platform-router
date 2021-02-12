CREATE ROLE db_router;

GRANT EXECUTE ON FUNCTION router_gettriggers() TO db_router;
GRANT EXECUTE ON FUNCTION router_getlivedatahandlers() TO db_router;
GRANT EXECUTE ON FUNCTION router_gettriggersbyid(varchar) TO db_router;

GRANT SELECT ON TABLE "LiveDataHandlers" TO db_router;
GRANT SELECT ON TABLE "Triggers" TO db_router;
GRANT SELECT ON TABLE "SensorLinks" TO db_router;
GRANT SELECT ON TABLE "TriggerActions" TO db_router;
