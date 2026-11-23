-- Vault v2: zero-knowledge key model.
--  * hardening of public.password (privileges, FK, index, explicit RLS policy)
--  * entry versioning + client-generated entry UUID (bound into AES-GCM AAD)
--  * public.user_keys holding the vault key wrapped with the password-derived KEK
--
-- Existing entries are encrypted with the old key model and cannot be migrated
-- (the server never had the key), so they are removed. Old accounts must be
-- deleted manually (Authentication -> Users): their GoTrue password is the old
-- master password, while clients now authenticate with a derived auth key.
--
-- `supabase db push` applies the whole file atomically.

-- ---------------------------------------------------------------------------
-- password: wipe v1 data
-- ---------------------------------------------------------------------------
TRUNCATE TABLE public.password RESTART IDENTITY;

-- ---------------------------------------------------------------------------
-- password: naming leftovers from the old "vaults" table
-- ---------------------------------------------------------------------------
ALTER TABLE public.password RENAME CONSTRAINT vaults_pkey TO password_pkey;
ALTER SEQUENCE public.vaults_id_seq RENAME TO password_id_seq;

-- ---------------------------------------------------------------------------
-- password: new columns
-- ---------------------------------------------------------------------------
ALTER TABLE public.password
    ADD COLUMN version    smallint NOT NULL,
    ADD COLUMN entry_uuid uuid     NOT NULL;

ALTER TABLE public.password
    ADD CONSTRAINT password_entry_uuid_key UNIQUE (entry_uuid);

-- ---------------------------------------------------------------------------
-- password: FK + index
-- ---------------------------------------------------------------------------
ALTER TABLE public.password
    ADD CONSTRAINT password_user_id_fkey
    FOREIGN KEY (user_id) REFERENCES auth.users(id) ON DELETE CASCADE;

CREATE INDEX IF NOT EXISTS password_user_id_idx ON public.password USING btree (user_id);

-- ---------------------------------------------------------------------------
-- password: RLS policy
-- ---------------------------------------------------------------------------
DROP POLICY IF EXISTS "Enable all for users based on user_id" ON public.password;

CREATE POLICY "owner only" ON public.password
    FOR ALL
    TO authenticated
    USING ((SELECT auth.uid()) = user_id)
    WITH CHECK ((SELECT auth.uid()) = user_id);

-- ---------------------------------------------------------------------------
-- password: privileges (anon: none, authenticated: DML only)
-- ---------------------------------------------------------------------------
REVOKE ALL ON TABLE public.password FROM anon, authenticated;
GRANT SELECT, INSERT, UPDATE, DELETE ON TABLE public.password TO authenticated;

REVOKE ALL ON SEQUENCE public.password_id_seq FROM anon, authenticated;
GRANT USAGE, SELECT ON SEQUENCE public.password_id_seq TO authenticated;

-- ---------------------------------------------------------------------------
-- user_keys: one row per user, vault key wrapped with the KEK
-- ---------------------------------------------------------------------------
CREATE TABLE public.user_keys (
    user_id           uuid        DEFAULT auth.uid() NOT NULL,
    kdf_version       smallint    NOT NULL,
    wrapped_vault_key text        NOT NULL,   -- base64(nonce | tag | ciphertext)
    created_at        timestamptz DEFAULT now() NOT NULL,
    updated_at        timestamptz DEFAULT now() NOT NULL,
    CONSTRAINT user_keys_pkey PRIMARY KEY (user_id),
    CONSTRAINT user_keys_user_id_fkey FOREIGN KEY (user_id) REFERENCES auth.users(id) ON DELETE CASCADE
);

ALTER TABLE public.user_keys OWNER TO postgres;

CREATE OR REPLACE TRIGGER handle_updated_at BEFORE UPDATE ON public.user_keys
    FOR EACH ROW EXECUTE FUNCTION extensions.moddatetime('updated_at');

ALTER TABLE public.user_keys ENABLE ROW LEVEL SECURITY;

CREATE POLICY "owner only" ON public.user_keys
    FOR ALL
    TO authenticated
    USING ((SELECT auth.uid()) = user_id)
    WITH CHECK ((SELECT auth.uid()) = user_id);

REVOKE ALL ON TABLE public.user_keys FROM anon, authenticated;
GRANT SELECT, INSERT, UPDATE, DELETE ON TABLE public.user_keys TO authenticated;
GRANT ALL ON TABLE public.user_keys TO service_role;

-- ---------------------------------------------------------------------------
-- Default privileges for future objects in schema public:
-- nothing for anon, only DML for authenticated (new tables still need RLS).
-- ---------------------------------------------------------------------------
ALTER DEFAULT PRIVILEGES FOR ROLE postgres IN SCHEMA public REVOKE ALL ON TABLES    FROM anon, authenticated;
ALTER DEFAULT PRIVILEGES FOR ROLE postgres IN SCHEMA public REVOKE ALL ON SEQUENCES FROM anon, authenticated;
ALTER DEFAULT PRIVILEGES FOR ROLE postgres IN SCHEMA public REVOKE ALL ON FUNCTIONS FROM anon;
ALTER DEFAULT PRIVILEGES FOR ROLE postgres IN SCHEMA public GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO authenticated;
ALTER DEFAULT PRIVILEGES FOR ROLE postgres IN SCHEMA public GRANT USAGE, SELECT ON SEQUENCES TO authenticated;
