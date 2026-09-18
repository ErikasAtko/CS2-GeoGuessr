-- Wipes the game schema so it can be rebuilt from the EF migrations.
--
-- HOW TO RUN (Rider, no local Postgres needed):
--   1. Open this file in Rider.
--   2. In the toolbar above the editor, pick your data source - the personal
--      cs2guessr_<name> one. Check this every time: cs2guessr_shared mirrors
--      main, and running this against it wipes the whole team's data.
--   3. Execute the whole file (Ctrl+Enter, then "Execute script" if asked).
--
-- AFTERWARD, from the CS2-GEO folder in a terminal:
--   dotnet ef database update    -- recreates the tables from Migrations/
--   dotnet run                   -- seeds maps and locations on startup
--
-- WHY EACH LINE:
--   locations has a foreign key to maps (fk_locations_maps_map_id), so it is
--   dropped first. CASCADE removes that constraint along with the table, so
--   the order is not strictly required - but dropping the child first keeps
--   the dependency visible to whoever reads this next.
--
--   __EFMigrationsHistory is EF's own bookkeeping table: it records which
--   migrations have already been applied. Leaving it behind is the classic
--   mistake - EF would see InitialCreate as "already applied", skip it, and
--   you would end up with an empty database that EF believes is up to date.
--   It is double-quoted because the name is mixed case; unquoted, Postgres
--   would fold it to __efmigrationshistory and drop nothing.

DROP TABLE IF EXISTS locations CASCADE;
DROP TABLE IF EXISTS maps CASCADE;
DROP TABLE IF EXISTS "__EFMigrationsHistory" CASCADE;
