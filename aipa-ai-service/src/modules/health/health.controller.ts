import { Controller, Get } from '@nestjs/common';
import { ApiTags, ApiOperation, ApiResponse } from '@nestjs/swagger';
import {
  HealthCheck,
  HealthCheckService,
  HttpHealthIndicator,
  MemoryHealthIndicator,
  DiskHealthIndicator,
} from '@nestjs/terminus';
import { PrismaService } from '../../common/database/prisma.service';

/**
 * Health check controller for monitoring service status
 */
@ApiTags('health')
@Controller('health')
export class HealthController {
  constructor(
    private health: HealthCheckService,
    private http: HttpHealthIndicator,
    private memory: MemoryHealthIndicator,
    private disk: DiskHealthIndicator,
    private prisma: PrismaService
  ) {}

  @Get()
  @ApiOperation({ summary: 'Check service health status' })
  @ApiResponse({
    status: 200,
    description: 'Service is healthy',
    schema: {
      type: 'object',
      properties: {
        status: { type: 'string', example: 'ok' },
        info: {
          type: 'object',
          properties: {
            memory_heap: { type: 'object' },
            memory_rss: { type: 'object' },
            storage: { type: 'object' },
          },
        },
        error: { type: 'object' },
        details: { type: 'object' },
      },
    },
  })
  @ApiResponse({
    status: 503,
    description: 'Service is unhealthy',
  })
  @HealthCheck()
  check() {
    return this.health.check([
      // Check memory usage - heap should not exceed 150MB
      () => this.memory.checkHeap('memory_heap', 150 * 1024 * 1024),
      // Check memory usage - RSS should not exceed 300MB
      () => this.memory.checkRSS('memory_rss', 300 * 1024 * 1024),
      // Check disk storage - should have at least 250MB free
      () =>
        this.disk.checkStorage('storage', {
          path: '/',
          thresholdPercent: 0.9,
        }),
    ]);
  }

  @Get('readiness')
  @ApiOperation({ summary: 'Check if service is ready to receive traffic' })
  @ApiResponse({
    status: 200,
    description: 'Service is ready',
  })
  @HealthCheck()
  checkReadiness() {
    return this.health.check([
      // Basic readiness check - service is able to process requests
      () => this.memory.checkHeap('memory_heap', 200 * 1024 * 1024),
    ]);
  }

  @Get('liveness')
  @ApiOperation({ summary: 'Check if service is alive' })
  @ApiResponse({
    status: 200,
    description: 'Service is alive',
  })
  @HealthCheck()
  checkLiveness() {
    return this.health.check([
      // Simple liveness check - service is running
      () => Promise.resolve({ liveness: { status: 'up' } }),
    ]);
  }

  @Get('openai')
  @ApiOperation({ summary: 'Check OpenAI service connectivity' })
  @ApiResponse({
    status: 200,
    description: 'OpenAI service is accessible',
  })
  @HealthCheck()
  checkOpenAI() {
    return this.health.check([
      // Check if OpenAI API is reachable
      () =>
        this.http.pingCheck('openai', 'https://api.openai.com/v1/models', {
          headers: {
            Authorization: `Bearer ${process.env.OPENAI_API_KEY || 'test'}`,
          },
        }),
    ]);
  }

  @Get('database')
  @ApiOperation({ summary: 'Check database connectivity' })
  @ApiResponse({
    status: 200,
    description: 'Database is accessible',
  })
  @HealthCheck()
  checkDatabase() {
    return this.health.check([
      async () => {
        const healthStatus = await this.prisma.getHealthStatus();
        return {
          database: {
            status: healthStatus.status === 'healthy' ? 'up' : 'down',
            responseTime: healthStatus.responseTime,
            error: healthStatus.error,
          },
        };
      },
    ]);
  }
}
