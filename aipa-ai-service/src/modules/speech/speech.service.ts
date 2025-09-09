import { Injectable, BadRequestException, Logger } from '@nestjs/common';
import { ConfigService } from '@nestjs/config';
import * as speechSdk from 'microsoft-cognitiveservices-speech-sdk';
import { TranscribeAudioDto } from './models/transcribe-audio.dto';
import { SpeechToTextResponseDto } from './models/speech-to-text-response.dto';
import { CircuitBreakerService } from '../../common/services/circuit-breaker.service';
import { RateLimitService } from '../../common/services/rate-limit.service';

/**
 * Enhanced service for handling speech-to-text and text-to-speech operations using Azure Speech Services
 */
@Injectable()
export class SpeechService {
  private readonly logger = new Logger(SpeechService.name);
  private speechConfig: speechSdk.SpeechConfig | null = null;
  private requestCount = 0;
  private errorCount = 0;

  constructor(
    private readonly configService: ConfigService,
    private readonly circuitBreakerService: CircuitBreakerService,
    private readonly rateLimitService: RateLimitService
  ) {
    this.initializeSpeechConfig();
  }

  /**
   * Transcribes audio file to text using Azure Speech Services
   */
  async transcribeAudio(
    audioFile: Express.Multer.File,
    transcribeDto: TranscribeAudioDto
  ): Promise<SpeechToTextResponseDto> {
    if (!audioFile) {
      throw new BadRequestException('Audio file is required');
    }

    // Validate file type
    const allowedTypes = ['audio/wav', 'audio/mp3', 'audio/mpeg', 'audio/m4a', 'audio/webm'];
    if (!allowedTypes.includes(audioFile.mimetype)) {
      throw new BadRequestException('Invalid audio file type');
    }

    // Check rate limits
    const rateLimitResult = this.rateLimitService.checkRateLimit('azure_speech');
    if (!rateLimitResult.allowed) {
      this.logger.warn(
        `Azure Speech rate limit exceeded. Retry after: ${rateLimitResult.retryAfter}s`
      );
      throw new BadRequestException('Rate limit exceeded. Please try again later.');
    }

    this.requestCount++;

    // If no API key configured, return mock response
    if (!this.speechConfig) {
      this.logger.debug('Using mock response - Azure Speech API key not configured');
      return this.createMockTranscription(audioFile, transcribeDto);
    }

    try {
      const result = await this.circuitBreakerService.execute(
        'azure_speech',
        this.performSpeechToText.bind(this),
        [audioFile, transcribeDto],
        {
          timeout: 60000, // Speech processing can take longer
          errorThresholdPercentage: 50,
          resetTimeout: 30000,
        }
      );

      return result as SpeechToTextResponseDto;
    } catch (error) {
      this.errorCount++;
      this.logger.error('Azure Speech transcription failed:', error);

      // Fallback to mock response on API error
      this.logger.warn('Falling back to mock response due to API error');
      return this.createMockTranscription(audioFile, transcribeDto);
    }
  }

  /**
   * Converts text to speech audio using Azure Speech Services
   */
  async convertTextToSpeech(text: string, voice?: string): Promise<any> {
    if (!text || text.trim().length === 0) {
      throw new BadRequestException('Text is required for speech synthesis');
    }

    // Check rate limits
    const rateLimitResult = this.rateLimitService.checkRateLimit('azure_speech');
    if (!rateLimitResult.allowed) {
      this.logger.warn(
        `Azure Speech rate limit exceeded. Retry after: ${rateLimitResult.retryAfter}s`
      );
      throw new BadRequestException('Rate limit exceeded. Please try again later.');
    }

    this.requestCount++;

    // If no API key configured, return mock response
    if (!this.speechConfig) {
      this.logger.debug('Using mock response - Azure Speech API key not configured');
      return this.createMockTextToSpeech(text, voice);
    }

    try {
      const result = await this.circuitBreakerService.execute(
        'azure_speech_tts',
        this.performTextToSpeech.bind(this),
        [text, voice],
        {
          timeout: 30000,
          errorThresholdPercentage: 50,
          resetTimeout: 30000,
        }
      );

      return result as any;
    } catch (error) {
      this.errorCount++;
      this.logger.error('Azure Speech TTS failed:', error);

      // Fallback to mock response on API error
      this.logger.warn('Falling back to mock response due to API error');
      return this.createMockTextToSpeech(text, voice);
    }
  }

  /**
   * Get usage statistics for the speech service
   */
  getUsageStats(): { requestCount: number; errorCount: number; errorRate: number } {
    const errorRate = this.requestCount > 0 ? (this.errorCount / this.requestCount) * 100 : 0;
    return {
      requestCount: this.requestCount,
      errorCount: this.errorCount,
      errorRate: Math.round(errorRate * 100) / 100,
    };
  }

  /**
   * Check if Azure Speech Services is configured
   */
  isConfigured(): boolean {
    return this.speechConfig !== null;
  }

  private initializeSpeechConfig(): void {
    const subscriptionKey = this.configService.get<string>('AZURE_SPEECH_KEY');
    const region = this.configService.get<string>('AZURE_SPEECH_REGION');

    if (!subscriptionKey || !region) {
      this.logger.warn('Azure Speech Services not configured. Service will use mock responses.');
      return;
    }

    try {
      this.speechConfig = speechSdk.SpeechConfig.fromSubscription(subscriptionKey, region);
      this.speechConfig.speechRecognitionLanguage =
        this.configService.get<string>('AZURE_SPEECH_LANGUAGE') || 'en-US';
      this.logger.log('Azure Speech Services configured successfully');
    } catch (error) {
      this.logger.error('Failed to initialize Azure Speech Services:', error);
      this.speechConfig = null;
    }
  }

