import * as Joi from 'joi';

export const validationSchema = Joi.object({
  // Environment
  NODE_ENV: Joi.string()
    .valid('development', 'production', 'test', 'staging')
    .default('development'),

  // Server
  PORT: Joi.number().port().default(3001),
  HOST: Joi.string().default('localhost'),

  // Database
  DATABASE_URL: Joi.string().required(),

  // CORS
  ALLOWED_ORIGINS: Joi.string().default('http://localhost:3000'),

  // OpenAI
  OPENAI_API_KEY: Joi.string().optional(),
  OPENAI_MODEL: Joi.string().default('gpt-3.5-turbo'),
  OPENAI_MAX_TOKENS: Joi.number().integer().min(1).max(4000).default(500),
  OPENAI_TEMPERATURE: Joi.number().min(0).max(2).default(0.7),

  // Logging
  LOG_LEVEL: Joi.string().valid('error', 'warn', 'info', 'debug', 'verbose').default('info'),

  // Security
  JWT_SECRET: Joi.string().optional(),

  // Rate Limiting
  RATE_LIMIT_TTL: Joi.number().integer().default(60),
  RATE_LIMIT_LIMIT: Joi.number().integer().default(10),

  // File Upload
  MAX_FILE_SIZE: Joi.number()
    .integer()
    .default(50 * 1024 * 1024), // 50MB

  // Health Check
  HEALTH_CHECK_TIMEOUT: Joi.number().integer().default(3000),
});

export interface EnvironmentVariables {
  NODE_ENV: 'development' | 'production' | 'test' | 'staging';
  PORT: number;
  HOST: string;
  DATABASE_URL: string;
  ALLOWED_ORIGINS: string;
  OPENAI_API_KEY?: string;
  OPENAI_MODEL: string;
  OPENAI_MAX_TOKENS: number;
  OPENAI_TEMPERATURE: number;
  LOG_LEVEL: 'error' | 'warn' | 'info' | 'debug' | 'verbose';
  JWT_SECRET?: string;
  RATE_LIMIT_TTL: number;
  RATE_LIMIT_LIMIT: number;
  MAX_FILE_SIZE: number;
  HEALTH_CHECK_TIMEOUT: number;
}
