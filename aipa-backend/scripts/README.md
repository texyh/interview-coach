# Scripts Directory

This directory contains utility scripts for the AIPA Backend project.

## Migration Script (`migrate.sh`)

The migration script handles Entity Framework Core migrations for the project.

### Usage

```bash
# Make the script executable (first time only)
chmod +x scripts/migrate.sh

# Add a new migration
./scripts/migrate.sh add <MigrationName>

# Update database to latest migration
./scripts/migrate.sh update

# Remove the last migration
./scripts/migrate.sh remove

# List all migrations
./scripts/migrate.sh list

# Check migration status
./scripts/migrate.sh status

# Reset database (WARNING: destructive)
./scripts/migrate.sh reset
```

### Examples

```bash
# Add a migration for user entity changes
./scripts/migrate.sh add UpdateUserEntity

# Update database after pulling changes
./scripts/migrate.sh update

# Check what migrations are applied
./scripts/migrate.sh status
```

### When to Run Migrations

Migrations should be run when:

1. **Entity changes**: Any time you modify domain entities or value objects
2. **Configuration changes**: When you update Entity Framework configurations
3. **New entities**: When you add new domain entities
4. **Relationship changes**: When you modify entity relationships
5. **After pulling changes**: When other developers have added migrations

### Development Workflow

1. Make changes to entities or configurations
2. Build the project to ensure no compilation errors
3. Add a new migration with a descriptive name
4. Update the database
5. Test your changes

### Important Notes

- Always run migrations after modifying entities or EF configurations
- Use descriptive migration names (e.g., `AddUserPreferences`, `UpdateInterviewEntity`)
- The script requires the .NET CLI to be installed
- Database reset is destructive and should only be used in development
- Always build the project before running migrations
