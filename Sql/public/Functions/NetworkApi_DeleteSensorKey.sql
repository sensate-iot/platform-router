
CREATE FUNCTION networkapi_deletesensorkey(key TEXT)
    RETURNS VOID
    LANGUAGE plpgsql
AS $$
BEGIN
    DELETE FROM "ApiKeys"
    WHERE "ApiKey" = key;
END;
$$;
