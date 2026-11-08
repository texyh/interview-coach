import { Test, TestingModule } from '@nestjs/testing';
import { AppController } from './app.controller';
import { AppService } from './app.service';

describe('AppController', () => {
  let appController: AppController;

  beforeEach(async () => {
    const app: TestingModule = await Test.createTestingModule({
      controllers: [AppController],
      providers: [AppService],
    }).compile();

    appController = app.get<AppController>(AppController);
  });

  describe('root', () => {
    it('should return welcome message with application info', () => {
      const result = appController.getHello();

      expect(result).toHaveProperty('message');
      expect(result).toHaveProperty('version');
      expect(result).toHaveProperty('timestamp');
      expect(result).toHaveProperty('environment');
      expect(result.message).toContain('AIPA AI Service');
      expect(result.version).toBe('0.0.1');
      expect(result.environment).toBe('test');
    });
  });
});
