# CS2-GeoGuessr

CS2-GeoGuessr is a GeoGuessr-style guessing game for Counter-Strike 2: you're shown a screenshot taken from somewhere on a CS2 map and have to click the spot on the map where you think it was taken - the closer the guess, the more points scored.

## Team

Product: CS2-GeoGuessr
Team: RDV - Roblox Developing Veterans
Team leader: Miron Beliajev

### Team Members

- Miron Beliajev (Team Leader, Full-stack) - [@meakomeow](https://github.com/meakomeow)
- Leon Sidun (Full-stack) - [@ilovekomaru](https://github.com/ilovekomaru)
- Erikas Atkociunas (Full-stack) - [@ErikasAtko](https://github.com/ErikasAtko)
- Lukas Stasytis (Full-stack) - [@ ]

Everyone on the team works across both backend and frontend to learn as much as possible, rather than sticking to fixed lanes.

## End-to-End Scenario

Your NOOB friend just carried you, and you've got no comeback because your hands were cold. Fire up CS2-GeoGuessr and flex your map knowledge instead: a screenshot from somewhere on Dust II pops up next to the full map. You study the lighting and geometry, then click the spot where you think it was taken. The game reveals the real location and scores you on how close you got. Beat your friend to prove you're not completely cooked.

## Roadmap

See [ROADMAP.md](./ROADMAP.md) for the Alpha / Beta / Final scope.

## Technology Baseline

- ASP.NET Core (backend-dominant design, per course requirements)
- React (frontend)

Stack decided; project scaffold has not been created yet (repository currently only contains this README, ROADMAP.md and a `.gitignore`).

## Branch Naming Rules

Pattern: `[initials]/[issue-number]-[short-title]`

Example - Miron picks up issue #3, "Add scoring logic":

`mb/3-add-scoring-logic`

- Initials first (lowercase), separated from the rest by `/`.
- Issue number next, then a short kebab-case description of the task.
- Branch off `main`; open a pull request back into `main` when ready for review.

## Code Formatting

- Backend (C#): run `dotnet format` before committing, once the backend project exists.
- Frontend (JavaScript/React): match whatever formatter (Prettier/ESLint) the frontend project is set up with, once it exists.

## Prerequisites

TBD - will be filled in once the ASP.NET Core + React project scaffold is created.

## Build and Run

TBD - will be filled in once the ASP.NET Core + React project scaffold is created.
