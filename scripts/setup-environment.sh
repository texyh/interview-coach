#!/usr/bin/env bash

#######################################
# AIPA Multi-Platform Environment Configuration Script
# Configures environment variables and secrets across all services
# Usage: ./setup-environment.sh [environment] [--validate-only]
#######################################

set -euo pipefail

# Configuration
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "${SCRIPT_DIR}/.." && pwd)"
ENVIRONMENT="${1:-development}"
VALIDATE_ONLY="${2:-}"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Service paths
BACKEND_PATH="${PROJECT_ROOT}/aipa-backend"
AI_SERVICE_PATH="${PROJECT_ROOT}/aipa-ai-service"
WEB_APP_PATH="${PROJECT_ROOT}/aipa-web-app"
MOBILE_APP_PATH="${PROJECT_ROOT}/aipa-mobile-app"

# Logging functions
log_info() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

log_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

log_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Validation functions
validate_environment() {
    case "${ENVIRONMENT}" in
        development|staging|production)
            log_info "Configuring for ${ENVIRONMENT} environment"
            ;;
        *)
            log_error "Invalid environment: ${ENVIRONMENT}"
            log_error "Valid environments: development, staging, production"
            exit 1
            ;;
    esac
}

validate_azure_cli() {
    if ! command -v az &> /dev/null; then
        log_warning "Azure CLI not found. Install it for Key Vault integration."
        return 1
    fi

    if ! az account show &> /dev/null; then
        log_warning "Azure CLI not logged in. Run 'az login' for Key Vault access."
        return 1
    fi

    log_success "Azure CLI is configured"
    return 0
}

validate_node_version() {
    if ! command -v node &> /dev/null; then
        log_error "Node.js not found. Please install Node.js 18+ and try again."
        exit 1
    fi

    local node_version
    node_version=$(node --version | sed 's/v//')
    local major_version
    major_version=$(echo "${node_version}" | cut -d. -f1)

    if [ "${major_version}" -lt 18 ]; then
        log_error "Node.js version ${node_version} is too old. Please install Node.js 18+ and try again."
        exit 1
    fi

    log_success "Node.js version ${node_version} is compatible"
}

validate_dotnet_version() {
    if ! command -v dotnet &> /dev/null; then
        log_error ".NET SDK not found. Please install .NET 8.0+ and try again."
        exit 1
    fi

    local dotnet_version
    dotnet_version=$(dotnet --version)
    local major_version
    major_version=$(echo "${dotnet_version}" | cut -d. -f1)

    if [ "${major_version}" -lt 8 ]; then
        log_error ".NET version ${dotnet_version} is too old. Please install .NET 8.0+ and try again."
        exit 1
    fi

    log_success ".NET version ${dotnet_version} is compatible"
}

# Environment file creation
create_backend_env() {
    local env_file="${BACKEND_PATH}/.env.${ENVIRONMENT}"
    log_info "Creating backend environment file: ${env_file}"

    cat > "${env_file}" << EOF
# AIPA Backend Environment Configuration - ${ENVIRONMENT}
# Generated on $(date)

# Environment
ASPNETCORE_ENVIRONMENT=${ENVIRONMENT}
DOTNET_ENVIRONMENT=${ENVIRONMENT}

# Application
APP_NAME=AIPA Backend API
APP_VERSION=1.0.0

# Database Configuration
$(generate_database_config)

# OpenAI Configuration
$(generate_openai_config)

# Azure Services
$(generate_azure_config)

# JWT Configuration
$(generate_jwt_config)

# Security
$(generate_security_config)

# Monitoring
$(generate_monitoring_config)
EOF

    log_success "Backend environment file created: ${env_file}"
}

create_ai_service_env() {
    local env_file="${AI_SERVICE_PATH}/.env.${ENVIRONMENT}"
    log_info "Creating AI service environment file: ${env_file}"

    cat > "${env_file}" << EOF
# AIPA AI Service Environment Configuration - ${ENVIRONMENT}
# Generated on $(date)

# Environment
NODE_ENV=${ENVIRONMENT}

# Application
APP_NAME=AIPA AI Service
APP_VERSION=1.0.0
PORT=3001

# Database Configuration
$(generate_database_config_node)

# Redis Configuration
$(generate_redis_config)

# OpenAI Configuration
$(generate_openai_config_node)

# Azure Services
$(generate_azure_config_node)

# Authentication
$(generate_jwt_config_node)

# File Upload
$(generate_file_upload_config)

# External APIs
$(generate_external_api_config)
EOF

    log_success "AI service environment file created: ${env_file}"
}

