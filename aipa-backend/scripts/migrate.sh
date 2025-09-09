#!/bin/bash

# Migration script for AIPA Backend
# This script handles EF Core migrations for the project

set -e  # Exit on any error

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Function to print colored output
print_info() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

print_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Function to show usage
show_usage() {
    echo "Usage: $0 [COMMAND] [OPTIONS]"
    echo ""
    echo "Commands:"
    echo "  add <name>           Add a new migration with the specified name"
    echo "  update              Update database to latest migration"
    echo "  remove              Remove the last migration"
    echo "  list                List all migrations"
    echo "  reset               Reset database (WARNING: destructive)"
    echo "  status              Show migration status"
    echo ""
    echo "Examples:"
    echo "  $0 add AddUserTable"
    echo "  $0 update"
    echo "  $0 remove"
    echo "  $0 list"
}

# Check if dotnet is installed
check_dotnet() {
    if ! command -v dotnet &> /dev/null; then
        print_error "dotnet CLI is not installed or not in PATH"
        exit 1
    fi
}

# Get project directory
PROJECT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
INFRASTRUCTURE_PROJECT="$PROJECT_DIR/src/AIPA.Infrastructure"
WEBAPI_PROJECT="$PROJECT_DIR/src/AIPA.WebApi"

# Check if projects exist
check_project() {
    if [ ! -f "$INFRASTRUCTURE_PROJECT/AIPA.Infrastructure.csproj" ]; then
        print_error "Infrastructure project not found at: $INFRASTRUCTURE_PROJECT"
        exit 1
    fi
    if [ ! -f "$WEBAPI_PROJECT/AIPA.WebApi.csproj" ]; then
        print_error "WebAPI project not found at: $WEBAPI_PROJECT"
        exit 1
    fi
}

# Add a new migration
add_migration() {
    local migration_name="$1"

    if [ -z "$migration_name" ]; then
        print_error "Migration name is required"
        show_usage
        exit 1
    fi

    print_info "Adding migration: $migration_name"

    cd "$WEBAPI_PROJECT"

    if dotnet ef migrations add "$migration_name" \
        --project "../AIPA.Infrastructure/AIPA.Infrastructure.csproj" \
        --startup-project "AIPA.WebApi.csproj" \
        --verbose; then
        print_success "Migration '$migration_name' added successfully"
    else
        print_error "Failed to add migration '$migration_name'"
        exit 1
    fi
}

# Update database
update_database() {
    print_info "Updating database to latest migration..."

    cd "$WEBAPI_PROJECT"

    if dotnet ef database update \
        --project "../AIPA.Infrastructure/AIPA.Infrastructure.csproj" \
        --startup-project "AIPA.WebApi.csproj" \
        --verbose; then
        print_success "Database updated successfully"
    else
        print_error "Failed to update database"
        exit 1
    fi
}

# Remove last migration
remove_migration() {
    print_warning "Removing the last migration..."
    read -p "Are you sure? This action cannot be undone. (y/N): " -n 1 -r
    echo

    if [[ ! $REPLY =~ ^[Yy]$ ]]; then
        print_info "Migration removal cancelled"
        exit 0
    fi

    cd "$WEBAPI_PROJECT"

    if dotnet ef migrations remove \
        --project "../AIPA.Infrastructure/AIPA.Infrastructure.csproj" \
        --startup-project "AIPA.WebApi.csproj" \
        --verbose; then
        print_success "Last migration removed successfully"
    else
        print_error "Failed to remove migration"
        exit 1
    fi
}

# List migrations
list_migrations() {
    print_info "Listing all migrations..."

    cd "$WEBAPI_PROJECT"

    dotnet ef migrations list \
        --project "../AIPA.Infrastructure/AIPA.Infrastructure.csproj" \
        --startup-project "AIPA.WebApi.csproj"
}

# Show migration status
show_status() {
    print_info "Checking migration status..."

    cd "$WEBAPI_PROJECT"

    echo "Applied migrations:"
    dotnet ef migrations list \
        --project "../AIPA.Infrastructure/AIPA.Infrastructure.csproj" \
        --startup-project "AIPA.WebApi.csproj" \
        --no-build 2>/dev/null || print_warning "Could not retrieve migration status"
}

# Reset database (destructive)
reset_database() {
    print_warning "This will completely reset the database and all data will be lost!"
    read -p "Are you sure you want to reset the database? Type 'RESET' to confirm: " confirm

    if [ "$confirm" != "RESET" ]; then
        print_info "Database reset cancelled"
        exit 0
    fi

    cd "$WEBAPI_PROJECT"

    print_info "Dropping database..."
    if dotnet ef database drop \
        --project "../AIPA.Infrastructure/AIPA.Infrastructure.csproj" \
        --startup-project "AIPA.WebApi.csproj" \
        --force --verbose; then
        print_success "Database dropped"
    else
        print_warning "Database drop failed or database doesn't exist"
    fi

    print_info "Updating database to latest migration..."
    if dotnet ef database update \
        --project "../AIPA.Infrastructure/AIPA.Infrastructure.csproj" \
        --startup-project "AIPA.WebApi.csproj" \
        --verbose; then
        print_success "Database reset and updated successfully"
    else
        print_error "Failed to update database after reset"
        exit 1
    fi
}

# Main script logic
main() {
    check_dotnet
    check_project

    if [ $# -eq 0 ]; then
        show_usage
        exit 1
    fi

    case "$1" in
        "add")
            add_migration "$2"
            ;;
        "update")
            update_database
            ;;
        "remove")
            remove_migration
            ;;
        "list")
            list_migrations
            ;;
        "status")
            show_status
            ;;
        "reset")
            reset_database
            ;;
        *)
            print_error "Unknown command: $1"
            show_usage
            exit 1
            ;;
    esac
}

# Run main function with all arguments
main "$@"
