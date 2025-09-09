import { Injectable, Logger } from '@nestjs/common';
import { InterviewContext } from './prompt-template.service';

export interface FallbackQuestion {
  id: string;
  questionText: string;
  category: 'behavioral' | 'technical' | 'situational' | 'general';
  difficultyLevel: 'junior' | 'mid' | 'senior' | 'lead';
  tags: string[];
  expectedDuration?: number; // in seconds
  industry?: string[];
  jobRoles?: string[];
}

/**
 * Service for managing fallback questions when OpenAI API is unavailable
 */
@Injectable()
export class FallbackQuestionService {
  private readonly logger = new Logger(FallbackQuestionService.name);
  private readonly questions: FallbackQuestion[] = [];

  constructor() {
    this.initializeFallbackQuestions();
  }

  /**
   * Get a random fallback question based on context
   */
  getRandomQuestion(context?: InterviewContext): FallbackQuestion {
    let filteredQuestions = this.questions;

    // Filter by interview type if specified
    if (context?.interviewType) {
      const typeMapping = {
        behavioral: 'behavioral',
        technical: 'technical',
        'system-design': 'technical',
        coding: 'technical',
      };
      const category = typeMapping[context.interviewType] || 'general';
      filteredQuestions = filteredQuestions.filter((q) => q.category === category);
    }

    // Filter by experience level if specified
    if (context?.experienceLevel) {
      filteredQuestions = filteredQuestions.filter(
        (q) => q.difficultyLevel === context.experienceLevel || q.difficultyLevel === 'mid'
      );
    }

    // Filter by industry if specified
    if (context?.industry) {
      filteredQuestions = filteredQuestions.filter(
        (q) => !q.industry || q.industry.includes(context.industry!)
      );
    }

    // Filter by job role if specified
    if (context?.jobRole) {
      filteredQuestions = filteredQuestions.filter(
        (q) =>
          !q.jobRoles ||
          q.jobRoles.some(
            (role) =>
              context.jobRole!.toLowerCase().includes(role.toLowerCase()) ||
              role.toLowerCase().includes(context.jobRole!.toLowerCase())
          )
      );
    }

    // If no questions match the criteria, fall back to general questions
    if (filteredQuestions.length === 0) {
      filteredQuestions = this.questions.filter((q) => q.category === 'general');
    }

    // If still no questions, use all questions
    if (filteredQuestions.length === 0) {
      filteredQuestions = this.questions;
    }

    // Return random question
    const randomIndex = Math.floor(Math.random() * filteredQuestions.length);
    const selectedQuestion = filteredQuestions[randomIndex];

    this.logger.debug(
      `Selected fallback question: ${selectedQuestion.id} - ${selectedQuestion.questionText.substring(0, 50)}...`
    );

    return selectedQuestion;
  }

  /**
   * Get multiple random questions
   */
  getRandomQuestions(count: number, context?: InterviewContext): FallbackQuestion[] {
    const questions: FallbackQuestion[] = [];
    const usedIds: Set<string> = new Set();

    for (let i = 0; i < count && usedIds.size < this.questions.length; i++) {
      let question: FallbackQuestion;
      let attempts = 0;

      do {
        question = this.getRandomQuestion(context);
        attempts++;
      } while (usedIds.has(question.id) && attempts < 10);

      if (!usedIds.has(question.id)) {
        questions.push(question);
        usedIds.add(question.id);
      }
    }

    return questions;
  }

  /**
   * Get questions by category
   */
  getQuestionsByCategory(category: FallbackQuestion['category']): FallbackQuestion[] {
    return this.questions.filter((q) => q.category === category);
  }

  /**
   * Get all available questions
   */
  getAllQuestions(): FallbackQuestion[] {
    return [...this.questions];
  }

  /**
   * Search questions by tags
   */
  searchByTags(tags: string[]): FallbackQuestion[] {
    return this.questions.filter((q) => tags.some((tag) => q.tags.includes(tag.toLowerCase())));
  }

