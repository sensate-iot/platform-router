create function authorizationctx_getapikeys()
    returns table(apikey text, revoked boolean, readonly boolean)
	language plpgsql
as $$
begin
return query
        SELECT "ApiKey", "Revoked", "ReadOnly" FROM "ApiKeys"
        WHERE "Type" = 0;
end;
$$;