create_web_app_env() {
    local env_file="${WEB_APP_PATH}/.env.${ENVIRONMENT}"
    log_info "Creating web app environment file: ${env_file}"

    cat > "${env_file}" << EOF
# AIPA Web App Environment Configuration - ${ENVIRONMENT}
# Generated on $(date)

# Environment
NODE_ENV=${ENVIRONMENT}

# Application
NEXT_PUBLIC_APP_NAME=AIPA Interview Preparation
NEXT_PUBLIC_APP_VERSION=1.0.0
NEXT_PUBLIC_ENVIRONMENT=${ENVIRONMENT}

# API Configuration
$(generate_api_urls)

# Feature Flags
$(generate_feature_flags)

# External Services
$(generate_external_services_config)

# Analytics
$(generate_analytics_config)
EOF

    log_success "Web app environment file created: ${env_file}"
}

create_mobile_app_env() {
    local env_file="${MOBILE_APP_PATH}/.env.${ENVIRONMENT}"
    log_info "Creating mobile app environment file: ${env_file}"

    cat > "${env_file}" << EOF
# AIPA Mobile App Environment Configuration - ${ENVIRONMENT}
# Generated on $(date)

# Environment
FLUTTER_ENV=${ENVIRONMENT}

# Application
APP_NAME=AIPA Mobile
APP_VERSION=1.0.0

# API Configuration
$(generate_mobile_api_config)

# Feature Flags
$(generate_mobile_feature_flags)

# Platform Configuration
$(generate_platform_config)
EOF

    log_success "Mobile app environment file created: ${env_file}"
}

# Configuration generators
generate_database_config() {
    case "${ENVIRONMENT}" in
        development)
            cat << EOF
ConnectionStrings__DefaultConnection="Server=localhost;Database=aipa_dev;User Id=aipa_dev;Password=dev_password_123;TrustServerCertificate=true;"
ConnectionStrings__ReadOnlyConnection="Server=localhost;Database=aipa_dev;User Id=aipa_readonly;Password=readonly_password_123;TrustServerCertificate=true;"
EOF
            ;;
        staging)
            cat << EOF
ConnectionStrings__DefaultConnection="Server=staging-db.postgres.database.azure.com;Database=aipa_staging;User Id=aipa_staging@staging-db;Password=\${STAGING_DB_PASSWORD};Ssl Mode=Require;"
ConnectionStrings__ReadOnlyConnection="Server=staging-db.postgres.database.azure.com;Database=aipa_staging;User Id=aipa_readonly@staging-db;Password=\${STAGING_DB_READONLY_PASSWORD};Ssl Mode=Require;"
EOF
            ;;
        production)
            cat << EOF
ConnectionStrings__DefaultConnection="\${aipa-db-connection-string}"
ConnectionStrings__ReadOnlyConnection="\${aipa-db-readonly-connection-string}"
EOF
            ;;
    esac
}

generate_database_config_node() {
    case "${ENVIRONMENT}" in
        development)
            cat << EOF
AIPA_AI_DB_HOST=localhost
AIPA_AI_DB_PORT=5432
AIPA_AI_DB_USERNAME=aipa_dev
AIPA_AI_DB_PASSWORD=dev_password_123
AIPA_AI_DB_NAME=aipa_ai_dev
EOF
            ;;
        staging)
            cat << EOF
AIPA_AI_DB_HOST=staging-db.postgres.database.azure.com
AIPA_AI_DB_PORT=5432
AIPA_AI_DB_USERNAME=aipa_staging
AIPA_AI_DB_PASSWORD=\${STAGING_AI_DB_PASSWORD}
AIPA_AI_DB_NAME=aipa_ai_staging
EOF
            ;;
        production)
            cat << EOF
AIPA_AI_DB_HOST=\${aipa-ai-db-host}
AIPA_AI_DB_PORT=\${aipa-ai-db-port}
AIPA_AI_DB_USERNAME=\${aipa-ai-db-username}
AIPA_AI_DB_PASSWORD=\${aipa-ai-db-password}
AIPA_AI_DB_NAME=\${aipa-ai-db-name}
EOF
            ;;
    esac
}

