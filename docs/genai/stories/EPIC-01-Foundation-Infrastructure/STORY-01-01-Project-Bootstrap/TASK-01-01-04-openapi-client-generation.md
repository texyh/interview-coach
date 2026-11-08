# TASK-01-01-04: OpenAPI Specification Generation and Client Code Generation

**Epic**: EPIC-01 Foundation Infrastructure
**Story**: STORY-01-01 Development Environment Setup
**Priority**: High
**Estimate**: 4-6 hours
**Status**: TODO

## Overview

Set up automated OpenAPI specification generation from .NET and NestJS backends, and configure client code generation for Frontend applications (Next.js Web App and Flutter Mobile App). This ensures frontend and backend APIs stay in sync automatically.

## Business Value

- **API Consistency**: Frontend and backend APIs are always synchronized
- **Developer Productivity**: Reduces manual client code writing
- **Type Safety**: Generated clients provide compile-time type checking
- **Documentation**: OpenAPI specs serve as API documentation
- **Maintenance**: Changes to backend APIs automatically update client code

## Acceptance Criteria

### Backend OpenAPI Generation

- [ ] **Configure .NET Web API OpenAPI**

  - Install and configure Swashbuckle.AspNetCore
  - Generate OpenAPI 3.0 specification
  - Expose swagger endpoint: `/swagger/v1/swagger.json`
  - Include proper API versioning and documentation

- [ ] **Configure NestJS OpenAPI**
  - Install and configure @nestjs/swagger
  - Generate OpenAPI 3.0 specification
  - Expose swagger endpoint: `/api/docs-json`
  - Include DTOs, decorators, and API documentation

### Frontend Client Generation

- [ ] **Next.js Web App Client**

  - Generate TypeScript client from OpenAPI specs
  - Integrate with existing service architecture
  - Configure axios-based HTTP clients
  - Support for both backend APIs

- [ ] **Flutter Mobile App Client**
  - Generate Dart client from OpenAPI specs (if possible)
  - Integrate with Dio HTTP client
  - Support for both backend APIs
  - Fallback to manual implementation if generation not viable

### Automation and Workflow

- [ ] **Build Pipeline Integration**
  - Automate client generation in development workflow
  - Update clients when API specifications change
  - Version control generated client code
  - Documentation for regeneration process

## Implementation Plan

### Phase 1: .NET Backend OpenAPI Setup

#### 1.1 Install Swagger Dependencies

```bash
cd aipa-backend/src/AIPA.WebApi
dotnet add package Swashbuckle.AspNetCore --version 6.5.0
dotnet add package Swashbuckle.AspNetCore.Annotations --version 6.5.0
```

#### 1.2 Configure Swagger in Program.cs

```csharp
// Add Swagger services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "AIPA API",
        Version = "v1",
        Description = "AI-Powered Interview Preparation Assistant API"
    });

    // Include XML comments
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);

    // Add JWT authentication
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
});

// Configure Swagger UI
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "AIPA API v1");
        c.RoutePrefix = "swagger";
    });
}
```

#### 1.3 Add XML Documentation

```xml
<!-- In AIPA.WebApi.csproj -->
<PropertyGroup>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
    <NoWarn>$(NoWarn);1591</NoWarn>
</PropertyGroup>
```

### Phase 2: NestJS Backend OpenAPI Setup

#### 2.1 Install Swagger Dependencies

```bash
cd aipa-ai-service
npm install @nestjs/swagger swagger-ui-express
```

#### 2.2 Configure Swagger in main.ts

```typescript
import { SwaggerModule, DocumentBuilder } from "@nestjs/swagger";

async function bootstrap() {
  const app = await NestFactory.create(AppModule);

  // Swagger configuration
  const config = new DocumentBuilder()
    .setTitle("AIPA AI Service")
    .setDescription(
      "AI-Powered Interview Preparation Assistant - AI Service API"
    )
    .setVersion("1.0")
    .addBearerAuth()
    .build();

  const document = SwaggerModule.createDocument(app, config);
  SwaggerModule.setup("api/docs", app, document);

  // Expose JSON endpoint
  app.use("/api/docs-json", (req, res) => {
    res.json(document);
  });

  await app.listen(3001);
}
```

#### 2.3 Add DTO Decorators

```typescript
// Example DTO with Swagger decorators
import { ApiProperty } from "@nestjs/swagger";

export class InterviewAnalysisDto {
  @ApiProperty({ description: "Interview session ID" })
  sessionId: string;

  @ApiProperty({ description: "Analysis score", minimum: 0, maximum: 100 })
  score: number;

  @ApiProperty({ description: "Feedback text" })
  feedback: string;
}
```

### Phase 3: Client Code Generation

#### 3.1 Next.js Web App Client Generation

##### Install OpenAPI Generator

```bash
cd aipa-web-app
npm install @openapitools/openapi-generator-cli --save-dev
```

##### Configure Generation Scripts

```json
// package.json scripts
{
  "scripts": {
    "generate:api": "npm run generate:backend-api && npm run generate:ai-api",
    "generate:backend-api": "openapi-generator-cli generate -i http://localhost:5000/swagger/v1/swagger.json -g typescript-axios -o src/lib/api/backend",
    "generate:ai-api": "openapi-generator-cli generate -i http://localhost:3001/api/docs-json -g typescript-axios -o src/lib/api/ai-service"
  }
}
```

##### Create API Service Integration

