CREATE FUNCTION generic_getblobs(idlist TEXT,
                                      start TIMESTAMP,
                                      "end" TIMESTAMP,
                                      ofst INTEGER DEFAULT NULL,
                                      lim INTEGER DEFAULT NULL,
                                      direction VARCHAR(12) DEFAULT 'ASC'
                                      )
    RETURNS TABLE(
        "ID" BIGINT,
        "SensorID" VARCHAR(24),
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

    IF upper(direction) NOT IN ('ASC', 'DESC', 'ASCENDING', 'DESCENDING') THEN
      RAISE EXCEPTION 'Unexpected value for parameter direction.
                       Allowed: ASC, DESC, ASCENDING, DESCENDING. Default: ASC';
   END IF;

	RETURN QUERY EXECUTE
	    format('SELECT "ID", "SensorID", "Path", "StorageType", "Timestamp", "FileSize" ' ||
	           'FROM "Blobs" ' ||
	           'WHERE "Timestamp" >= $1 AND "Timestamp" < $2 AND "SensorID" = ANY($3) ' ||
	           'ORDER BY "Timestamp" %s ' ||
	           'OFFSET %s ' ||
	           'LIMIT %s',
	        direction, ofst, lim)
    USING start, "end", sensorIds;
END
$$;
