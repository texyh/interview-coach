// Development environment configuration
export default {
  // Application
  app: {
    name: 'AIPA AI Service',
    version: '1.0.0',
    port: 3001,
    host: 'localhost',
    environment: 'development',
    debug: true,
    logLevel: 'debug',
    corsEnabled: true,
  },

  // Database
  database: {
    type: 'postgresql',
    host: 'localhost',
    port: 5432,
    username: 'aipa_dev',
    password: 'dev_password_123',
    database: 'aipa_ai_dev',
    synchronize: true,
    logging: true,
    ssl: false,
    retryAttempts: 3,
    retryDelay: 3000,
  },

  // Redis Cache
  cache: {
    enabled: true,
    host: 'localhost',
    port: 6379,
    password: '',
    db: 0,
    ttl: 3600, // 1 hour
    keyPrefix: 'aipa:ai:dev:',
    retryAttempts: 3,
    retryDelay: 1000,
  },

  // OpenAI Configuration
  openai: {
    apiKey: 'sk-dev-key-placeholder-replace-with-actual-key',
    model: 'gpt-4o',
    maxTokens: 4000,
    temperature: 0.7,
    baseUrl: 'https://api.openai.com/v1',
    timeout: 30000,
    retryAttempts: 3,
    retryDelay: 1000,
  },

  // Azure Services
  azure: {
    speechService: {
      subscriptionKey: 'dev-speech-key-placeholder',
      region: 'eastus',
      language: 'en-US',
      enabled: false, // Disabled in dev to avoid costs
    },
    keyVault: {
      enabled: false,
      vaultUri: '',
    },
    applicationInsights: {
      connectionString: '',
      enabled: false,
    },
  },

  // Authentication
  auth: {
    jwt: {
      secret: 'dev-jwt-secret-key-minimum-32-chars-long-for-security',
      expiresIn: '1h',
      refreshExpiresIn: '7d',
      issuer: 'http://localhost:3001',
      audience: 'aipa-clients',
    },
    bcrypt: {
      saltRounds: 10,
    },
  },

  // Rate Limiting
  rateLimit: {
    enabled: true,
    windowMs: 60000, // 1 minute
    max: 100, // Requests per window
    skipSuccessfulRequests: false,
    skipFailedRequests: false,
  },

  // File Upload
  fileUpload: {
    maxFileSize: 10 * 1024 * 1024, // 10MB
    allowedMimeTypes: [
      'audio/mpeg',
      'audio/wav',
      'audio/ogg',
      'audio/webm',
      'application/pdf',
      'text/plain',
      'application/json',
    ],
    uploadPath: './uploads/dev',
    tempPath: './temp/dev',
  },

  // Security
  security: {
    cors: {
      origin: [
        'http://localhost:3000', // Next.js dev server
        'http://localhost:3002', // Alternative dev port
        'http://127.0.0.1:3000',
      ],
      credentials: true,
      methods: ['GET', 'POST', 'PUT', 'DELETE', 'PATCH', 'OPTIONS'],
      allowedHeaders: ['Content-Type', 'Authorization', 'x-api-key'],
    },
    helmet: {
      contentSecurityPolicy: false, // Disabled in development
      crossOriginEmbedderPolicy: false,
    },
    apiKey: {
      enabled: false, // Disabled in development
      key: 'dev-api-key-placeholder',
    },
  },

  // Monitoring
  monitoring: {
    enabled: true,
    healthCheck: {
      enabled: true,
      path: '/health',
      timeout: 5000,
    },
    metrics: {
      enabled: true,
      path: '/metrics',
    },
    logging: {
      level: 'debug',
      format: 'combined',
      enableConsole: true,
      enableFile: false,
    },
  },

  // Interview Preparation Features
  interview: {
    maxQuestions: 20,
    maxSessionDuration: 3600, // 1 hour
    supportedTypes: ['technical', 'behavioral', 'system-design', 'coding'],
    aiAnalysis: {
      enabled: true,
      detailedFeedback: true,
      confidenceScoring: true,
    },
    voiceAnalysis: {
      enabled: false, // Disabled in dev
      minRecordingDuration: 5000, // 5 seconds
      maxRecordingDuration: 300000, // 5 minutes
    },
  },

  // External APIs
  external: {
    backendApi: {
      baseUrl: 'http://localhost:5000/api',
      timeout: 10000,
      retryAttempts: 3,
    },
    webApp: {
      baseUrl: 'http://localhost:3000',
    },
  },

  // External API Configuration
  externalApis: {
    openai: {
      timeout: 30000,
      maxRetries: 3,
      rateLimitPerMinute: 60,
    },
    azureSpeech: {
      timeout: 60000,
      maxRetries: 2,
      rateLimitPerMinute: 100,
      region: process.env.AZURE_SPEECH_REGION || 'eastus',
      language: 'en-US',
      voice: 'en-US-JennyNeural',
    },
  },

  // Circuit Breaker Configuration
  circuitBreaker: {
    timeout: 30000,
    errorThresholdPercentage: 50,
    resetTimeout: 30000,
    rollingCountTimeout: 10000,
    rollingCountBuckets: 10,
  },

  // Rate Limiting Configuration
  rateLimiting: {
    defaultWindow: 60000, // 1 minute
    openaiLimit: 60,
    azureSpeechLimit: 100,
  },

  // Development Tools
  development: {
    swagger: {
      enabled: true,
      path: '/api/docs',
      title: 'AIPA AI Service API',
      description: 'AI-powered interview preparation service',
      version: '1.0.0',
    },
    hotReload: true,
    watchFiles: true,
    debugRoutes: true,
  },
};
