# TASK-01-01-01: Create Azure DevOps Project

**Task Type**: Setup
**Estimated Effort**: 2 hours
**Priority**: Critical
**Dependencies**: None

## Task Description

Create and configure the Azure DevOps project for the AI Interview Preparation Assistant (AIPA) platform, including all necessary project settings, permissions, and integrations.

## Acceptance Criteria

### Project Creation

- [ ] Azure DevOps organization is accessible
- [ ] New project "AIPA-Platform" created with appropriate visibility settings
- [ ] Project description and metadata configured

### Repository Setup

- [ ] Git repositories created for each service:
  - `aipa-backend` (.NET 9 API)
  - `aipa-ai-service` (NestJS)
  - `aipa-web-app` (Next.js)
  - `aipa-mobile-app` (Flutter)
  - `aipa-infrastructure` (IaC and scripts)
- [ ] Default branch set to `main` for all repositories
- [ ] Repository permissions configured

### Work Item Configuration

- [ ] Custom work item types configured (Epic, Story, Task, Bug)
- [ ] Work item templates created with required fields
- [ ] Area paths configured for different components
- [ ] Iteration paths configured for sprints/releases

### Team Configuration

- [ ] Development team members added with appropriate permissions
- [ ] Team capacity and working days configured
- [ ] Notification rules configured for work item updates

## Implementation Steps

### 1. Project Creation

1. Navigate to Azure DevOps organization
2. Create new project with following settings:
   - **Name**: AIPA-Platform
   - **Description**: AI Interview Preparation Assistant - Local-First Development Platform
   - **Visibility**: Private
   - **Process Template**: Agile
   - **Version Control**: Git

### 2. Repository Setup

```bash
# Create repositories via Azure DevOps CLI or UI
az repos create --name "aipa-backend" --project "AIPA-Platform"
az repos create --name "aipa-ai-service" --project "AIPA-Platform"
az repos create --name "aipa-web-app" --project "AIPA-Platform"
az repos create --name "aipa-mobile-app" --project "AIPA-Platform"
az repos create --name "aipa-infrastructure" --project "AIPA-Platform"
```

### 3. Branch Policies

Configure branch policies for `main` branch:

- Require pull request reviews (minimum 1 reviewer)
- Check for linked work items
- Check for comment resolution
- Require build validation

### 4. Work Item Customization

- Import work item templates from process template
- Configure custom fields for story estimation
- Set up area paths for bounded contexts:
  - `AIPA-Platform\Backend\Users`
  - `AIPA-Platform\Backend\Interviews`
  - `AIPA-Platform\Backend\Content`
  - `AIPA-Platform\Backend\Analytics`
  - `AIPA-Platform\AI-Service`
  - `AIPA-Platform\Frontend\Web`
  - `AIPA-Platform\Frontend\Mobile`
  - `AIPA-Platform\Infrastructure`

## Validation Criteria

- [ ] All team members can access the project
- [ ] Repositories are visible and cloneable
- [ ] Work items can be created and assigned
- [ ] Branch policies prevent direct commits to main
- [ ] Email notifications are working properly

## Security Considerations

- [ ] Repository permissions follow principle of least privilege
- [ ] Service connections are secured with proper authentication
- [ ] Audit logging is enabled for the project
- [ ] External contributor access is properly restricted

## Documentation

Document the following in project wiki:

- Project structure and repository purpose
- Branch strategy and policies
- Work item workflow and approval process
- Team roles and responsibilities

## Troubleshooting

**Common Issues:**

- **Permission denied**: Ensure organization admin rights
- **Repository creation fails**: Check naming conventions and organization limits
- **Branch policies not applying**: Verify policy configuration and inheritance

**Resources:**

- [Azure DevOps Documentation](https://docs.microsoft.com/en-us/azure/devops/)
- [Git Repository Management](https://docs.microsoft.com/en-us/azure/devops/repos/git/)
