# AGENTS.md

Guidance for AI coding agents (and humans) working in this repository.

## Project overview

**My Tarot Reader** — an AI-assisted tarot reading web application.

- **Backend:** ASP.NET Core 8 Web API (Clean Architecture) in `Backend/`
- **Frontend:** React 19 + Vite + TypeScript SPA in `Frontend/`
- **Hosting:** Render free tier (API, sleeps when idle → first request 10–30 s cold start) + Vercel (SPA)

## Feature surface

- Daily tarot card draw — guest (Redis cooldown + device fingerprint) or signed-in user
- 78-card Rider–Waite library, reversed cards included
- Daily check-in with a 7-day streak cycle & white/red coin rewards
- Google OAuth login (HttpOnly cookies, device-bound refresh tokens)
- AI tarot readings (Google Gemini), bilingual UI (en / vi)

## Conventions live in skills

Before writing or reviewing code, load the matching skill — conventions (folder layout, naming, error codes, API envelope, query keys... ) are documented there:

- **HTTP / REST docs (not conventions):** `Backend/README.md`, `Frontend/README.md`, root `README.md`
- **`.agent/skills/backend-dotnet-architecture/SKILL.md`** — backend Clean Architecture: layering, folder tree, endpoints, error/infra conventions
- **`.agent/skills/backend-dotnet-testing/SKILL.md`** — backend unit-test conventions (naming, structure, assertions)
- **`.agent/skills/frontend-react-architecture/SKILL.md`** — frontend React conventions: data flow, folder tree, namespaces, styling, i18n

## Working rules (always)

1. **Read the relevant skill first**, then follow it — skill files are the source of truth for convention.
2. **Backend:** always run through `Backend/scripts/*.{sh,cmd}` (run-local, clean-build, add-migration, update-database), never raw `dotnet run` / `dotnet ef`.
3. **Never commit real secrets.** `appsettings*.json` and `.env*` keep placeholders / local-only values; document keys in READMEs, not values.
4. **Frontend:** run `npm run lint` and `npm run build` to verify before finishing.
5. Keep changes minimal and consistent with the existing barrel/folder conventions.
6. Only read .env and appsettings.json for config. DO NOT read .env.local, appsettings.Development.json
