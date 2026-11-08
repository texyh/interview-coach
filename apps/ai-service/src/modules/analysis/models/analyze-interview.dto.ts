import { ApiProperty } from '@nestjs/swagger';
import { IsArray, IsString, IsNotEmpty, IsOptional } from 'class-validator';

export class AnalyzeInterviewDto {
  @ApiProperty({
    description: 'List of interview questions asked',
    example: ['Tell me about yourself', 'What are your strengths?'],
    type: [String],
  })
  @IsArray()
  @IsString({ each: true })
  @IsNotEmpty({ each: true })
  readonly questions: string[];

  @ApiProperty({
    description: 'List of candidate responses',
    example: ['I am a software engineer...', 'My strength is problem solving...'],
    type: [String],
  })
  @IsArray()
  @IsString({ each: true })
  @IsNotEmpty({ each: true })
  readonly responses: string[];

  @ApiProperty({
    description: 'Job position context',
    example: 'Senior Software Engineer',
    required: false,
  })
  @IsOptional()
  @IsString()
  readonly jobPosition?: string;

  @ApiProperty({
    description: 'Company context',
    example: 'Google',
    required: false,
  })
  @IsOptional()
  @IsString()
  readonly company?: string;
}
