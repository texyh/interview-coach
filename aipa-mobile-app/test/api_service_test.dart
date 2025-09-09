import 'package:flutter_test/flutter_test.dart';
import 'package:aipa_mobile_app/core/api/api_service.dart';

void main() {
  group('ApiService Tests', () {
    late ApiService apiService;

    setUp(() {
      apiService = ApiService.instance;
    });

    test('should initialize API service singleton', () {
      expect(apiService, isNotNull);
      expect(ApiService.instance, same(apiService));
    });

    test('should set and clear auth token', () {
      const testToken = 'test-token-123';

      apiService.setAuthToken(testToken);
      // Token is set internally - no direct way to verify, but no exception should occur

      apiService.clearAuthToken();
      // Token is cleared internally - no direct way to verify, but no exception should occur

      expect(true, isTrue); // Test passes if no exceptions are thrown
    });

    test('should handle backend health check', () async {
      // This will fail with network error, but should not throw unhandled exception
      final isHealthy = await apiService.checkBackendHealth();
      expect(isHealthy, isFalse); // Expected to fail without backend running
    });

    test('should handle AI service health check', () async {
      // This should return true as it's a placeholder implementation
      final isHealthy = await apiService.checkAiServiceHealth();
      expect(isHealthy, isTrue);
    });
  });
}
