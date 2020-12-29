CREATE FUNCTION networkapi_selectblobs(idlist TEXT, offst INTEGER DEFAULT NULL, lim INTEGER DEFAULT NULL)
    RETURNS TABLE(
       	"ID" BIGINT,
		"SensorID" VARCHAR(24),
		"FileName" TEXT,
		"Path" TEXT,
		"StorageType" INTEGER,
		"Timestamp" TIMESTAMP,
		"FileSize" BIGINT
                 )
    LANGUAGE plpgsql
AS $$
DECLARE sensorIds VARCHAR(24)[];
BEGIN
	sensorIds = ARRAY(SELECT DISTINCT UNNEST(string_to_array(idlist, ',')));

	RETURN QUERY	
	SELECT b."ID",
	       b."SensorID",
		   b."FileName",
		   b."Path",
		   b."StorageType",
		   b."Timestamp",
		   b."FileSize"
	FROM "Blobs" AS b
	WHERE b."SensorID" = ANY (sensorIds)
	OFFSET offst
	LIMIT lim;
END;
$$
