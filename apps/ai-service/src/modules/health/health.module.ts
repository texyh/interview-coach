import { Module } from '@nestjs/common';
import { TerminusModule } from '@nestjs/terminus';
import { HttpModule } from '@nestjs/axios';
import { DatabaseModule } from '../../common/database/database.module';
import { HealthController } from './health.controller';

/**
 * Health module for monitoring service status and dependencies
 */
@Module({
  imports: [TerminusModule, HttpModule, DatabaseModule],
  controllers: [HealthController],
})
export class HealthModule {}
