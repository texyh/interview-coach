# aipa_mobile_app

# AIPA Mobile App

AIPA (AI-Powered Interview Preparation Assistant) Mobile App - A cross-platform Flutter application for interview preparation powered by AI.

## Architecture

This Flutter application follows Clean Architecture principles with the following layer organization:

### Project Structure

```
lib/
├── core/                     # Core utilities and shared components
│   ├── constants/           # App-wide constants
│   ├── error/              # Error handling and exceptions
│   ├── network/            # Network client configuration
│   ├── theme/              # App themes and styling
│   └── utils/              # Utility functions and validators
├── features/               # Feature modules
│   ├── auth/               # Authentication feature
│   │   ├── data/           # Data layer (models, repositories, datasources)
│   │   ├── domain/         # Domain layer (entities, repositories, use cases)
│   │   └── presentation/   # Presentation layer (pages, widgets, providers)
│   └── interview/          # Interview feature (similar structure)
├── shared/                 # Shared UI components and utilities
│   ├── widgets/            # Reusable UI widgets
│   └── utils/              # Shared utility functions
└── main.dart              # Application entry point
```

## Tech Stack

### Core Dependencies

- **Flutter SDK**: Cross-platform mobile framework
- **Dart 3.8.1**: Programming language

### State Management

- **Riverpod 2.6.1**: Modern state management with hooks support
- **Flutter Hooks 0.20.5**: Lifecycle and state hooks
- **Riverpod Annotation**: Code generation for providers

### Data & Serialization

- **Freezed 3.1.0**: Immutable data classes and unions
- **JSON Annotation 4.9.0**: JSON serialization annotations
- **JSON Serializable 6.8.0**: JSON serialization code generation
- **Dartz 0.10.1**: Functional programming (Either types)

### Network & Backend

- **Dio 5.7.0**: HTTP client with interceptors
- **HTTP 1.2.2**: Basic HTTP client
- **Supabase Flutter 2.6.0**: Backend-as-a-Service integration

### Navigation & Routing

- **Go Router 14.6.1**: Declarative routing solution

### Local Storage

- **Shared Preferences 2.3.2**: Key-value storage
- **Hive 2.2.3**: Fast NoSQL database
- **Path Provider 2.1.4**: File system path access

### UI & Media

- **Cached Network Image 3.4.1**: Image caching and loading
- **Cupertino Icons 1.0.8**: iOS-style icons

### Development Tools

- **Build Runner 2.4.13**: Code generation runner
- **Flutter Lints 5.0.0**: Dart linting rules
- **Mockito 5.4.4**: Mocking for tests

## Getting Started

### Prerequisites

- Flutter SDK 3.32.8 or higher
- Dart 3.8.1 or higher
- Android Studio / Xcode for platform-specific development

### Installation

1. **Clone the repository**

   ```bash
   git clone <repository-url>
   cd aipa-mobile-app
   ```

2. **Install dependencies**

   ```bash
   flutter pub get
   ```

3. **Generate code** (for Freezed, Riverpod, and JSON serialization)

   ```bash
   dart run build_runner build --delete-conflicting-outputs
   ```

4. **Configure environment**

   - Update Supabase credentials in `lib/core/constants/app_constants.dart`
   - Configure API endpoints

5. **Run the app**
   ```bash
   flutter run
   ```

### Build for Production

**Web:**

```bash
flutter build web
```

**Android:**

```bash
flutter build apk --release
# or for app bundle
flutter build appbundle --release
```

**iOS:**

```bash
flutter build ios --release
```

## Development Guidelines

### Code Style

Following Flutter and Dart best practices:

- Use `const` constructors for immutable widgets
- Prefer composition over inheritance
- Use descriptive variable names with auxiliary verbs
- Keep lines under 80 characters
- Use trailing commas for better formatting

### State Management with Riverpod

- Use `@riverpod` annotation for generating providers
- Prefer `AsyncNotifierProvider` and `NotifierProvider`
- Avoid `StateProvider` and `StateNotifierProvider`
- Use `ref.invalidate()` for manual provider updates
- Implement proper cancellation for async operations

### Error Handling

- Use `SelectableText.rich` for error display instead of SnackBars
- Handle empty states within displaying screens
- Use `AsyncValue` for proper loading and error states
- Display errors in red color for visibility

### Widget Organization

- Create small, private widget classes instead of methods
- Structure files: exported widget, subwidgets, helpers, static content, types
- Use `ConsumerWidget` for state-dependent widgets
- Use `HookConsumerWidget` when combining Riverpod and Flutter Hooks

### Data Models

- Use Freezed for immutable data classes
- Include `createdAt`, `updatedAt`, and `isDeleted` fields in database models
- Use `@JsonSerializable(fieldRename: FieldRename.snake)` for models
- Implement `@JsonKey(includeFromJson: true, includeToJson: false)` for read-only fields

### Code Generation

After modifying annotated classes, run:

```bash
dart run build_runner build --delete-conflicting-outputs
```

## Features

### Authentication

- Email/password sign in and sign up
- User profile management
- Secure token handling
- Supabase integration

### Interview Preparation

- AI-powered mock interviews
- Performance analysis and feedback
- Progress tracking
- Personalized recommendations

### UI/UX

- Material Design 3
- Light and dark theme support
- Responsive design for tablets
- Smooth animations and transitions

## Testing

Run tests with:

```bash
flutter test
```

For integration tests:

```bash
flutter drive --target=test_driver/app.dart
```

## Contributing

1. Fork the repository
2. Create a feature branch
3. Follow the coding guidelines
4. Write tests for new features
5. Run linting and tests
6. Submit a pull request

## Environment Variables

Configure the following in `lib/core/constants/app_constants.dart`:

- `supabaseUrl`: Your Supabase project URL
- `supabaseAnonKey`: Your Supabase anonymous key
- `baseUrl`: Backend API endpoint
- `aiServiceUrl`: AI service endpoint

## Deployment

### Web Deployment

Build the web version and deploy to your hosting platform:

```bash
flutter build web
# Deploy contents of build/web/ directory
```

### Mobile App Store

Follow platform-specific deployment guides:

- [Android Play Store](https://docs.flutter.dev/deployment/android)
- [iOS App Store](https://docs.flutter.dev/deployment/ios)

## Support

For support and questions:

- Check the Flutter documentation: https://docs.flutter.dev/
- Riverpod documentation: https://riverpod.dev/
- Supabase documentation: https://supabase.io/docs

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Getting Started

This project is a starting point for a Flutter application.

A few resources to get you started if this is your first Flutter project:

- [Lab: Write your first Flutter app](https://docs.flutter.dev/get-started/codelab)
- [Cookbook: Useful Flutter samples](https://docs.flutter.dev/cookbook)

For help getting started with Flutter development, view the
[online documentation](https://docs.flutter.dev/), which offers tutorials,
samples, guidance on mobile development, and a full API reference.
