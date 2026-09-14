# CS2-GeoGuessr Roadmap

This roadmap follows the Alpha / Beta / Final structure expected from the lab project, and maps CS2-GeoGuessr's own features onto the graded technical requirements from each Lab Assignment - so "covering the requirements" means something concrete, not just a feature list.

## Alpha - matches Lab Assignment #1 (deadline: week 7, 1.5 points)

Features:

- [ ] Basic UI, singleplayer only
- [ ] Core gameplay loop: a screenshot from a CS2 map pops up, the full map pops up, the player clicks where on the map they think the screenshot was taken
- [ ] Scoring based on the distance between the guess and the correct location

Requirement coverage still needed:

- [ ] A user scenario that can be demonstrated end to end through a real interface (see [End-to-End Scenario](./README.md#end-to-end-scenario))
- [ ] Own `class`, `struct`, `record` and `enum` types, at least one of them immutable (e.g. an immutable `GuessResult` record, a `Point` struct for map coordinates, a `MapName` or `RoundStatus` enum)
- [ ] Property usage in a `struct` and a `class`
- [ ] Named and optional arguments in a real method signature (e.g. `CalculateScore(Point guess, Point actual, double maxDistance = 1000)`)
- [ ] An extension method (e.g. `IEnumerable<Round>.AverageScore()`)
- [ ] Iterating through collections the right way
- [ ] A stream used to load data (e.g. reading map/screenshot metadata from a file or web service)
- [ ] LINQ to Objects used where appropriate (or justified where it isn't)
- [ ] One standard .NET interface implemented (e.g. `IComparable<Round>` to sort rounds by score)
- [ ] All changes reviewed via pull requests, each with a description of what/why; every team member has authored at least 3 merged PRs and meaningfully reviewed at least 3 teammates' PRs
- [ ] Uniform coding style across the project

## Beta - matches Lab Assignment #2 (deadline: week 11, 1.5 points)

Features:

- [ ] Multiplayer support
- [ ] Create a lobby and play together with friends

Requirement coverage still needed:

- [ ] A relational database with Entity Framework storing all data (players, rounds, lobbies), instead of in-memory state
- [ ] A generic type with a generic method, demonstrating removed duplication (e.g. a generic `Repository<T>` used for `Round` and `Lobby`)
- [ ] At least one custom exception type, thrown and meaningfully handled (e.g. `LobbyFullException` when joining a full lobby)
- [ ] `async`/`await` for all I/O - no synchronous I/O
- [ ] A shared-memory scenario identified and handled with concurrent collections or proper synchronization (e.g. tracking active players in a lobby)
- [ ] Dependency Injection used everywhere reasonable - no dependencies created manually with `new`
- [ ] Unit and integration test coverage of at least 50%
- [ ] Same PR review requirements as Alpha
- [ ] Uniform coding style across the project

## Final - matches Lab Assignment #3 (deadline: week 15, 2.0 points)

Features:

- [ ] Variety of different maps to guess on
- [ ] Player statistics and profiles
- [ ] Account system
- [ ] Ranking system (maybe)
- [ ] Difficulty levels (maybe)

Requirement coverage still needed:

- [ ] A 4-6 minute demo video showing the app and its features, uploaded publicly (e.g. YouTube) or provided as a file
- [ ] Application in a stable state, able to demonstrate additional user flows on request
- [ ] Value proposition clearly articulated
- [ ] Application shown to solve the problem the team defined
- [ ] Unit and integration test coverage of at least 80%
- [ ] Entity Framework migrations used and run automatically
- [ ] A CI pipeline gating every pull request: automated tests plus at least one extra gate
- [ ] Live metrics or health/performance monitoring (OpenTelemetry or similar)
- [ ] Same PR review requirements as Alpha
- [ ] Uniform coding style across the project
