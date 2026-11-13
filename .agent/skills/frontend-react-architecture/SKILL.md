---
name: frontend-react-architecture
description: >-
  React 19 + Vite 8 + TypeScript 6 frontend architecture conventions for My Tarot Reader (antd v6, TanStack Query v5, react-router v7, zustand, react-i18next, axios). MUST be used whenever writing, reviewing, or editing Frontend code: pages, components, API/hooks layers, routing, i18n, styling, or state. Trigger on requests like "add a page for...", "fix this component...", "wire up this API...", "refactor this hook...", or any TypeScript/React feature work under Frontend/.
---

# Frontend React Architecture — My Tarot Reader

Standard conventions for the My Tarot Reader React frontend. Pairs with the backend `backend-dotnet-architecture` skill — the frontend mirrors its `ApiResponse<T>` envelope, DTO naming (`*Request` / `*Result`), and ordering (`POST/PUT/DELETE => 200 + null data`, cache invalidation).

## 1. Stack

- **React 19** + **TypeScript 6** (`verbatimModuleSyntax`, `erasableSyntaxOnly` — NO TS `enum`, use `as const` arrays + unions).
- **Vite 8** build; path alias `@/*` → `src/*` (tsconfig + `vite.config.ts`). Node engine `>=22.12` (CI uses Node 24).
- **react-router-dom v7** — declarative `<BrowserRouter>` / `<Routes>` mode (no `createBrowserRouter`).
- **@tanstack/react-query v5** — server state, used ONLY in `src/hooks/api/`.
- **antd v6** — UI kit; styling via `ConfigProvider` theme + `theme.useToken()`.
- **axios** — HTTP client with envelope unwrap + auth interceptors.
- **react-i18next** — i18n, single `translation` namespace, default `vi`, fallback `en`.
- **zustand v5** (`persist`) — client-only UI state (theme + language).
- **@fingerprintjs/fingerprintjs** — device fingerprint → `X-Device-Id` header.
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

- **`src/api/*.api.ts`** — pure HTTP. Object literal of arrow fns named `<action>(<args>): Promise<T>`. Every read returns the unwrapped payload; every mutation returns `Promise<void>` (or its created result when the backend returns one, e.g. `shopApi.createOrder`).
- **`src/hooks/api/*.hooks.ts`** — the ONLY place `useQuery` / `useMutation` live. Components consume hooks, never api.
- Components never touch `queryClient` directly; mutations invalidate via the hook's `onSuccess`.
- Documented exceptions: `App.tsx` owns the `QueryClientProvider` (+ dev-only `ReactQueryDevtools`), and `routes/AppGate/SessionExpiredHandler.tsx` calls `useQueryClient().removeQueries(...)` on session expiry.

## 3. Canonical folder tree

```
src/
├── api/
│   ├── config.api.ts       # axiosClient: withCredentials, X-Device-Id, unwrap, refresh queue
│   ├── url.api.ts          # API_URL endpoint map (as const)
│   ├── index.ts            # barrel
│   └── <domain>.api.ts     # auth, tarot, aiTarot, aiTarotAdmin, aiDeepTarot, streak, wallet, shop, shopAdmin, admin
├── assets/
│   └── cards/              # 78 Rider–Waite card images (.webp)
├── components/
│   ├── index.ts            # barrel re-exporting component folders
│   └── <Component>/
│       ├── index.ts
│       └── <ComponentName>.tsx
├── constants/
│   ├── index.ts
│   └── <domain>.constants.ts   # as const data + helpers (common, theme, language, tarot, aiDeepTarot, streak, wallet)
├── hooks/
│   ├── api/                # TanStack Query hooks (+ queryKey.ts, index.ts)
│   ├── custom/             # generic hooks (useRouteTitle) — no barrel
│   └── stores/             # zustand stores (useThemeStore, useLanguageStore) + index.ts
├── i18n/
│   ├── index.ts            # i18next init, merges all locale modules
│   └── locales/<domain>/{en,vi}.<domain>.ts   # domains: pages, components, tarot, errors
├── layouts/
│   ├── BaseLayout/         # shared shell: sider, header, mobile drawer, user footer, logout, theme provider
│   ├── GuestLayout.tsx
│   ├── RegisterLayout.tsx
│   ├── AdminLayout.tsx
│   └── index.ts
├── pages/
│   ├── common/             # NotFoundPage
│   ├── guest/              # GuestHomePage, GuestTarotPage, LoginPage, LoginCallbackPage
│   ├── register/           # authenticated pages (DrawPage, ResultPage, HistoryPage, HomePage, LibraryPage, WalletPage, ShopPage, ShopCheckoutPage)
│   └── admin/              # reserved — no pages yet
├── routes/
│   ├── AppRouter.tsx       # BrowserRouter + SessionExpiredHandler + AppGate
│   ├── index.ts
│   └── AppGate/            # route tables + guards
│       ├── AppGate.tsx
│       ├── GuestRoute.tsx
│       ├── RegisterRoute.tsx
│       ├── NotFoundLayoutRoute.tsx
│       ├── SessionExpiredHandler.tsx
│       ├── url.routes.ts   # WEB_URL map (as const) + publicRoutes / registerRoutes
│       └── index.ts
├── types/
│   ├── index.ts
│   ├── enums.types.ts      # as-const unions (UserRole, PaymentOrderStatus, ...)
│   └── dtos/               # backend DTOs (auth, tarot, aiTarot, aiTarotAdmin, aiDeepTarot, streak, wallet, shop, shopAdmin, admin, pagination, response)
├── utils/
│   ├── index.ts
│   └── <domain>.utils.ts   # pure helpers (common, datetime, error, aiTarot, aiDeepTarot, shop)
├── App.tsx                 # QueryClientProvider + AppRouter (+ dev-only ReactQueryDevtools)
├── index.css               # global styles
└── main.tsx                # createRoot + i18n init
```

