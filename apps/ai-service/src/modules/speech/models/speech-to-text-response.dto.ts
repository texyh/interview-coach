import { ApiProperty } from '@nestjs/swagger';

export class SpeechToTextResponseDto {
  @ApiProperty({
    description: 'Transcribed text from audio',
    example: 'Hello, my name is John and I am applying for the software engineer position.',
  })
  readonly transcription: string;

  @ApiProperty({
    description: 'Confidence score of the transcription',
    example: 0.95,
    minimum: 0,
    maximum: 1,
  })
  readonly confidence: number;

  @ApiProperty({
    description: 'Detected language of the audio',
    example: 'en',
  })
  readonly language: string;

  @ApiProperty({
    description: 'Duration of the audio in seconds',
    example: 30.5,
  })
  readonly duration: number;

  @ApiProperty({
    description: 'Timestamp when transcription was completed',
    example: '2025-01-07T12:00:00Z',
  })
  readonly timestamp: Date;
}