generate_redis_config() {
    case "${ENVIRONMENT}" in
        development)
            cat << EOF
AIPA_REDIS_HOST=localhost
AIPA_REDIS_PORT=6379
AIPA_REDIS_PASSWORD=
EOF
            ;;
        staging)
            cat << EOF
AIPA_REDIS_HOST=staging-redis.redis.cache.windows.net
AIPA_REDIS_PORT=6380
AIPA_REDIS_PASSWORD=\${STAGING_REDIS_PASSWORD}
EOF
            ;;
        production)
            cat << EOF
AIPA_REDIS_HOST=\${aipa-redis-host}
AIPA_REDIS_PORT=\${aipa-redis-port}
AIPA_REDIS_PASSWORD=\${aipa-redis-password}
EOF
            ;;
    esac
}

generate_openai_config() {
    case "${ENVIRONMENT}" in
        development)
            cat << EOF
OpenAI__ApiKey=sk-dev-key-placeholder-replace-with-actual-key
OpenAI__Model=gpt-4o
OpenAI__MaxTokens=4000
OpenAI__Temperature=0.7
OpenAI__BaseUrl=https://api.openai.com/v1
EOF
            ;;
        staging)
            cat << EOF
OpenAI__ApiKey=\${STAGING_OPENAI_API_KEY}
OpenAI__Model=gpt-4o
OpenAI__MaxTokens=4000
OpenAI__Temperature=0.7
OpenAI__BaseUrl=https://api.openai.com/v1
EOF
            ;;
        production)
            cat << EOF
OpenAI__ApiKey=\${openai-api-key}
OpenAI__Model=\${openai-model}
OpenAI__MaxTokens=\${openai-max-tokens}
OpenAI__Temperature=\${openai-temperature}
OpenAI__BaseUrl=\${openai-base-url}
EOF
            ;;
    esac
}

generate_openai_config_node() {
    case "${ENVIRONMENT}" in
        development)
            cat << EOF
OPENAI_API_KEY=sk-dev-key-placeholder-replace-with-actual-key
OPENAI_MODEL=gpt-4o
OPENAI_MAX_TOKENS=4000
OPENAI_TEMPERATURE=0.7
OPENAI_BASE_URL=https://api.openai.com/v1
EOF
            ;;
        staging)
            cat << EOF
OPENAI_API_KEY=\${STAGING_OPENAI_API_KEY}
OPENAI_MODEL=gpt-4o
OPENAI_MAX_TOKENS=4000
OPENAI_TEMPERATURE=0.7
OPENAI_BASE_URL=https://api.openai.com/v1
EOF
            ;;
        production)
            cat << EOF
OPENAI_API_KEY=\${openai-api-key}
OPENAI_MODEL=\${openai-model}
OPENAI_MAX_TOKENS=\${openai-max-tokens}
OPENAI_TEMPERATURE=\${openai-temperature}
OPENAI_BASE_URL=\${openai-base-url}
EOF
            ;;
    esac
}

generate_azure_config() {
    case "${ENVIRONMENT}" in
        development)
            cat << EOF
Azure__KeyVault__Enabled=false
Azure__ApplicationInsights__Enabled=false
Azure__Speech__SubscriptionKey=dev-speech-key-placeholder
Azure__Speech__Region=eastus
Azure__Speech__Language=en-US
EOF
            ;;
        staging)
            cat << EOF
Azure__KeyVault__Enabled=false
Azure__KeyVault__VaultUri=https://aipa-staging-kv.vault.azure.net/
Azure__ApplicationInsights__Enabled=true
Azure__ApplicationInsights__ConnectionString=\${STAGING_APPINSIGHTS_CONNECTION_STRING}
Azure__Speech__SubscriptionKey=\${STAGING_AZURE_SPEECH_KEY}
Azure__Speech__Region=eastus
Azure__Speech__Language=en-US
EOF
            ;;
        production)
            cat << EOF
