#!/bin/bash

# Start Application Script for AIPA Backend
# This script starts the web API application

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
    echo "Usage: $0 [ENVIRONMENT] [OPTIONS]"
    echo ""
    echo "Environments:"
    echo "  dev, development     Start in Development mode (default)"
    echo "  staging             Start in Staging mode"
    echo "  prod, production    Start in Production mode"
    echo ""
    echo "Options:"
    echo "  --watch, -w         Enable hot reload (development only)"
    echo "  --build, -b         Build before running"
    echo "  --migrate, -m       Run migrations before starting"
    echo "  --seed, -s          Seed database after migrations"
    echo "  --verbose, -v       Verbose output"
    echo "  --help, -h          Show this help message"
    echo ""
    echo "Examples:"
    echo "  $0                  # Start in development mode"
    echo "  $0 dev --watch      # Start with hot reload"
    echo "  $0 dev -bmw         # Build, migrate, and watch"
    echo "  $0 staging -bm      # Build and migrate for staging"
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
WEBAPI_PROJECT="$PROJECT_DIR/src/AIPA.WebApi"
SOLUTION_FILE="$PROJECT_DIR/AIPA.sln"

# Check if projects exist
check_projects() {
    if [ ! -f "$WEBAPI_PROJECT/AIPA.WebApi.csproj" ]; then
        print_error "WebApi project not found at: $WEBAPI_PROJECT"
        exit 1
    fi
    
    if [ ! -f "$SOLUTION_FILE" ]; then
        print_error "Solution file not found at: $SOLUTION_FILE"
        exit 1
    fi
}

# Build the solution
build_solution() {
    print_info "Building solution..."
    
    cd "$PROJECT_DIR"
    
    if dotnet build "$SOLUTION_FILE" --configuration Release; then
        print_success "Solution built successfully"
    else
        print_error "Failed to build solution"
        exit 1
    fi
}

# Run migrations
run_migrations() {
    print_info "Running database migrations..."
    
    if [ -x "$PROJECT_DIR/scripts/migrate.sh" ]; then
        if "$PROJECT_DIR/scripts/migrate.sh" update; then
            print_success "Migrations completed successfully"
        else
            print_error "Failed to run migrations"
            exit 1
        fi
    else
        print_warning "Migration script not found or not executable"
        print_info "Skipping migrations..."
    fi
}

# Seed database
seed_database() {
    print_info "Seeding database..."
    print_warning "Database seeding not yet implemented"
    # TODO: Implement database seeding when seeder is available
}

# Start the application
start_app() {
    local environment="$1"
    local watch_mode="$2"
    local verbose="$3"
    
    print_info "Starting AIPA Backend API..."
    print_info "Environment: $environment"
    print_info "Project: $WEBAPI_PROJECT"
    
    cd "$WEBAPI_PROJECT"
    
    # Set environment variable
    export ASPNETCORE_ENVIRONMENT="$environment"
    
    # Build run command
    local run_cmd="dotnet run"
    
    if [ "$watch_mode" = "true" ] && [ "$environment" = "Development" ]; then
        run_cmd="dotnet watch run"
        print_info "Hot reload enabled"
    fi
    
    if [ "$verbose" = "true" ]; then
        run_cmd="$run_cmd --verbosity normal"
    fi
    
    print_info "Executing: $run_cmd"
    print_info "Press Ctrl+C to stop the application"
    echo ""
    
    # Start the application
    exec $run_cmd
}

# Parse arguments
parse_arguments() {
    local environment="Development"
    local build_before="false"
    local run_migrations="false"
    local seed_db="false"
    local watch_mode="false"
    local verbose="false"
    
    while [[ $# -gt 0 ]]; do
        case $1 in
            dev|development)
                environment="Development"
                shift
                ;;
            staging)
                environment="Staging"
                shift
                ;;
            prod|production)
                environment="Production"
                shift
                ;;
            --build|-b)
                build_before="true"
                shift
                ;;
            --migrate|-m)
                run_migrations="true"
                shift
                ;;
            --seed|-s)
                seed_db="true"
                shift
                ;;
            --watch|-w)
                watch_mode="true"
                shift
                ;;
            --verbose|-v)
                verbose="true"
                shift
                ;;
            --help|-h)
                show_usage
                exit 0
                ;;
            -*|--*)
                # Handle combined flags like -bmw
                if [[ $1 =~ ^-[bmswv]+$ ]]; then
                    for ((i=1; i<${#1}; i++)); do
                        case ${1:$i:1} in
                            b) build_before="true" ;;
                            m) run_migrations="true" ;;
                            s) seed_db="true" ;;
                            w) watch_mode="true" ;;
                            v) verbose="true" ;;
                            *) print_error "Unknown option: -${1:$i:1}"; exit 1 ;;
                        esac
                    done
                else
                    print_error "Unknown option $1"
                    show_usage
                    exit 1
                fi
                shift
                ;;
            *)
                print_error "Unknown argument: $1"
                show_usage
                exit 1
                ;;
        esac
    done
    
    # Validate watch mode only works in development
    if [ "$watch_mode" = "true" ] && [ "$environment" != "Development" ]; then
        print_warning "Watch mode is only available in Development environment"
        watch_mode="false"
    fi
    
    # Execute pre-start tasks
    if [ "$build_before" = "true" ]; then
        build_solution
    fi
    
    if [ "$run_migrations" = "true" ]; then
        run_migrations
    fi
    
    if [ "$seed_db" = "true" ]; then
        seed_database
    fi
    
    # Start the application
    start_app "$environment" "$watch_mode" "$verbose"
}

# Main script logic
main() {
    print_info "AIPA Backend Startup Script"
    echo ""
    
    check_dotnet
    check_projects
    
    if [ $# -eq 0 ]; then
        # Default: start in development mode
        parse_arguments "dev"
    else
        parse_arguments "$@"
    fi
}

# Handle Ctrl+C gracefully
trap 'echo -e "\n${YELLOW}[INFO]${NC} Application stopped by user"; exit 0' INT

# Run main function with all arguments
main "$@"
