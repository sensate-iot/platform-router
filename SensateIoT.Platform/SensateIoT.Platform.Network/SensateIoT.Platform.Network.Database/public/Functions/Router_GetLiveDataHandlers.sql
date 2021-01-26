---
--- Get the active live data handlers.
---

CREATE FUNCTION router_getlivedatahandlers()
	RETURNS TABLE(
		"Name" VARCHAR(64)
	)
	LANGUAGE PLPGSQL
AS
$$
BEGIN
	RETURN QUERY
	SELECT ldh."Name"
	FROM "LiveDataHandlers" AS ldh
	WHERE ldh."Enabled" = True;
END;
$$