Azure__KeyVault__Enabled=true
Azure__KeyVault__VaultUri=\${azure-keyvault-uri}
Azure__KeyVault__ClientId=\${azure-keyvault-client-id}
Azure__KeyVault__TenantId=\${azure-keyvault-tenant-id}
Azure__ApplicationInsights__Enabled=true
Azure__ApplicationInsights__ConnectionString=\${azure-appinsights-connection-string}
Azure__Speech__SubscriptionKey=\${azure-speech-key}
Azure__Speech__Region=\${azure-speech-region}
Azure__Speech__Language=\${azure-speech-language}
EOF
            ;;
    esac
}

generate_azure_config_node() {
    case "${ENVIRONMENT}" in
        development)
            cat << EOF
AZURE_KEYVAULT_ENABLED=false
AZURE_APPINSIGHTS_ENABLED=false
AZURE_SPEECH_KEY=dev-speech-key-placeholder
AZURE_SPEECH_REGION=eastus
AZURE_SPEECH_LANGUAGE=en-US
EOF
            ;;
        staging)
            cat << EOF
AZURE_KEYVAULT_ENABLED=false
AZURE_KEYVAULT_URI=https://aipa-staging-kv.vault.azure.net/
AZURE_APPINSIGHTS_ENABLED=true
AZURE_APPINSIGHTS_CONNECTION_STRING=\${STAGING_APPINSIGHTS_CONNECTION_STRING}
AZURE_SPEECH_KEY=\${STAGING_AZURE_SPEECH_KEY}
AZURE_SPEECH_REGION=eastus
AZURE_SPEECH_LANGUAGE=en-US
EOF
            ;;
        production)
            cat << EOF
AZURE_KEYVAULT_ENABLED=true
AZURE_KEYVAULT_URI=\${azure-keyvault-uri}
AZURE_KEYVAULT_CLIENT_ID=\${azure-keyvault-client-id}
AZURE_KEYVAULT_TENANT_ID=\${azure-keyvault-tenant-id}
AZURE_APPINSIGHTS_ENABLED=true
AZURE_APPINSIGHTS_CONNECTION_STRING=\${azure-appinsights-connection-string}
AZURE_SPEECH_KEY=\${azure-speech-key}
AZURE_SPEECH_REGION=\${azure-speech-region}
AZURE_SPEECH_LANGUAGE=\${azure-speech-language}
EOF
            ;;
    esac
}

generate_jwt_config() {
    case "${ENVIRONMENT}" in
        development)
            cat << EOF
JWT__SecretKey=dev-jwt-secret-key-minimum-32-chars-long-for-security-purposes
JWT__Issuer=http://localhost:5000
JWT__Audience=aipa-clients
JWT__ExpiryMinutes=60
JWT__RefreshTokenExpiryDays=7
EOF
            ;;
        staging)
            cat << EOF
JWT__SecretKey=\${STAGING_JWT_SECRET_KEY}
JWT__Issuer=https://api-staging.aipa.com
JWT__Audience=aipa-clients
JWT__ExpiryMinutes=120
JWT__RefreshTokenExpiryDays=14
EOF
            ;;
        production)
            cat << EOF
JWT__SecretKey=\${jwt-secret-key}
JWT__Issuer=\${jwt-issuer}
JWT__Audience=\${jwt-audience}
JWT__ExpiryMinutes=\${jwt-expiry-minutes}
JWT__RefreshTokenExpiryDays=\${jwt-refresh-expiry-days}
EOF
            ;;
    esac
}

generate_jwt_config_node() {
    case "${ENVIRONMENT}" in
        development)
            cat << EOF
JWT_SECRET_KEY=dev-jwt-secret-key-minimum-32-chars-long-for-security-purposes
JWT_ISSUER=http://localhost:3001
JWT_AUDIENCE=aipa-clients
JWT_EXPIRES_IN=1h
JWT_REFRESH_EXPIRES_IN=7d
EOF
            ;;
        staging)
            cat << EOF
JWT_SECRET_KEY=\${STAGING_JWT_SECRET_KEY}
JWT_ISSUER=https://ai-staging.aipa.com
JWT_AUDIENCE=aipa-clients
JWT_EXPIRES_IN=2h
JWT_REFRESH_EXPIRES_IN=14d
EOF
            ;;
        production)
            cat << EOF
