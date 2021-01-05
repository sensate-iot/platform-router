CREATE ROLE db_dataapi;

GRANT EXECUTE ON FUNCTION dataapi_selectinvocationcount(TEXT) TO db_dataapi;
GRANT EXECUTE ON FUNCTION dataapi_selectsensorlinkbysensorid(VARCHAR(24)) TO db_dataapi;
GRANT EXECUTE ON FUNCTION dataapi_selectsensorlinkbyuserid(UUID) TO db_dataapi;
GRANT EXECUTE ON FUNCTION dataapi_selectsensorlinkcountbyuserid(UUID) TO db_dataapi;

GRANT SELECT ON TABLE "TriggerActions" TO db_dataapi;
GRANT SELECT ON TABLE "Triggers" TO db_dataapi;
GRANT SELECT ON TABLE "SensorLinks" TO db_dataapi;
GRANT SELECT ON TABLE "TriggerInvocations" TO db_dataapi;
