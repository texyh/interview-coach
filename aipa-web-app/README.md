# AIPA Web App

The web application component of AIPA (AI Interview Preparation Assistant) - a comprehensive SaaS platform for interview preparation.

## 🚀 Tech Stack

- **Framework**: React 19 with TypeScript
- **Build Tool**: Vite
- **UI Library**: Ant Design 5.x
- **Styling**: Tailwind CSS 4.x
- **Icons**: Ant Design Icons

## 📋 Prerequisites

- Node.js 18+
- npm or yarn

## 🛠️ Installation

1. **Clone the repository** (if not already cloned)
   ```bash
   git clone <repository-url>
   cd interview-preper/aipa-web-app
   ```

2. **Install dependencies**
   ```bash
   npm install
   ```

3. **Start development server**
   ```bash
   npm run dev
   ```

4. **Open your browser**
   Navigate to `http://localhost:5173`

## 📜 Available Scripts

- `npm run dev` - Start development server
- `npm run build` - Build for production
- `npm run preview` - Preview production build
- `npm run lint` - Run ESLint
- `npm run lint:fix` - Fix ESLint issues
- `npm run type-check` - Run TypeScript type checking
- `npm run clean` - Clean build artifacts

## 🏗️ Project Structure

```
src/
├── components/     # Reusable UI components
├── pages/         # Page components
├── hooks/         # Custom React hooks
├── services/      # API services and utilities
├── types/         # TypeScript type definitions
├── utils/         # Helper functions
├── styles/        # Global styles and Tailwind config
├── App.tsx        # Main application component
├── main.tsx       # Application entry point
└── index.css      # Global CSS with Tailwind directives
```

## 🎨 Design System

### Colors
- **Primary**: Indigo (#4f46e5, #6366f1)
- **Secondary**: Slate grays
- **Accent**: Green for success states

### Components
- Custom Tailwind utility classes in `index.css`
- Ant Design components with custom theme
- Responsive design with mobile-first approach

## 🔧 Configuration

### Tailwind CSS
- Configuration: `tailwind.config.js`
- PostCSS config: `postcss.config.js`
- Custom utilities defined in `src/index.css`

### Ant Design
- Theme configuration in `App.tsx`
- Custom color tokens aligned with design system

## 🌐 Browser Support

- Chrome 90+
- Firefox 88+
- Safari 14+
- Edge 90+

## 📱 Responsive Design

- Mobile-first approach
- Breakpoints: 640px (sm), 768px (md), 1024px (lg), 1280px (xl)
- Touch-friendly interface elements

## 🔒 Development Guidelines

- Use TypeScript for all new code
- Follow React best practices and hooks guidelines
- Implement proper error boundaries
- Use semantic HTML and accessibility features
- Write clean, maintainable CSS with Tailwind utilities

## 🚀 Deployment

1. **Build for production**
   ```bash
   npm run build
   ```

2. **Preview build locally**
   ```bash
   npm run preview
   ```

3. **Deploy the `dist` folder** to your hosting provider

## 🤝 Contributing

1. Follow the established coding standards
2. Use conventional commit messages
3. Test your changes thoroughly
4. Update documentation as needed

## 📞 Support

For questions or issues related to the web application, please refer to the main project documentation or create an issue in the repository.

---

**AIPA Web App** - Part of the AI Interview Preparation Assistant platform