# CLAUDE.md

## Project overview

`MyTarotReader` — a full-stack app with a .NET 8 Clean Architecture backend and a React + TypeScript frontend.

## Tech stack

- **Backend**: .NET 8, EF Core, Clean Architecture (Api / Application / Infrastructure / Domain).
- **Frontend**: React + TypeScript, TanStack Query, `<UI library, e.g. Ant Design>`, i18n.

## Conventions — always follow the skills

This repo has two skills that define **mandatory** conventions. Consult and follow them for any related task, even if the request doesn't mention "architecture" or "convention" explicitly:

- **`dotnet-clean-architecture-api`** — backend: layering, Controller/Service/Interface/DTO naming, `ApiResponse` shape, `JwtHelper`, XML doc comments, EF Core + Api performance rules.
- **`react-typescript-frontend`** — frontend: PascalCase/camelCase rules, file naming (`x.hooks.ts`, `x.api.ts`, `x.types.ts`), page/component folder structure, the Controller → api → hooks → page flow, TanStack Query usage, route/theme constants, i18n keys.

Do not deviate from either skill's naming, folder structure, or code shape without being told to.

## Essential commands

1. Backend

- Run local

```
./scripts/run-local.sh
```

- Remove bin obj folder and build

```
./scripts/clean-build.sh
```

- Add migrations, the migration files are saved on `Infrastructure/Persistence/

```
./scripts/add-migration.sh <name>
```

- Update database

```
./scripts/update-database.sh

```

2. Frontend

- Run local

```
npm run dev
```

- Build project

```
npm run build
```

## Key conventions at a glance

- Backend dependency flow: `Api -> Application -> Domain` and `Api -> Infrastructure -> Application -> Domain`. Api never injects Infrastructure directly.
- Controller routes: `api/v{n}/{resource}`; methods are `Async`, take `CancellationToken`, return `Ok(ApiResponse.Success(...))`; `userId` always comes from `JwtHelper.GetUserId`, never a parameter.
- DTOs are `record`s with full XML doc; entities are `class`es with per-property XML doc.
- Frontend API flow: `{N}Controller` → `{n}.api.ts` → `{n}.hooks.ts` (TanStack Query) → Page. Pages never call `apiClient` directly.
- One TanStack hook = one API call. GET → `useQuery`, POST/PUT/DELETE → `useMutation` + cache invalidation on success.
- All URLs in `constants/route.constants.ts`; all theme values in `constants/theme.constants.ts`. No hardcoded strings/colors.
- Every page/component must be responsive (mobile-friendly).
- Errors surfaced via `getErrorMessage` / `getFormFieldErrors`, never raw `err.response.data`.

## Before finishing any change

- New backend entity → DbSet in `IAppDbContext`/`AppDbContext` + EF Core Configuration + migration via the project's script.
- New backend service → registered in `DIExtension`.
- New frontend API → matching `{n}.api.ts`, `{n}.types.ts`, `{n}.hooks.ts`, and an `index.ts` barrel export in every touched folder.
- Run the relevant checklist at the end of the two skills above before considering a feature done.

## After finishing any changeAfter

- Always build the project to make sure the project run properly
- Fix the error and warning (Warning as error)
