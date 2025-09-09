// Staging environment configuration
export default {
  // Application
  app: {
    name: 'AIPA AI Service',
    version: '1.0.0',
    port: parseInt(process.env.PORT || '3001'),
    host: '0.0.0.0',
    environment: 'staging',
    debug: false,
    logLevel: 'info',
    corsEnabled: true,
  },

  // Database
  database: {
    type: 'postgresql',
    host: process.env.AIPA_AI_DB_HOST || 'staging-db-host',
    port: parseInt(process.env.AIPA_AI_DB_PORT || '5432'),
    username: process.env.AIPA_AI_DB_USERNAME || 'aipa_staging',
    password: process.env.AIPA_AI_DB_PASSWORD || 'staging_password_123',
    database: process.env.AIPA_AI_DB_NAME || 'aipa_ai_staging',
    synchronize: false,
    logging: true,
    ssl: {
      rejectUnauthorized: false, // More lenient for staging
    },
    retryAttempts: 3,
    retryDelay: 3000,
  },

  // Redis Cache
  cache: {
    enabled: true,
    host: process.env.AIPA_REDIS_HOST || 'staging-redis-host',
    port: parseInt(process.env.AIPA_REDIS_PORT || '6379'),
    password: process.env.AIPA_REDIS_PASSWORD || '',
    db: 1, // Different DB from dev/prod
    ttl: 900, // 15 minutes
    keyPrefix: 'aipa:ai:staging:',
    retryAttempts: 3,
    retryDelay: 1000,
  },

  // OpenAI Configuration
  openai: {
    apiKey: process.env.OPENAI_API_KEY || 'staging-key-placeholder',
    model: process.env.OPENAI_MODEL || 'gpt-4o',
    maxTokens: parseInt(process.env.OPENAI_MAX_TOKENS || '4000'),
    temperature: parseFloat(process.env.OPENAI_TEMPERATURE || '0.7'),
    baseUrl: process.env.OPENAI_BASE_URL || 'https://api.openai.com/v1',
    timeout: 30000,
    retryAttempts: 3,
    retryDelay: 1500,
  },

  // Azure Services
  azure: {
    speechService: {
      subscriptionKey: process.env.AZURE_SPEECH_KEY || 'staging-speech-key',
      region: process.env.AZURE_SPEECH_REGION || 'eastus',
      language: process.env.AZURE_SPEECH_LANGUAGE || 'en-US',
      enabled: true,
    },
    keyVault: {
      enabled: false, // Usually disabled in staging
      vaultUri: process.env.AZURE_KEYVAULT_URI || '',
    },
    applicationInsights: {
      connectionString:
        process.env.AZURE_APPINSIGHTS_CONNECTION_STRING || 'staging-appinsights-connection',
      enabled: true,
      samplingRate: 0.5, // Higher sampling in staging
    },
  },

  // Authentication
  auth: {
    jwt: {
      secret:
        process.env.JWT_SECRET_KEY || 'staging-jwt-secret-key-minimum-32-chars-long-for-security',
      expiresIn: process.env.JWT_EXPIRES_IN || '2h', // Longer for testing
      refreshExpiresIn: process.env.JWT_REFRESH_EXPIRES_IN || '14d',
      issuer: process.env.JWT_ISSUER || 'https://staging.aipa.com',
      audience: process.env.JWT_AUDIENCE || 'aipa-clients',
    },
    bcrypt: {
      saltRounds: parseInt(process.env.BCRYPT_SALT_ROUNDS || '10'),
    },
  },

  // Rate Limiting - More lenient than production
  rateLimit: {
    enabled: true,
    windowMs: 60000, // 1 minute
    max: 200, // Higher than production for testing
    skipSuccessfulRequests: false,
    skipFailedRequests: false,
    standardHeaders: true,
    legacyHeaders: false,
  },

  // File Upload
  fileUpload: {
    maxFileSize: parseInt(process.env.FILE_UPLOAD_MAX_SIZE || String(20 * 1024 * 1024)), // 20MB for staging
    allowedMimeTypes: [
      'audio/mpeg',
      'audio/wav',
      'audio/ogg',
      'audio/webm',
      'application/pdf',
      'text/plain',
      'application/json',
      'image/jpeg', // Additional types for testing
      'image/png',
    ],
    uploadPath: process.env.FILE_UPLOAD_PATH || './uploads/staging',
    tempPath: process.env.FILE_TEMP_PATH || './temp/staging',
    cleanupInterval: 1800000, // 30 minutes
  },

  // Security - Staging appropriate
  security: {
    cors: {
      origin: [
        process.env.CORS_ORIGIN_WEB_APP || 'https://staging.aipa.com',
        process.env.CORS_ORIGIN_MOBILE || 'https://mobile-staging.aipa.com',
        'https://localhost:3000', // For local frontend testing
        'http://localhost:3000',
      ].filter(Boolean),
      credentials: true,
      methods: ['GET', 'POST', 'PUT', 'DELETE', 'PATCH', 'OPTIONS'],
      allowedHeaders: ['Content-Type', 'Authorization', 'x-api-key'],
    },
    helmet: {
      contentSecurityPolicy: {
        directives: {
          defaultSrc: ["'self'"],
          styleSrc: ["'self'", "'unsafe-inline'"],
          scriptSrc: ["'self'", "'unsafe-eval'"], // More permissive for staging
          imgSrc: ["'self'", 'data:', 'https:'],
          connectSrc: ["'self'", 'https://api.openai.com', 'wss:'],
          fontSrc: ["'self'"],
          objectSrc: ["'none'"],
          mediaSrc: ["'self'"],
          frameSrc: ["'self'"],
        },
      },
      crossOriginEmbedderPolicy: false, // Disabled for easier testing
      crossOriginOpenerPolicy: false,
      crossOriginResourcePolicy: { policy: 'cross-origin' },
      dnsPrefetchControl: true,
      frameguard: { action: 'sameorigin' },
      hidePoweredBy: true,
      hsts: false, // Disabled in staging
      ieNoOpen: true,
      noSniff: true,
      referrerPolicy: { policy: 'strict-origin-when-cross-origin' },
      xssFilter: true,
    },
    apiKey: {
      enabled: false, // Disabled for easier testing
      key: process.env.API_KEY || 'staging-api-key',
      headerName: 'x-api-key',
    },
  },

  // Monitoring - Enhanced for testing
  monitoring: {
    enabled: true,
    healthCheck: {
      enabled: true,
      path: '/health',
      timeout: 8000,
      checks: {
        database: true,
        redis: true,
        openai: true,
        azure: false, // May be unreliable in staging
      },
    },
    metrics: {
      enabled: true,
      path: '/metrics',
      collectDefaultMetrics: true,
      collectHttpMetrics: true,
    },
    logging: {
      level: 'debug', // More verbose for debugging
      format: 'combined',
      enableConsole: true,
      enableFile: true,
      filePath: './logs/ai-service-staging.log',
      maxFileSize: '20m',
      maxFiles: 5,
      enableStructuredLogging: true,
    },
  },

  // Interview Preparation Features
  interview: {
    maxQuestions: parseInt(process.env.INTERVIEW_MAX_QUESTIONS || '25'), // Higher for testing
    maxSessionDuration: parseInt(process.env.INTERVIEW_MAX_DURATION || '3600'), // 1 hour
    supportedTypes: ['technical', 'behavioral', 'system-design', 'coding'],
    aiAnalysis: {
      enabled: true,
      detailedFeedback: true,
      confidenceScoring: true,
      cacheResults: false, // Disabled for fresh testing
    },
    voiceAnalysis: {
      enabled: true,
      minRecordingDuration: 2000, // 2 seconds for easier testing
      maxRecordingDuration: 300000, // 5 minutes
      supportedFormats: ['mp3', 'wav', 'ogg', 'webm'],
    },
  },

  // External APIs
  external: {
    backendApi: {
      baseUrl: process.env.BACKEND_API_URL || 'https://api-staging.aipa.com',
      timeout: 12000,
      retryAttempts: 2,
      retryDelay: 1000,
    },
    webApp: {
      baseUrl: process.env.WEB_APP_URL || 'https://staging.aipa.com',
    },
  },

  // Staging Settings
  staging: {
    swagger: {
      enabled: true, // Enabled for API testing
      path: '/api/docs',
      title: 'AIPA AI Service API - Staging',
      description: 'AI-powered interview preparation service (Staging)',
      version: '1.0.0-staging',
    },
    compression: {
      enabled: true,
      level: 4, // Lower compression for faster response
      threshold: 1024,
    },
    gracefulShutdown: {
      enabled: true,
      timeout: 5000, // Shorter for faster deployments
      signals: ['SIGTERM', 'SIGINT'],
    },
  },

  // Performance - Staging optimized
  performance: {
    clustering: {
      enabled: false, // Disabled for easier debugging
      workers: 1,
    },
    caching: {
      enabled: true,
      ttl: 180, // 3 minutes - shorter for testing
      maxSize: 50,
    },
  },

  // Testing Features
  testing: {
    mockData: {
      enabled: true,
      generateTestUsers: true,
      generateTestInterviews: true,
    },
    debugging: {
      enableVerboseLogging: true,
      logAllRequests: true,
      enableProfiler: true,
    },
    features: {
      enableExperimentalFeatures: true,
      allowUnsafeOperations: true, // Only in staging
    },
  },
};