  /**
   * Initialize fallback questions repository
   */
  private initializeFallbackQuestions(): void {
    const fallbackQuestions: FallbackQuestion[] = [
      // Behavioral Questions
      {
        id: 'behavioral-001',
        questionText: 'Tell me about yourself and your background.',
        category: 'behavioral',
        difficultyLevel: 'junior',
        tags: ['introduction', 'background', 'general'],
        expectedDuration: 120,
      },
      {
        id: 'behavioral-002',
        questionText: 'Describe a challenging situation at work and how you handled it.',
        category: 'behavioral',
        difficultyLevel: 'mid',
        tags: ['problem-solving', 'challenge', 'conflict-resolution'],
        expectedDuration: 180,
      },
      {
        id: 'behavioral-003',
        questionText: 'Tell me about a time when you had to work with a difficult team member.',
        category: 'behavioral',
        difficultyLevel: 'mid',
        tags: ['teamwork', 'conflict', 'communication'],
        expectedDuration: 180,
      },
      {
        id: 'behavioral-004',
        questionText: 'Describe a project where you took initiative and leadership.',
        category: 'behavioral',
        difficultyLevel: 'senior',
        tags: ['leadership', 'initiative', 'project-management'],
        expectedDuration: 200,
      },
      {
        id: 'behavioral-005',
        questionText: 'How do you handle tight deadlines and pressure?',
        category: 'behavioral',
        difficultyLevel: 'mid',
        tags: ['stress-management', 'deadlines', 'pressure'],
        expectedDuration: 150,
      },

      // Technical Questions
      {
        id: 'technical-001',
        questionText: 'Explain the difference between synchronous and asynchronous programming.',
        category: 'technical',
        difficultyLevel: 'junior',
        tags: ['programming', 'concepts', 'async'],
        expectedDuration: 180,
        jobRoles: ['Software Engineer', 'Developer', 'Backend Engineer'],
      },
      {
        id: 'technical-002',
        questionText: 'What are the principles of REST API design?',
        category: 'technical',
        difficultyLevel: 'mid',
        tags: ['api', 'rest', 'web-development'],
        expectedDuration: 200,
        jobRoles: ['Software Engineer', 'Backend Engineer', 'Full Stack Developer'],
      },
      {
        id: 'technical-003',
        questionText: 'How would you optimize a slow database query?',
        category: 'technical',
        difficultyLevel: 'mid',
        tags: ['database', 'optimization', 'performance'],
        expectedDuration: 240,
        jobRoles: ['Software Engineer', 'Database Developer', 'Backend Engineer'],
      },
      {
        id: 'technical-004',
        questionText: 'Explain microservices architecture and its benefits and challenges.',
        category: 'technical',
        difficultyLevel: 'senior',
        tags: ['architecture', 'microservices', 'system-design'],
        expectedDuration: 300,
        jobRoles: ['Senior Engineer', 'Architect', 'Technical Lead'],
      },
      {
        id: 'technical-005',
        questionText: 'How do you ensure code quality and maintainability in your projects?',
        category: 'technical',
        difficultyLevel: 'mid',
        tags: ['code-quality', 'testing', 'best-practices'],
        expectedDuration: 200,
        jobRoles: ['Software Engineer', 'Senior Engineer'],
      },

      // Situational Questions
      {
        id: 'situational-001',
        questionText:
          "If you disagreed with your manager's technical decision, how would you handle it?",
        category: 'situational',
        difficultyLevel: 'mid',
        tags: ['conflict', 'management', 'communication'],
        expectedDuration: 180,
      },
      {
        id: 'situational-002',
        questionText: 'How would you approach learning a new technology for a project?',
        category: 'situational',
        difficultyLevel: 'junior',
        tags: ['learning', 'adaptation', 'growth'],
        expectedDuration: 150,
      },
      {
        id: 'situational-003',
        questionText:
          'What would you do if you realized you made a mistake that affected production?',
        category: 'situational',
        difficultyLevel: 'mid',
        tags: ['responsibility', 'problem-solving', 'production'],
        expectedDuration: 180,
      },
      {
        id: 'situational-004',
        questionText:
          'How would you handle a situation where requirements keep changing during development?',
        category: 'situational',
        difficultyLevel: 'senior',
        tags: ['agility', 'change-management', 'stakeholder-management'],
        expectedDuration: 200,
      },

      // General Questions
      {
        id: 'general-001',
        questionText: 'What motivates you in your work?',
        category: 'general',
        difficultyLevel: 'junior',
        tags: ['motivation', 'values', 'career'],
        expectedDuration: 120,
      },
      {
        id: 'general-002',
        questionText: 'Where do you see yourself in 5 years?',
        category: 'general',
        difficultyLevel: 'junior',
        tags: ['career-goals', 'ambition', 'growth'],
        expectedDuration: 150,
      },
      {
        id: 'general-003',
        questionText: 'What are your greatest strengths and weaknesses?',
        category: 'general',
        difficultyLevel: 'junior',
        tags: ['self-awareness', 'strengths', 'weaknesses'],
        expectedDuration: 180,
      },
      {
        id: 'general-004',
        questionText: 'Why are you interested in this position and our company?',
        category: 'general',
        difficultyLevel: 'junior',
        tags: ['company-interest', 'position-fit', 'research'],
        expectedDuration: 150,
      },
      {
        id: 'general-005',
        questionText: 'Do you have any questions for us?',
        category: 'general',
        difficultyLevel: 'junior',
        tags: ['curiosity', 'engagement', 'questions'],
        expectedDuration: 120,
      },
    ];

    this.questions.push(...fallbackQuestions);
    this.logger.log(`Initialized ${fallbackQuestions.length} fallback questions`);
  }
}
