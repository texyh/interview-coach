import { ApiProperty } from '@nestjs/swagger';

export class ChatCompletionResponseDto {
  @ApiProperty({
    description: 'Generated AI response text',
    example: 'I am a passionate software engineer with 5 years of experience...',
  })
  readonly response: string;

  @ApiProperty({
    description: 'Number of tokens used in generation',
    example: 85,
  })
  readonly tokensUsed: number;

  @ApiProperty({
    description: 'Quality score of the response',
    example: 0.92,
    minimum: 0,
    maximum: 1,
  })
  readonly qualityScore: number;

  @ApiProperty({
    description: 'Suggestions for improvement',
    example: ['Add more specific examples', 'Include quantifiable achievements'],
    type: [String],
  })
  readonly suggestions: string[];

  @ApiProperty({
    description: 'Timestamp when response was generated',
    example: '2025-01-07T12:00:00Z',
  })
  readonly timestamp: Date;
}
