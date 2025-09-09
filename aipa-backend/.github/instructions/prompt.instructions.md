---
applyTo: "**"
---

# Development Guidelines

## Core Principles

- **DO NOT add comments except for complex logic** - Code should be self-explanatory
- **DO NOT recreate anything that exists** - Always check for existing functionality first
- **DO NOT modify auto-generated content** - Respect existing implementations
- **BUILD after every change** - Ensure the project compiles successfully
- **RUN MIGRATIONS after entity changes** - Use scripts/migrate.sh for database updates
- **Create todo lists** - Plan implementation before coding
- **If you delete a file, recreate it** with the correct content immediately

## Mandatory Process Flow
** Understand the existing architecture before making changes.**
1. **Ask clarifying questions** if requirements are unclear
2. **Check existing codebase first** - Use file search, grep, and read operations
3. **Create a comprehensive todo list** of what needs to be implemented
4. **Plan the implementation** before writing any code
5. **Don't create duplicate functionality** - Verify uniqueness
6. **Build the project after every significant change** - Catch errors early
7. **Run migrations when entities/configurations change** - Use `./scripts/migrate.sh add <MigrationName>` then `./scripts/migrate.sh update`
   ** Always question why existing code exists before modifying it**
   **Preserve business rules unless explicitly told they're incorrect**
   **Add functionality, don't remove unless there's a clear justification**

## Migration Requirements

**ALWAYS run migrations when:**

- Creating new entities or value objects
- Modifying existing entities (properties, relationships, constraints)
- Updating Entity Framework configurations
- Adding/removing entity properties
- Changing database schemas

**Migration workflow:**

1. Make entity/configuration changes
2. Build project to ensure no errors: `dotnet build`
3. Add migration: `./scripts/migrate.sh add <DescriptiveName>`
4. Update database: `./scripts/migrate.sh update`
5. Test changes

## Critical Rules - NEVER Make These Mistakes

- ❌ Don't recreate existing features or files
- ❌ Don't assume functionality doesn't exist without checking
- ❌ Don't skip building the project after changes
- ❌ Don't proceed without understanding existing architecture
- ❌ Don't create duplicate implementations
- ❌ Don't modify files without understanding their purpose
- ❌ Don't change entities without running migrations
- ❌ Don't skip migration steps when modifying database schema
