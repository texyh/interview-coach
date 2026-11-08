# interview-coach (AIPA)

AIPA is an AI interview preparation assistant. Candidates practise interviews and the app gives them AI-generated questions, speech analysis and feedback.

## Monorepo layout

| Path | What | Stack |
|---|---|---|
| `apps/backend` | Core API (users, auth, sessions) | .NET 8, Clean Architecture (Domain / Application / Infrastructure / WebApi), EF Core, PostgreSQL |
| `apps/ai-service` | AI service: question generation, answer analysis, feedback, speech | NestJS 11, Prisma, OpenAI, Azure Speech |
| `apps/web` | Web client | React 19, Vite, Tailwind |
| `apps/mobile` | Mobile client | Flutter |
| `docs/` | Architecture, standards, environment setup | |
| `docs/genai/` | Product requirements, epics, stories and task breakdowns | |
| `scripts/` | Dev workflow and environment setup | |

The two Node apps are npm workspaces, managed from the root `package.json`.

## Getting started

```bash
npm install            # installs apps/ai-service and apps/web
npm run dev:api        # .NET backend
npm run dev:ai         # NestJS AI service
npm run dev:web        # React web app
cd apps/mobile && flutter pub get && flutter run
```

Copy each app's `.env.example` to `.env` before running. See [docs/ENVIRONMENT_CONFIGURATION.md](docs/ENVIRONMENT_CONFIGURATION.md) and [docs/DEVELOPER_ONBOARDING.md](docs/DEVELOPER_ONBOARDING.md).

## Docs

- [Architecture](docs/ARCHITECTURE.md)
- [Coding standards](docs/CODING_STANDARDS.md)
- [Security guidelines](docs/SECURITY_GUIDELINES.md)
- [User journey roadmap](docs/planning/USER_JOURNEY_ROADMAP.md)

## License

[MIT](LICENSE)
