import { Injectable } from '@nestjs/common';

@Injectable()
export class AppService {
  getHello(): string {
    return 'Hello World!';
  }

  getAppInfo(): {
    message: string;
    version: string;
    timestamp: string;
    environment: string;
  } {
    return {
      message: 'Welcome to AIPA AI Service - AI-Powered Interview Preparation Assistant',
      version: process.env.npm_package_version || '1.0.0',
      timestamp: new Date().toISOString(),
      environment: process.env.NODE_ENV || 'development',
    };
  }

  getStatus(): {
    status: string;
    uptime: number;
    version: string;
    environment: string;
    memory: NodeJS.MemoryUsage;
  } {
    return {
      status: 'ok',
      uptime: process.uptime(),
      version: process.env.npm_package_version || '1.0.0',
      environment: process.env.NODE_ENV || 'development',
      memory: process.memoryUsage(),
    };
  }
}
