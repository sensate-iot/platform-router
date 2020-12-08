---
--- Select triggers and last invocations based.
---
--- @author Michel Megens
--- @email  michel@michelmegens.net
---

CREATE FUNCTION triggerservice_gettriggersbysensorid(idlist TEXT)
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
		"Message" TEXT,
		"LastInvocation" TIMESTAMP
    )
    LANGUAGE plpgsql
AS
$$
DECLARE sensorIds VARCHAR(24)[];
BEGIN
	sensorIds = ARRAY(SELECT DISTINCT UNNEST(string_to_array(idlist, ',')));
	
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
		ta."Message",
		inv."Timestamp"
	FROM "TriggerActions" AS ta
	INNER JOIN "Triggers" AS t ON t."ID" = ta."TriggerID"
	LEFT JOIN (
		SELECT tinv."ActionID", MAX(tinv."Timestamp") AS "Timestamp"
		FROM "TriggerInvocations" AS tinv
		GROUP BY tinv."ActionID"
	) inv ON inv."ActionID" = ta."ID"
	WHERE t."SensorID" = ANY (sensorIds)
	ORDER BY ta."ID";
END;
$$;
