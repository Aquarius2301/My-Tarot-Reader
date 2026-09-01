---
name: frontend-react-architecture
description: Mandatory naming, folder, and data-fetching conventions for the React + TypeScript frontend, paired with the .NET Clean Architecture backend. MUST use this skill whenever creating or editing a component, page, hook, api file, types file, or i18n entry — even if the user only says "add a login page", "create a hook for X", "call this API from the UI" without mentioning conventions explicitly. Covers PascalCase/camelCase rules, file naming (x.hooks.ts, x.api.ts, x.types.ts), page/component folder structure (common/protected/public), the Controller-to-api-to-hooks-to-page call flow, TanStack Query usage (query vs mutation, cache invalidation), centralized route and theme constants, lazy-loaded pages, responsive/mobile design, error-message helpers, and i18n key structure.
---

# React + TypeScript Frontend — Conventions

This skill describes naming and architecture rules for the frontend. It mirrors the backend's Controller naming (see the .NET Clean Architecture skill) — the same `{N}` from `{N}Controller` drives the frontend file names too. Follow every rule below exactly.

## 1. Casing rules

- **PascalCase**: components, pages, layouts (both the file and the exported symbol).
- **camelCase**: hooks, utils, api files, and any other non-component file.

## 2. File naming

- Hooks / utils / api files follow `x.{folder_name}.ts`:
  - `apiClient.utils.ts` (utils/)
  - `auth.api.ts` (api/)
  - `auth.hooks.ts` or `useAuth.hooks.ts` (hooks/)
  - `auth.types.ts` (types/)
- **Hooks file naming depends on how many hooks it exports**:
  - Multiple hooks in one file → `x.hooks.ts` (e.g. `auth.hooks.ts` exporting `useLogin`, `useLogout`, `useRefreshToken`...).
  - Exactly one hook in the file → `useX.hooks.ts` (e.g. `useAuth.hooks.ts` exporting only `useAuth`).
- Every folder has an `index.ts` that re-exports (barrel) every file in that folder.

## 3. Page & component folder structure

Each page or component gets its own folder:

```
PageOrComponentName/
├── index.ts              # barrel export
├── PageOrComponentName.tsx
└── components/            # only if the page/component is complex enough to split
    ├── index.ts
    └── SomeSubComponent.tsx
```

- Only add a `components/` subfolder when the page is complex enough to warrant splitting into sub-components — don't create it preemptively for a simple page.

Top-level `pages/` folder is split into three groups:

- `common/` — components shared across multiple pages.
- `protected/` — pages that require authentication.
- `public/` — guest-accessible pages.

## 4. API call flow

Every API call follows this fixed chain, keyed off the backend Controller name (`{N}`):

```
{N}Controller (backend)  ->  {n}.api.ts (api/)  ->  {n}.hooks.ts (hooks/api/, TanStack Query)  ->  Page
```

Example: `AuthController` → `auth.api.ts` → `auth.hooks.ts` → the page that needs auth.

- Pages **never** call `apiClient`/axios directly — always go through a TanStack Query hook.
- Each `x.api.ts` file only defines the raw axios calls (one function per backend endpoint), no React Query logic inside it.

## 5. Types (Request/Response)

- Use `interface`, not `type`, for API request/response shapes.
- Each API function has its own dedicated Request and Response interface.
- All request/response interfaces for one api file live together in the matching `x.types.ts` (e.g. `auth.api.ts` uses `auth.types.ts`).
- Interface names can mirror the backend's Request/Response DTO names directly — e.g. backend `CreateProductRequest`/`ProductResponse` → frontend `CreateProductRequest`/`ProductResponse` interfaces with matching fields.

## 6. TanStack Query hooks

- Each hook uses **exactly one** API call — never combine two API calls in one hook.
- **GET** → query hook (`useQuery`).
- **POST / PUT / DELETE** → mutation hook (`useMutation`).
- After a successful mutation, always invalidate the query cache for the affected query key(s) via `queryClient.invalidateQueries`.
- Query keys should be centralized (a `QUERY_KEY` constant per query), not inline string literals scattered across hooks.

