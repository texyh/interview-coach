import { Module } from '@nestjs/common';
import { PrismaService } from './prisma.service';

/**
 * Database Module
 * Provides Prisma service for database operations
 */
@Module({
  providers: [PrismaService],
  exports: [PrismaService],
})
export class DatabaseModule {}
