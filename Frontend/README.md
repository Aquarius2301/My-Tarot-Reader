# Frontend

React 19 + Vite + TypeScript SPA for **My Tarot Reader**. Renders the tarot draw flows (guest + authenticated), the 78-card library, check-in streak & coins, history, and Google OAuth — in a bilingual (English / Vietnamese) UI.

## Requirements

- [Node.js 24](https://nodejs.org/) + npm (CI uses Node 24)
- Backend API running (see [../Backend/README.md](../Backend/README.md))

## Folder Structure

```
src/
├── api/                # axios client + typed API modules (one per backend controller)
│   ├── config.api.ts   # axiosClient: withCredentials, X-Device-Id, envelope unwrap, refresh queue
│   ├── url.api.ts      # API_URL endpoint map (as const)
│   └── <domain>.api.ts # auth, tarot, streak ...
├── components/         # shared feature folders (tarot/, layouts/, error/, modal/, coins/)
├── constants/          # as-const data + helpers (theme, tarot, language, streak)
├── hooks/
│   ├── api/            # TanStack Query hooks + hierarchical query keys
│   ├── custom/         # generic hooks (useDocumentTitle)
│   └── stores/         # zustand stores (theme, language)
├── i18n/               # i18next init + locale modules (pages, components, tarot, errors)
├── pages/              # guest/, auth/, shared/ page sections
├── routes/             # AppRouter, WEB_URL map, route guards, SessionExpiredHandler
├── types/              # enums + DTOs mirroring the backend
├── utils/              # pure helpers (common, datetime, error)
├── App.tsx             # QueryClientProvider + AppRouter
└── main.tsx            # createRoot + fingerprint warm-up
```

## Configuration

Copy the placeholder values in `.env` and adjust for your setup. There is no committed `.env.example` — `.env` holds local-only/placeholder values.

| Variable | Meaning | Example |
| --- | --- | --- |
| `VITE_API_URL` | Backend API base URL (axios `baseURL`) | `http://localhost:5271` |
| `VITE_GOOGLE_CLIENT_ID` | Google OAuth client ID for the custom redirect flow | `your-google-client-id` |

`VITE_API_URL` is baked into the bundle at build time — on Vercel it must be set as an Environment Variable in the project, not only locally.

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
- CI (GitHub Actions → `../.github/workflows/ci.yml`) lints and builds on PR/push, then triggers a Vercel deploy hook on `main`.

## Code Conventions

Full conventions live in `.agent/skills/frontend-react-architecture/SKILL.md` — read it before writing code. Highlights:

- **Data flow is top-down:** Page/Component → `hooks/api/<domain>.hooks.ts` (React Query, the *only* place `useQuery`/`useMutation` live) → `api/<domain>.api.ts` (axios, typed DTOs) → `axiosClient`. Components never call `src/api` or axios directly.
- **DTOs:** `interface`s in `src/types/dtos/` mirroring backend `camelCase` records; `ApiResponse<T>` and `ApiErrorResponse` mirror the backend envelope; the axios interceptor unwraps `response.data.data`.
- **Query keys:** hierarchical `as const` arrays in `src/hooks/api/queryKey.ts`; mutations invalidate only affected keys on `onSuccess`.
- **State:** zustand stores (`persist`) for UI-only prefs (theme/language); no TS `enum` — unions derived from `as const` arrays.
- **Styling:** theme via antd `ConfigProvider` + `theme.useToken()` (never hardcoded hex); `message`/`modal` via `App.useApp()` (never static antd imports); icons only from `@ant-design/icons`.
- **Loading/error UX:** `<Spin fullscreen />` for loading, `<ErrorComponent type="server" onRetry={refetch} />` for query errors, antd `<Empty>` for empty lists.
- **i18n:** default `vi`, fallback `en`, single `translation` namespace; all user-facing strings go through `useTranslation().t()` — never hardcode copy.
- **Imports:** `@/` alias (→ `src/`) + folder barrels only, never deep file imports.
- **Type-only imports:** `import type` required (`verbatimModuleSyntax`).