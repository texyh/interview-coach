// Production environment configuration with Azure Key Vault integration
export default {
  // Application
  app: {
    name: 'AIPA AI Service',
    version: '1.0.0',
    port: parseInt(process.env.PORT || '3001'),
    host: '0.0.0.0',
    environment: 'production',
    debug: false,
    logLevel: 'info',
    corsEnabled: true,
  },

  // Database - All secrets from Azure Key Vault
  database: {
    type: 'postgresql',
    host: process.env.AIPA_AI_DB_HOST || '${aipa-ai-db-host}',
    port: parseInt(process.env.AIPA_AI_DB_PORT || '5432'),
    username: process.env.AIPA_AI_DB_USERNAME || '${aipa-ai-db-username}',
    password: process.env.AIPA_AI_DB_PASSWORD || '${aipa-ai-db-password}',
    database: process.env.AIPA_AI_DB_NAME || '${aipa-ai-db-name}',
    synchronize: false, // Never true in production
    logging: false,
    ssl: {
      rejectUnauthorized: true,
      ca: process.env.AIPA_AI_DB_SSL_CERT || '${aipa-ai-db-ssl-cert}',
    },
    retryAttempts: 5,
    retryDelay: 5000,
    extra: {
      max: 20, // Connection pool size
      min: 5,
      acquire: 60000,
      idle: 10000,
    },
  },

  // Redis Cache - Azure Cache for Redis
  cache: {
    enabled: true,
    host: process.env.AIPA_REDIS_HOST || '${aipa-redis-host}',
    port: parseInt(process.env.AIPA_REDIS_PORT || '6380'),
    password: process.env.AIPA_REDIS_PASSWORD || '${aipa-redis-password}',
    db: 0,
    ttl: 1800, // 30 minutes in production
    keyPrefix: 'aipa:ai:prod:',
    tls: {
      servername: process.env.AIPA_REDIS_HOST || '${aipa-redis-host}',
    },
    retryAttempts: 3,
    retryDelay: 2000,
  },

  // OpenAI Configuration
  openai: {
    apiKey: process.env.OPENAI_API_KEY || '${openai-api-key}',
    model: process.env.OPENAI_MODEL || 'gpt-4o',
    maxTokens: parseInt(process.env.OPENAI_MAX_TOKENS || '4000'),
    temperature: parseFloat(process.env.OPENAI_TEMPERATURE || '0.7'),
    baseUrl: process.env.OPENAI_BASE_URL || 'https://api.openai.com/v1',
    timeout: 30000,
    retryAttempts: 3,
    retryDelay: 2000,
  },

  // Azure Services
  azure: {
    speechService: {
      subscriptionKey: process.env.AZURE_SPEECH_KEY || '${azure-speech-key}',
      region: process.env.AZURE_SPEECH_REGION || 'eastus',
      language: process.env.AZURE_SPEECH_LANGUAGE || 'en-US',
      enabled: true,
    },
    keyVault: {
      enabled: true,
      vaultUri: process.env.AZURE_KEYVAULT_URI || '${azure-keyvault-uri}',
      clientId: process.env.AZURE_KEYVAULT_CLIENT_ID || '${azure-keyvault-client-id}',
      tenantId: process.env.AZURE_KEYVAULT_TENANT_ID || '${azure-keyvault-tenant-id}',
    },
    applicationInsights: {
      connectionString:
        process.env.AZURE_APPINSIGHTS_CONNECTION_STRING || '${azure-appinsights-connection-string}',
      enabled: true,
      samplingRate: 0.1,
    },
  },

  // Authentication
  auth: {
    jwt: {
      secret: process.env.JWT_SECRET_KEY || '${jwt-secret-key}',
      expiresIn: process.env.JWT_EXPIRES_IN || '1h',
      refreshExpiresIn: process.env.JWT_REFRESH_EXPIRES_IN || '7d',
      issuer: process.env.JWT_ISSUER || '${jwt-issuer}',
      audience: process.env.JWT_AUDIENCE || '${jwt-audience}',
    },
    bcrypt: {
      saltRounds: parseInt(process.env.BCRYPT_SALT_ROUNDS || '12'), // Higher in production
    },
  },

  // Rate Limiting - More restrictive in production
  rateLimit: {
    enabled: true,
    windowMs: 60000, // 1 minute
    max: 60, // Reduced from dev
    skipSuccessfulRequests: false,
    skipFailedRequests: false,
    standardHeaders: true,
    legacyHeaders: false,
  },

  // File Upload
  fileUpload: {
    maxFileSize: parseInt(process.env.FILE_UPLOAD_MAX_SIZE || String(10 * 1024 * 1024)), // 10MB
    allowedMimeTypes: [
      'audio/mpeg',
      'audio/wav',
      'audio/ogg',
      'audio/webm',
      'application/pdf',
      'text/plain',
      'application/json',
    ],
    uploadPath: process.env.FILE_UPLOAD_PATH || '/app/uploads',
    tempPath: process.env.FILE_TEMP_PATH || '/app/temp',
    cleanupInterval: 3600000, // 1 hour
  },

  // Security - Production hardened
  security: {
    cors: {
      origin: [
        process.env.CORS_ORIGIN_WEB_APP || '${cors-origin-web-app}',
        process.env.CORS_ORIGIN_MOBILE || '${cors-origin-mobile}',
        process.env.CORS_ORIGIN_ADMIN || '${cors-origin-admin}',
      ].filter(Boolean),
      credentials: true,
      methods: ['GET', 'POST', 'PUT', 'DELETE', 'PATCH'],
      allowedHeaders: ['Content-Type', 'Authorization', 'x-api-key'],
      optionsSuccessStatus: 200,
    },
    helmet: {
      contentSecurityPolicy: {
        directives: {
          defaultSrc: ["'self'"],
          styleSrc: ["'self'", "'unsafe-inline'"],
          scriptSrc: ["'self'"],
          imgSrc: ["'self'", 'data:', 'https:'],
          connectSrc: ["'self'", 'https://api.openai.com'],
          fontSrc: ["'self'"],
          objectSrc: ["'none'"],
          mediaSrc: ["'self'"],
          frameSrc: ["'none'"],
        },
      },
      crossOriginEmbedderPolicy: true,
      crossOriginOpenerPolicy: true,
      crossOriginResourcePolicy: { policy: 'cross-origin' },
      dnsPrefetchControl: true,
      frameguard: { action: 'deny' },
      hidePoweredBy: true,
      hsts: {
        maxAge: 31536000,
        includeSubDomains: true,
        preload: true,
      },
      ieNoOpen: true,
      noSniff: true,
      originAgentCluster: true,
      permittedCrossDomainPolicies: false,
      referrerPolicy: { policy: 'no-referrer' },
      xssFilter: true,
    },
    apiKey: {
      enabled: true,
      key: process.env.API_KEY || '${api-key}',
      headerName: 'x-api-key',
    },
    vulnerabilityScanning: {
      enabled: true,
      schedule: '0 2 * * *', // Daily at 2 AM
    },
    dependencyCheck: {
      enabled: true,
      schedule: '0 3 * * 1', // Weekly on Monday at 3 AM
    },
  },

  // Monitoring - Production level
  monitoring: {
    enabled: true,
    healthCheck: {
      enabled: true,
      path: '/health',
      timeout: 10000,
      checks: {
        database: true,
        redis: true,
        openai: true,
        azure: true,
      },
    },
    metrics: {
      enabled: true,
      path: '/metrics',
      collectDefaultMetrics: true,
      collectHttpMetrics: true,
    },
    logging: {
      level: 'info',
      format: 'json',
      enableConsole: false,
      enableFile: true,
      filePath: '/app/logs/ai-service.log',
      maxFileSize: '50m',
      maxFiles: 10,
      enableStructuredLogging: true,
    },
  },

  // Interview Preparation Features
  interview: {
    maxQuestions: parseInt(process.env.INTERVIEW_MAX_QUESTIONS || '15'), // Reduced for production
    maxSessionDuration: parseInt(process.env.INTERVIEW_MAX_DURATION || '2700'), // 45 minutes
    supportedTypes: ['technical', 'behavioral', 'system-design', 'coding'],
    aiAnalysis: {
      enabled: true,
      detailedFeedback: true,
      confidenceScoring: true,
      cacheResults: true,
    },
    voiceAnalysis: {
      enabled: true,
      minRecordingDuration: 3000, // 3 seconds
      maxRecordingDuration: 180000, // 3 minutes
      supportedFormats: ['mp3', 'wav', 'ogg', 'webm'],
    },
  },

  // External APIs
  external: {
    backendApi: {
      baseUrl: process.env.BACKEND_API_URL || '${backend-api-url}',
      timeout: 15000,
      retryAttempts: 3,
      retryDelay: 2000,
    },
    webApp: {
      baseUrl: process.env.WEB_APP_URL || '${web-app-url}',
    },
  },

  // Production Settings
  production: {
    swagger: {
      enabled: false, // Disabled in production
    },
    compression: {
      enabled: true,
      level: 6,
      threshold: 1024,
    },
    gracefulShutdown: {
      enabled: true,
      timeout: 10000,
      signals: ['SIGTERM', 'SIGINT'],
    },
  },

  // Performance
  performance: {
    clustering: {
      enabled: true,
      workers: parseInt(process.env.WEB_CONCURRENCY || '0') || 4,
    },
    caching: {
      enabled: true,
      ttl: 300, // 5 minutes
      maxSize: 100, // Max cached items
    },
  },
};
