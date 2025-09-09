# 🚀 AIPA Platform - Quick Start Guide

## Start Services Easily

Use the `start-service.sh` script to quickly start any service in the AIPA platform:

```bash
# Show all available services
./start-service.sh

# Start AI Service (NestJS)
./start-service.sh ai-service

# Start Backend API (.NET Core)
./start-service.sh backend

# Start Web App (React/Vite)
./start-service.sh web-app

# Start Mobile App (Flutter)
./start-service.sh mobile-app
```

## Service Ports

| Service | Port | Technology | Description |
|---------|------|------------|-------------|
| **AI Service** | 3000 | NestJS + OpenAI | AI processing and interview analysis |
| **Backend API** | 5000 | .NET Core | REST API with Clean Architecture |
| **Web App** | 5173 | React + Vite | Main web interface |
| **Mobile App** | N/A | Flutter | Cross-platform mobile app |

## Development Workflow

1. **Start Backend First**: `./start-service.sh backend`
2. **Start AI Service**: `./start-service.sh ai-service`
3. **Start Web App**: `./start-service.sh web-app`
4. **Test Mobile App**: `./start-service.sh mobile-app`

## Advanced Options

```bash
# Start in production mode
./start-service.sh ai-service --prod

# Build before starting
./start-service.sh web-app --build

# Show service status
./start-service.sh --status

# Show help
./start-service.sh --help
```

## Prerequisites

- **Node.js 18+** (for AI Service and Web App)
- **.NET 9 SDK** (for Backend)
- **Flutter SDK** (for Mobile App)
- **PostgreSQL** (for database services)

## Quick Setup

```bash
# 1. Clone the repository
git clone <repository-url>
cd interview-preper

# 2. Make script executable
chmod +x start-service.sh

# 3. Start your preferred service
./start-service.sh web-app
```

## Troubleshooting

If you encounter permission issues:
```bash
chmod +x start-service.sh
```

For dependency issues, each service will automatically install dependencies when first started.

---

**Happy coding with AIPA! 🎉**