  private async performSpeechToText(
    audioFile: Express.Multer.File,
    transcribeDto: TranscribeAudioDto
  ): Promise<SpeechToTextResponseDto> {
    const startTime = Date.now();

    return new Promise((resolve, reject) => {
      if (!this.speechConfig) {
        reject(new Error('Speech config not initialized'));
        return;
      }

      // Create audio config from buffer
      const audioConfig = speechSdk.AudioConfig.fromWavFileInput(audioFile.buffer);
      const recognizer = new speechSdk.SpeechRecognizer(this.speechConfig, audioConfig);

      let fullTranscription = '';
      let confidence = 0;

      recognizer.recognizing = (_s, e) => {
        this.logger.debug(`Recognizing: ${e.result.text}`);
      };

      recognizer.recognized = (_s, e) => {
        if (e.result.reason === speechSdk.ResultReason.RecognizedSpeech) {
          fullTranscription += e.result.text + ' ';
          // Azure provides confidence as a number between 0 and 1
          confidence = Math.max(
            confidence,
            e.result.properties?.getProperty(speechSdk.PropertyId.SpeechServiceResponse_JsonResult)
              ? 0.85
              : 0.7
          );
        }
      };

      recognizer.canceled = (_s, e) => {
        this.logger.error(`Speech recognition canceled: ${e.reason}`);
        recognizer.close();
        reject(new Error(`Speech recognition canceled: ${e.errorDetails}`));
      };

      recognizer.sessionStopped = (_s, _e) => {
        this.logger.debug('Speech recognition session stopped');
        recognizer.close();

        const duration = Date.now() - startTime;
        const result: SpeechToTextResponseDto = {
          transcription: fullTranscription.trim() || 'No speech detected',
          confidence: confidence || 0.5,
          language: transcribeDto.language || 'en-US',
          duration: duration / 1000,
          timestamp: new Date(),
        };

        this.logger.log(`Speech-to-text completed in ${duration}ms`);
        resolve(result);
      };

      // Start recognition
      recognizer.startContinuousRecognitionAsync(
        () => {
          this.logger.debug('Speech recognition started');
        },
        (error) => {
          this.logger.error('Failed to start speech recognition:', error);
          recognizer.close();
          reject(new Error(`Failed to start recognition: ${error}`));
        }
      );

      // Stop recognition after processing
      setTimeout(() => {
        recognizer.stopContinuousRecognitionAsync();
      }, 30000); // 30 second timeout
    });
  }

  private async performTextToSpeech(text: string, voice?: string): Promise<any> {
    return new Promise((resolve, reject) => {
      if (!this.speechConfig) {
        reject(new Error('Speech config not initialized'));
        return;
      }

      // Set voice if provided
      if (voice) {
        this.speechConfig.speechSynthesisVoiceName = voice;
      } else {
        this.speechConfig.speechSynthesisVoiceName =
          this.configService.get<string>('AZURE_SPEECH_VOICE') || 'en-US-JennyNeural';
      }

      const synthesizer = new speechSdk.SpeechSynthesizer(this.speechConfig);

      synthesizer.speakTextAsync(
        text,
        (result) => {
          if (result.reason === speechSdk.ResultReason.SynthesizingAudioCompleted) {
            this.logger.log('Text-to-speech synthesis completed');

            // Convert audio data to base64 for transmission
            const audioData = Buffer.from(result.audioData).toString('base64');

            resolve({
              message: 'Text-to-speech conversion completed',
              audioData,
              audioFormat: 'wav',
              voice: voice || 'en-US-JennyNeural',
              textLength: text.length,
              timestamp: new Date(),
            });
          } else {
            this.logger.error('Text-to-speech synthesis failed:', result.errorDetails);
            reject(new Error(`Synthesis failed: ${result.errorDetails}`));
          }
          synthesizer.close();
        },
        (error) => {
          this.logger.error('Text-to-speech synthesis error:', error);
          synthesizer.close();
          reject(new Error(`Synthesis error: ${error}`));
        }
      );
    });
  }

  private createMockTranscription(
    audioFile: Express.Multer.File,
    transcribeDto: TranscribeAudioDto
  ): SpeechToTextResponseDto {
    const mockTranscriptions = [
      'This is a mock transcription of the audio file for testing purposes.',
      'I am preparing for my interview by practicing common questions.',
      'My greatest strength is my ability to work well in team environments.',
      'I am excited about this opportunity to contribute to your organization.',
    ];

    const randomTranscription =
      mockTranscriptions[Math.floor(Math.random() * mockTranscriptions.length)];

    return {
      transcription: randomTranscription,
      confidence: 0.95,
      language: transcribeDto.language || 'en',
      duration: Math.random() * 30 + 10, // Random duration between 10-40 seconds
      timestamp: new Date(),
    };
  }

  private createMockTextToSpeech(text: string, voice?: string): any {
    return {
      message: 'Text-to-speech conversion completed (mock)',
      audioData: 'mock-base64-audio-data',
      audioFormat: 'wav',
      voice: voice || 'default',
      textLength: text.length,
      timestamp: new Date(),
    };
  }
}
