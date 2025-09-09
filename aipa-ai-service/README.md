# AIPA AI Service - AI Interview Preparation Assistant

A NestJS-based AI microservice for the AI Interview Preparation Assistant (AIPA) platform, providing speech processing, OpenAI integrations, interview analysis, and personalized feedback generation.

## 🏗️ Architecture

This microservice follows NestJS modular architecture with Clean Code principles:

- **Speech Module** - Speech-to-text and text-to-speech operations
- **OpenAI Module** - AI response generation and analysis
- **Analysis Module** - Interview performance analysis and scoring
- **Feedback Module** - Personalized feedback and improvement suggestions
- **Common Module** - Shared utilities, filters, guards, and interceptors

## 🚀 Technologies Used

- **NestJS** - Progressive Node.js framework
- **TypeScript** - Type-safe JavaScript
- **OpenAI API** - AI language model integration
- **Swagger/OpenAPI** - API documentation
- **Class Validator** - Input validation
- **Multer** - File upload handling
- **Jest** - Testing framework

## 📁 Project Structure

```
aipa-ai-service/
├── src/
│   ├── modules/
│   │   ├── speech/              # Speech processing module
│   │   │   ├── models/          # DTOs and interfaces
│   │   │   ├── speech.controller.ts
│   │   │   ├── speech.service.ts
│   │   │   └── speech.module.ts
│   │   ├── openai/              # OpenAI integration module
│   │   │   ├── models/
│   │   │   ├── openai.controller.ts
│   │   │   ├── openai.service.ts
│   │   │   └── openai.module.ts
│   │   ├── analysis/            # Interview analysis module
│   │   │   ├── models/
│   │   │   ├── analysis.controller.ts
│   │   │   ├── analysis.service.ts
│   │   │   └── analysis.module.ts
│   │   └── feedback/            # Feedback generation module
│   │       ├── models/
│   │       ├── feedback.controller.ts
│   │       ├── feedback.service.ts
│   │       └── feedback.module.ts
│   ├── common/                  # Shared utilities
│   │   ├── decorators/
│   │   ├── filters/
│   │   ├── guards/
│   │   ├── interceptors/
│   │   ├── types/
│   │   ├── utils/
│   │   ├── configs/
│   │   └── validators/
│   ├── app.module.ts
│   └── main.ts
├── test/                        # E2E tests
├── Dockerfile
├── .env.example
├── .gitignore
└── README.md
```

## 🛠️ Development Setup

### Prerequisites

