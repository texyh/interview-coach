// Configuration manager for different environments
import developmentConfig from './development.config';
import productionConfig from './production.config';
import stagingConfig from './staging.config';

export interface AppConfig {
  app: {
    name: string;
    version: string;
    port: number;
    host: string;
    environment: string;
    debug: boolean;
    logLevel: string;
    corsEnabled: boolean;
  };
  database: {
    type: string;
    host: string;
    port: number;
    username: string;
    password: string;
    database: string;
    synchronize: boolean;
    logging: boolean;
    ssl?: any;
    retryAttempts: number;
    retryDelay: number;
    extra?: any;
  };
  cache: {
    enabled: boolean;
    host: string;
    port: number;
    password: string;
    db: number;
    ttl: number;
    keyPrefix: string;
    tls?: any;
    retryAttempts: number;
    retryDelay: number;
  };
  openai: {
    apiKey: string;
    model: string;
    maxTokens: number;
    temperature: number;
    baseUrl: string;
    timeout: number;
    retryAttempts: number;
    retryDelay: number;
  };
  azure: {
    speechService: {
      subscriptionKey: string;
      region: string;
      language: string;
      enabled: boolean;
    };
    keyVault: {
      enabled: boolean;
      vaultUri: string;
      clientId?: string;
      tenantId?: string;
    };
    applicationInsights: {
      connectionString: string;
      enabled: boolean;
      samplingRate?: number;
    };
  };
  auth: {
    jwt: {
      secret: string;
      expiresIn: string;
      refreshExpiresIn: string;
      issuer: string;
      audience: string;
    };
    bcrypt: {
      saltRounds: number;
    };
  };
  rateLimit: {
    enabled: boolean;
    windowMs: number;
    max: number;
    skipSuccessfulRequests: boolean;
    skipFailedRequests: boolean;
    standardHeaders?: boolean;
    legacyHeaders?: boolean;
  };
  fileUpload: {
    maxFileSize: number;
    allowedMimeTypes: string[];
    uploadPath: string;
    tempPath: string;
    cleanupInterval?: number;
  };
  security: {
    cors: {
      origin: string[];
      credentials: boolean;
      methods: string[];
      allowedHeaders: string[];
      optionsSuccessStatus?: number;
    };
    helmet: any;
    apiKey: {
      enabled: boolean;
      key: string;
      headerName?: string;
    };
    vulnerabilityScanning?: {
      enabled: boolean;
      schedule: string;
    };
    dependencyCheck?: {
      enabled: boolean;
      schedule: string;
    };
  };
  monitoring: {
    enabled: boolean;
    healthCheck: {
      enabled: boolean;
      path: string;
      timeout: number;
      checks?: any;
    };
    metrics: {
      enabled: boolean;
      path: string;
      collectDefaultMetrics?: boolean;
      collectHttpMetrics?: boolean;
    };
    logging: {
      level: string;
      format: string;
      enableConsole: boolean;
      enableFile: boolean;
      filePath?: string;
      maxFileSize?: string;
      maxFiles?: number;
      enableStructuredLogging?: boolean;
    };
  };
  interview: {
    maxQuestions: number;
    maxSessionDuration: number;
    supportedTypes: string[];
    aiAnalysis: {
      enabled: boolean;
      detailedFeedback: boolean;
      confidenceScoring: boolean;
      cacheResults?: boolean;
    };
    voiceAnalysis: {
      enabled: boolean;
      minRecordingDuration: number;
      maxRecordingDuration: number;
      supportedFormats?: string[];
    };
  };
  external: {
    backendApi: {
      baseUrl: string;
      timeout: number;
      retryAttempts: number;
      retryDelay?: number;
    };
    webApp: {
      baseUrl: string;
    };
  };
  [key: string]: any; // Allow additional properties for environment-specific configs
}

/**
 * Get configuration based on environment
 */
function getConfig(): AppConfig {
  const environment = process.env.NODE_ENV || 'development';

  switch (environment.toLowerCase()) {
    case 'production':
      return productionConfig as AppConfig;
    case 'staging':
      return stagingConfig as AppConfig;
    case 'development':
    case 'dev':
    default:
      return developmentConfig as AppConfig;
  }
}

/**
 * Validate configuration object
 */
