CREATE FUNCTION public.livedataservice_createauditlog(
	route text,
	method integer,
	address inet,
	author text)
    RETURNS void
    LANGUAGE 'plpgsql'   
AS $BODY$
BEGIN
    INSERT INTO "AuditLogs" ("Route",
                             "Method",
                             "Address",
                             "AuthorId",
                             "Timestamp")
    VALUES (route,
            method,
            address,
            author,
            NOW());
END
$BODY$;
