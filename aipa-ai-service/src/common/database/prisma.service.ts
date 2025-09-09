import { Injectable, OnModuleInit, OnModuleDestroy, Logger } from '@nestjs/common';
import { PrismaClient } from '@prisma/client';
import { ConfigService } from '@nestjs/config';

/**
 * Prisma Service for database connection management
 * Provides typed access to Prisma Client functionality
 */
@Injectable()
export class PrismaService implements OnModuleInit, OnModuleDestroy {
  private readonly logger = new Logger(PrismaService.name);
  private prismaClient: PrismaClient;

  constructor(private readonly configService: ConfigService) {
    const isDevelopment = this.configService.get<string>('NODE_ENV') === 'development';

    this.prismaClient = new PrismaClient({
      datasources: {
        db: {
          url: this.configService.get<string>('DATABASE_URL'),
        },
      },
      log: isDevelopment ? ['query', 'info', 'warn', 'error'] : ['warn', 'error'],
      errorFormat: 'pretty',
    });
  }

  // Provide access to the Prisma client
  getClient(): PrismaClient {
    return this.prismaClient;
  }

  async onModuleInit(): Promise<void> {
    try {
      await this.prismaClient.$connect();
      this.logger.log('Database connection established successfully');

      // Test the connection
      await this.prismaClient.$executeRaw`SELECT 1`;
      this.logger.log('Database connectivity test passed');
    } catch (error) {
      this.logger.error('Failed to connect to database:', error);
      throw error;
    }
  }

  async onModuleDestroy(): Promise<void> {
    try {
      await this.prismaClient.$disconnect();
      this.logger.log('Database connection closed successfully');
    } catch (error) {
      this.logger.error('Error closing database connection:', error);
    }
  }

  /**
   * Execute a transaction with automatic rollback on error
   */
  async executeTransaction<T>(operations: (prisma: PrismaClient) => Promise<T>): Promise<T> {
    return this.prismaClient.$transaction(operations);
  }

  /**
   * Get database health status
   */
  async getHealthStatus(): Promise<{
    status: 'healthy' | 'unhealthy';
    responseTime: number;
    error?: string;
  }> {
    const startTime = Date.now();

    try {
      await this.prismaClient.$executeRaw`SELECT 1`;
      const responseTime = Date.now() - startTime;

      return {
        status: 'healthy',
        responseTime,
      };
    } catch (error) {
      return {
        status: 'unhealthy',
        responseTime: Date.now() - startTime,
        error: (error as Error).message,
      };
    }
  }
}
