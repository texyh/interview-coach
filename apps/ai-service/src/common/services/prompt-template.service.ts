import { Injectable, Logger } from '@nestjs/common';
import { ConfigService } from '@nestjs/config';

export interface PromptTemplate {
  id: string;
  name: string;
  description: string;
  category: 'question-generation' | 'response-analysis' | 'feedback-generation' | 'coaching';
  template: string;
  variables: string[];
  model?: string;
  maxTokens?: number;
  temperature?: number;
}

export interface InterviewContext {
  jobRole?: string;
  company?: string;
  industry?: string;
  experienceLevel?: 'junior' | 'mid' | 'senior' | 'lead';
  interviewType?: 'technical' | 'behavioral' | 'system-design' | 'coding';
}

/**
 * Service for managing AI prompt templates for different interview scenarios
 */
@Injectable()
export class PromptTemplateService {
  private readonly logger = new Logger(PromptTemplateService.name);
  private readonly templates: Map<string, PromptTemplate> = new Map();

  constructor(private readonly configService: ConfigService) {
    this.initializeDefaultTemplates();
  }

  /**
   * Get a prompt template by ID
   */
  getTemplate(templateId: string): PromptTemplate | null {
    return this.templates.get(templateId) || null;
  }

  /**
   * Get all templates by category
   */
  getTemplatesByCategory(category: PromptTemplate['category']): PromptTemplate[] {
    return Array.from(this.templates.values()).filter((template) => template.category === category);
  }

  /**
   * Render a template with provided variables
   */
  renderTemplate(templateId: string, variables: Record<string, string>): string {
    const template = this.getTemplate(templateId);
    if (!template) {
      throw new Error(`Template not found: ${templateId}`);
    }

    let renderedTemplate = template.template;

    // Replace variables in the template
    for (const [key, value] of Object.entries(variables)) {
      const placeholder = `{{${key}}}`;
      renderedTemplate = renderedTemplate.replace(new RegExp(placeholder, 'g'), value);
    }

    // Check for unreplaced variables
    const unreplacedVariables = renderedTemplate.match(/\{\{[^}]+\}\}/g);
    if (unreplacedVariables) {
      this.logger.warn(`Unreplaced variables in template ${templateId}:`, unreplacedVariables);
    }

