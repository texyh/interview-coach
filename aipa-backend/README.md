# AIPA Backend - AI Interview Preparation Assistant

A .NET 8.0 web API backend service built with Clean Architecture principles for the AI Interview Preparation Assistant (AIPA) platform.

## 🏗️ Architecture

This project follows Clean Architecture principles with the following layers:

- **AIPA.Domain** - Core business logic and entities (no dependencies)
- **AIPA.Application** - Use cases, commands, queries, and application services
- **AIPA.Infrastructure** - Data access, external services, and infrastructure concerns
- **AIPA.WebApi** - REST API controllers, middleware, and presentation logic

## 🚀 Technologies Used

- **.NET 8.0** - Latest LTS framework
- **MediatR** - CQRS pattern implementation
- **Entity Framework Core** - ORM with PostgreSQL provider
- **AutoMapper** - Object-to-object mapping
- **FluentValidation** - Input validation
- **Serilog** - Structured logging
- **Swagger/OpenAPI** - API documentation
- **xUnit** - Unit testing framework

## 📁 Project Structure

```
AIPA.sln
├── src/
│   ├── AIPA.Domain/
│   │   ├── Entities/
│   │   ├── Interfaces/
│   │   └── ValueObjects/
│   ├── AIPA.Application/
│   │   ├── Commands/
│   │   ├── Queries/
│   │   ├── DTOs/
│   │   └── Mappings/
│   ├── AIPA.Infrastructure/
│   │   ├── Data/
│   │   ├── Repositories/
│   │   └── Services/
│   └── AIPA.WebApi/
│       ├── Controllers/
│       └── Middleware/
└── tests/
    ├── AIPA.Domain.Tests/
    ├── AIPA.Application.Tests/
    ├── AIPA.Infrastructure.Tests/
    └── AIPA.WebApi.Tests/
```

## 🛠️ Development Setup

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [PostgreSQL](https://www.postgresql.org/download/) (for database)
- [Docker](https://docs.docker.com/get-docker/) (optional, for containerization)

### Getting Started

1. **Clone the repository**

   ```bash
   git clone <repository-url>
   cd aipa-backend
   ```

2. **Restore dependencies**

   ```bash
   dotnet restore
   ```

3. **Build the solution**

   ```bash
   dotnet build
   ```

4. **Run tests**

   ```bash
   dotnet test
   ```

5. **Run the API**
   ```bash
   dotnet run --project src/AIPA.WebApi
   ```

The API will be available at `https://localhost:5001` and `http://localhost:5000`.

### Configuration

Update `appsettings.json` in the WebApi project with your database connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=aipa_db;Username=your_user;Password=your_password"
  }
}
```

### Database Migrations

To create and apply database migrations:

```bash
# Add a new migration
dotnet ef migrations add InitialCreate --project src/AIPA.Infrastructure --startup-project src/AIPA.WebApi

# Update the database
dotnet ef database update --project src/AIPA.Infrastructure --startup-project src/AIPA.WebApi
```

## 🐳 Docker

### Build and run with Docker

```bash
# Build the Docker image
docker build -t aipa-backend .

# Run the container
docker run -p 8080:8080 aipa-backend
```

### Docker Compose (with PostgreSQL)

```yaml
version: "3.8"
services:
  api:
    build: .
    ports:
      - "8080:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=Host=db;Database=aipa_db;Username=postgres;Password=postgres
    depends_on:
      - db

  db:
    image: postgres:15
    environment:
      - POSTGRES_DB=aipa_db
      - POSTGRES_USER=postgres
      - POSTGRES_PASSWORD=postgres
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data

volumes:
  postgres_data:
```

## 📚 API Documentation

When running in Development mode, Swagger UI is available at:

- `https://localhost:5001/swagger`
- `http://localhost:5000/swagger`

## 🧪 Testing

Run all tests:

```bash
dotnet test
```

Run tests with coverage:

```bash
dotnet test --collect:"XPlat Code Coverage"
```

## 🔍 Code Quality

This project follows:

- Clean Architecture principles
- SOLID principles
- Domain-Driven Design (DDD) patterns
- Command Query Responsibility Segregation (CQRS)
- Repository pattern
- Unit of Work pattern

## 📝 Contributing

1. Follow the existing code style and architecture patterns
2. Add unit tests for new features
3. Update documentation as needed
4. Ensure all tests pass before submitting

## 📄 License

This project is part of the AIPA (AI Interview Preparation Assistant) platform.

## 🔗 Related Projects

- [AIPA Web App](../aipa-web-app) - Next.js frontend
- [AIPA Mobile App](../aipa-mobile-app) - Flutter mobile application
- [AIPA AI Service](../aipa-ai-service) - AI/ML microservice
