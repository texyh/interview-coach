#!/bin/bash

# AIPA Mobile App - API Client Generation Script
# This script regenerates Dart API clients from OpenAPI specifications

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Script configuration
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
BACKEND_SPEC_URL="http://localhost:5030/swagger/v1/swagger.json"
AI_SERVICE_SPEC_URL="http://localhost:3001/api/docs-json"

# Output directories
BACKEND_OUTPUT_DIR="$PROJECT_ROOT/lib/core/api/backend"
AI_SERVICE_OUTPUT_DIR="$PROJECT_ROOT/lib/core/api/ai_service"

echo -e "${BLUE}🚀 AIPA Mobile App - API Client Generation${NC}"
echo "=================================================="

# Function to check if a service is available
check_service() {
    local url=$1
    local service_name=$2

    echo -e "${YELLOW}🔍 Checking $service_name availability...${NC}"

    if curl -s --connect-timeout 5 "$url" > /dev/null 2>&1; then
        echo -e "${GREEN}✅ $service_name is available${NC}"
        return 0
    else
        echo -e "${RED}❌ $service_name is not available at $url${NC}"
        return 1
    fi
}

# Function to generate API client
generate_client() {
    local spec_url=$1
    local output_dir=$2
    local package_name=$3
    local service_name=$4

    echo -e "${YELLOW}📦 Generating $service_name Dart client...${NC}"

    # Remove existing generated code
    if [ -d "$output_dir" ]; then
        rm -rf "$output_dir"
    fi
    # Generate the client
    openapi-generator generate \
        -i "$spec_url" \
        -g dart \
        -o "$output_dir" \
        --additional-properties=pubName="$package_name" \
        --additional-properties=pubDescription="Generated Dart client for $service_name" \
        --additional-properties=pubVersion="1.0.0" \
        --additional-properties=useEnumExtension=true \
        --additional-properties=enumUnknownDefaultCase=true \
        --additional-properties=serializationLibrary=native_serialization

    if [ $? -eq 0 ]; then
        echo -e "${GREEN}✅ $service_name client generated successfully${NC}"

        # Remove test directories if they exist (they cause issues in Flutter)
        if [ -d "$output_dir/test" ]; then
            rm -rf "$output_dir/test"
        fi
        return 0
    else
        echo -e "${RED}❌ Failed to generate $service_name client${NC}"
        return 1
    fi
}

# Main execution
main() {
    echo -e "${BLUE}Starting API client generation process...${NC}"
    # Ensure openapi-generator is available
    if ! command -v openapi-generator &> /dev/null; then
        echo -e "${RED}❌ openapi-generator is not installed${NC}"
        echo "Please install it using:"
        echo "npm install @openapitools/openapi-generator-cli -g"
        echo "Or:"
        echo "brew install openapi-generator"
        exit 1
    fi
    # Check if backend service is available
    backend_available=false
    if check_service "$BACKEND_SPEC_URL" "AIPA Backend"; then
        backend_available=true
    fi
    # Check if AI service is available
    ai_service_available=false
    if check_service "$AI_SERVICE_SPEC_URL" "AIPA AI Service"; then
        ai_service_available=true
    fi
    # Generate backend client if available
    if [ "$backend_available" = true ]; then
        if generate_client "$BACKEND_SPEC_URL" "$BACKEND_OUTPUT_DIR" "aipa_backend_api" "AIPA Backend"; then
            echo -e "${GREEN}✅ Backend client generation completed${NC}"
        else
            echo -e "${RED}❌ Backend client generation failed${NC}"
            exit 1
        fi
    else
        echo -e "${YELLOW}⚠️  Skipping backend client generation (service not available)${NC}"
    fi
    # Generate AI service client if available
    if [ "$ai_service_available" = true ]; then
        if generate_client "$AI_SERVICE_SPEC_URL" "$AI_SERVICE_OUTPUT_DIR" "aipa_ai_service_api" "AIPA AI Service"; then
            echo -e "${GREEN}✅ AI Service client generation completed${NC}"
        else
            echo -e "${RED}❌ AI Service client generation failed${NC}"
            exit 1
        fi
    else
        echo -e "${YELLOW}⚠️  Skipping AI service client generation (service not available)${NC}"
    fi

    # Run Flutter analysis to check for issues
    echo -e "${YELLOW}🔍 Running Flutter analysis...${NC}"
    cd "$PROJECT_ROOT"

    if flutter analyze --no-pub; then
        echo -e "${GREEN}✅ Flutter analysis passed${NC}"
    else
        echo -e "${YELLOW}⚠️  Flutter analysis found some issues (this may include pre-existing issues)${NC}"
    fi
    echo ""
    echo -e "${GREEN}🎉 API client generation process completed!${NC}"
    echo ""
    echo "Generated clients:"
    [ "$backend_available" = true ] && echo "  📦 Backend client: $BACKEND_OUTPUT_DIR"
    [ "$ai_service_available" = true ] && echo "  📦 AI Service client: $AI_SERVICE_OUTPUT_DIR"
    echo ""
    echo "Next steps:"
    echo "  1. Review the generated code"
    echo "  2. Update your API service integration if needed"
    echo "  3. Run 'flutter pub get' to update dependencies"
    echo "  4. Test your application with the new clients"
}

# Run main function
main "$@"
