# MyTarotReader — Frontend

> ⚠️ **Part of an app under active development.** The UI and its conventions are still evolving.

The frontend of MyTarotReader is a **React + TypeScript single-page application** built with **Vite**, featuring tarot draws, AI readings, an AI chat reader, reading history, and Google sign-in.

## Tech stack

- **React 19** + **TypeScript**, built with **Vite**
- **Ant Design (antd)** — component library
- **TanStack Query** — server-state / data fetching
- **Zustand** — lightweight client-state stores
- **react-router-dom** — routing
- **i18next + react-i18next** — internationalization (English / Vietnamese)
- **axios** — HTTP client (wrapped in `src/api/config.api.ts`)
- **@fingerprintjs/fingerprintjs** — guest device fingerprinting
- **@react-oauth/google** — Google sign-in
- **react-markdown** — rendering the AI chat responses

## Folder structure

```
Frontend/
├── index.html
├── vite.config.ts            # "@" alias → ./src, vendor chunk splitting
├── vercel.json               # SPA rewrite: all routes → /index.html
├── .env[.local]              # VITE_API_URL, VITE_GOOGLE_CLIENT_ID
└── src/
    ├── api/                  # axios client + per-feature API modules (…api.ts)
    ├── assets/cards/         # 78 tarot card images (webp)
    ├── constants/            # route, theme, queryKey, enum, language, tarot, …
    ├── hooks/                # useDocumentTitle
    │   ├── api/              # TanStack Query hooks per feature (…hooks.ts)
    │   └── store/            # zustand stores (language, theme)
    ├── i18n/                 # i18next setup
    │   └── locales/          # en/ vi → errors, pages, tarot namespaces
    ├── layouts/MainLayout/   # shared header/footer/drawer shell
    ├── pages/
    │   ├── common/           # e.g. hero section
    │   ├── public/           # guest-facing pages (Home, Login, GuestDraw, …)
    │   └── protected/        # auth-required pages (AuthHome, AuthDraw, History, …)
    ├── routes/               # AppRouter + ProtectedRoute / PublicRoute
    ├── types/                # per-feature TypeScript types (…types.ts)
    └── utils/                # error, datetime, tarot, fingerprint, common helpers
```

## Routing / pages

Routes are centralized in `src/constants/route.constants.ts`.

| Route | Page | Access |
| --- | --- | --- |
| `/guest` | Home (landing / hero) | public |
| `/login` | Login (Google OAuth) | public |
| `/guest/draw` | Guest draw | public |
| `/` | Authenticated home (`AuthHome`) | protected |
| `/draw` | Authenticated draw (`AuthDraw`) | protected |
| `/draw/ai` | AI draw (`AIDrawTarotPage`) | protected |
| `/draw/ai-chat` | AI tarot chat (`AiTarotChatPage`) | protected |
| `/history` | Reading history (`HistoryPage`) | protected |

Protected routes are guarded by `ProtectedRoute`; guest-only routes by `PublicRoute` (both in `src/routes/components/`). Pages are lazy-loaded and wrapped with `RouteTitle` for document titles.

## Data-flow convention

The frontend follows a strict, one-way flow so that pages never talk to the network layer directly:

```
{N}Controller (API)  →  {n}.api.ts  →  {n}.hooks.ts (TanStack Query)  →  Page/Component
```

- **One TanStack hook = one API call.** A hook file exists per feature under `src/hooks/api/` (e.g. `tarot.hooks.ts`, `aiChat.hooks.ts`, `history.hooks.ts`).
- **GET** → `useQuery`.
- **POST / PUT / DELETE** → `useMutation`, and invalidate the related query cache on success.
- Pages and components call the hooks — **never** `axiosClient` directly.
- Api modules export typed objects (e.g. `tarotApi.draw(...)`) backed by the shared `axiosClient` from `src/api/config.api.ts`.

### Example: reading history

1. `HistoryController` exposes `api/v1/history`.
2. `src/api/history.api.ts` calls `GET /api/v1/history`.
3. `src/hooks/api/history.hooks.ts` exposes `useHistory()` (`useQuery`) and `useDeleteHistory()` (`useMutation` + invalidation).
4. `src/pages/protected/HistoryPage/` consumes the hooks and renders the list.

## Key conventions

- **All URLs** live in `src/constants/route.constants.ts` (both web paths and `API_URL`).
- **All theme values** (colors, sizing) live in `src/constants/theme.constants.ts` — no hard-coded colors.
- **Errors** are surfaced via `getErrorMessage` / `getFormFieldErrors` in `src/utils/error` — never raw `err.response.data`.
- **Every folder** with reusable code exports a barrel `index.ts`.
- **Responsive everywhere** — every page/component must be mobile-friendly.
- **i18n** — all user-facing strings live under `src/i18n/locales/{en,vi}/...` and are referenced by key, never hard-coded.

## Environment variables

Set these in a local `.env.local` (they are **baked in at build time** — set them as Environment Variables in your Vercel project for deployed builds):

| Variable | Purpose |
| --- | --- |
| `VITE_API_URL` | Base URL of the backend API (e.g. `http://localhost:5271`) |
| `VITE_GOOGLE_CLIENT_ID` | Google OAuth client ID for sign-in |

## Scripts

```bash
# Start the dev server (hot reload)
npm run dev

# Type-check + production build
npm run build

# Lint with oxlint (React + TypeScript plugins)
npm run lint

# Preview the production build locally
npm run preview
```

## CI

GitHub Actions (`.github/workflows/ci.yml`) runs `npm run lint` and `npm run build` on every push/PR to `main`, then deploys the frontend to **Vercel** via a Deploy Hook when code is merged to `main`. `vercel.json` rewrites every route to `index.html` so client-side routing works on refresh.

## Conventions (full reference)

The complete conventions — including the file-naming rules (`x.hooks.ts`, `x.api.ts`, `x.types.ts`) and the page/component folder layout — are defined in the project's [`CLAUDE.md`](../CLAUDE.md) and the **`react-typescript-frontend`** skill.
