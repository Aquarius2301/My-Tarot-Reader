---
name: frontend-react-architecture
description: >-
  React 19 + Vite + TypeScript frontend architecture conventions for Khang's My Tarot Reader project (antd v6, TanStack Query v5, react-router v7, zustand, react-i18next, axios). MUST be used whenever writing, reviewing, or editing Frontend code: pages, components, API/hooks layers, routing, i18n, styling, or state. Trigger on requests like "add a page for...", "fix this component...", "wire up this API...", "refactor this hook...", or any TypeScript/React feature work under Frontend/.
---

# Frontend React Architecture — My Tarot Reader

Standard conventions for the My Tarot Reader React frontend. Pairs with the backend `backend-dotnet-architecture` skill — the frontend mirrors its `ApiResponse<T>` envelope, DTO naming (`*Request`/`*Result`), and ordering (`POST/PUT/DELETE => 200 + null data`, cache invalidation).

## 1. Stack

- **React 19**, **TypeScript 6** (`verbatimModuleSyntax`, `erasableSyntaxOnly` — NO TS `enum`, use `as const` arrays + unions).
- **Vite 8** build; path alias `@/*` -> `src/*` (tsconfig + vite.config).
- **react-router-dom v7** — declarative `<BrowserRouter>/<Routes>` mode (no `createBrowserRouter`).
- **@tanstack/react-query v5** — server state, used ONLY in `src/hooks/api/`.
- **antd v6** — UI kit; styling via `ConfigProvider` theme + `theme.useToken()`.
- **axios** — HTTP client with envelope unwrap + auth interceptors.
- **react-i18next** — i18n, single `translation` namespace, default `vi`, fallback `en`.
- **zustand v5** (`persist`) — client-only UI state (theme/language).
- **@fingerprintjs/fingerprintjs** — device fingerprint -> `X-Device-Id` header.
- **oxlint** — linting (`npm run lint`). **@ant-design/icons** must be declared in `package.json` (used project-wide).

## 2. Layering & data flow

NEVER call `src/api` directly from a Page or Component. Data flows strictly top-down:

```
Page/Component
   └── src/hooks/api/<domain>.hooks.ts   // TanStack Query (useQuery/useMutation/useQueryClient)
          └── src/api/<domain>.api.ts     // axios calls, typed with DTOs; no React
                 └── axiosClient (src/api/config.api.ts)
                        └── Backend API
```

- **`src/api/*.api.ts`** — pure HTTP. Object literal of arrow fns named `<action>(<args>): Promise<T>`. Every read returns the unwrapped payload; every mutation returns `Promise<void>`.
- **`src/hooks/api/*.hooks.ts`** — the ONLY place `useQuery`/`useMutation` live. Components consume hooks, never api.
- Components never touch `queryClient` directly; mutations invalidate via the hook's `onSuccess`.

## 3. Canonical folder tree

```
src/
├── api/
│   ├── config.api.ts       # axiosClient: withCredentials, X-Device-Id, unwrap, refresh queue
│   ├── url.api.ts          # API_URL endpoint map (as const)
│   ├── index.ts            # barrel
│   └── <domain>.api.ts     # one file per backend controller (auth, tarot, streak, wallet, ...)
├── components/
│   ├── index.ts            # barrel re-exporting component folders
│   └── <Component>/
│       ├── index.ts
│       └── <ComponentName>.tsx
├── constants/
│   ├── index.ts
│   └── <domain>.constants.ts   # as const data + helpers (theme, tarot, language, streak)
├── hooks/
│   ├── api/                # TanStack Query hooks  (+ queryKey.ts)
│   ├── custom/             # generic hooks (useDocumentTitle)
│   └── stores/             # zustand stores (useThemeStore, useLanguageStore)
├── i18n/
│   ├── index.ts            # i18next init, merges all locale modules
│   └── locales/<domain>/{en,vi}.<domain>.ts
├── pages/
│   ├── auth/               # authenticated pages
│   ├── guest/              # public pages
│   └── shared/             # shared page-level sections (home, tarot, ...)
├── routes/
│   ├── AppRouter.tsx       # lazy routes, PublicRoute/ProtectedRoute
│   ├── url.routes.ts       # WEB_URL map (as const)
│   ├── index.ts
│   └── components/         # ProtectedRoute, PublicRoute, RouteTitle, SessionExpiredHandler
├── types/
│   ├── index.ts
│   ├── enums.types.ts      # as-const unions (UserRole, ...)
│   └── dtos/               # backend DTOs (auth.types.ts, tarot.types.ts, ...)
├── utils/
│   ├── index.ts
│   └── <domain>.utils.ts   # pure helpers (common, datetime, error)
├── App.tsx                 # QueryClientProvider + AppRouter (+ dev-only ReactQueryDevtools)
└── main.tsx                # createRoot + StrictMode; fingerprints warm-up
```