JWT_SECRET_KEY=\${jwt-secret-key}
JWT_ISSUER=\${jwt-issuer}
JWT_AUDIENCE=\${jwt-audience}
JWT_EXPIRES_IN=\${jwt-expires-in}
JWT_REFRESH_EXPIRES_IN=\${jwt-refresh-expires-in}
EOF
            ;;
    esac
}

generate_security_config() {
    case "${ENVIRONMENT}" in
        development)
            cat << EOF
Security__EnableSecurityHeaders=true
Security__RequireHttps=false
CORS__AllowedOrigins__0=http://localhost:3000
CORS__AllowedOrigins__1=http://localhost:3001
CORS__AllowCredentials=true
EOF
            ;;
        staging)
            cat << EOF
Security__EnableSecurityHeaders=true
Security__RequireHttps=true
CORS__AllowedOrigins__0=https://staging.aipa.com
CORS__AllowedOrigins__1=https://ai-staging.aipa.com
CORS__AllowCredentials=true
EOF
            ;;
        production)
            cat << EOF
Security__EnableSecurityHeaders=true
Security__RequireHttps=true
CORS__AllowedOrigins__0=\${cors-origin-web-app}
CORS__AllowedOrigins__1=\${cors-origin-mobile}
CORS__AllowCredentials=true
EOF
            ;;
    esac
}

generate_monitoring_config() {
    case "${ENVIRONMENT}" in
        development)
            cat << EOF
Monitoring__EnableMetrics=true
Monitoring__EnableTracing=true
Monitoring__SamplingRate=1.0
HealthChecks__Enabled=true
HealthChecks__DatabaseTimeout=5
HealthChecks__ExternalServiceTimeout=10
EOF
            ;;
        staging)
            cat << EOF
Monitoring__EnableMetrics=true
Monitoring__EnableTracing=true
Monitoring__SamplingRate=0.5
HealthChecks__Enabled=true
HealthChecks__DatabaseTimeout=8
HealthChecks__ExternalServiceTimeout=15
EOF
            ;;
        production)
            cat << EOF
Monitoring__EnableMetrics=true
Monitoring__EnableTracing=true
Monitoring__SamplingRate=\${monitoring-sampling-rate}
HealthChecks__Enabled=true
HealthChecks__DatabaseTimeout=\${healthcheck-db-timeout}
HealthChecks__ExternalServiceTimeout=\${healthcheck-external-timeout}
EOF
            ;;
    esac
}

generate_file_upload_config() {
    case "${ENVIRONMENT}" in
        development)
            cat << EOF
FILE_UPLOAD_MAX_SIZE=10485760
FILE_UPLOAD_PATH=./uploads/dev
FILE_TEMP_PATH=./temp/dev
EOF
            ;;
        staging)
            cat << EOF
FILE_UPLOAD_MAX_SIZE=20971520
FILE_UPLOAD_PATH=./uploads/staging
FILE_TEMP_PATH=./temp/staging
EOF
            ;;
        production)
            cat << EOF
FILE_UPLOAD_MAX_SIZE=\${file-upload-max-size}
FILE_UPLOAD_PATH=\${file-upload-path}
FILE_TEMP_PATH=\${file-temp-path}
EOF
            ;;
    esac
}

generate_external_api_config() {
    case "${ENVIRONMENT}" in
        development)
            cat << EOF
BACKEND_API_URL=http://localhost:5000/api
WEB_APP_URL=http://localhost:3000
EOF
            ;;
        staging)
            cat << EOF
BACKEND_API_URL=https://api-staging.aipa.com
WEB_APP_URL=https://staging.aipa.com
EOF
            ;;
        production)
            cat << EOF
BACKEND_API_URL=\${backend-api-url}
WEB_APP_URL=\${web-app-url}
EOF
            ;;
    esac
}

generate_api_urls() {
    case "${ENVIRONMENT}" in
        development)
            cat << EOF
NEXT_PUBLIC_API_BASE_URL=http://localhost:5000/api
NEXT_PUBLIC_AI_SERVICE_URL=http://localhost:3001
EOF
            ;;
        staging)
            cat << EOF
NEXT_PUBLIC_API_BASE_URL=https://api-staging.aipa.com
NEXT_PUBLIC_AI_SERVICE_URL=https://ai-staging.aipa.com
EOF
            ;;
        production)
            cat << EOF
