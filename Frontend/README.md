# Frontend

React 19 + Vite + TypeScript SPA for **My Tarot Reader**. Renders the tarot draw flows (guest + authenticated), the AI tarot and AI deep tarot spreads, the 78-card library, check-in streak & coins, reading history, wallet and Google OAuth — in a bilingual (English / Vietnamese) UI.

## Requirements

- [Node.js 24](https://nodejs.org/) + npm (CI uses Node 24)
- Backend API running (see [../Backend/README.md](../Backend/README.md))

## Folder Structure

```
src/
├── api/                # axios client + typed API modules (one per backend controller)
│   ├── config.api.ts   # axiosClient: withCredentials, X-Device-Id, envelope unwrap, refresh queue
│   ├── url.api.ts      # API_URL endpoint map (as const)
│   └── <domain>.api.ts # auth, tarot, aiTarot, aiDeepTarot, streak, wallet
├── assets/
│   └── cards/          # 78 Rider–Waite card images (.webp), resolved via import.meta.glob
├── components/         # shared feature folders
│   ├── coins/          # coin badges
│   ├── copyButton/
│   ├── error/          # ErrorComponent (server / not-found variants)
│   ├── layouts/        # MainLayout + components/ (header, footer, drawer, dropdown, coin badge)
│   ├── loading/        # BootScreen
│   ├── modal/          # ResponsiveModal
│   └── tarot/          # TarotCard + helpers that render the card assets
├── constants/          # as-const data + helpers (theme, tarot, language, streak, wallet, common, aiTarot, aiDeepTarot)
├── hooks/
│   ├── api/            # TanStack Query hooks + hierarchical query keys (queryKey.ts)
│   ├── custom/         # generic hooks (useDocumentTitle)
│   └── stores/         # zustand stores (theme, language)
├── i18n/               # i18next init + locale modules (pages, components, tarot, errors)
├── pages/
│   ├── auth/           # signed-in pages
│   │   ├── DrawPage/       # TarotPage, AiTarotPage, AiDeepTarotPage/{TwelveHouses,TwelveMonths,Crossroads}
│   │   ├── ResultPage/     # AiTarotResultPage, AiDeepTarotResultPage
│   │   ├── HistoryPage/    # HistoryTarotPage, HistoryAiTarotPage, HistoryAiDeepTarotPage
│   │   ├── HomePage/ LibraryPage/ WalletPage/
│   │   └── ShopPage/ ShopCheckoutPage/      # PayOS red-coin shop + checkout/polling page
│   ├── guest/          # guest HomePage, TarotPage, LoginPage, LoginCallbackPage
│   └── shared/         # home/ + tarot/ sections reused by the guest and auth pages
├── routes/
│   ├── components/     # ProtectedRoute, PublicRoute, RouteTitle, SessionExpiredHandler
│   ├── AppRouter.tsx   # lazy route table
│   └── url.routes.ts   # WEB_URL map (as const)
├── types/              # enums + DTOs mirroring the backend
├── utils/              # pure helpers (common, datetime, error, aiTarot, aiDeepTarot)
├── App.tsx             # QueryClientProvider + AppRouter
├── index.css           # global styles
└── main.tsx            # createRoot + fingerprint warm-up
```

## Configuration

Copy the placeholder values in `.env` and adjust for your setup. There is no committed `.env.example` — `.env` holds local-only/placeholder values, even though `.gitignore` whitelists `!.env.example` and so expects one to exist.

| Variable | Meaning | Example |
| --- | --- | --- |
| `VITE_API_URL` | Backend API base URL (axios `baseURL`) | `http://localhost:5271` |
| `VITE_GOOGLE_CLIENT_ID` | Google OAuth client ID for the custom redirect flow | `your-google-client-id` |

`VITE_API_URL` is baked into the bundle at build time — on Vercel it must be set as an Environment Variable in the project, not only locally.

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

## Scripts

| Command | Purpose |
| --- | --- |
| `npm run dev` | Start the Vite dev server (HMR) |
| `npm run build` | Type-check + build for production |
| `npm run preview` | Serve the production build locally |
| `npm run lint` | Run oxlint |

## Deployment

- Hosted on **Vercel** as an SPA — `vercel.json` rewrites all routes to `index.html`.
- CI (GitHub Actions → `../.github/workflows/ci.yml`) runs on pushes to `main` and on pull requests targeting `main`, and the frontend job is skipped entirely unless something under `Frontend/**` changed. After a successful lint + build on `main` it fires the Vercel deploy hook.

## Code Conventions

Full conventions live in `.agent/skills/frontend-react-architecture/SKILL.md` — read it before writing code. Highlights:

- **Data flow is top-down:** Page/Component → `hooks/api/<domain>.hooks.ts` (React Query — the *only* place `useQuery`/`useMutation` live) → `api/<domain>.api.ts` (axios, typed DTOs) → `axiosClient`. Components never call `src/api` or axios directly. Two deliberate exceptions sit outside `hooks/api`: `App.tsx` owns the `QueryClientProvider`, and `routes/components/SessionExpiredHandler.tsx` calls `useQueryClient().removeQueries(...)` to purge cached data when the session expires.
- **DTOs:** `interface`s in `src/types/dtos/` mirroring backend `camelCase` records; `ApiResponse<T>` and `ApiErrorResponse` mirror the backend envelope; the axios interceptor unwraps `response.data.data`.
- **Query keys:** hierarchical `as const` arrays in `src/hooks/api/queryKey.ts`; mutations invalidate only affected keys on `onSuccess`.
- **State:** zustand stores (`persist`) for UI-only prefs (theme/language); no TS `enum` — unions derived from `as const` arrays.
- **Styling:** theme via antd `ConfigProvider` + `theme.useToken()` (never hardcoded hex); `message`/`modal` via `App.useApp()` (never static antd imports); icons only from `@ant-design/icons`.
- **Loading/error UX:** `<Spin fullscreen />` for loading, `<ErrorComponent type="server" onRetry={refetch} />` for query errors, antd `<Empty>` for empty lists.
- **i18n:** default `vi`, fallback `en`, single `translation` namespace; all user-facing strings go through `useTranslation().t()` — never hardcode copy.
- **Imports:** `@/` alias (→ `src/`) + folder barrels only, never deep file imports. The one exception is `AiDeepTarotPage/`, which has no `index.ts` — `AppRouter.tsx` lazy-imports each spread subfolder directly.
- **Type-only imports:** `import type` required (`verbatimModuleSyntax`).

## Known gaps

- The shop's PayOS `ReturnUrl` / `CancelUrl` in `Backend/src/Api/appsettings.json` are still placeholders; the frontend doesn't rely on them (it opens `checkoutUrl` in a new tab and polls `GET /api/shop/orders/{id}`), but they must be set to real URLs before PayOS redirects are used.
- There is no 404 / catch-all route in `AppRouter` — an unknown URL renders `MainLayout` with an empty outlet.
- The backend's `AIChatMessage` / `AIChatTarotReading` entities have no controller, service or page yet, so the data model is ahead of the feature.
- No `env.d.ts`, so `import.meta.env.VITE_*` names are not type-checked (see [Configuration](#configuration)).