Every folder of pages/components has an `index.ts` barrel (page folder: `export { default } from "./X";`). Import via `@/` alias and barrel, NEVER deep-import into a file (`@/pages/auth/DrawPage/TarotPage/TarotPage` is wrong; `@/pages/auth/DrawPage/TarotPage` is right). Grouped pages under a shared parent folder (e.g. `DrawPage/{TarotPage, AiTarotPage}`, `HistoryPage/{HistoryTarotPage, HistoryAiTarotPage}`) keep per-page barrels; import the page barrel directly.

## 4. File & naming conventions

- **Files**: camelCase + domain suffix — `auth.hooks.ts`, `auth.api.ts`, `tarot.types.ts`, `common.constants.ts`, `error.utils.ts`, `useThemeStore.hooks.ts`.
- **Components/Pages**: PascalCase `.tsx`, default export, feature-folder `XxxPage/`, `XxxCard/`, `XxxModal/`, props interface `XxxProps` (exported).
- **Hooks**: `useXxx` — queries `useGetXxx`/`useXxx` (data), mutations `useCreateXxx`/`useDeleteXxx` from their `onSuccess` invalidation.
- **Constants**: `UPPER_SNAKE` via `as const` objects/arrays; derive types from them (`type UserRole = (typeof USER_ROLE)[number]`). No TS enums.
- **DTOs**: `interface`, mirrors backend `camelCase` records. List item DTOs named `XxxItem`/`XxxResult`.
- **i18n files**: `{lang}.{domain}.ts` with `as const` (e.g. `vi.pages.ts`).

## 5. API layer

### `config.api.ts` — the axiosClient

- `baseURL = import.meta.env.VITE_API_URL ?? ""`, `withCredentials: true` (auth is HttpOnly cookies — never read/write tokens in JS).
- **Request interceptor** sets `X-Device-Id` from cached FingerprintJS `getVisitorId()` (best-effort; skip on failure).
- **Success interceptor** unwraps the envelope: returns `response.data.data`, so api fns resolve to the payload only.
- **Error interceptor**: 401 -> whitelist check (login/refresh/logout) -> single-flight refresh queue (one in-flight refresh, queued retries) -> on refresh failure dispatch `AUTH_SESSION_EXPIRED_EVENT` on `window` and reject with `ApiErrorResponse`. All other errors reject with `error.response.data` (`ApiErrorResponse`).

```ts
// src/api/auth.api.ts
export const authApi = {
  googleLogin: (body: GoogleLoginRequest): Promise<void> =>
    axiosClient.post(API_URL.auth.login, body),
  getCurrentUser: (): Promise<GetCurrentUserResult> =>
    axiosClient.get(API_URL.auth.getCurrentUser),
};
```

### `url.api.ts` — single endpoint map

```ts
export const API_URL = {
  auth: { login: "/api/auth/oauth", refresh: "/api/auth/refresh", ... },
  tarot: { getLastDrawnCardForAuth: "/api/tarot/draw", ... },
} as const;
```

One `<domain>.api.ts` per backend controller; api fns only take DTO args. Never expose `AxiosResponse`; always typed payloads or `void`.

## 7. React Query conventions

- Query keys live in `src/hooks/api/queryKey.ts` as **hierarchical `as const` arrays**: `TAROT_READING_QUERY_KEY = ["tarotReading"]`, then `GET_CARD_FOR_AUTH_QUERY_KEY = [...TAROT_READING_QUERY_KEY, "getCardForAuth"]`. Secondary keys must be prefixed with their domain — never bare (`["getAll"]` is wrong).
- Queries: small focused hooks with sensible `staleTime` (`Infinity` for daily/cooldown data; default 5 min from the global QueryClient otherwise). `retry: false` where 401/expected errors occur.
- Mutations: `onSuccess` invalidates only the affected keys (or `removeQueries` for auth-scoped cache). Backend returns `200 + null data` for writes, so mutations only signal success/error.
- `useGetCurrentUser` gates routing — no duplicated `staleTime`; rely on the app-wide QueryClient default.

## 8. Routing & guards

- `AppRouter.tsx`: `BrowserRouter` -> `<SessionExpiredHandler />` -> `<Suspense fallback={<MainLayout><Spin fullscreen /></MainLayout>}>` -> `<Routes>`.
- Routes declared as `publicRoutes`/`protectedRoutes` arrays driven by `WEB_URL` constants; each entry `{ titleKey, path, component }`; pages loaded via `React.lazy`.
- Public routes render inside `<PublicRoute>` (MainLayout only); protected inside `<ProtectedRoute>` (calls `useGetCurrentUser`: loading -> Spin, error/no data -> `<Navigate to={WEB_URL.guestHome} replace />`, ok -> MainLayout + `<Outlet />`).
- `RouteTitle` + `useDocumentTitle(titleKey)` set localized `<title>`.
- `SessionExpiredHandler` listens for `AUTH_SESSION_EXPIRED_EVENT` -> `queryClient.removeQueries({ queryKey: AUTH_QUERY_KEY })` + navigate to guest home (no full reload).

## 9. State

