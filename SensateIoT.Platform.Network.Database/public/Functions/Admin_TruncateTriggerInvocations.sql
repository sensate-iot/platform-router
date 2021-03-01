CREATE FUNCTION public.admin_truncatetriggerinvocations()
    RETURNS void
    LANGUAGE 'plpgsql'
AS $$
BEGIN
    DELETE FROM "TriggerInvocations"
	WHERE "Timestamp" < (now() - '31 days'::interval);
END
$$;