Every page/component feature folder has an `index.ts` barrel (page folder: `export { default } from "./X";` or a named export). Import via `@/` alias and barrel, NEVER deep-import into a file (`@/pages/register/DrawPage/TarotPage/TarotPage` is wrong; `@/pages/register/DrawPage/TarotPage` is right). Grouped pages under a shared parent folder (`DrawPage/{TarotPage, AiTarotPage, AiDeepTarotPage}`, `HistoryPage/{...}`, `ResultPage/{...}`) keep per-page barrels; import the page barrel directly.

Known barrel exceptions:

- `pages/register/index.ts` and `pages/guest/index.ts` export `React.lazy` components (route-level code splitting).
- `pages/register/DrawPage/AiDeepTarotPage/` itself has no `index.ts`; each spread subfolder (`TwelveHouses`, `TwelveMonths`, `Crossroads`) has one, and `pages/register/index.ts` lazy-imports those subfolder barrels directly.
- `hooks/custom/` has no barrel — import `@/hooks/custom/useRouteTitle` directly.
- `pages/common/` has no barrel — import `@/pages/common/NotFoundPage`.

## 4. File & naming conventions

- **Files**: camelCase + domain suffix — `auth.hooks.ts`, `auth.api.ts`, `tarot.types.ts`, `common.constants.ts`, `error.utils.ts`, `useThemeStore.hooks.ts`.
- **Components/Pages**: PascalCase `.tsx`, default or named export, feature-folder `XxxPage/`, `XxxCard/`, `XxxModal/`, props interface `XxxProps` (exported).
- **Hooks**: `useXxx` — queries `useGetXxx` (data), mutations `useCreateXxx` / `useDeleteXxx` / `useCheckIn` / `useLogin` etc., with invalidation in `onSuccess`.
- **Constants**: `UPPER_SNAKE` via `as const` objects/arrays; derive types from them (`type UserRole = (typeof USER_ROLE)[number]`). No TS enums.
- **DTOs**: `interface`, mirrors backend `camelCase` records. List item DTOs named `XxxItem` / `XxxResult`; paginated payloads use `PageParams` + `PaginatedResult<TItem>`.
- **i18n files**: `{lang}.{domain}.ts` with `as const` (e.g. `vi.pages.ts`).

## 5. API layer

### `config.api.ts` — the axiosClient

- `baseURL = import.meta.env.VITE_API_URL ?? ""`, `withCredentials: true` (auth is HttpOnly cookies — never read/write tokens in JS).
- **Request interceptor** sets `X-Device-Id` from cached FingerprintJS `getVisitorId()` (best-effort; skip on failure).
- **Success interceptor** unwraps the envelope: returns `response.data.data`, so api fns resolve to the payload only.
- **Error interceptor**: `401` → whitelist check (login / refresh / logout) → single-flight refresh queue (one in-flight refresh, queued retries) → on refresh failure dispatch `AUTH_SESSION_EXPIRED_EVENT` on `window` and reject with `ApiErrorResponse`. The `/me` probe is excluded from the event to avoid a guest `me → refresh → event` loop. All other errors reject with `error.response.data` (`ApiErrorResponse`).

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

Auth flow: Google login uses the **implicit OIDC redirect** — `GoogleLoginPanel` builds the authorization URL with `VITE_GOOGLE_CLIENT_ID`, stores a `nonce` in `sessionStorage`, then `window.location.assign(...)`. `LoginCallbackPage` reads `id_token` from the URL fragment, clears it from the address bar, verifies the nonce, and calls `useLogin()` → backend sets the HttpOnly cookies.

## 6. React Query conventions

