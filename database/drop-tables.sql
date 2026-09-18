-- Wipes the game schema so it can be rebuilt from the EF migrations.
--

DROP TABLE IF EXISTS locations CASCADE;
DROP TABLE IF EXISTS maps CASCADE;
DROP TABLE IF EXISTS "__EFMigrationsHistory" CASCADE;