NEXT_PUBLIC_API_BASE_URL=\${api-base-url}
NEXT_PUBLIC_AI_SERVICE_URL=\${ai-service-url}
EOF
            ;;
    esac
}

generate_feature_flags() {
    case "${ENVIRONMENT}" in
        development)
            cat << EOF
NEXT_PUBLIC_ENABLE_VOICE_ANALYSIS=true
NEXT_PUBLIC_ENABLE_REAL_TIME=true
NEXT_PUBLIC_ENABLE_ANALYTICS=false
NEXT_PUBLIC_ENABLE_DEBUG_PANEL=true
NEXT_PUBLIC_SHOW_BETA_FEATURES=true
EOF
            ;;
        staging)
            cat << EOF
NEXT_PUBLIC_ENABLE_VOICE_ANALYSIS=true
NEXT_PUBLIC_ENABLE_REAL_TIME=true
NEXT_PUBLIC_ENABLE_ANALYTICS=true
NEXT_PUBLIC_ENABLE_DEBUG_PANEL=true
NEXT_PUBLIC_SHOW_BETA_FEATURES=true
EOF
            ;;
        production)
            cat << EOF
NEXT_PUBLIC_ENABLE_VOICE_ANALYSIS=\${feature-voice-analysis}
NEXT_PUBLIC_ENABLE_REAL_TIME=\${feature-real-time}
NEXT_PUBLIC_ENABLE_ANALYTICS=true
NEXT_PUBLIC_ENABLE_DEBUG_PANEL=false
NEXT_PUBLIC_SHOW_BETA_FEATURES=false
EOF
            ;;
    esac
}

generate_external_services_config() {
    case "${ENVIRONMENT}" in
        development)
            cat << EOF
NEXT_PUBLIC_GOOGLE_ANALYTICS_ID=
NEXT_PUBLIC_SENTRY_DSN=
NEXT_PUBLIC_AZURE_APPLICATION_INSIGHTS_CONNECTION_STRING=
EOF
            ;;
        staging)
            cat << EOF
NEXT_PUBLIC_GOOGLE_ANALYTICS_ID=\${STAGING_GOOGLE_ANALYTICS_ID}
NEXT_PUBLIC_SENTRY_DSN=\${STAGING_SENTRY_DSN}
NEXT_PUBLIC_AZURE_APPLICATION_INSIGHTS_CONNECTION_STRING=\${STAGING_APPINSIGHTS_CONNECTION_STRING}
EOF
            ;;
        production)
            cat << EOF
NEXT_PUBLIC_GOOGLE_ANALYTICS_ID=\${google-analytics-id}
NEXT_PUBLIC_SENTRY_DSN=\${sentry-dsn}
NEXT_PUBLIC_AZURE_APPLICATION_INSIGHTS_CONNECTION_STRING=\${azure-appinsights-connection-string}
EOF
            ;;
    esac
}

generate_analytics_config() {
    case "${ENVIRONMENT}" in
        development)
            cat << EOF
# Analytics disabled in development
NEXT_PUBLIC_ANALYTICS_ENABLED=false
EOF
            ;;
        staging)
            cat << EOF
NEXT_PUBLIC_ANALYTICS_ENABLED=true
NEXT_PUBLIC_ANALYTICS_DEBUG=true
EOF
            ;;
        production)
            cat << EOF
NEXT_PUBLIC_ANALYTICS_ENABLED=true
NEXT_PUBLIC_ANALYTICS_DEBUG=false
EOF
            ;;
    esac
}

generate_mobile_api_config() {
    case "${ENVIRONMENT}" in
        development)
            cat << EOF
API_BASE_URL=http://localhost:5000/api
AI_SERVICE_URL=http://localhost:3001
EOF
            ;;
        staging)
            cat << EOF
API_BASE_URL=https://api-staging.aipa.com
AI_SERVICE_URL=https://ai-staging.aipa.com
EOF
            ;;
        production)
            cat << EOF
API_BASE_URL=\${mobile-api-base-url}
AI_SERVICE_URL=\${mobile-ai-service-url}
EOF
            ;;
    esac
}

generate_mobile_feature_flags() {
    case "${ENVIRONMENT}" in
        development)
            cat << EOF
ENABLE_VOICE_ANALYSIS=true
ENABLE_OFFLINE_MODE=true
ENABLE_PUSH_NOTIFICATIONS=false
ENABLE_BIOMETRIC_AUTH=false
EOF
            ;;
        staging)
            cat << EOF