- [Node.js](https://nodejs.org/) (v18 or higher)
- [npm](https://www.npmjs.com/) or [yarn](https://yarnpkg.com/)
- [OpenAI API Key](https://platform.openai.com/api-keys)

### Getting Started

1. **Clone and navigate to the service**

   ```bash
   cd aipa-ai-service
   ```

2. **Install dependencies**

   ```bash
   npm install
   ```

3. **Set up environment variables**

   ```bash
   cp .env.example .env
   # Edit .env with your actual API keys and configuration
   ```

4. **Start the development server**
   ```bash
   npm run start:dev
   ```

The service will be available at `http://localhost:3001`.

### Available Scripts

```bash
# Development
npm run start:dev      # Start with hot reload
npm run start:debug    # Start with debug mode

# Production
npm run build          # Build the application
npm run start:prod     # Start production server

# Testing
npm run test           # Run unit tests
npm run test:e2e       # Run end-to-end tests
npm run test:cov       # Run tests with coverage

# Code Quality
npm run lint           # Run ESLint
npm run format         # Format code with Prettier
```

### Configuration

Update your `.env` file with the required configuration:

```env
# Required for OpenAI integration
OPENAI_API_KEY=your_openai_api_key_here

# Optional configurations
PORT=3001
NODE_ENV=development
ALLOWED_ORIGINS=http://localhost:3000
```

## 📚 API Documentation

When running in development mode, Swagger UI is available at:

- `http://localhost:3001/api`

### API Endpoints

#### Speech Module

- `POST /speech/transcribe` - Convert audio to text
- `POST /speech/text-to-speech` - Convert text to speech
- `GET /speech/test` - Health check

#### OpenAI Module

- `POST /openai/generate` - Generate AI responses
- `POST /openai/analyze` - Analyze responses
- `GET /openai/test` - Health check

#### Analysis Module

- `POST /analysis/interview` - Analyze complete interview
- `POST /analysis/communication` - Analyze communication patterns
- `POST /analysis/performance` - Generate performance metrics
- `GET /analysis/test` - Health check

#### Feedback Module

- `POST /feedback/generate` - Generate personalized feedback
- `POST /feedback/improvements` - Get improvement suggestions
- `POST /feedback/practice-plan` - Generate practice plan
- `GET /feedback/test` - Health check

## 🐳 Docker

### Development with Docker

```bash
# Build development image
docker build --target dev -t aipa-ai-service:dev .

# Run development container
docker run -p 3001:3001 -v $(pwd):/app aipa-ai-service:dev
```

### Production with Docker

```bash
# Build production image
docker build -t aipa-ai-service:prod .

# Run production container
docker run -p 3001:3001 --env-file .env aipa-ai-service:prod
```

### Docker Compose

```yaml
version: '3.8'
services:
  aipa-ai-service:
    build: .
    ports:
      - '3001:3001'
    environment:
      - NODE_ENV=development
      - PORT=3001
      - OPENAI_API_KEY=${OPENAI_API_KEY}
    volumes:
      - .:/app
      - /app/node_modules
```

## 🧪 Testing

### Unit Tests

```bash
# Run all unit tests
npm run test

# Run tests in watch mode
npm run test:watch

# Run tests with coverage
npm run test:cov
```

### E2E Tests

```bash
# Run end-to-end tests
npm run test:e2e
```

### Test Structure

- Unit tests are located alongside source files (`*.spec.ts`)
- E2E tests are in the `/test` directory
- Mock data and utilities are in `/test/helpers`

## 🔍 Code Quality

This project follows NestJS and TypeScript best practices:

### Coding Standards

- **TypeScript** with strict type checking
- **ESLint** for code linting
- **Prettier** for code formatting
- **JSDoc** for public API documentation
- **Clean Code** principles and patterns

### Code Organization

- **Modular architecture** with feature-based modules
- **Single responsibility** principle for services and controllers
- **Dependency injection** for loose coupling
- **Input validation** using class-validator
- **Error handling** with global exception filters

### Development Guidelines

1. **Follow NestJS conventions** for file naming and structure
2. **Use TypeScript types** for all parameters and return values
3. **Validate all inputs** using DTOs and class-validator
4. **Document public APIs** with JSDoc comments
5. **Write tests** for all business logic
6. **Use environment variables** for configuration

## 🔧 Performance Considerations

- **Async/await** patterns for non-blocking operations
- **Global interceptors** for request/response logging
- **Input validation** with transformation pipes
- **Error handling** with structured error responses
- **Health checks** for monitoring

## 🔒 Security

- **Input validation** and sanitization
- **CORS** configuration for cross-origin requests
- **Rate limiting** for API protection
- **Environment variables** for sensitive data
- **Global exception filters** to prevent data leaks

## 🚀 Deployment

### Environment Setup

1. Set up production environment variables
2. Configure OpenAI API access
3. Set up monitoring and logging
4. Configure load balancing (if needed)

### Production Checklist

- [ ] Environment variables configured
- [ ] OpenAI API key set up
- [ ] CORS origins configured for production
- [ ] Logging configured
- [ ] Health checks enabled
- [ ] Performance monitoring set up

## 📈 Monitoring

- **Health checks** at `/health` endpoint
- **Swagger documentation** at `/api`
- **Request/response logging** via interceptors
- **Error logging** via global exception filters

## 🤝 Integration

This AI service integrates with:

- **AIPA Backend** (.NET API) - Main business logic
- **AIPA Web App** (Next.js) - Frontend interface
- **OpenAI API** - AI language model
- **Speech Services** - Audio processing

## 📝 Contributing

1. Follow the existing code style and architecture patterns
2. Add unit tests for new features
3. Update API documentation for new endpoints
4. Ensure all tests pass before submitting
5. Follow the pull request template

## 📄 License

This project is part of the AIPA (AI Interview Preparation Assistant) platform.

## 🔗 Related Projects

- [AIPA Backend](../aipa-backend) - .NET Core API
- [AIPA Web App](../aipa-web-app) - Next.js frontend
- [AIPA Mobile App](../aipa-mobile-app) - Flutter mobile application
  <!--[![Backers on Open Collective](https://opencollective.com/nest/backers/badge.svg)](https://opencollective.com/nest#backer)
  [![Sponsors on Open Collective](https://opencollective.com/nest/sponsors/badge.svg)](https://opencollective.com/nest#sponsor)-->

## Description

[Nest](https://github.com/nestjs/nest) framework TypeScript starter repository.

## Project setup

```bash
$ npm install
```

## Compile and run the project

```bash
# development
$ npm run start

# watch mode
$ npm run start:dev

# production mode
$ npm run start:prod
```

## Run tests

```bash
# unit tests
$ npm run test

# e2e tests
$ npm run test:e2e

# test coverage
$ npm run test:cov
```

## Deployment

When you're ready to deploy your NestJS application to production, there are some key steps you can take to ensure it runs as efficiently as possible. Check out the [deployment documentation](https://docs.nestjs.com/deployment) for more information.

If you are looking for a cloud-based platform to deploy your NestJS application, check out [Mau](https://mau.nestjs.com), our official platform for deploying NestJS applications on AWS. Mau makes deployment straightforward and fast, requiring just a few simple steps:

```bash
$ npm install -g mau
$ mau deploy
```

With Mau, you can deploy your application in just a few clicks, allowing you to focus on building features rather than managing infrastructure.

## Resources

Check out a few resources that may come in handy when working with NestJS:

- Visit the [NestJS Documentation](https://docs.nestjs.com) to learn more about the framework.
- For questions and support, please visit our [Discord channel](https://discord.gg/G7Qnnhy).
- To dive deeper and get more hands-on experience, check out our official video [courses](https://courses.nestjs.com/).
- Deploy your application to AWS with the help of [NestJS Mau](https://mau.nestjs.com) in just a few clicks.
- Visualize your application graph and interact with the NestJS application in real-time using [NestJS Devtools](https://devtools.nestjs.com).
- Need help with your project (part-time to full-time)? Check out our official [enterprise support](https://enterprise.nestjs.com).
- To stay in the loop and get updates, follow us on [X](https://x.com/nestframework) and [LinkedIn](https://linkedin.com/company/nestjs).
- Looking for a job, or have a job to offer? Check out our official [Jobs board](https://jobs.nestjs.com).

## Support

Nest is an MIT-licensed open source project. It can grow thanks to the sponsors and support by the amazing backers. If you'd like to join them, please [read more here](https://docs.nestjs.com/support).

## Stay in touch

- Author - [Kamil Myśliwiec](https://twitter.com/kammysliwiec)
- Website - [https://nestjs.com](https://nestjs.com/)
- Twitter - [@nestframework](https://twitter.com/nestframework)

## License

Nest is [MIT licensed](https://github.com/nestjs/nest/blob/master/LICENSE).
