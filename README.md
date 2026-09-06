# MyTarotReader

> ⚠️ **Under active development** — this web application is a work in progress. Features, APIs, and data are subject to change and may be reset at any time. Do not rely on it as a production service.

An **AI-assisted online tarot reading** web app. Draw tarot cards, get AI-generated readings, chat with an AI tarot reader, and keep a history of your readings — all from your browser.

## Live demo

🔗 **https://my-tarot-reader.vercel.app/**

> The demo pulls the latest build of the frontend. Some features (sign-in with Google, AI readings) depend on third-party service keys and may be limited or unavailable on the free preview.

## Who it's for

- **Curious readers** who want to try a tarot draw without creating an account.
- **Regular users** looking for AI-personalized readings and a saved history.
- **Developers** interested in a full-stack reference application built with Clean Architecture and modern React tooling.

You can use the app as a **guest** or **sign in with Google** to unlock personalized features.

## Features

- **Guest draw** — draw tarot cards immediately with no account (`/guest/draw`).
- **Authenticated draw** — draw and save readings to your profile.
- **AI draw** — a single AI-generated reading from your draw (`/draw/ai`).
- **AI tarot chat** — have a conversational reading with an AI tarot reader (`/draw/ai-chat`).
- **Reading history** — browse and delete past readings.
- **Google OAuth sign-in** — JWT-based authentication.
- **Wallet coins** — white/red coin economy for services such as AI draws.

## Tech stack

| Layer | Technology |
| --- | --- |
| Backend | .NET 8, ASP.NET Core, EF Core (Clean Architecture: Api / Application / Infrastructure / Domain) |
| Database | SQL Server, Redis |
| Integrating services | Google OAuth, Google Gemini (AI), JWT |
| Frontend | React 19, TypeScript, Vite, Ant Design, TanStack Query, Zustand, react-router, i18next (EN/VI) |

## Repository layout

```
My-Tarot-Reader/
├── Backend/     # .NET 8 Clean Architecture API
└── Frontend/    # React + TypeScript web app
```

## Getting started

Each side of the project has its own setup guide:

- **[Backend README](Backend/README.md)** — prerequisites, configuration, migrations, and running the API locally.
- **[Frontend README](Frontend/README.md)** — environment variables, running the dev server, and the frontend architecture.

Quick start (summary):

```bash
# 1. Backend — from inside Backend/ (needs .NET 8, SQL Server, Redis + API keys)
./scripts/run-local.sh

# 2. Frontend — from inside Frontend/ (needs .env.local with VITE_API_URL / VITE_GOOGLE_CLIENT_ID)
npm install
npm run dev
```

## Further reading

- **[Backend README](Backend/README.md)** — folder structure, endpoints, and deep usage of the API.
- **[Frontend README](Frontend/README.md)** — folder structure, data-flow conventions, and deep usage of the web app.
- **[`CLAUDE.md`](CLAUDE.md)** — the project's conventions doc (architecture rules, naming, and checklists) used by contributors and Claude Code.

## Contributing

This is an early-stage project. Contributions are welcome, but please first read the conventions in [`CLAUDE.md`](CLAUDE.md) and the relevant layer README, and make sure the build passes with warnings treated as errors.

## License

`MyTarotReader` is provided as a **reference / personal project** and is not yet licensed for public redistribution. Contact the maintainers before reuse.
