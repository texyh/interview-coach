import { ApiProperty } from '@nestjs/swagger';
import { IsString, IsNotEmpty, IsOptional, IsNumber, Min, Max } from 'class-validator';

export class GenerateResponseDto {
  @ApiProperty({
    description: 'The prompt or question to generate response for',
    example: 'Tell me about yourself as a software engineer',
  })
  @IsString()
  @IsNotEmpty()
  readonly prompt: string;

  @ApiProperty({
    description: 'Context for the interview (job role, company, etc.)',
    example: 'Software Engineer position at Google',
    required: false,
  })
  @IsOptional()
  @IsString()
  readonly context?: string;

  @ApiProperty({
    description: 'Temperature for response creativity (0-1)',
    example: 0.7,
    minimum: 0,
    maximum: 1,
    required: false,
  })
  @IsOptional()
  @IsNumber()
  @Min(0)
  @Max(1)
  readonly temperature?: number;

  @ApiProperty({
    description: 'Maximum tokens in response',
    example: 200,
    required: false,
  })
  @IsOptional()
  @IsNumber()
  @Min(1)
  readonly maxTokens?: number;
}
