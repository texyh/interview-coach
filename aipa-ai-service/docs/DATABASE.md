# Database Setup and Integration

This document explains the database setup and integration for the AIPA AI Service using Prisma ORM with PostgreSQL.

## Overview

The service uses Prisma as the ORM to interact with a PostgreSQL database. The database schema is designed to support the AI-powered interview analysis platform with the following main entities:

- **Users**: System users with role-based access
- **Interview Sessions**: Individual interview sessions
- **Interview Questions**: Questions asked during sessions
- **Interview Responses**: User responses to questions
- **AI Analysis**: AI-powered analysis of responses
- **Speech Analysis**: Speech quality and transcription analysis
- **Feedback Reports**: Comprehensive feedback and recommendations
- **API Usage**: Tracking of external API usage and costs

## Database Schema

The schema includes 12 models with comprehensive relationships:

### Core Models

- `User` - User accounts and profiles
- `InterviewSession` - Interview session management
- `InterviewQuestion` - Question bank and session questions
- `InterviewResponse` - User responses with audio/text

### Analysis Models

- `AiAnalysis` - AI-powered content analysis
- `SpeechAnalysis` - Speech quality and transcription
- `FeedbackReport` - Comprehensive feedback reports

### Supporting Models

- `ApiUsage` - API usage tracking
- `UserPreferences` - User settings and preferences
- `SystemSettings` - Application configuration
- `AuditLog` - System audit trails
- `Notification` - User notifications

## Setup Instructions

### Prerequisites

1. **PostgreSQL Database**

   ```bash
   # Using Docker (recommended for development)
   docker run --name aipa-postgres \
     -e POSTGRES_DB=postgres \
     -e POSTGRES_USER=postgres \
     -e POSTGRES_PASSWORD=password \
     -p 5432:5432 \
     -d postgres:15
   ```

2. **Environment Variables**
   Update your `.env` file with the database connection:
   ```env
   DATABASE_URL="postgresql://postgres:password@localhost:5432/postgres"
   ```

### Database Migration

1. **Generate Prisma Client**

   ```bash
   npm run db:generate
   ```

2. **Run Initial Migration**

   ```bash
   npm run db:migrate
   ```

3. **Seed Development Data**
   ```bash
   npm run db:seed
   ```

### Available Commands

| Command                   | Description                            |
| ------------------------- | -------------------------------------- |
| `npm run db:generate`     | Generate Prisma client                 |
| `npm run db:migrate`      | Run database migrations in development |
| `npm run db:migrate:prod` | Deploy migrations to production        |
| `npm run db:seed`         | Seed database with sample data         |
| `npm run db:reset`        | Reset database and re-run migrations   |
| `npm run db:studio`       | Open Prisma Studio (database GUI)      |

## Service Integration

### PrismaService

The `PrismaService` provides:

- Connection management with automatic connect/disconnect
- Health checking capabilities
- Transaction support
- Connection pooling
- Structured logging

```typescript
// Example usage in a service
constructor(private prisma: PrismaService) {}

async createUser(userData: CreateUserDto) {
  const client = this.prisma.getClient();
  return client.user.create({ data: userData });
}
```

### Health Checks

Database health is monitored through the `/health/database` endpoint:

- Connection status
- Response time monitoring
- Error reporting

### Error Handling

The service includes comprehensive error handling:

- Connection failures
- Query timeouts
- Constraint violations
- Transaction rollbacks

## Development Workflow

### Schema Changes

1. **Modify Schema**
   Edit `prisma/schema.prisma`

2. **Create Migration**

   ```bash
   npm run db:migrate
   ```

3. **Update Seed Data** (if needed)
   Modify `prisma/seed.ts`

4. **Test Changes**
   ```bash
   npm run db:seed
   npm run test
   ```

### Production Deployment

1. **Deploy Migrations**

   ```bash
   npm run db:migrate:prod
   ```

2. **Generate Client**
   ```bash
   npm run db:generate
   ```

## Troubleshooting

### Common Issues

1. **Connection Refused**
   - Verify PostgreSQL is running
   - Check DATABASE_URL in .env
   - Ensure correct host/port

2. **Migration Errors**
   - Check for schema conflicts
   - Review migration files in `prisma/migrations/`
   - Consider `npm run db:reset` for development

3. **TypeScript Errors**
   - Run `npm run db:generate` after schema changes
   - Restart TypeScript server in your IDE

### Database Performance

The schema includes optimizations:

- Proper indexing on foreign keys
- Efficient relationship modeling
- JSON fields for flexible data
- Appropriate field types and constraints

## Security Considerations

- Connection strings should use environment variables
- Database user should have minimal required permissions
- Enable SSL in production environments
- Regular backup and recovery procedures
- Audit logging for sensitive operations

## Monitoring and Maintenance

### Health Monitoring

- Database connection health checks
- Query performance monitoring
- Connection pool status

### Data Retention

- Automatic cleanup of old API usage records
- Configurable retention policies
- Archive strategies for historical data

### Backup Strategy

- Regular automated backups
- Point-in-time recovery capabilities
- Cross-region backup replication (production)

## Future Considerations

- Read replicas for scaling
- Database sharding for large datasets
- Caching layer integration
- Advanced analytics queries
- Data warehouse integration
