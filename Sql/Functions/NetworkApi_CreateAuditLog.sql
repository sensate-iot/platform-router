CREATE FUNCTION networkapi_createauditlog(
	route text,
	method integer,
	address inet,
	author text)
    RETURNS void
    LANGUAGE 'plpgsql'

    COST 100
    VOLATILE 
    
AS
$$
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
END;
$$
