#!/bin/bash

# AIPA Project - Complete Development Workflow Script
# This script manages the entire development workflow including all client generation

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
PURPLE='\033[0;35m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# Project structure
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"

# Service directories
BACKEND_DIR="$PROJECT_ROOT/aipa-backend"
AI_SERVICE_DIR="$PROJECT_ROOT/aipa-ai-service"
WEB_APP_DIR="$PROJECT_ROOT/aipa-web-app"
MOBILE_APP_DIR="$PROJECT_ROOT/aipa-mobile-app"

# Service URLs
BACKEND_URL="http://localhost:5030"
AI_SERVICE_URL="http://localhost:3001"
WEB_APP_URL="http://localhost:3000"

echo -e "${BLUE}🚀 AIPA Project - Complete Development Workflow${NC}"
echo "=============================================================="

# Function to print a section header
print_section() {
    echo ""
    echo -e "${PURPLE}📋 $1${NC}"
    echo "----------------------------------------"
}

# Function to check if a service is running
check_service() {
    local url=$1
    local service_name=$2

    if curl -s --connect-timeout 2 "$url" > /dev/null 2>&1; then
        echo -e "${GREEN}✅ $service_name is running${NC}"
        return 0
    else
        echo -e "${RED}❌ $service_name is not running${NC}"
        return 1
    fi
}

# Function to generate web app clients
generate_web_clients() {
    print_section "Generating Web App Clients"

    cd "$WEB_APP_DIR"

    # Check if package.json has the generate scripts
    if grep -q "generate:backend" package.json && grep -q "generate:ai-service" package.json; then
        echo -e "${YELLOW}🔄 Running backend client generation...${NC}"
        npm run generate:backend || echo -e "${RED}Backend client generation failed${NC}"

        echo -e "${YELLOW}🔄 Running AI service client generation...${NC}"
        npm run generate:ai-service || echo -e "${RED}AI service client generation failed${NC}"

        echo -e "${GREEN}✅ Web app client generation completed${NC}"
    else
        echo -e "${YELLOW}⚠️  Web app generation scripts not found${NC}"
    fi
}

# Function to generate mobile app clients
generate_mobile_clients() {
    print_section "Generating Mobile App Clients"

    cd "$MOBILE_APP_DIR"

    # Check if generation script exists
    if [ -f "scripts/generate_api_clients.sh" ]; then
        echo -e "${YELLOW}🔄 Running mobile app client generation...${NC}"
        ./scripts/generate_api_clients.sh
        echo -e "${GREEN}✅ Mobile app client generation completed${NC}"
    else
        echo -e "${YELLOW}⚠️  Mobile app generation script not found${NC}"
    fi
}

# Function to run health checks
run_health_checks() {
    print_section "Service Health Checks"

    local services_running=0

    if check_service "$BACKEND_URL/swagger" "Backend API"; then
        ((services_running++))
    fi

    if check_service "$AI_SERVICE_URL/api/docs" "AI Service API"; then
        ((services_running++))
    fi

    if check_service "$WEB_APP_URL" "Web Application"; then
        ((services_running++))
    fi

    echo ""
    echo -e "${CYAN}📊 Services Status: $services_running/3 running${NC}"
    return $services_running
}

# Function to show help
show_help() {
    echo ""
    echo -e "${BLUE}AIPA Development Workflow Script${NC}"
    echo ""
    echo "Usage: $0 [command]"
    echo ""
    echo "Commands:"
    echo "  generate     Generate all API clients"
    echo "  web          Generate only web app clients"
    echo "  mobile       Generate only mobile app clients"
    echo "  health       Check service health"
    echo "  full         Run complete workflow (health check + generation)"
    echo "  help         Show this help message"
    echo ""
    echo "Examples:"
    echo "  $0 full      # Run complete workflow"
    echo "  $0 generate  # Generate all clients"
    echo "  $0 health    # Check all services"
    echo ""
}

# Main execution based on command
case "${1:-full}" in
    "generate")
        print_section "Starting Client Generation"
        generate_web_clients
        generate_mobile_clients
        echo -e "${GREEN}🎉 All client generation completed!${NC}"
        ;;

    "web")
        generate_web_clients
        ;;

    "mobile")
        generate_mobile_clients
        ;;

    "health")
        run_health_checks
        ;;

    "full")
        print_section "Starting Complete Development Workflow"

        # Run health checks first
        run_health_checks
        services_count=$?

        if [ $services_count -gt 0 ]; then
            echo -e "${YELLOW}🔄 Proceeding with client generation for available services...${NC}"
            generate_web_clients
            generate_mobile_clients
            echo -e "${GREEN}🎉 Complete development workflow finished!${NC}"
        else
            echo -e "${RED}❌ No services are running. Please start your backend services first.${NC}"
            echo ""
            echo "To start services:"
            echo "  1. Backend: cd aipa-backend && dotnet run"
            echo "  2. AI Service: cd aipa-ai-service && npm run start:dev"
            echo "  3. Web App: cd aipa-web-app && npm run dev"
        fi
        ;;

    "help"|"-h"|"--help")
        show_help
        ;;

    *)
        echo -e "${RED}❌ Unknown command: $1${NC}"
        show_help
        exit 1
        ;;
esac

echo ""
echo -e "${CYAN}📝 Development Notes:${NC}"
echo "  • Ensure all backend services are running before generating clients"
echo "  • Generated clients are automatically integrated into the applications"
echo "  • Run 'flutter pub get' in mobile app after client generation"
echo "  • Run 'npm install' in web app if new dependencies are added"
echo ""