function validateConfig(config: AppConfig): string[] {
  const errors: string[] = [];

  // Required fields validation
  const requiredFields = [
    'app.name',
    'app.port',
    'database.host',
    'database.database',
    'openai.apiKey',
    'auth.jwt.secret',
  ];

  requiredFields.forEach((field) => {
    const value = getNestedValue(config, field);
    if (!value || (typeof value === 'string' && value.trim() === '')) {
      errors.push(`Required field '${field}' is missing or empty`);
    }
  });

  // Validation rules
  if (config.app.port < 1 || config.app.port > 65535) {
    errors.push('App port must be between 1 and 65535');
  }

  if (config.openai.maxTokens < 1 || config.openai.maxTokens > 32000) {
    errors.push('OpenAI maxTokens must be between 1 and 32000');
  }

  if (config.openai.temperature < 0 || config.openai.temperature > 2) {
    errors.push('OpenAI temperature must be between 0 and 2');
  }

  if (config.auth.jwt.secret.length < 32) {
    errors.push('JWT secret must be at least 32 characters long');
  }

  if (config.auth.bcrypt.saltRounds < 8 || config.auth.bcrypt.saltRounds > 15) {
    errors.push('Bcrypt salt rounds must be between 8 and 15');
  }

  if (config.fileUpload.maxFileSize > 100 * 1024 * 1024) {
    errors.push('File upload size cannot exceed 100MB');
  }

  // Environment-specific validations
  if (config.app.environment === 'production') {
    if (config.app.debug === true) {
      errors.push('Debug mode should be disabled in production');
    }

    if (config.database.synchronize === true) {
      errors.push('Database synchronize should be disabled in production');
    }

    if (!config.database.ssl) {
      errors.push('SSL should be enabled for database in production');
    }

    if (config.auth.bcrypt.saltRounds < 12) {
      errors.push('Bcrypt salt rounds should be at least 12 in production');
    }
  }

  return errors;
}

/**
 * Get nested value from object using dot notation
 */
function getNestedValue(obj: any, path: string): any {
  return path.split('.').reduce((current, key) => current?.[key], obj);
}

/**
 * Configuration validation result
 */
export interface ConfigValidationResult {
  isValid: boolean;
  errors: string[];
  warnings: string[];
}

/**
 * Comprehensive configuration validation
 */
export function validateConfiguration(config?: AppConfig): ConfigValidationResult {
  const currentConfig = config || getConfig();
  const errors = validateConfig(currentConfig);
  const warnings: string[] = [];

  // Add warnings for common issues
  if (currentConfig.app.environment === 'development') {
    if (
      currentConfig.openai.apiKey.includes('placeholder') ||
      currentConfig.openai.apiKey.includes('dev-key')
    ) {
      warnings.push('Using placeholder OpenAI API key in development');
    }
  }

  if (
    currentConfig.cache.enabled &&
    !currentConfig.cache.password &&
    currentConfig.app.environment !== 'development'
  ) {
    warnings.push('Redis cache has no password set');
  }

  if (!currentConfig.monitoring.enabled) {
    warnings.push('Monitoring is disabled');
  }

  return {
    isValid: errors.length === 0,
    errors,
    warnings,
  };
}

/**
 * Print configuration validation results
 */
export function printValidationResults(result: ConfigValidationResult): void {
  console.log('\n=== Configuration Validation Results ===');

  if (result.isValid) {
    console.log('✅ Configuration is valid');
  } else {
    console.log('❌ Configuration validation failed');
    console.log('\nErrors:');
    result.errors.forEach((error, index) => {
      console.log(`  ${index + 1}. ${error}`);
    });
  }

  if (result.warnings.length > 0) {
    console.log('\nWarnings:');
    result.warnings.forEach((warning, index) => {
      console.log(`  ${index + 1}. ${warning}`);
    });
  }

  console.log('\n==========================================');
}

// Export the configuration
export const config = getConfig();
export default config;

// Validate configuration on module load
if (process.env.NODE_ENV !== 'test') {
  const validationResult = validateConfiguration(config);

  if (!validationResult.isValid) {
    console.error('Configuration validation failed:');
    validationResult.errors.forEach((error) => {
      console.error(`  - ${error}`);
    });

    if (process.env.NODE_ENV === 'production') {
      process.exit(1); // Exit in production if configuration is invalid
    }
  }

  if (validationResult.warnings.length > 0) {
    console.warn('Configuration warnings:');
    validationResult.warnings.forEach((warning) => {
      console.warn(`  - ${warning}`);
    });
  }
}
