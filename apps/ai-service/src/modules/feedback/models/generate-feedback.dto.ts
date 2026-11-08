import { ApiProperty } from '@nestjs/swagger';
import { IsObject, IsString, IsOptional, IsNumber, Min, Max } from 'class-validator';

export class GenerateFeedbackDto {
  @ApiProperty({
    description: 'Interview analysis results',
    example: {
      overallScore: 7.8,
      strengths: ['communication'],
      improvements: ['examples'],
    },
  })
  @IsObject()
  readonly analysisResult: any;

  @ApiProperty({
    description: 'Job position being interviewed for',
    example: 'Senior Software Engineer',
    required: false,
  })
  @IsOptional()
  @IsString()
  readonly jobPosition?: string;

  @ApiProperty({
    description: 'Experience level of the candidate',
    example: 'mid-level',
    required: false,
  })
  @IsOptional()
  @IsString()
  readonly experienceLevel?: string;

  @ApiProperty({
    description: 'Desired feedback depth (1-10)',
    example: 7,
    minimum: 1,
    maximum: 10,
    required: false,
  })
  @IsOptional()
  @IsNumber()
  @Min(1)
  @Max(10)
  readonly feedbackDepth?: number;
}
