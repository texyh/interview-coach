# 🎨 AIPA UI Design Prompt for Lovable

## **Project Overview**

You are designing the user interface for **AIPA (AI Interview Preparation Assistant)** - a comprehensive SaaS platform that helps job seekers prepare for interviews using AI-powered mock interviews, personalized coaching, and intelligent feedback.

## **Core Value Proposition**

Transform interview preparation through:

- **Personalized AI Coaching**: Resume analysis and tailored preparation plans
- **Realistic Mock Interviews**: Voice/video-enabled AI interviews with industry-specific content
- **Intelligent Feedback**: Advanced speech analysis and performance scoring
- **Comprehensive Learning**: Rich educational resources with expert contributions

## **Target Audience**

- **Primary**: Individual job seekers (ages 22-45) preparing for career advancement
- **Secondary**: Enterprise users through HR integrations
- **Key Personas**:
  - Recent graduates entering the workforce
  - Mid-career professionals switching industries
  - Executives preparing for leadership roles
  - International professionals (multi-language support needed)

## **Key User Journeys**

### **1. Discovery & Onboarding Journey**

```
Visit Landing Page → Sign Up → Upload Resume → AI Analysis → Personalized Setup
```

### **2. Interview Preparation Journey**

```
Choose Industry/Role → Customize Interview → Start Mock Interview → Voice/Video Recording → Real-time Feedback
```

### **3. Learning & Improvement Journey**

```
View Results → Access Analytics → Get Recommendations → Practice Weak Areas → Track Progress
```

## **Design Requirements**

### **Brand Identity**

- **Color Palette**: Professional blues (#1e40af, #3b82f6) with accent greens (#10b981) for success states
- **Typography**: Modern, readable fonts - Inter for body, system fonts for headers
- **Tone**: Professional yet approachable, encouraging, supportive
- **Visual Style**: Clean, modern, tech-forward with subtle animations

### **Core Design Principles**

1. **Simplicity First**: Reduce cognitive load during high-stress interview preparation
2. **Progress Visualization**: Clear indicators for preparation stages and improvement
3. **Mobile-First**: Responsive design with excellent mobile experience
4. **Accessibility**: WCAG 2.1 AA compliance with screen reader support
5. **Emotional Design**: Build confidence through positive reinforcement and clear progress

### **Technical Constraints**

- **Platforms**: Web (Next.js), Mobile (Flutter - iOS/Android)
- **Key Interactions**: Voice recording, video streaming, real-time feedback
- **Performance**: Fast loading, smooth animations, offline capabilities

## **Required Screen Designs**

### **1. Landing Page & Marketing**

- Hero section with clear value proposition
- Feature highlights with icons/visuals
- Social proof and testimonials
- Pricing tiers (Free, Premium, Enterprise)
- Call-to-action buttons

### **2. Authentication & Onboarding**

- Login/Register forms with social login options
- Password reset flow
- Email verification screens
- Welcome/onboarding flow with resume upload
- Profile setup and preferences

### **3. Dashboard & Analytics**

- Main dashboard with progress overview
- Performance analytics with charts/graphs
- Interview history and results summary
- Skill gap analysis visualization
- Achievement/progress badges

### **4. Interview Setup & Configuration**

- Industry and role selection
- Interview type selection (text/voice/video)
- Difficulty level selection
- Custom interview creation
- Preparation checklist

### **5. Interview Interface**

- Clean interview chat interface
- Voice recording controls with visual feedback
- Video recording with camera controls
- Real-time feedback overlay
- Pause/resume functionality
- Progress indicators

### **6. Results & Feedback**

- Comprehensive results dashboard
- Performance scoring visualization
- Detailed feedback with improvement suggestions
- Audio/video playback for review
- Comparative analysis charts

### **7. Content & Learning**

- Question bank browser with search/filter
- Video tutorials and resources
- Expert content contributions
- Bookmarking and favorites
- Progress tracking for learning paths

### **8. Profile & Settings**

- User profile management
- Subscription and billing
- Notification preferences
- Privacy and data settings
- Account deletion options

### **9. Admin/Management (Enterprise)**

- Content management interface
- User analytics and reporting
- Subscription management
- System settings and configuration

## **UI Component Library Needs**

### **Core Components**

- Navigation (sidebar, bottom nav, breadcrumbs)
- Forms (inputs, selects, file uploads)
- Buttons (primary, secondary, ghost, danger)
- Cards (content, interview, result cards)
- Modals and dialogs
- Progress bars and indicators

### **Specialized Components**

- Audio recording interface with waveform
- Video recording with controls
- Charts and data visualization
- Interview chat bubbles
- Feedback rating systems
- Skill assessment displays

## **Interaction Patterns**

### **Micro-interactions**

- Loading states with skeleton screens
- Success animations for completed tasks
- Hover states and button feedback
- Form validation with inline feedback
- Progress animations during AI processing

### **Workflow Patterns**

- Step-by-step wizards for complex flows
- Drag-and-drop for file uploads
- Swipe gestures for mobile navigation
- Pull-to-refresh for data updates
- Infinite scroll for content lists

## **Responsive Design Requirements**

### **Breakpoint Strategy**

- Mobile: 320px - 768px
- Tablet: 768px - 1024px
- Desktop: 1024px+
- Focus on mobile-first approach

### **Mobile-Specific Considerations**

- Touch-friendly interface elements (44px minimum)
- Optimized for thumb navigation
- Offline functionality indicators
- Native mobile patterns (swipe, pull-to-refresh)

## **Design System Guidelines**

### **Spacing & Layout**

- 8px grid system for consistency
- Generous white space for breathing room
- Clear visual hierarchy with proper typography scales
- Consistent margins and padding

### **Color Usage**

- Primary: Professional blue (#1e40af) for CTAs
- Secondary: Gray scale for text and backgrounds
- Success: Green (#10b981) for positive feedback
- Warning: Orange/amber for cautions
- Error: Red for errors and critical feedback

### **Typography Hierarchy**

- H1: 32px - Major headings
- H2: 24px - Section headers
- H3: 20px - Subsection headers
- Body: 16px - Main content
- Caption: 14px - Secondary information
- Small: 12px - Metadata and labels

## **Success Criteria for Designs**

### **User Experience Metrics**

- Intuitive navigation with clear information architecture
- Fast task completion for common workflows
- Positive emotional response during stressful interview preparation
- Accessibility compliance across all screens
- Performance optimization for smooth interactions

### **Technical Implementation Readiness**

- Component-based design ready for React/Flutter implementation
- Consistent design tokens for theme management
- Responsive layouts that work across all devices
- Interactive prototypes demonstrating key flows
- Design documentation for developer handoff

## **Additional Context**

- **Competition**: Glassdoor, Pramp, Interviewing.io
- **Market Position**: AI-powered, comprehensive preparation platform
- **Growth Stage**: MVP development with enterprise expansion planned
- **Cultural Context**: Global platform with multi-language support

---

**Please generate comprehensive UI designs that capture the professional yet approachable nature of AIPA, focusing on building user confidence and providing clear, actionable feedback throughout the interview preparation journey.**
