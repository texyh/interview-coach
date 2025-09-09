---
applyTo: "**"
---

do not recreate anything that exists, including features. we have generate api clients using open api generator. the can be found in api/backend. dont modify anythting there since it auto generated. create a todolist to execute all the tasks. if you delete a file ensure you create it again with the right content.

also connect everything to the backend. please do create anything you are not sure. always ask me for clarify question

**Project Context & Rules**

I'm working on the AIPA Angular App (Angular 20 + TypeScript + Ant Design + TailwindCSS). Please follow these strict guidelines:

**CRITICAL RULES:**

1. **Never recreate existing features** - Check what exists first, build on top of it
2. **Use auto-generated API clients** - All backend APIs are in `src/lib/api/backend/` (auto-generated from OpenAPI). **NEVER modify these files**
3. **Connect directly to backend** - Use the generated TypeScript clients with Angular HTTP interceptors
4. **Follow Angular architecture** - Use services, components, and NgRx store for state management
5. **Check environment variables** - Use Angular environment files for backend URL configuration
6. **🚨 ALWAYS USE CENTRALIZED API SERVICE** - NEVER create new Configuration instances in components

Use generated services directly in components.

**CODING STANDARDS:**

- Use early returns for better readability
- Always use TailwindCSS classes, never CSS/style tags
- Use descriptive names with "handle" prefix for event functions
- Implement accessibility (aria-labels, tabindex, keyboard navigation)
- Use const instead of function declarations
- Follow TypeScript best practices with proper typing

**BEFORE YOU CODE:**

1. **Create a todo list** of what needs to be implemented
2. **Check existing files** - Don't recreate what's already there
3. **🔥 Check centralized API service** - Use `src/services/api.service.ts` for ALL backend calls
4. **Verify the backend API** - Check `src/lib/api/backend/api/` for available endpoints
5. **Ask clarifying questions** if anything is unclear
6. **Plan the implementation** before writing code

**ARCHITECTURE PATTERN:**

- Frontend Component → Centralized API Service (`src/services/api.service.ts`) → Generated API Client → Backend
- Use `backendApi.users`, `backendApi.auth`, `backendApi.health`, etc. from centralized service
- Handle loading states, error handling, and success messages properly
- 🚨 NEVER create new Configuration or API instances in components

**IF YOU'RE UNSURE:**

- Ask me to clarify requirements
- Check the existing codebase first
- Don't assume - verify the available backend endpoints
- Don't create duplicate functionality
- 🚨 NEVER create new API configurations

Please confirm you understand these rules before starting any implementation.