```ts
// api/tarot.api.ts
export const tarotApi = {
  createDrawForAuthHistory: (card: CreateDrawForAuthRequest): Promise<void> => {
    return axiosClient.post(`${API_URL.TAROT.AUTH_DRAW}`, card);
  },
};

// hooks/api/tarot.hooks.ts
export const useCreateDrawForAuth = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (card: CreateDrawForAuthRequest) =>
      tarotApi.createDrawForAuthHistory(card),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: LAST_DRAWN_CARD_QUERY_KEY });
    },
  });
};

// Page
const { mutate, isPending } = useCreateDrawForAuth();

mutate(
  { cardCode: selectedCard.cardCode, isReversed: selectedCard.isReversed },
  {
    onSuccess: () => setReDraw(false),
    onError: (err) => message.error(getErrorMessage(err)),
  },
);
```

## 7. Routes & URLs

- Every API url and every web (frontend route) url lives in a single file: `constants/route.constants.ts`, at the project root's constants folder.
- No hardcoded URL strings anywhere else — always reference this file.

## 8. Styling & theme

- `constants/theme.constants.ts` already holds the project's theme constants (colors, spacing, breakpoints, etc.) — always reuse it instead of hardcoding values, to keep styling consistent across the app.
- Every page and component must be **responsive**, with a working mobile layout — never assume desktop-only viewport.

## 9. Pages

- Pages are **lazy-loaded** (`React.lazy` + `Suspense`, or the router's built-in lazy loading).
- Pages use TanStack Query hooks for all data fetching and mutation — never call `apiClient` directly in a page.

## 10. Error handling

- `utils/error.utils.ts` exposes:
  - `getErrorMessage(err)` — general error message extraction from an API error, used in `onError` callbacks (e.g. with an antd `message.error(...)`).
  - `getFormFieldErrors(err)` — field-level error extraction for forms, mapping backend validation errors to form field errors.
- Never read `err.response.data...` ad hoc in a component — always go through these two helpers.

## 11. i18n (`i18n/locales`)

- `errors/` folder: backend error messages, keyed as `error.server.xxx` (`server` marks it as coming from the backend).
- `pages/` folder: page-level translations, keyed as `page.{page_name}.{key}`.

## 12. Checklist for a new frontend feature

- [ ] Component/Page/Layout file and symbol are PascalCase; hook/util/api/types files are camelCase.
- [ ] Hook file named `x.hooks.ts` (multiple hooks) or `useX.hooks.ts` (single hook).
- [ ] Folder has an `index.ts` barrel export.
- [ ] Page/component folder has `index.ts` + main `.tsx`, with `components/` only if complexity warrants it.
- [ ] Page placed in the correct `pages/` group: `common/`, `protected/`, or `public/`.
- [ ] Flow respected: `{N}Controller` → `{n}.api.ts` → `{n}.hooks.ts` → Page (no direct `apiClient` calls in pages).
- [ ] Request/Response are `interface`s in `{n}.types.ts`, one pair per API function, named after the matching backend DTOs where practical.
- [ ] Each TanStack hook wraps exactly one API call; GET → `useQuery`, POST/PUT/DELETE → `useMutation`.
- [ ] Mutation hooks invalidate the relevant query key(s) on success.
- [ ] All URLs (API + web) live in `constants/route.constants.ts` — no hardcoded strings.
- [ ] Styling reuses `constants/theme.constants.ts` — no hardcoded colors/spacing values.
- [ ] Layout is responsive and verified on mobile viewport.
- [ ] Page is lazy-loaded.
- [ ] Errors surfaced via `getErrorMessage` / `getFormFieldErrors`, never raw `err.response.data`.
- [ ] Backend error keys added under `error.server.xxx`; page copy added under `page.{page_name}.{key}`.
