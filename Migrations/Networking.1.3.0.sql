CREATE OR REPLACE FUNCTION triggerservice_gettriggersbysensorid(id VARCHAR(24))
    RETURNS TABLE(
		"TriggerID" BIGINT,
		"ActionID" BIGINT,
		"SensorID" VARCHAR(24),
		"KeyValue" VARCHAR(32),
		"LowerEdge" NUMERIC,
		"UpperEdge" NUMERIC,
		"FormalLanguage" TEXT,
		"Type" INTEGER,
		"Channel" INTEGER,
		"Target" VARCHAR(255),
		"Message" TEXT
    )
    LANGUAGE plpgsql
AS
$$
BEGIN
	RETURN QUERY
	SELECT 
		DISTINCT ON (ta."ID")
		t."ID" AS "TriggerID",
		ta."ID" AS "ActionID",
		t."SensorID",
		t."KeyValue",
		t."LowerEdge",
		t."UpperEdge",
		t."FormalLanguage",
		t."Type",
		ta."Channel",
		ta."Target",
		ta."Message"
	FROM "TriggerActions" AS ta
	INNER JOIN "Triggers" AS t ON t."ID" = ta."TriggerID"
	WHERE t."SensorID" = id
	ORDER BY ta."ID";
END;
$$;

CREATE OR REPLACE FUNCTION triggerservice_gettriggers()
    RETURNS TABLE(
		"TriggerID" BIGINT,
		"ActionID" BIGINT,
		"SensorID" VARCHAR(24),
		"KeyValue" VARCHAR(32),
		"LowerEdge" NUMERIC,
		"UpperEdge" NUMERIC,
		"FormalLanguage" TEXT,
		"Type" INTEGER,
		"Channel" INTEGER,
		"Target" VARCHAR(255),
		"Message" TEXT
    )
    LANGUAGE plpgsql
AS
$$
BEGIN
	RETURN QUERY
	SELECT 
		DISTINCT ON (ta."ID")
		t."ID" AS "TriggerID",
		ta."ID" AS "ActionID",
		t."SensorID",
		t."KeyValue",
		t."LowerEdge",
		t."UpperEdge",
		t."FormalLanguage",
		t."Type",
		ta."Channel",
		ta."Target",
		ta."Message"
	FROM "TriggerActions" AS ta
	INNER JOIN "Triggers" AS t ON t."ID" = ta."TriggerID"
	ORDER BY ta."ID";
END;
$$;

CREATE OR REPLACE FUNCTION generic_getblobs(idlist TEXT,
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

GRANT EXECUTE ON FUNCTION public.generic_getblobs(text, timestamp without time zone, timestamp without time zone, integer, integer, character varying) TO db_dataapi;
GRANT EXECUTE ON FUNCTION public.generic_getblobs(text, timestamp without time zone, timestamp without time zone, integer, integer, character varying) TO db_networkapi;
REVOKE ALL ON FUNCTION public.generic_getblobs(text, timestamp without time zone, timestamp without time zone, integer, integer, character varying) FROM PUBLIC;

GRANT EXECUTE ON FUNCTION triggerservice_gettriggers() TO db_triggerservice;
GRANT EXECUTE ON FUNCTION triggerservice_gettriggersbysensorid(id VARCHAR(24)) TO db_triggerservice;

DROP FUNCTION triggerservice_createinvocation(triggerid BIGINT, actionid BIGINT, timestmp TIMESTAMP);
DROP FUNCTION public.admin_truncatetriggerinvocations();
DROP FUNCTION dataapi_selectinvocationcount(idlist TEXT);
DROP TABLE "TriggerInvocations";