- Query keys live in `src/hooks/api/queryKey.ts` as **hierarchical `as const` arrays**: `TAROT_READING_QUERY_KEY = ["tarotReading"]`, then `GET_CARD_FOR_AUTH_QUERY_KEY = [...TAROT_READING_QUERY_KEY, "getCardForAuth"]`. Secondary keys must be prefixed with their domain — never bare (`["getAll"]` is wrong). Parameterised keys append the parameter: `[...GET_ORDER_STATUS_QUERY_KEY, orderId]`, `[...GET_ALL_AI_DEEP_TAROT_QUERY_KEY, page, pageSize]`.
- Global defaults in `App.tsx`: `staleTime: 5 * 60 * 1000`, `refetchOnWindowFocus: false`. Do not duplicate the global `staleTime`.
- Queries: small focused hooks. Use `staleTime: Infinity` for daily/cooldown data (streak), `retry: false` where 401/404 is expected, `placeholderData: keepPreviousData` for paginated lists, and `refetchInterval` for order polling (while status is `pending`).
- Mutations: `onSuccess` invalidates only the affected keys. Coin-changing actions also invalidate `AUTH_QUERY_KEY` (header balances) and `GET_WALLET_QUERY_KEY`; auth actions invalidate or remove both. Backend writes return `200 + null data`, so mutations only signal success/error.
- `useGetCurrentUser(enabled?)` gates routing; it uses `AUTH_QUERY_KEY` and `retry: false`.

## 7. Routing & guards

- `AppRouter.tsx`: `BrowserRouter` → `<SessionExpiredHandler />` → `<AppGate />`.
- `AppGate.tsx`: probes `useGetCurrentUser` (skipped on `/login` and `/login/callback`), blocks the first paint with `<AppLoading />` until auth resolves, then renders:
  - `publicRoutes` inside `<GuestRoute />`
  - `registerRoutes` inside `<RegisterRoute />`
  - a `*` catch-all inside `<NotFoundLayoutRoute user={data} />`
- Routes are `{ titleKey, path, component }` entries in `AppGate/url.routes.ts`, driven by the `WEB_URL` constants and lazy page exports.
- `GuestRoute`: renders `GuestLayout`; when authenticated, redirects `/guest → /` and `/draw/guest → /draw`.
- `RegisterRoute`: renders `RegisterLayout` with the current user (`fullName`, `picture`, red/white coins); guests/errors navigate to `/guest`.
- `NotFoundLayoutRoute`: chooses `GuestLayout` / `RegisterLayout` / `AdminLayout` based on the current user, then renders `NotFoundPage`.
- `useRouteTitle(routes)` (`hooks/custom`) matches the current path and returns the translated title; layouts set `document.title` as `${title} | ${APP_NAME}`.
- `SessionExpiredHandler` listens for `AUTH_SESSION_EXPIRED_EVENT` → `removeQueries` for `AUTH_QUERY_KEY` + `WALLET_QUERY_KEY` → `navigate(WEB_URL.guestHome, { replace: true })` (no full reload).
- `WEB_URL` also defines admin paths (`/admin`, `/admin/statistics`, `/admin/shop-packages`, `/admin/ai-tarot-settings`) and admin API/hooks exist, but **no admin routes/pages are wired in `AppGate` yet**.

## 8. State

- zustand stores in `src/hooks/stores/` for UI-only prefs (`useThemeStore`, `useLanguageStore`), persisted via `persist(...)` (`create<T>()(persist(...))`) with storage keys `tarot-theme` / `tarot-language`.
- Consume with selectors: `useThemeStore((s) => s.mode)`.
- The language store calls `i18n.changeLanguage` on toggle/set and syncs the persisted language at module load.

## 9. Styling & antd

- Theme built from `src/constants/theme.constants.ts`: `layoutPalettes` per variant (`guest | register | admin`) × mode (`light | dark`), consumed via `getPalette(variant, mode)` and `getTheme(isDark, palette)`.
- `BaseLayout` wraps everything in `<ConfigProvider theme={getTheme(...)}>` + `<AntdApp>`; `AppLoading` builds its own `ConfigProvider` because it renders before any layout. `index.html` applies the persisted theme background before React mounts.
- Components style via `theme.useToken()` tokens (never hardcoded hex) and inline `style` props. No CSS modules / no CSS-in-JS lib.
- Use `App.useApp()` for `message` / `modal` (available inside `AntdApp`). Never import static `message` / `notification` from `"antd"`.
- Icons ONLY from `@ant-design/icons` (declared dependency).

## 10. i18n

- Setup in `src/i18n/index.ts`: resources merged into a single `translation` namespace; `lng: "vi"`, `fallbackLng: "en"`.
- Locale modules are TS `as const` objects at `src/i18n/locales/<domain>/{lang}.<domain>.ts` — domains `pages`, `components`, `tarot`, `errors`; key namespaces `page.*`, `component.*`, `error.*`, `tarot.*`.
- All user-facing strings go through `useTranslation().t()` or `<Trans i18nKey=...>` (with `components` mapping, and interpolation via `values`). Never hardcode copy.
- 78-card meanings live under `tarot.*`; the big tarot locale files are manual chunks in `vite.config.ts` (`tarot-locales-{vi,en}`) to avoid a giant bundle.

