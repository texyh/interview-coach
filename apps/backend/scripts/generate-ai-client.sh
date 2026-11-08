#!/bin/bash

# AI Service Client Generation Script
# This script generates the C# client from the AI service OpenAPI specification

set -e

echo "🔄 Generating AI Service Client..."

# Configuration
AI_SERVICE_URL="http://localhost:3001"
OUTPUT_DIR="src/AIPA.Infrastructure/ExternalServices/Generated"
OUTPUT_FILE="$OUTPUT_DIR/AIServiceClient.cs"
NAMESPACE="AIPA.Infrastructure.ExternalServices.Generated"

# Create output directory if it doesn't exist
mkdir -p "$OUTPUT_DIR"

# Check if AI service is running
echo "📡 Checking AI service availability at $AI_SERVICE_URL..."
if ! curl -s --fail "$AI_SERVICE_URL/health" > /dev/null 2>&1; then
    echo "⚠️  AI service is not running at $AI_SERVICE_URL"
    echo "Please start the AI service before running this script."
    echo "Expected endpoints:"
    echo "  - $AI_SERVICE_URL/health (health check)"
    echo "  - $AI_SERVICE_URL/api/docs-json (OpenAPI spec)"
    exit 1
fi

echo "✅ AI service is available"

# Check if OpenAPI spec endpoint is accessible
echo "📋 Fetching OpenAPI specification..."
if ! curl -s --fail "$AI_SERVICE_URL/api/docs-json" > /dev/null 2>&1; then
    echo "❌ Cannot access OpenAPI specification at $AI_SERVICE_URL/api/docs-json"
    echo "Please ensure the AI service exposes the OpenAPI spec at this endpoint."
    exit 1
fi

echo "✅ OpenAPI specification is accessible"

# Generate client using NSwag
echo "🛠️  Generating C# client..."

# Check if we're in the project root
if [ ! -f "AIPA.sln" ]; then
    echo "❌ Please run this script from the project root directory (where AIPA.sln is located)"
    exit 1
fi

# Use dotnet tool for NSwag or fall back to manual generation
if command -v nswag &> /dev/null; then
    nswag openapi2csclient \
        /input:"$AI_SERVICE_URL/api/docs-json" \
        /output:"$OUTPUT_FILE" \
        /namespace:"$NAMESPACE" \
        /generateClientInterfaces:true \
        /generateExceptionClasses:true \
        /exceptionClass:AIServiceException \
        /wrapDtoExceptions:true \
        /useHttpClientCreationMethod:true \
        /generateOptionalParameters:true \
        /generateJsonMethods:false \
        /classStyle:Poco \
        /operationGenerationMode:MultipleClientsFromOperationId \
        /generateResponseClasses:true \
        /generateImmutableArrayProperties:true \
        /generateImmutableDictionaryProperties:true \
        /jsonLibrary:SystemTextJson \
        /arrayType:"System.Collections.Generic.ICollection" \
        /dictionaryType:"System.Collections.Generic.IDictionary" \
        /arrayInstanceType:"System.Collections.Generic.List" \
        /dictionaryInstanceType:"System.Collections.Generic.Dictionary" \
        /arrayBaseType:"System.Collections.Generic.ICollection" \
        /dictionaryBaseType:"System.Collections.Generic.IDictionary" \
        /httpClientType:"System.Net.Http.HttpClient" \
        /useBaseUrl:false
else
    echo "⚠️  NSwag CLI not found. Using dotnet build to trigger client generation..."
    echo "The client will be generated during the next build process."

    # Create a placeholder file to indicate the client should be generated
    cat > "$OUTPUT_FILE" << 'EOF'
// This file will be auto-generated during build when the AI service is available
// Run: dotnet build (with AI service running at http://localhost:3001)

namespace AIPA.Infrastructure.ExternalServices.Generated
{
    // Placeholder - actual client will be generated from OpenAPI spec
    public partial class AIServiceClient
    {
        // This will be replaced by the generated client
    }
}
EOF
fi

echo "✅ AI Service client generation completed"
echo "📁 Generated file: $OUTPUT_FILE"
echo ""
echo "Next steps:"
echo "1. Build the project: dotnet build"
echo "2. Review the generated client code"
echo "3. Update your services to use the generated client if needed"
echo ""
echo "Note: The client will be regenerated automatically during debug builds when the AI service is running."
