# Frontend

React 19 + Vite + TypeScript SPA for **My Tarot Reader**. Renders the guest and signed-in tarot draw flows, AI tarot and AI deep tarot spreads, the 78-card library, check-in streak & coins, reading history, wallet, the PayOS red-coin shop and Google OAuth — in a bilingual (English / Vietnamese) UI with role-based light/dark themes.

## Requirements

- [Node.js](https://nodejs.org/) `>=22.12` + npm (CI uses Node 24)
- Backend API running (see [../Backend/README.md](../Backend/README.md))

## Tech Stack

| Area | Choice |
| --- | --- |
| UI | React 19, Ant Design v6 (`@ant-design/icons`), Plus Jakarta Sans |
| Build | Vite 8, TypeScript 6 (`verbatimModuleSyntax`, `erasableSyntaxOnly`) |
| Routing | react-router-dom v7 (declarative `<BrowserRouter>`/`<Routes>`) |
| Server state | TanStack Query v5 (only inside `src/hooks/api/`) |
| Client state | Zustand v5 (`persist`) for theme + language |
| HTTP | axios (`withCredentials`, envelope unwrap, single-flight refresh) |
| i18n | i18next + react-i18next (default `vi`, fallback `en`) |
| Device identity | `@fingerprintjs/fingerprintjs` → `X-Device-Id` header |
| Lint | oxlint |

## Folder Structure

```
src/
├── api/                     # axios client + typed API modules (one per backend controller)
│   ├── config.api.ts        # axiosClient: withCredentials, X-Device-Id, envelope unwrap, refresh queue
│   ├── url.api.ts           # API_URL endpoint map (as const)
│   └── <domain>.api.ts      # auth, tarot, aiTarot, aiTarotAdmin, aiDeepTarot, streak, wallet, shop, shopAdmin, admin
├── assets/
│   └── cards/               # 78 Rider–Waite card images (.webp)
├── components/              # shared presentational components (barrel: components/index.ts)
│   ├── AppLoading/          # themed full-screen splash
│   ├── AppTitle/            # document/app title helper
│   ├── Coin/                # white/red coin badge
│   ├── CopyButton/
│   ├── EmptyView/           # antd Empty wrapper with optional action button
│   ├── ErrorView/           # generic / offline error state with retry
│   ├── PaginationBar/
│   ├── ResponsiveModal/
│   ├── Tarot/               # TarotCard + TarotDeck (fan spread, shuffle, select, deal)
│   └── TarotMeaningCard/
├── constants/               # as-const data + helpers (common, theme, language, tarot, aiDeepTarot, streak, wallet)
├── hooks/
│   ├── api/                 # TanStack Query hooks + hierarchical query keys (queryKey.ts)
│   ├── custom/              # generic hooks (useRouteTitle)
│   └── stores/              # zustand stores (useThemeStore, useLanguageStore)
├── i18n/                    # i18next init + locale modules (pages, components, tarot, errors)
├── layouts/                 # BaseLayout + role layouts
│   ├── BaseLayout/          # shared shell: sider, header, mobile drawer, user footer, logout, theme provider
│   ├── GuestLayout.tsx
│   ├── RegisterLayout.tsx
│   └── AdminLayout.tsx
├── pages/
│   ├── common/              # NotFoundPage
│   ├── guest/               # GuestHomePage, GuestTarotPage, LoginPage, LoginCallbackPage
│   ├── register/            # signed-in pages
│   │   ├── DrawPage/        # TarotPage, AiTarotPage, AiDeepTarotPage/{TwelveHouses,TwelveMonths,Crossroads}
│   │   ├── ResultPage/      # AiTarotResultPage, AiDeepTarotResultPage
│   │   ├── HistoryPage/     # HistoryTarotPage, HistoryAiTarotPage, HistoryAiDeepTarotPage
│   │   ├── HomePage/ LibraryPage/ WalletPage/
│   │   └── ShopPage/ ShopCheckoutPage/   # PayOS red-coin shop + checkout/polling page
│   └── admin/               # reserved — no pages yet (see Known gaps)
├── routes/
│   ├── AppRouter.tsx        # BrowserRouter + SessionExpiredHandler + AppGate
│   └── AppGate/             # route table + guards
│       ├── AppGate.tsx
│       ├── GuestRoute.tsx
│       ├── RegisterRoute.tsx
│       ├── NotFoundLayoutRoute.tsx
│       ├── SessionExpiredHandler.tsx
│       └── url.routes.ts    # WEB_URL map + public/register route tables
├── types/                   # enums + DTOs mirroring the backend
│   ├── enums.types.ts       # as-const unions (no TS enums)
│   └── dtos/                # auth, tarot, aiTarot, aiTarotAdmin, aiDeepTarot, streak, wallet, shop, shopAdmin, admin, pagination, response
├── utils/                   # pure helpers (common, datetime, error, aiTarot, aiDeepTarot, shop)
├── App.tsx                  # QueryClientProvider + AppRouter (+ dev-only ReactQueryDevtools)
├── index.css                # global styles
└── main.tsx                 # createRoot + i18n init
```

`index.html` also contains a small inline script that applies the persisted theme background before React mounts (avoids a light/dark flash).

## Configuration

Copy the placeholder values in `.env` and adjust them for your setup. There is no committed `.env.example`; `.env` holds local-only/placeholder values. `.env.local` is ignored via `*.local`.

| Variable | Meaning | Example |
| --- | --- | --- |
| `VITE_API_URL` | Backend API base URL (axios `baseURL`) | `http://localhost:5271` |
| `VITE_GOOGLE_CLIENT_ID` | Google OAuth client ID for the custom redirect flow | `your-google-client-id` |

`VITE_API_URL` and `VITE_GOOGLE_CLIENT_ID` are baked into the bundle at build time — on Vercel they must be set as Environment Variables in the project, not only locally.

There is no `env.d.ts` declaring an `ImportMetaEnv` interface, so neither variable is type-checked; only `vite/client` types are loaded. A typo in `import.meta.env.VITE_*` compiles cleanly and fails at runtime.

## Install & Run

```bash
npm install     # install dependencies
npm run dev     # start the Vite dev server → http://localhost:5173
```

Production-like build & preview:

```bash
npm run build   # type-check (tsc -b) + production build → dist/
npm run preview # serve the dist/ build locally
```

Linting:

```bash
npm run lint    # oxlint
```

There is no frontend test suite; verification is `npm run lint` + `npm run build`.

## Scripts

| Command | Purpose |
| --- | --- |
| `npm run dev` | Start the Vite dev server (HMR) |
| `npm run build` | Type-check + build for production |
| `npm run preview` | Serve the production build locally |
| `npm run lint` | Run oxlint |

The build splits vendor code and the two large tarot locale files into separate chunks (`vite.config.ts` → `manualChunks`).

## Routing & Auth

Routes are declared as `publicRoutes` / `registerRoutes` arrays in `src/routes/AppGate/url.routes.ts`, keyed by the `WEB_URL` map, and rendered by `AppGate`.

**Public routes** (guarded by `GuestRoute`; authenticated users are redirected to the matching signed-in page):

| Path | Page |
| --- | --- |
| `/guest` | `GuestHomePage` |
| `/draw/guest` | `GuestTarotPage` |
| `/login` | `LoginPage` |
| `/login/callback` | `LoginCallbackPage` |

**Signed-in routes** (guarded by `RegisterRoute`; guests are redirected to `/guest`):

| Path | Page |
| --- | --- |
| `/` | `HomePage` |
| `/library` | `LibraryPage` |
| `/draw` | `TarotPage` |
| `/draw/ai` | `AiTarotPage` |
| `/draw/deep/twelve-houses` | `TwelveHousesPage` |
| `/draw/deep/twelve-months` | `TwelveMonthsPage` |
| `/draw/deep/crossroads` | `CrossroadsPage` |
| `/history`, `/history/ai`, `/history/deep` | tarot / AI tarot / AI deep tarot history |
| `/result/ai/:readingId`, `/result/deep/:readingId` | AI result pages |
| `/wallet` | `WalletPage` |
| `/shop`, `/shop/checkout/:orderId` | shop + checkout/polling |

Unknown URLs render `NotFoundPage` inside the layout matching the current user (`GuestLayout`, `RegisterLayout` or `AdminLayout`).

**Session flow**

- Auth is HttpOnly cookies — no tokens are read or written from JS. `axiosClient` uses `withCredentials: true`.
- The request interceptor adds `X-Device-Id` from FingerprintJS (best-effort).
- The response interceptor unwraps `response.data.data`; on `401` (outside the login/refresh/logout whitelist) it runs a single-flight refresh queue and retries queued requests.
- If refresh fails, it dispatches `AUTH_SESSION_EXPIRED_EVENT`; `SessionExpiredHandler` removes the `AUTH` and `WALLET` query caches and navigates to `/guest` (no full reload).
- Google login uses the implicit OIDC flow (`id_token` in the URL fragment) with a `nonce` kept in `sessionStorage`; the callback page verifies it and completes login.

## Deployment

- Hosted on **Vercel** as an SPA — `vercel.json` rewrites all routes to `index.html`.
- CI (GitHub Actions → `../.github/workflows/ci.yml`) runs on pushes to `main` and pull requests targeting `main`. The frontend job is skipped unless something under `Frontend/**` changed, then runs `npm ci` + `npm run lint` + `npm run build`. After a successful build on `main` it fires the Vercel deploy hook.
- Set `VITE_API_URL` / `VITE_GOOGLE_CLIENT_ID` in the Vercel project environment (build-time values).

## Code Conventions

Full conventions live in `.agent/skills/frontend-react-architecture/SKILL.md` — read it before writing code (note: parts of that skill reference older names such as `pages/auth`, `MainLayout` and `ErrorComponent`; the tree above reflects the current code). Highlights:

- **Data flow is top-down:** Page/Component → `hooks/api/<domain>.hooks.ts` (React Query — the *only* place `useQuery`/`useMutation` live) → `api/<domain>.api.ts` (axios, typed DTOs) → `axiosClient`. Components never call `src/api` or axios directly. Two deliberate exceptions sit outside `hooks/api`: `App.tsx` owns the `QueryClientProvider`, and `routes/AppGate/SessionExpiredHandler.tsx` calls `useQueryClient().removeQueries(...)` to purge cached data when the session expires.
- **DTOs:** `interface`s in `src/types/dtos/` mirroring backend `camelCase` records; `ApiResponse<T>` and `ApiErrorResponse` mirror the backend envelope; the axios interceptor unwraps `response.data.data`.
- **Query keys:** hierarchical `as const` arrays in `src/hooks/api/queryKey.ts`; mutations invalidate only affected keys on `onSuccess`.
- **State:** zustand stores (`persist`) for UI-only prefs (theme/language); no TS `enum` — unions derived from `as const` arrays.
- **Styling:** theme via antd `ConfigProvider` + `theme.useToken()` (never hardcoded hex); palettes per layout role (`guest | register | admin`) and mode (`light | dark`) live in `src/constants/theme.constants.ts`; `message`/`modal` via `App.useApp()` (never static antd imports); icons only from `@ant-design/icons`.
- **Loading/error UX:** `<AppLoading />` for boot/route loading, `<ErrorView type="server" onRetry={refetch} />` for query errors, `<EmptyView />` for empty lists.
- **i18n:** default `vi`, fallback `en`, single `translation` namespace; all user-facing strings go through `useTranslation().t()` — never hardcode copy. Locale modules live in `src/i18n/locales/{pages,components,tarot,errors}/{en,vi}.*.ts`.
- **Imports:** `@/` alias (→ `src/`) + folder barrels only, never deep file imports. One exception: `AiDeepTarotPage/` itself has no `index.ts`, so `pages/register/index.ts` lazy-imports each spread subfolder barrel (`TwelveHouses`, `TwelveMonths`, `Crossroads`) directly.
- **Type-only imports:** `import type` required (`verbatimModuleSyntax`).

## Known gaps

- **Admin UI is not wired up.** `WEB_URL` defines `/admin`, `/admin/statistics`, `/admin/shop-packages` and `/admin/ai-tarot-settings`, and `api/admin.api.ts`, `api/shopAdmin.api.ts`, `api/aiTarotAdmin.api.ts` plus their hooks/types exist — but `pages/admin/` is empty, `AppGate` declares no admin routes, and `AdminLayout` is only used to render the 404 page for admin users.
- The shop's PayOS `ReturnUrl` / `CancelUrl` in `Backend/src/Api/appsettings.json` are still placeholders; the frontend doesn't rely on them (it opens `checkoutUrl` in a new tab and polls `GET /api/shop/orders/{id}`), but they must be set to real URLs before PayOS redirects are used.
- No `env.d.ts`, so `import.meta.env.VITE_*` names are not type-checked (see [Configuration](#configuration)).
