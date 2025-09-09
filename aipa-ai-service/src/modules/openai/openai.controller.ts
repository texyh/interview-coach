import { Controller, Post, Body, Get } from '@nestjs/common';
import { ApiTags, ApiOperation, ApiResponse } from '@nestjs/swagger';
import { OpenaiService } from './openai.service';
import { GenerateResponseDto } from './models/generate-response.dto';
import { ChatCompletionResponseDto } from './models/chat-completion-response.dto';

/**
 * Controller for handling OpenAI API interactions
 * for AI interview preparation system
 */
@ApiTags('openai')
@Controller('openai')
export class OpenaiController {
  constructor(private readonly openaiService: OpenaiService) {}

  /**
   * Test endpoint for smoke testing
   */
  @Get('test')
  @ApiOperation({ summary: 'Test OpenAI module' })
  @ApiResponse({ status: 200, description: 'OpenAI module is working' })
  getTest(): { message: string } {
    return { message: 'OpenAI module is working correctly' };
  }

  /**
   * Check if OpenAI service is configured
   */
  @Get('status')
  @ApiOperation({ summary: 'Check OpenAI service status' })
  @ApiResponse({ status: 200, description: 'OpenAI service status' })
  getStatus(): { configured: boolean; message: string } {
    const configured = this.openaiService.isConfigured();
    return {
      configured,
      message: configured
        ? 'OpenAI service is configured and ready'
        : 'OpenAI service is not configured - using mock responses',
    };
  }

  /**
   * Generates AI response for interview questions
   */
  @Post('generate')
  @ApiOperation({ summary: 'Generate AI response for interview question' })
  @ApiResponse({
    status: 201,
    description: 'AI response generated successfully',
    type: ChatCompletionResponseDto,
  })
  @ApiResponse({
    status: 400,
    description: 'Invalid request parameters',
  })
  async generateResponse(
    @Body() generateDto: GenerateResponseDto
  ): Promise<ChatCompletionResponseDto> {
    return this.openaiService.generateResponse(generateDto);
  }

  /**
   * Analyzes interview responses and provides feedback
   */
  @Post('analyze')
  @ApiOperation({ summary: 'Analyze interview response' })
  @ApiResponse({
    status: 200,
    description: 'Analysis completed successfully',
  })
  @ApiResponse({
    status: 400,
    description: 'Invalid request parameters',
  })
  async analyzeResponse(@Body() body: { response: string; question: string }) {
    return this.openaiService.analyzeResponse(body.response, body.question);
  }
}
