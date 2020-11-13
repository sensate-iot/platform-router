---
--- Routing trigger lookup function.
---
--- @author Michel Megens
--- @email  michel@michelmegens.net
---

CREATE FUNCTION router_gettriggers()
    RETURNS TABLE(
        "SensorID" varchar(24),
        "ActionCount" BIGINT,
        "TextTrigger" BOOLEAN
    )
    LANGUAGE plpgsql
AS
$$
BEGIN
	RETURN QUERY
	SELECT "Triggers"."SensorID",
		   COUNT("TriggerActions"."ID") AS "ActionCount",
		   "Triggers"."FormalLanguage" IS NOT NULL AS "TextTrigger"
	FROM "Triggers"
	LEFT JOIN "TriggerActions" ON "Triggers"."ID" = "TriggerActions"."TriggerID"
	GROUP BY "Triggers"."SensorID", "Triggers"."FormalLanguage";
end;
$$;
