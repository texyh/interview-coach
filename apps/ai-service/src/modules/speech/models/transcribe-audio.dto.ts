import { ApiProperty } from '@nestjs/swagger';
import { IsString, IsOptional, IsIn } from 'class-validator';

export class TranscribeAudioDto {
  @ApiProperty({
    description: 'Language of the audio',
    example: 'en',
    required: false,
  })
  @IsOptional()
  @IsString()
  @IsIn(['en', 'es', 'fr', 'de', 'it', 'pt', 'zh', 'ja', 'ko'])
  readonly language?: string;

  @ApiProperty({
    description: 'Audio file to transcribe',
    type: 'string',
    format: 'binary',
  })
  readonly audio: any;
}
