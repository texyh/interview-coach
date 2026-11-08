import { Module } from '@nestjs/common';
import { ConfigModule } from '@nestjs/config';
import { APP_INTERCEPTOR } from '@nestjs/core';
import { AppController } from './app.controller';
import { AppService } from './app.service';
import { SpeechModule } from './modules/speech/speech.module';
import { OpenaiModule } from './modules/openai/openai.module';
import { AnalysisModule } from './modules/analysis/analysis.module';
import { FeedbackModule } from './modules/feedback/feedback.module';
import { HealthModule } from './modules/health/health.module';
import { MonitoringModule } from './modules/monitoring/monitoring.module';
import { DatabaseModule } from './common/database/database.module';
import { CommonServicesModule } from './common/common-services.module';
import { ExternalApiLoggingInterceptor } from './common/interceptors/external-api-logging.interceptor';
import { validationSchema } from './common/config/env.validation';

@Module({
  imports: [
    ConfigModule.forRoot({
      isGlobal: true,
      envFilePath: '.env',
      validationSchema,
      validationOptions: {
        abortEarly: false,
      },
    }),
    CommonServicesModule,
    DatabaseModule,
    SpeechModule,
    OpenaiModule,
    AnalysisModule,
    FeedbackModule,
    HealthModule,
    MonitoringModule,
  ],
  controllers: [AppController],
  providers: [
    AppService,
    {
      provide: APP_INTERCEPTOR,
      useClass: ExternalApiLoggingInterceptor,
    },
  ],
})
export class AppModule {}
