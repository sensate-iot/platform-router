CREATE FUNCTION networkapi_selectblobbyid(id BIGINT)
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
	SELECT b."ID",
	       b."SensorID",
		   b."FileName",
		   b."Path",
		   b."StorageType",
		   b."Timestamp",
		   b."FileSize"
	FROM "Blobs" AS b
	WHERE b."ID" = id;
END;
$$
