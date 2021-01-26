CREATE FUNCTION dataapi_selectinvocationcount(idlist TEXT)
    RETURNS TABLE("Count" BIGINT)
    LANGUAGE plpgsql
AS $$
DECLARE sensorIds VARCHAR(24)[];
BEGIN
	sensorIds = ARRAY(SELECT DISTINCT UNNEST(string_to_array(idlist, ',')));

	RETURN QUERY

    SELECT COUNT(*)
    FROM "TriggerInvocations" AS inv
    INNER JOIN "Triggers" AS t ON t."SensorID" = ANY(sensorIds)
    WHERE inv."TriggerID" = t."ID";
END
$$;