```typescript
// src/services/api.ts
import { Configuration, DefaultApi as BackendApi } from "@/lib/api/backend";
import { DefaultApi as AIServiceApi } from "@/lib/api/ai-service";

const backendConfig = new Configuration({
  basePath: process.env.NEXT_PUBLIC_BACKEND_URL,
  accessToken: () => getAccessToken(),
});

const aiConfig = new Configuration({
  basePath: process.env.NEXT_PUBLIC_AI_SERVICE_URL,
  accessToken: () => getAccessToken(),
});

export const backendApi = new BackendApi(backendConfig);
export const aiServiceApi = new AIServiceApi(aiConfig);
```

#### 3.2 Flutter Mobile App Client Generation

##### Install OpenAPI Generator for Dart

```bash
# Global installation
npm install @openapitools/openapi-generator-cli -g

# Or use Docker
docker pull openapitools/openapi-generator-cli
```

##### Configure Generation for Flutter

```bash
# Generate Dart client for backend API
openapi-generator-cli generate \
  -i http://localhost:5000/swagger/v1/swagger.json \
  -g dart \
  -o lib/core/api/backend \
  --additional-properties=pubName=aipa_backend_api

# Generate Dart client for AI service
openapi-generator-cli generate \
  -i http://localhost:3001/api/docs-json \
  -g dart \
  -o lib/core/api/ai_service \
  --additional-properties=pubName=aipa_ai_service_api
```

##### Integrate with Flutter App

```dart
// lib/core/api/api_client.dart
import 'package:aipa_backend_api/api.dart' as backend;
import 'package:aipa_ai_service_api/api.dart' as ai;

class ApiClient {
  late final backend.DefaultApi backendApi;
  late final ai.DefaultApi aiServiceApi;

  ApiClient() {
    // Configure backend client
    backendApi = backend.DefaultApi(
      backend.ApiClient(
        basePath: AppConstants.baseUrl,
        authentication: backend.HttpBearerAuth()
          ..accessToken = getAccessToken(),
      ),
    );

    // Configure AI service client
    aiServiceApi = ai.DefaultApi(
      ai.ApiClient(
        basePath: AppConstants.aiServiceUrl,
        authentication: ai.HttpBearerAuth()
          ..accessToken = getAccessToken(),
      ),
    );
  }
}
```

### Phase 4: Automation and Workflow

#### 4.1 Create Generation Scripts

```bash
#!/bin/bash
# scripts/generate-clients.sh

echo "Starting API client generation..."

# Start backend services
echo "Starting .NET backend..."
cd aipa-backend && dotnet run --project src/AIPA.WebApi &
BACKEND_PID=$!

echo "Starting NestJS AI service..."
cd aipa-ai-service && npm run start:dev &
AI_PID=$!

# Wait for services to be ready
sleep 10

# Generate clients
echo "Generating Next.js clients..."
cd aipa-web-app && npm run generate:api

echo "Generating Flutter clients..."
cd aipa-mobile-app && ./scripts/generate-dart-clients.sh

# Clean up
kill $BACKEND_PID $AI_PID

echo "API client generation completed!"
```

#### 4.2 Add to Development Workflow

```json
// package.json in root workspace
{
  "scripts": {
    "postinstall": "npm run generate:clients",
    "generate:clients": "./scripts/generate-clients.sh",
    "dev:full": "npm run generate:clients && concurrently \"npm run dev:backend\" \"npm run dev:ai\" \"npm run dev:web\""
  }
}
```

## Technical Considerations

### OpenAPI Specification Quality

- Use proper HTTP status codes and responses
- Include comprehensive schema definitions
- Add detailed descriptions and examples
- Support API versioning strategy

### Client Generation Configuration

- Configure proper naming conventions
- Handle authentication token management
- Set up error handling and retry logic
- Optimize bundle size for generated clients

### Development Workflow Integration

- Automate client regeneration on API changes
- Version control considerations for generated code
- Documentation and team onboarding process

## Testing Strategy

### Backend API Testing

- Verify OpenAPI spec generation
- Test swagger UI functionality
- Validate API documentation accuracy

### Frontend Client Testing

- Test generated client functionality
- Verify type safety and IntelliSense
- Integration tests with actual API calls
- Error handling validation

## Success Metrics

- [ ] OpenAPI specs generated successfully from both backends
- [ ] TypeScript clients work seamlessly in Next.js app
- [ ] Dart clients integrate properly with Flutter app (or fallback implemented)
- [ ] Development workflow includes automated client generation
- [ ] APIs and clients stay synchronized during development

## Risks and Mitigation

- **Risk**: Flutter Dart client generation may not work well
  - **Mitigation**: Implement fallback manual client approach
- **Risk**: Generated clients may be too large/complex
  - **Mitigation**: Configure selective generation, tree-shaking
- **Risk**: Breaking API changes disrupt frontend
  - **Mitigation**: API versioning strategy, gradual migration approach

## Dependencies

- Completed: TASK-01-01-01 (.NET backend structure)
- Completed: TASK-01-01-02 (NestJS AI service and Next.js web app)
- Requires: Running backend services for spec generation

## Definition of Done

- [ ] Both backends generate comprehensive OpenAPI 3.0 specifications
- [ ] Next.js web app uses generated TypeScript clients
- [ ] Flutter mobile app uses generated Dart clients OR manual implementation
- [ ] Automated generation scripts and workflow configured
- [ ] Documentation updated with API client usage instructions
- [ ] Client generation integrated into development workflow
- [ ] Team can efficiently work with synchronized APIs
