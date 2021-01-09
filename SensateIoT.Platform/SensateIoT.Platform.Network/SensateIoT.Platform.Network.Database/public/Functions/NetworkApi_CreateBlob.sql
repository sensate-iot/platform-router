CREATE FUNCTION networkapi_createblob(sensorid VARCHAR(24), filename TEXT, path TEXT, storage INTEGER, filesize INTEGER)
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

    INSERT INTO "Blobs" ("SensorID",
                         "FileName",
                         "Path",
                         "StorageType",
                         "FileSize",
                         "Timestamp")
    VALUES (sensorid,
            filename,
            path,
            storage,
            filesize,
            NOW())
    RETURNING
       	"Blobs"."ID",
		"Blobs"."SensorID",
		"Blobs"."FileName",
		"Blobs"."Path",
		"Blobs"."StorageType",
		"Blobs"."Timestamp",
		"Blobs"."FileSize";
END;
$$;
