import { Controller, Post, Body, Get } from '@nestjs/common';
import { ApiTags, ApiOperation, ApiResponse } from '@nestjs/swagger';
import { AnalysisService } from './analysis.service';
import { AnalyzeInterviewDto } from './models/analyze-interview.dto';

/**
 * Controller for handling interview analysis operations
 */
@ApiTags('analysis')
@Controller('analysis')
export class AnalysisController {
  constructor(private readonly analysisService: AnalysisService) {}

  /**
   * Test endpoint for smoke testing
   */
  @Get('test')
  @ApiOperation({ summary: 'Test analysis module' })
  @ApiResponse({ status: 200, description: 'Analysis module is working' })
  getTest(): { message: string } {
    return { message: 'Analysis module is working correctly' };
  }

  /**
   * Analyzes complete interview session
   */
  @Post('interview')
  @ApiOperation({ summary: 'Analyze complete interview session' })
  @ApiResponse({
    status: 200,
    description: 'Interview analyzed successfully',
  })
  analyzeInterview(@Body() analyzeDto: AnalyzeInterviewDto) {
    return this.analysisService.analyzeInterview(analyzeDto);
  }

  /**
   * Analyzes communication patterns and speaking habits
   */
  @Post('communication')
  @ApiOperation({ summary: 'Analyze communication patterns' })
  @ApiResponse({
    status: 200,
    description: 'Communication patterns analyzed successfully',
  })
  analyzeCommunication(@Body() body: { audioFile: string; transcript: string }) {
    return this.analysisService.analyzeCommunication(body.audioFile, body.transcript);
  }

  /**
   * Provides performance metrics and scoring
   */
  @Post('performance')
  @ApiOperation({ summary: 'Generate performance metrics' })
  @ApiResponse({
    status: 200,
    description: 'Performance metrics generated successfully',
  })
  generatePerformanceMetrics(@Body() body: { responses: string[]; questions: string[] }) {
    return this.analysisService.generatePerformanceMetrics(body.responses, body.questions);
  }
}