ENABLE_VOICE_ANALYSIS=true
ENABLE_OFFLINE_MODE=true
ENABLE_PUSH_NOTIFICATIONS=true
ENABLE_BIOMETRIC_AUTH=true
EOF
            ;;
        production)
            cat << EOF
ENABLE_VOICE_ANALYSIS=\${mobile-feature-voice-analysis}
ENABLE_OFFLINE_MODE=\${mobile-feature-offline-mode}
ENABLE_PUSH_NOTIFICATIONS=\${mobile-feature-push-notifications}
ENABLE_BIOMETRIC_AUTH=\${mobile-feature-biometric-auth}
EOF
            ;;
    esac
}

generate_platform_config() {
    cat << EOF
# Android Configuration
ANDROID_MIN_SDK_VERSION=21
ANDROID_TARGET_SDK_VERSION=34
ANDROID_COMPILE_SDK_VERSION=34

# iOS Configuration
IOS_MIN_VERSION=12.0
IOS_TARGET_VERSION=17.0
EOF
}

# Validation functions
validate_configuration() {
    local service_name="$1"
    local env_file="$2"

    log_info "Validating ${service_name} configuration..."

    if [[ ! -f "${env_file}" ]]; then
        log_error "Environment file not found: ${env_file}"
        return 1
    fi

    # Check for required variables based on service
    local required_vars=()
    case "${service_name}" in
        "Backend")
            required_vars=("ConnectionStrings__DefaultConnection" "OpenAI__ApiKey" "JWT__SecretKey")
            ;;
        "AI Service")
            required_vars=("OPENAI_API_KEY" "JWT_SECRET_KEY" "AIPA_AI_DB_HOST")
            ;;
        "Web App")
            required_vars=("NEXT_PUBLIC_API_BASE_URL" "NEXT_PUBLIC_AI_SERVICE_URL")
            ;;
        "Mobile App")
            required_vars=("API_BASE_URL" "AI_SERVICE_URL")
            ;;
    esac

    local missing_vars=()
    for var in "${required_vars[@]}"; do
        if ! grep -q "^${var}=" "${env_file}" 2>/dev/null; then
            missing_vars+=("${var}")
        fi
    done

    if [[ ${#missing_vars[@]} -gt 0 ]]; then
        log_error "Missing required variables in ${service_name}:"
        for var in "${missing_vars[@]}"; do
            log_error "  - ${var}"
        done
        return 1
    fi

    log_success "${service_name} configuration is valid"
    return 0
}

# Azure Key Vault functions
setup_azure_keyvault() {
    if [[ "${ENVIRONMENT}" != "production" ]]; then
        log_info "Azure Key Vault setup is only needed for production environment"
        return 0
    fi

    if ! validate_azure_cli; then
        log_warning "Skipping Azure Key Vault setup due to Azure CLI issues"
        return 1
    fi

    local vault_name="${AZURE_KEYVAULT_NAME:-aipa-prod-kv}"
    local resource_group="${AZURE_RESOURCE_GROUP:-aipa-production}"

    log_info "Setting up Azure Key Vault secrets for production..."

    # Create secrets (with placeholder values - replace with actual values)
    local secrets=(
        "aipa-db-connection-string"
        "aipa-db-readonly-connection-string"
        "openai-api-key"
        "jwt-secret-key"
        "azure-speech-key"
        "azure-appinsights-connection-string"
    )

    for secret in "${secrets[@]}"; do
        if ! az keyvault secret show --vault-name "${vault_name}" --name "${secret}" &>/dev/null; then
            log_warning "Secret '${secret}' not found in Key Vault. Please create it manually."
        else
            log_success "Secret '${secret}' exists in Key Vault"
        fi
    done
}

# Installation functions
install_dependencies() {
    log_info "Installing dependencies for all services..."

    # Backend dependencies
    if [[ -f "${BACKEND_PATH}/src/AIPA.WebApi/AIPA.WebApi.csproj" ]]; then
        log_info "Installing .NET dependencies..."
        (cd "${BACKEND_PATH}" && dotnet restore)
        log_success ".NET dependencies installed"
    fi

    # AI Service dependencies
    if [[ -f "${AI_SERVICE_PATH}/package.json" ]]; then
        log_info "Installing AI Service dependencies..."
        (cd "${AI_SERVICE_PATH}" && npm ci)
        log_success "AI Service dependencies installed"
    fi

    # Web App dependencies
    if [[ -f "${WEB_APP_PATH}/package.json" ]]; then
        log_info "Installing Web App dependencies..."
        (cd "${WEB_APP_PATH}" && npm ci)
        log_success "Web App dependencies installed"
    fi

    # Mobile App dependencies
    if [[ -f "${MOBILE_APP_PATH}/pubspec.yaml" ]]; then
        log_info "Installing Flutter dependencies..."
        if command -v flutter &> /dev/null; then
            (cd "${MOBILE_APP_PATH}" && flutter pub get)
            log_success "Flutter dependencies installed"
        else
            log_warning "Flutter not found. Skipping mobile app dependencies."
        fi
    fi
}

# Main execution
main() {
    log_info "AIPA Environment Configuration Script"
    log_info "======================================"

    # Validate environment parameter
    validate_environment

    # If validation only mode
    if [[ "${VALIDATE_ONLY}" == "--validate-only" ]]; then
        log_info "Running in validation-only mode"

        validate_node_version
        validate_dotnet_version
        validate_azure_cli

        # Validate existing configuration files
        local validation_failed=false

        if [[ -f "${BACKEND_PATH}/.env.${ENVIRONMENT}" ]]; then
            validate_configuration "Backend" "${BACKEND_PATH}/.env.${ENVIRONMENT}" || validation_failed=true
        else
            log_warning "Backend environment file not found: ${BACKEND_PATH}/.env.${ENVIRONMENT}"
        fi

        if [[ -f "${AI_SERVICE_PATH}/.env.${ENVIRONMENT}" ]]; then
            validate_configuration "AI Service" "${AI_SERVICE_PATH}/.env.${ENVIRONMENT}" || validation_failed=true
        else
            log_warning "AI Service environment file not found: ${AI_SERVICE_PATH}/.env.${ENVIRONMENT}"
        fi

        if [[ -f "${WEB_APP_PATH}/.env.${ENVIRONMENT}" ]]; then
            validate_configuration "Web App" "${WEB_APP_PATH}/.env.${ENVIRONMENT}" || validation_failed=true
        else
            log_warning "Web App environment file not found: ${WEB_APP_PATH}/.env.${ENVIRONMENT}"
        fi

        if [[ -f "${MOBILE_APP_PATH}/.env.${ENVIRONMENT}" ]]; then
            validate_configuration "Mobile App" "${MOBILE_APP_PATH}/.env.${ENVIRONMENT}" || validation_failed=true
        else
            log_warning "Mobile App environment file not found: ${MOBILE_APP_PATH}/.env.${ENVIRONMENT}"
        fi

        if [[ "${validation_failed}" == true ]]; then
            log_error "Configuration validation failed"
            exit 1
        else
            log_success "All configurations are valid"
            exit 0
        fi
    fi

    # Validate prerequisites
    validate_node_version
    validate_dotnet_version

    # Create environment files
    create_backend_env
    create_ai_service_env
    create_web_app_env
    create_mobile_app_env

    # Setup Azure Key Vault for production
    setup_azure_keyvault

    # Install dependencies
    install_dependencies

    # Validate configurations
    validate_configuration "Backend" "${BACKEND_PATH}/.env.${ENVIRONMENT}"
    validate_configuration "AI Service" "${AI_SERVICE_PATH}/.env.${ENVIRONMENT}"
    validate_configuration "Web App" "${WEB_APP_PATH}/.env.${ENVIRONMENT}"
    validate_configuration "Mobile App" "${MOBILE_APP_PATH}/.env.${ENVIRONMENT}"

    log_success "Environment configuration completed successfully!"
    log_info ""
    log_info "Next steps:"
    log_info "1. Review and update the generated .env.${ENVIRONMENT} files with actual values"
    log_info "2. For production: Configure Azure Key Vault secrets"
    log_info "3. Test the configuration by running: ./setup-environment.sh ${ENVIRONMENT} --validate-only"
    log_info "4. Start the services in ${ENVIRONMENT} mode"
}

# Execute main function
main "$@"