## 11. Error & loading UX

Standard per-page pattern:

- Boot / route loading: `<AppLoading />` (themed splash). `BaseLayout` also wraps the outlet in `<Suspense fallback={<AppLoading />}>`.
- Query error: `<ErrorView type="server" onRetry={refetch} />` (also supports offline / generic / not-found variants).
- Empty lists: `<EmptyView description=... button=... />` (antd `Empty` wrapper).
- Mutation feedback via `App.useApp().message`.
- `src/utils/error.utils.ts`:
  - `getErrorMessage(error)` translates the `ApiErrorResponse.message` i18n key (falls back to `error.system.internalServerError`).
  - `getFormFieldErrors<T>(error)` maps backend `ValidationError[]` onto antd form fields (key camelCased).
- `src/components/AppTitle` renders a page title/subtitle with a skeleton loading state.

## 12. DTO types

Mirror the backend `ApiResponse<T>`:

```ts
export interface ApiResponse<T> {
  success: boolean;
  message: null | string;
  data: T | null;
}
export interface ValidationError {
  key: string;
  value: string;
}
export interface ApiErrorResponse extends ApiResponse<ValidationError[]> {}
```

DTOs in `src/types/dtos/*.types.ts` as interfaces mirroring backend records (`GetCurrentUserResult`, `CreateDrawForAuthRequest`, `GetAllReadingItem`, ...). Union types from `as const` arrays in `src/types/enums.types.ts` (`USER_ROLE`, `PAYMENT_ORDER_STATUS`). Pagination uses `PageParams` and `PaginatedResult<TItem>`.

## 13. Env & tooling

- Only `VITE_API_URL` (axios baseURL) and `VITE_GOOGLE_CLIENT_ID` (custom OAuth redirect flow) exist; read via `import.meta.env`.
- There is no `env.d.ts` — `import.meta.env.VITE_*` is not type-checked.
- `npm run lint` (oxlint), `npm run build` (`tsc -b && vite build`), `npm run dev` / `npm run preview`.
- `verbatimModuleSyntax` requires `import type` for type-only imports; `erasableSyntaxOnly` forbids TS `enum` and other non-erasable syntax.
- Only read `.env` for config; never read `.env.local`.
- No frontend test suite — verify with `npm run lint` + `npm run build`.

## 14. Known pitfalls

- Deep imports into files instead of folder barrels — always use the barrel (except the documented exceptions above).
- Static `message` / `notification` from antd — use `App.useApp()`.
- Bare/non-hierarchical query keys and duplicates of the global `staleTime`.
- Hardcoded colors or user-facing strings.
- Missing `index.ts` barrel in a page/component folder.
- Editing `pages/admin/` or assuming admin routes exist — the admin UI is not wired into `AppGate` yet.
- Forgetting to invalidate `AUTH_QUERY_KEY` + `GET_WALLET_QUERY_KEY` after coin-changing mutations.

## Quick checklist for new code

- [ ] Page/component folder has an `index.ts` barrel; imported via `@/` alias, never a deep file import
- [ ] Component/page is PascalCase `.tsx` with an exported `XxxProps` interface
- [ ] File name convention respected: `<domain>.hooks.ts`, `<domain>.api.ts`, `<domain>.types.ts`, `<domain>.constants.ts`, `<domain>.utils.ts`
- [ ] UI strings use `t()` / `<Trans>`; locale keys only in `i18n/locales/<domain>/{en,vi}.<domain>.ts`
- [ ] HTTP goes through `src/api/<domain>.api.ts` (typed, envelope unwrapped); components never call axios directly
- [ ] React Query only in `src/hooks/api/`; query keys hierarchical (`[...DOMAIN_QUERY_KEY, ...]`), mutation `onSuccess` invalidates affected keys only
- [ ] DTOs are `interface`s in `src/types/dtos/`; unions from `as const` arrays (no TS enums)
- [ ] Styling via `theme.useToken()` tokens + `ConfigProvider` theme; `App.useApp()` for `message`; icons from `@ant-design/icons`
- [ ] Loading = `<AppLoading />`; query error = `<ErrorView type="server" onRetry={refetch} />`; empty = `<EmptyView />`
- [ ] Type-only imports use the `type` keyword (`verbatimModuleSyntax`)
- [ ] Coin-changing mutations invalidate `AUTH_QUERY_KEY` + `GET_WALLET_QUERY_KEY`
- [ ] Verify with `npm run lint` and `npm run build`
