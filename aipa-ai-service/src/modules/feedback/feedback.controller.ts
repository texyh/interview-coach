import { Controller, Post, Body, Get } from '@nestjs/common';
import { ApiTags, ApiOperation, ApiResponse } from '@nestjs/swagger';
import { FeedbackService } from './feedback.service';
import { GenerateFeedbackDto } from './models/generate-feedback.dto';

/**
 * Controller for handling feedback generation and management
 */
@ApiTags('feedback')
@Controller('feedback')
export class FeedbackController {
  constructor(private readonly feedbackService: FeedbackService) {}

  /**
   * Test endpoint for smoke testing
   */
  @Get('test')
  @ApiOperation({ summary: 'Test feedback module' })
  @ApiResponse({ status: 200, description: 'Feedback module is working' })
  getTest(): { message: string } {
    return { message: 'Feedback module is working correctly' };
  }

  /**
   * Generates personalized feedback for interview performance
   */
  @Post('generate')
  @ApiOperation({ summary: 'Generate personalized feedback' })
  @ApiResponse({
    status: 200,
    description: 'Feedback generated successfully',
  })
  generateFeedback(@Body() feedbackDto: GenerateFeedbackDto) {
    return this.feedbackService.generateFeedback(feedbackDto);
  }

  /**
   * Provides improvement suggestions and recommendations
   */
  @Post('improvements')
  @ApiOperation({ summary: 'Get improvement suggestions' })
  @ApiResponse({
    status: 200,
    description: 'Improvement suggestions generated successfully',
  })
  getImprovementSuggestions(@Body() body: { analysisResult: any }) {
    return this.feedbackService.getImprovementSuggestions(body.analysisResult);
  }

  /**
   * Generates practice recommendations based on performance
   */
  @Post('practice-plan')
  @ApiOperation({ summary: 'Generate practice plan' })
  @ApiResponse({
    status: 200,
    description: 'Practice plan generated successfully',
  })
  generatePracticePlan(@Body() body: { weakAreas: string[]; jobRole: string }) {
    return this.feedbackService.generatePracticePlan(body.weakAreas, body.jobRole);
  }
}
