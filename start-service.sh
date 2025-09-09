#!/bin/bash

# AIPA Platform Service Starter
# This script helps you easily start any service in the AIPA platform

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
PURPLE='\033[0;35m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# Function to print colored output
print_colored() {
    echo -e "${1}${2}${NC}"
}

# Function to show usage
show_usage() {
    echo
    print_colored $CYAN "🚀 AIPA Platform Service Starter"
    echo
    print_colored $YELLOW "Usage: $0 [service-name] [options]"
    echo
    print_colored $BLUE "Available Services:"
    echo "  ai-service     - NestJS AI processing service (Port: 3000)"
    echo "  backend        - .NET Core API service (Port: 5000)"
    echo "  web-app        - React Vite web application (Port: 5173)"
    echo "  mobile-app     - Flutter mobile application"
    echo
    print_colored $BLUE "Options:"
    echo "  --dev          - Start in development mode (default)"
    echo "  --prod         - Start in production mode"
    echo "  --build        - Build before starting"
    echo "  --help         - Show this help message"
    echo
    print_colored $BLUE "Examples:"
    echo "  $0 ai-service"
    echo "  $0 backend --prod"
    echo "  $0 web-app --build"
    echo
}

# Function to check if a service directory exists
check_service_exists() {
    local service=$1
    local dir_name

    case $service in
        "ai-service")
            dir_name="aipa-ai-service"
            ;;
        "backend")
            dir_name="aipa-backend"
            ;;
        "web-app")
            dir_name="aipa-web-app"
            ;;
        "mobile-app")
            dir_name="aipa-mobile-app"
            ;;
        *)
            print_colored $RED "❌ Error: Unknown service '$service'"
            show_usage
            exit 1
            ;;
    esac

    if [ ! -d "$dir_name" ]; then
        print_colored $RED "❌ Error: Service directory '$dir_name' not found!"
        exit 1
    fi

    echo "$dir_name"
}

# Function to start AI service
start_ai_service() {
    local mode=${1:-dev}
    local service_dir=$(check_service_exists "ai-service")

    print_colored $GREEN "🤖 Starting AIPA AI Service (NestJS)..."

    cd "$service_dir"

    # Install dependencies if needed
    if [ ! -d "node_modules" ]; then
        print_colored $YELLOW "📦 Installing dependencies..."
        npm install
    fi

    # Generate Prisma client if needed
    if [ -d "prisma" ] && [ ! -d "node_modules/.prisma" ]; then
        print_colored $YELLOW "🗄️ Generating Prisma client..."
        npx prisma generate
    fi

    # Start the service
    print_colored $BLUE "🚀 Starting NestJS server on port 3000..."
    if [ "$mode" = "prod" ]; then
        npm run build
        npm run start:prod
    else
        npm run start:dev
    fi
}

# Function to start backend service
start_backend_service() {
    local mode=${1:-dev}
    local service_dir=$(check_service_exists "backend")

    print_colored $GREEN "🔧 Starting AIPA Backend (.NET Core)..."

    cd "$service_dir"

    # Restore packages if needed
    if [ ! -d "obj" ] || [ ! -d "bin" ]; then
        print_colored $YELLOW "📦 Restoring .NET packages..."
        dotnet restore
    fi

    # Build if requested or in production mode
    if [ "$mode" = "prod" ] || [ "$2" = "--build" ]; then
        print_colored $YELLOW "🔨 Building .NET application..."
        dotnet build --configuration Release
    fi

    # Start the service
    print_colored $BLUE "🚀 Starting .NET API server on port 5000..."
    if [ "$mode" = "prod" ]; then
        dotnet run --configuration Release
    else
        dotnet run
    fi
}

# Function to start web app
start_web_app() {
    local mode=${1:-dev}
    local service_dir=$(check_service_exists "web-app")

    print_colored $GREEN "🌐 Starting AIPA Web App (React/Vite)..."

    cd "$service_dir"

    # Install dependencies if needed
    if [ ! -d "node_modules" ]; then
        print_colored $YELLOW "📦 Installing dependencies..."
        npm install
    fi

    # Start the service
    print_colored $BLUE "🚀 Starting Vite dev server on port 5173..."
    if [ "$mode" = "prod" ]; then
        npm run build
        npm run preview
    else
        npm run dev
    fi
}

# Function to start mobile app
start_mobile_app() {
    local mode=${1:-dev}
    local service_dir=$(check_service_exists "mobile-app")

    print_colored $GREEN "📱 Starting AIPA Mobile App (Flutter)..."

    cd "$service_dir"

    # Get dependencies if needed
    if [ ! -d ".dart_tool" ]; then
        print_colored $YELLOW "📦 Getting Flutter dependencies..."
        flutter pub get
    fi

    # Start the service
    print_colored $BLUE "🚀 Starting Flutter development server..."
    if [ "$mode" = "prod" ]; then
        print_colored $YELLOW "Building Flutter app..."
        flutter build apk --release
        print_colored $GREEN "✅ APK built successfully!"
        echo "📱 Find your APK at: $service_dir/build/app/outputs/flutter-apk/app-release.apk"
    else
        flutter run
    fi
}

# Function to show service status
show_status() {
    print_colored $CYAN "📊 AIPA Platform Services Status"
    echo

    services=("aipa-ai-service" "aipa-backend" "aipa-web-app" "aipa-mobile-app")
    ports=("3000" "5000" "5173" "N/A")

    for i in "${!services[@]}"; do
        service="${services[$i]}"
        port="${ports[$i]}"

        if [ -d "$service" ]; then
            print_colored $GREEN "✅ $service (Port: $port)"
        else
            print_colored $RED "❌ $service - Directory not found"
        fi
    done

    echo
    print_colored $YELLOW "💡 Use '$0 <service-name>' to start any service"
}

# Main script logic
main() {
    local service=""
    local mode="dev"
    local build=false

    # Parse arguments
    while [[ $# -gt 0 ]]; do
        case $1 in
            --help|-h)
                show_usage
                exit 0
                ;;
            --prod)
                mode="prod"
                shift
                ;;
            --dev)
                mode="dev"
                shift
                ;;
            --build)
                build=true
                shift
                ;;
            --status)
                show_status
                exit 0
                ;;
            ai-service|backend|web-app|mobile-app)
                if [ -z "$service" ]; then
                    service=$1
                else
                    print_colored $RED "❌ Error: Multiple services specified"
                    show_usage
                    exit 1
                fi
                shift
                ;;
            *)
                print_colored $RED "❌ Error: Unknown option '$1'"
                show_usage
                exit 1
                ;;
        esac
    done

    # If no service specified, show status
    if [ -z "$service" ]; then
        show_status
        echo
        print_colored $YELLOW "Please specify a service to start:"
        print_colored $BLUE "  $0 ai-service    # Start AI service"
        print_colored $BLUE "  $0 backend       # Start backend API"
        print_colored $BLUE "  $0 web-app       # Start web application"
        print_colored $BLUE "  $0 mobile-app    # Start mobile app"
        echo
        print_colored $YELLOW "Or use:"
        print_colored $BLUE "  $0 --status      # Show service status"
        print_colored $BLUE "  $0 --help        # Show help"
        exit 0
    fi

    # Start the specified service
    case $service in
        "ai-service")
            start_ai_service "$mode"
            ;;
        "backend")
            start_backend_service "$mode" "$build"
            ;;
        "web-app")
            start_web_app "$mode"
            ;;
        "mobile-app")
            start_mobile_app "$mode"
            ;;
    esac
}

# Run main function with all arguments
main "$@"
