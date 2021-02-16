CREATE FUNCTION networkapi_deleteblobbyid(id BIGINT)
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
	WHERE "Blobs"."ID" = id
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