    return renderedTemplate;
  }

  /**
   * Generate a prompt for interview question generation
   */
  generateQuestionPrompt(context: InterviewContext): string {
    const template = this.getTemplate('question-generation-basic');
    if (!template) {
      throw new Error('Question generation template not found');
    }

    const variables = {
      jobRole: context.jobRole || 'Software Engineer',
      company: context.company || 'a technology company',
      industry: context.industry || 'technology',
      experienceLevel: context.experienceLevel || 'mid',
      interviewType: context.interviewType || 'behavioral',
    };

    return this.renderTemplate('question-generation-basic', variables);
  }

  /**
   * Generate a prompt for response analysis
   */
  generateAnalysisPrompt(question: string, response: string, context?: InterviewContext): string {
    const template = this.getTemplate('response-analysis-basic');
    if (!template) {
      throw new Error('Response analysis template not found');
    }

    const variables = {
      question,
      response,
      jobRole: context?.jobRole || 'Software Engineer',
      experienceLevel: context?.experienceLevel || 'mid',
    };

    return this.renderTemplate('response-analysis-basic', variables);
  }

  /**
   * Add or update a template
   */
  addTemplate(template: PromptTemplate): void {
    this.templates.set(template.id, template);
    this.logger.log(`Template added/updated: ${template.id}`);
  }

  /**
   * Get template configuration (model, maxTokens, temperature)
   */
  getTemplateConfig(templateId: string): {
    model: string;
    maxTokens: number;
    temperature: number;
  } {
    const template = this.getTemplate(templateId);
    if (!template) {
      return {
        model: this.configService.get<string>('OPENAI_MODEL') || 'gpt-3.5-turbo',
        maxTokens: this.configService.get<number>('OPENAI_MAX_TOKENS') || 500,
        temperature: this.configService.get<number>('OPENAI_TEMPERATURE') || 0.7,
      };
    }

    return {
      model: template.model || this.configService.get<string>('OPENAI_MODEL') || 'gpt-3.5-turbo',
      maxTokens: template.maxTokens || this.configService.get<number>('OPENAI_MAX_TOKENS') || 500,
      temperature:
        template.temperature || this.configService.get<number>('OPENAI_TEMPERATURE') || 0.7,
    };
  }

  /**
   * Initialize default prompt templates
   */
  private initializeDefaultTemplates(): void {
    const defaultTemplates: PromptTemplate[] = [
      {
        id: 'question-generation-basic',
        name: 'Basic Question Generation',
        description: 'Generate interview questions based on job role and type',
        category: 'question-generation',
        template: `You are an expert interview coach. Generate a relevant {{interviewType}} interview question for a {{experienceLevel}} {{jobRole}} position at {{company}} in the {{industry}} industry.

The question should:
- Be appropriate for the candidate's experience level
- Test relevant skills for the role
- Be clear and specific
- Allow for a comprehensive response

Generate only the question, without additional context.`,
        variables: ['jobRole', 'company', 'industry', 'experienceLevel', 'interviewType'],
        maxTokens: 200,
        temperature: 0.8,
      },
      {
        id: 'response-analysis-basic',
        name: 'Basic Response Analysis',
        description: 'Analyze interview responses and provide structured feedback',
        category: 'response-analysis',
        template: `You are an expert interview evaluator. Analyze the candidate's response to the interview question for a {{experienceLevel}} {{jobRole}} position.

Question: "{{question}}"

Candidate Response: "{{response}}"

Provide analysis in this JSON format:
{
  "score": number (1-10),
  "strengths": ["strength1", "strength2"],
  "improvements": ["improvement1", "improvement2"],
  "overallFeedback": "overall assessment",
  "relevance": number (1-10),
  "clarity": number (1-10),
  "completeness": number (1-10)
}

Focus on:
- Content relevance and accuracy
- Communication clarity
- Use of specific examples
- Structure and organization
- Areas for improvement`,
        variables: ['question', 'response', 'jobRole', 'experienceLevel'],
        maxTokens: 400,
        temperature: 0.3,
      },
      {
        id: 'feedback-generation-constructive',
        name: 'Constructive Feedback Generation',
        description: 'Generate encouraging and actionable feedback',
        category: 'feedback-generation',
        template: `You are a supportive interview coach. Based on the candidate's performance, provide constructive and encouraging feedback.

Performance Summary:
- Overall Score: {{overallScore}}/10
- Strengths: {{strengths}}
- Areas for Improvement: {{improvements}}

Provide feedback that:
- Acknowledges their strengths positively
- Offers specific, actionable improvement suggestions
- Includes encouragement and motivation
- Suggests concrete next steps
- Maintains a supportive tone

Keep the feedback balanced and constructive.`,
        variables: ['overallScore', 'strengths', 'improvements'],
        maxTokens: 300,
        temperature: 0.7,
      },
      {
        id: 'coaching-tips-generation',
        name: 'Coaching Tips Generation',
        description: 'Generate specific coaching tips and practice suggestions',
        category: 'coaching',
        template: `You are an interview preparation coach. Based on the identified weak areas, provide specific coaching tips and practice exercises.

Weak Areas: {{weakAreas}}
Job Role: {{jobRole}}
Experience Level: {{experienceLevel}}

Provide 3-5 specific, actionable coaching tips that include:
- What to focus on
- How to practice
- Specific techniques to improve
- Resources or frameworks to use
- Timeline for improvement

Make the tips practical and achievable.`,
        variables: ['weakAreas', 'jobRole', 'experienceLevel'],
        maxTokens: 400,
        temperature: 0.6,
      },
    ];

    defaultTemplates.forEach((template) => {
      this.templates.set(template.id, template);
    });

    this.logger.log(`Initialized ${defaultTemplates.length} default prompt templates`);
  }
}
