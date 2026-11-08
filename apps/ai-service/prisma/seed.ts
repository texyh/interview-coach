import { PrismaClient } from '@prisma/client';

const prisma = new PrismaClient();

async function main() {
  // Seed data for development and testing
  console.log('Starting database seeding...');

  // Create sample users
  const user1 = await prisma.user.upsert({
    where: { email: 'demo@example.com' },
    update: {},
    create: {
      email: 'demo@example.com',
      name: 'Demo User',
      role: 'CANDIDATE',
    },
  });

  const user2 = await prisma.user.upsert({
    where: { email: 'admin@example.com' },
    update: {},
    create: {
      email: 'admin@example.com',
      name: 'Admin User',
      role: 'INTERVIEWER',
    },
  });

  // Create sample interview session
  const session = await prisma.interviewSession.create({
    data: {
      userId: user1.id,
      title: 'Software Engineer Interview Practice',
      description: 'Mock interview for software engineering position',
      industry: 'Technology',
      role: 'Software Engineer',
      difficultyLevel: 'INTERMEDIATE',
      sessionType: 'PRACTICE',
      status: 'COMPLETED',
      duration: 45,
      questionCount: 5,
      categories: ['technical', 'behavioral', 'system-design'],
      scheduledAt: new Date('2024-01-15T10:00:00Z'),
      startedAt: new Date('2024-01-15T10:00:00Z'),
      completedAt: new Date('2024-01-15T10:45:00Z'),
    },
  });

  // Create sample interview questions
  const questions = await Promise.all([
    prisma.interviewQuestion.create({
      data: {
        sessionId: session.id,
        questionText: 'Tell me about yourself and your background in software development.',
        questionType: 'BEHAVIORAL',
        category: 'introduction',
        difficultyLevel: 'BEGINNER',
        expectedDuration: 120,
        order: 1,
        tags: ['introduction', 'background'],
      },
    }),
    prisma.interviewQuestion.create({
      data: {
        sessionId: session.id,
        questionText: 'Explain the difference between REST and GraphQL APIs.',
        questionType: 'TECHNICAL',
        category: 'api',
        difficultyLevel: 'INTERMEDIATE',
        expectedDuration: 300,
        order: 2,
        tags: ['api', 'web-development'],
      },
    }),
    prisma.interviewQuestion.create({
      data: {
        sessionId: session.id,
        questionText: 'How would you design a scalable system for a social media platform?',
        questionType: 'PROBLEM_SOLVING',
        category: 'system-design',
        difficultyLevel: 'ADVANCED',
        expectedDuration: 600,
        order: 3,
        tags: ['system-design', 'scalability'],
      },
    }),
  ]);

  // Create sample interview responses
  const responses = await Promise.all([
    prisma.interviewResponse.create({
      data: {
        sessionId: session.id,
        questionId: questions[0].id,
        textResponse:
          'I am a software engineer with 3 years of experience in full-stack development...',
        audioUrl: 'https://example.com/audio/response1.mp3',
        audioDuration: 115,
        responseType: 'AUDIO',
      },
    }),
    prisma.interviewResponse.create({
      data: {
        sessionId: session.id,
        questionId: questions[1].id,
        textResponse:
          'REST APIs use HTTP methods and are stateless, while GraphQL provides a single endpoint...',
        audioUrl: 'https://example.com/audio/response2.mp3',
        audioDuration: 280,
        responseType: 'AUDIO',
      },
    }),
  ]);

  // Create sample AI analysis
  await prisma.aiAnalysis.create({
    data: {
      userId: user1.id,
      sessionId: session.id,
      responseId: responses[0].id,
      overallScore: 78.5,
      relevanceScore: 82.0,
      clarityScore: 75.0,
      completenessScore: 77.0,
      strengths: ['Clear communication', 'Relevant experience'],
      improvements: ['More specific examples', 'Better structure'],
      suggestions: ['Practice STAR method', 'Prepare specific examples'],
      analysisType: 'RESPONSE_ANALYSIS',
      status: 'COMPLETED',
      processingTime: 1.25,
      openaiModel: 'gpt-4-turbo',
      confidenceScore: 88.5,
    },
  });

  // Create sample speech analysis
  await prisma.speechAnalysis.create({
    data: {
      userId: user1.id,
      sessionId: session.id,
      responseId: responses[0].id,
      clarity: 82.5,
      pace: 145.0, // words per minute
      volume: 75.0,
      pauseCount: 8,
      averagePauseTime: 1.2,
      fillerWordCount: 3,
      fillerWords: { um: 2, uh: 1 },
      pronunciationScore: 88.0,
      transcription:
        'I am a software engineer with three years of experience in full-stack development...',
      transcriptionConf: 0.94,
      status: 'COMPLETED',
      processingTime: 2.1,
    },
  });

  // Create sample feedback report
  await prisma.feedbackReport.create({
    data: {
      userId: user1.id,
      sessionId: session.id,
      overallScore: 78.5,
      performanceLevel: 'PROFICIENT',
      strengths: [
        'Clear and confident communication',
        'Relevant technical knowledge',
        'Good pace and articulation',
      ],
      improvements: [
        'Provide more specific examples',
        'Structure answers using frameworks like STAR',
        'Reduce filler words',
      ],
      recommendations: [
        'Practice with more behavioral questions',
        'Review system design patterns',
        'Record practice sessions for self-review',
      ],
      nextSteps: [
        'Schedule follow-up practice session',
        'Focus on system design questions',
        'Prepare portfolio of projects to discuss',
      ],
    },
  });

  // Create sample API usage tracking
  await prisma.apiUsage.create({
    data: {
      provider: 'OPENAI',
      service: 'chat-completion',
      endpoint: '/v1/chat/completions',
      requestCount: 5,
      tokensUsed: 1250,
      costAmount: 0.025,
      currency: 'USD',
      model: 'gpt-4-turbo',
      responseTime: 2.5,
      date: new Date('2024-01-15'),
      hour: 10,
    },
  });

  console.log('Database seeding completed successfully!');
  console.log({
    users: [user1.email, user2.email],
    session: session.title,
    questions: questions.length,
    responses: responses.length,
  });
}

main()
  .catch((e) => {
    console.error('Error during seeding:', e);
    process.exit(1);
  })
  .finally(async () => {
    await prisma.$disconnect();
  });
