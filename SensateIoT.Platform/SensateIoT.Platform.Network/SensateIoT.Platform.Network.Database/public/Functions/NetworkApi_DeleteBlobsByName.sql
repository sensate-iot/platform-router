CREATE FUNCTION networkapi_deleteblobsbyname(sensorid VARCHAR(24), filename TEXT)
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
BEGIN

	RETURN QUERY	
	DELETE FROM "Blobs"
	WHERE "Blobs"."SensorID" = sensorid AND
	      "Blobs"."FileName" = filename
	RETURNING 
	       "Blobs"."ID",
	       "Blobs"."SensorID",
		   "Blobs"."FileName",
		   "Blobs"."Path",
		   "Blobs"."StorageType",
		   "Blobs"."Timestamp",
		   "Blobs"."FileSize";
END;
$$