import { Module } from '@nestjs/common';
import { MonitoringController } from './monitoring.controller';
import { OpenaiModule } from '../openai/openai.module';
import { SpeechModule } from '../speech/speech.module';

/**
 * Module for monitoring external API integrations
 */
@Module({
  imports: [OpenaiModule, SpeechModule],
  controllers: [MonitoringController],
})
export class MonitoringModule {}
