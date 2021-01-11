CREATE OR REPLACE FUNCTION public.networkapi_incrementrequestcount(
	key text)
    RETURNS void
    LANGUAGE 'plpgsql'

    COST 100
    VOLATILE 
    
AS
$$
BEGIN
    UPDATE "ApiKeys"
        SET "RequestCount" = "RequestCount" + 1
    WHERE "ApiKey" = key;
END;
$$;