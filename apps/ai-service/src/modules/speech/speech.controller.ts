import { Controller, Post, Body, UseInterceptors, UploadedFile, Get } from '@nestjs/common';
import { FileInterceptor } from '@nestjs/platform-express';
import { ApiTags, ApiOperation, ApiResponse, ApiConsumes } from '@nestjs/swagger';
import { SpeechService } from './speech.service';
import { TranscribeAudioDto } from './models/transcribe-audio.dto';
import { SpeechToTextResponseDto } from './models/speech-to-text-response.dto';

/**
 * Controller for handling speech-to-text and text-to-speech operations
 * for AI interview preparation system
 */
@ApiTags('speech')
@Controller('speech')
export class SpeechController {
  constructor(private readonly speechService: SpeechService) {}

  /**
   * Test endpoint for smoke testing
   */
  @Get('test')
  @ApiOperation({ summary: 'Test speech module' })
  @ApiResponse({ status: 200, description: 'Speech module is working' })
  getTest(): { message: string } {
    return { message: 'Speech module is working correctly' };
  }

  /**
   * Transcribes audio file to text using speech-to-text AI
   */
  @Post('transcribe')
  @UseInterceptors(FileInterceptor('audio'))
  @ApiOperation({ summary: 'Transcribe audio to text' })
  @ApiConsumes('multipart/form-data')
  @ApiResponse({
    status: 200,
    description: 'Audio transcribed successfully',
    type: SpeechToTextResponseDto,
  })
  @ApiResponse({ status: 400, description: 'Invalid audio file' })
  async transcribeAudio(
    @UploadedFile() audioFile: Express.Multer.File,
    @Body() transcribeDto: TranscribeAudioDto
  ): Promise<SpeechToTextResponseDto> {
    return this.speechService.transcribeAudio(audioFile, transcribeDto);
  }

  /**
   * Converts text to speech audio file
   */
  @Post('text-to-speech')
  @ApiOperation({ summary: 'Convert text to speech' })
  @ApiResponse({
    status: 200,
    description: 'Text converted to speech successfully',
  })
  convertTextToSpeech(@Body() body: { text: string; voice?: string }) {
    return this.speechService.convertTextToSpeech(body.text, body.voice);
  }
}