- zustand stores in `src/hooks/stores/` for UI-only prefs (`useThemeStore`, `useLanguageStore`), persisted via `persist(...)` (`create<T>()(persist(...))`).
- Consume with selectors: `useThemeStore((s) => s.mode)`.
- Language store syncs `i18n.changeLanguage` on toggle/set + at module load.

## 10. Styling & antd

- Theme built from constants in `src/constants/theme.constants.ts`: `COLOR_PALETTES` per role (`guest|registered|pro`) + mode (`dark|light`); `getThemeByRole`/`getPaletteByRole` feed `ConfigProvider` and inline styles.
- Components style via `theme.useToken()` tokens (never hardcoded hex) and inline `style` props. No CSS modules / no CSS-in-JS lib.
- App is wrapped in `<AntdApp>` (MainLayout) -> use `App.useApp()` for `message`/`modal`. Never import static `message`/`notification` from `"antd"`.
- Icons ONLY from `@ant-design/icons` (declared dependency).

## 11.i18n

- Setup in `src/i18n/index.ts`: resources merged into a single `translation` namespace; `lng: "vi"`, `fallbackLng: "en"`.
- Locale modules are TS `as const` objects at `src/i18n/locales/<domain>/{lang}.<domain>.ts` — key namespaces `page.*`, `component.*`, `error.*`, `tarot.*`.
- All user-facing strings go through `useTranslation().t()` or `<Trans i18nKey=...>` (with `components` mapping, and interpolation via `values`). Never hardcode copy.
- 78-card meanings live under `tarot.meaning.<code>.<upright|reversed>.<section>`; big tarot locales are manual chunks in vite.config to avoid a giant bundle.

## 12. Error & loading UX

Standard per-page pattern:

- Loading: `<Spin fullscreen />`.
- Query error: `data === undefined` -> `<ErrorComponent type="server" onRetry={refetch} />`.
- Empty lists: antd `<Empty>`.
- Mutation feedback via `App.useApp().message`: `onSuccess`/`onError` callbacks showing `t(...)` key or `getErrorMessage(error)`.
- `src/utils/error.utils.ts`: `getErrorMessage(error)` translates the `ApiErrorResponse.message` i18n key (falls back to `error.system.internalServerError`); `getFormFieldErrors` maps backend `ValidationError[]` onto antd form fields (key camelCased).

## 13. DTO types

Mirror backend `ApiResponse<T>`:

```ts
export interface ApiResponse<T> {
  success: boolean;
  message: null | string;
  data: T | null;
}
export interface ApiErrorResponse extends ApiResponse<ValidationError[]> {}
```

DTOs in `src/types/dtos/*.types.ts` as interfaces mirroring backend records (`GetCurrentUserResult`, `CreateDrawForAuthRequest`, `GetAllReadingItem`, ...). Union types from `as const` arrays in `src/types/enums.types.ts`.

## 14. Env & tooling

- Only `VITE_API_URL` (axios baseURL) and `VITE_GOOGLE_CLIENT_ID` (custom OAuth redirect flow) exist; read via `import.meta.env`.
- `npm run lint` (oxlint), `npm run build` (`tsc -b && vite build`), `npm run dev` / `npm run preview`.
- `verbatimModuleSyntax` requires `import type` for type-only imports.
- Only read `.env` for config; never read `.env.local`.

## 15.Known pitfalls

- Deep imports into files instead of folder barrels — always use the barrel.
- Static `message`/`notification` from antd — use `App.useApp()`.
- Bare/non-hierarchical query keys and duplicates of the global `staleTime`.
- Hardcoded colors or user-facing strings.
- Missing `index.ts` barrel in a page/component folder.

## Quick checklist for new code

- [ ] Page/component folder has an `index.ts` barrel; imported via `@/` alias, never a deep file import
- [ ] Component/page is PascalCase `.tsx` with default export + exported `XxxProps` interface
- [ ] File name convention respected: `<domain>.hooks.ts`, `<domain>.api.ts`, `<domain>.types.ts`, `<domain>.constants.ts`, `<domain>.utils.ts`
- [ ] UI strings use `t()`/`<Trans>`; locale keys only in `i18n/locales/<domain>/{en,vi}.<domain>.ts`
- [ ] HTTP goes through `src/api/<domain>.api.ts` (typed, envelope unwrapped); components never call axios directly
- [ ] React Query only in `src/hooks/api/`; query keys hierarchical (`[...DOMAIN_QUERY_KEY, ...]`), mutation `onSuccess` invalidates affected keys only
- [ ] DTOs are `interface`s in `src/types/dtos/`; unions from `as const` arrays (no TS enums)
- [ ] Styling via `theme.useToken()` tokens + `ConfigProvider` theme; `App.useApp()` for `message`; icons from `@ant-design/icons`
- [ ] Loading = `<Spin fullscreen />`; query error = `<ErrorComponent type="server" onRetry={refetch} />`; empty = `<Empty>`
- [ ] No `import type` without the `type` keyword (`verbatimModuleSyntax`)
- [ ] Verify with `npm run lint` and `npm run build`
