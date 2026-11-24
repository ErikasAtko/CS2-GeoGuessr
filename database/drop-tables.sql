-- Wipes the game schema so it can be rebuilt from the EF migrations: the next app start
-- recreates every table and seeds the maps again (see README, "Remaking DB").
--
-- Every table the migrations create has to be listed here, or the next start fails trying to
-- create a table that is still there. Add a line whenever a migration adds a table.

DROP TABLE IF EXISTS
    player_guesses,
    match_participants,
    match_rounds,
    matches,
    lobby_memberships,
    lobbies,
    guest_sessions,
    players,
    locations,
    maps,
    "__EFMigrationsHistory"
    CASCADE;
