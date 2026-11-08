import { Controller, Get } from '@nestjs/common';
import { ApiTags, ApiOperation, ApiResponse } from '@nestjs/swagger';
import { AppService } from './app.service';

@ApiTags('app')
@Controller()
export class AppController {
  constructor(private readonly appService: AppService) {}

  @Get()
  @ApiOperation({ summary: 'Get application welcome message' })
  @ApiResponse({
    status: 200,
    description: 'Welcome message',
    schema: {
      type: 'object',
      properties: {
        message: { type: 'string' },
        version: { type: 'string' },
        timestamp: { type: 'string' },
        environment: { type: 'string' },
      },
    },
  })
  getHello(): {
    message: string;
    version: string;
    timestamp: string;
    environment: string;
  } {
    return this.appService.getAppInfo();
  }

  @Get('status')
  @ApiOperation({ summary: 'Get application status' })
  @ApiResponse({
    status: 200,
    description: 'Application status information',
    schema: {
      type: 'object',
      properties: {
        status: { type: 'string' },
        uptime: { type: 'number' },
        version: { type: 'string' },
        environment: { type: 'string' },
        memory: { type: 'object' },
      },
    },
  })
  getStatus(): {
    status: string;
    uptime: number;
    version: string;
    environment: string;
    memory: NodeJS.MemoryUsage;
  } {
    return this.appService.getStatus();
  }
}
