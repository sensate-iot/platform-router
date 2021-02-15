---
--- Delete (bulk) sensor relations.
---
--- @author Michel Megens
--- @email  michel@michelmegens.net
---

CREATE FUNCTION networkapi_deletebysensorids(idlist TEXT)
    RETURNS VOID
    LANGUAGE plpgsql
AS
$$
DECLARE sensorIds VARCHAR(24)[];
BEGIN
	sensorIds = ARRAY(SELECT DISTINCT UNNEST(string_to_array(idlist, ',')));
	
	-- Delete triggers/actions/invocations
	DELETE FROM "Triggers"
	WHERE "SensorID" = ANY(sensorIds);
	
	-- Delete blobs
	DELETE FROM "Blobs"
	WHERE "SensorID" = ANY(sensorIds);
	
	-- Delete sensor links
	DELETE FROM "SensorLinks"
	WHERE "SensorId" = ANY(sensorIds);
END;
$$;
