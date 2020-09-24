create function authorizationctx_getapikey(key TEXT)
    returns table(apikey text, userid text, revoked boolean, readonly boolean)
	language plpgsql
as $$
begin
return query
        SELECT "ApiKey", "UserId", "Revoked", "ReadOnly" FROM "ApiKeys"
        WHERE "Type" = 0 AND "ApiKey" = key;
end;
$$;
