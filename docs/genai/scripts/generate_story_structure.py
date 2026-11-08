#!/usr/bin/env python3
"""
Generate complete EPIC, Story, and Task structure for Azure DevOps import
This script creates all necessary folders and markdown files for the AI Interview Preparation Assistant project
"""

import os

# Base directory for stories
STORIES_BASE = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "stories")

# EPIC definitions with their stories
EPICS_STRUCTURE = {
    "EPIC-01-Foundation-Infrastructure": {
        "title": "Foundation & Infrastructure Setup",
        "stories": [
            "STORY-01-01-Project-Bootstrap-Development-Environment",
            "STORY-01-02-NET-Backend-Foundation-Clean-Architecture",
            "STORY-01-03-NestJS-AI-Service-Development",
            "STORY-01-04-Database-Design-PostgreSQL-Setup",
            "STORY-01-05-Docker-Compose-Local-Development",
            "STORY-01-06-NET-Aspire-Orchestration-Setup",
            "STORY-01-07-Basic-Authentication-JWT-Implementation",
            "STORY-01-08-Core-API-Endpoints-Development",
        ],
    },
    "EPIC-02-User-Management-Authentication": {
        "title": "User Management & Authentication",
        "stories": [
            "STORY-02-01-JWT-Authentication-System",
            "STORY-02-02-Role-Based-Access-Control-RBAC",
            "STORY-02-03-User-Registration-Email-Verification",
            "STORY-02-04-Password-Management-Reset-System",
            "STORY-02-05-User-Profile-Management",
            "STORY-02-06-Subscription-Tier-Management",
            "STORY-02-07-Stripe-Payment-Integration",
            "STORY-02-08-Billing-Invoice-Management",
            "STORY-02-09-User-Preferences-Settings",
            "STORY-02-10-Account-Security-Two-Factor-Auth",
        ],
    },
    "EPIC-03-AI-Powered-Interview-System": {
        "title": "AI-Powered Interview System",
        "stories": [
            "STORY-03-01-OpenAI-Integration-Setup",
            "STORY-03-02-Interview-Session-Management",
            "STORY-03-03-Question-Generation-AI-Engine",
            "STORY-03-04-Response-Analysis-Scoring-System",
            "STORY-03-05-Real-Time-Feedback-Generation",
            "STORY-03-06-Azure-Speech-Services-Integration",
            "STORY-03-07-Voice-Interview-Functionality",
            "STORY-03-08-Interview-Recording-Playback",
            "STORY-03-09-Custom-Interview-Templates",
            "STORY-03-10-Interview-Scheduling-System",
            "STORY-03-11-Performance-Analytics-Tracking",
            "STORY-03-12-AI-Coaching-Recommendations",
        ],
    },
    "EPIC-04-Content-Management-System": {
        "title": "Content Management System",
        "stories": [
            "STORY-04-01-Question-Bank-Architecture",
            "STORY-04-02-Content-Creation-Interface",
            "STORY-04-03-Question-Categorization-Tagging",
            "STORY-04-04-Content-Review-Approval-Workflow",
            "STORY-04-05-Expert-Contributor-Portal",
            "STORY-04-06-Content-Versioning-History",
            "STORY-04-07-Advanced-Search-Filtering",
            "STORY-04-08-Content-Quality-Metrics",
            "STORY-04-09-Bulk-Import-Export-Tools",
            "STORY-04-10-Content-Analytics-Insights",
            "STORY-04-11-Industry-Specific-Content",
            "STORY-04-12-Content-Localization-Support",
        ],
    },
    "EPIC-05-Mobile-Application-Development": {
        "title": "Mobile Application Development",
        "stories": [
            "STORY-05-01-Flutter-Project-Setup-Architecture",
            "STORY-05-02-Mobile-Authentication-Integration",
            "STORY-05-03-Responsive-UI-Design-System",
            "STORY-05-04-Interview-Session-Mobile-Interface",
            "STORY-05-05-Voice-Recording-Audio-Processing",
            "STORY-05-06-Offline-Mode-Data-Synchronization",
            "STORY-05-07-Push-Notifications-System",
            "STORY-05-08-Camera-Integration-Video-Recording",
            "STORY-05-09-Mobile-Performance-Optimization",
            "STORY-05-10-App-Store-Deployment-Pipeline",
            "STORY-05-11-Mobile-Analytics-Crash-Reporting",
            "STORY-05-12-Accessibility-Compliance-Features",
        ],
    },
    "EPIC-06-Analytics-Performance-Tracking": {
        "title": "Analytics & Performance Tracking",
        "stories": [
            "STORY-06-01-Real-Time-Analytics-Dashboard",
            "STORY-06-02-User-Performance-Metrics",
            "STORY-06-03-Interview-Success-Analytics",
            "STORY-06-04-Skill-Assessment-Tracking",
            "STORY-06-05-Progress-Visualization-Charts",
            "STORY-06-06-Comparative-Benchmarking-System",
            "STORY-06-07-AI-Powered-Insights-Generation",
            "STORY-06-08-Custom-Report-Builder",
            "STORY-06-09-Achievement-Badge-System",
            "STORY-06-10-Learning-Path-Recommendations",
            "STORY-06-11-Performance-Prediction-Models",
            "STORY-06-12-Export-Sharing-Capabilities",
        ],
    },
}


def create_story_structure(epic_folder, story_id, story_title):
    """Create story folder and README with task list"""
    story_folder = os.path.join(epic_folder, story_id)
    os.makedirs(story_folder, exist_ok=True)

    # Create story README
    story_readme = os.path.join(story_folder, "README.md")

    # Generate 6-8 tasks for each story
    tasks = generate_tasks_for_story(story_id, story_title)

    story_content = f"""# {story_id.replace("-", " ").title()}: {story_title}

**Story ID**: {story_id}
**Epic**: {epic_folder.split("/")[-1]}
**Story Points**: 8
**Priority**: High
**Assignee**: Development Team

## User Story

**As a** user of the AI Interview Preparation Assistant
**I want to** {story_title.lower()}
**So that** I can improve my interview preparation and performance

## Acceptance Criteria

{generate_acceptance_criteria(story_title)}

## Technical Requirements

{generate_technical_requirements(story_title)}

## Tasks in this Story

{chr(10).join([f"{i + 1}. **[{task['id']}] {task['title']}**" for i, task in enumerate(tasks)])}

## Definition of Done

- [ ] All tasks completed and tested
- [ ] Code review completed and approved
- [ ] Unit tests written with >80% coverage
- [ ] Integration tests passing
- [ ] Documentation updated
- [ ] Security review completed
- [ ] Performance requirements met
- [ ] User acceptance testing completed

## Dependencies

- Previous stories in epic completed
- Required infrastructure and services available
- Team capacity and expertise available

## Risks

- **Risk**: Technical complexity may cause delays
  - **Mitigation**: Break down into smaller tasks and seek expert guidance

---

**Story Owner**: Development Team Lead
**Stakeholders**: Product Owner, UX Designer, QA Engineer
**Estimated Completion**: {len(tasks) * 4} hours over {len(tasks)} days
"""

    with open(story_readme, "w") as f:
        f.write(story_content)

    # Create individual task files
    for task in tasks:
        create_task_file(story_folder, task)

    return story_folder


def generate_tasks_for_story(story_id, story_title):
    """Generate appropriate tasks for a story based on its type"""
    base_tasks = [
        {
            "id": f"{story_id.replace('STORY', 'TASK')}-01",
            "title": "Requirements Analysis and Design",
            "hours": 4,
        },
        {
            "id": f"{story_id.replace('STORY', 'TASK')}-02",
            "title": "Core Implementation",
            "hours": 8,
        },
        {
            "id": f"{story_id.replace('STORY', 'TASK')}-03",
            "title": "Testing and Validation",
            "hours": 4,
        },
        {
            "id": f"{story_id.replace('STORY', 'TASK')}-04",
            "title": "Integration and Documentation",
            "hours": 3,
        },
        {
            "id": f"{story_id.replace('STORY', 'TASK')}-05",
            "title": "Security and Performance Review",
            "hours": 3,
        },
        {
            "id": f"{story_id.replace('STORY', 'TASK')}-06",
            "title": "Deployment and Verification",
            "hours": 2,
        },
    ]

    # Add story-specific tasks based on content
    if "API" in story_title or "Backend" in story_title:
        base_tasks.append(
            {
                "id": f"{story_id.replace('STORY', 'TASK')}-07",
                "title": "API Documentation and Swagger",
                "hours": 2,
            }
        )

    if "UI" in story_title or "Interface" in story_title or "Mobile" in story_title:
        base_tasks.append(
            {
                "id": f"{story_id.replace('STORY', 'TASK')}-07",
                "title": "UI/UX Testing and Accessibility",
                "hours": 3,
            }
        )

    if "Database" in story_title or "Data" in story_title:
        base_tasks.append(
            {
                "id": f"{story_id.replace('STORY', 'TASK')}-07",
                "title": "Data Migration and Backup Strategy",
                "hours": 4,
            }
        )

    return base_tasks[:6]  # Keep to 6 tasks per story for consistency


def create_task_file(story_folder, task):
    """Create individual task markdown file"""
    task_filename = (
        f"{task['id']}-{task['title'].replace(' ', '-').replace('/', '-')}.md"
    )
    task_filepath = os.path.join(story_folder, task_filename)

    task_content = f"""# {task["id"]}: {task["title"]}

**Task ID**: {task["id"]}
**Story**: {os.path.basename(story_folder)}
**Estimated Hours**: {task["hours"]}
**Priority**: High
**Assignee**: Developer

## Task Description

{generate_task_description(task["title"])}

## Acceptance Criteria

{generate_task_acceptance_criteria(task["title"])}

## Technical Implementation

{generate_technical_implementation(task["title"])}

## Subtasks

{generate_subtasks(task["title"], task["hours"])}

## Definition of Done

- [ ] Implementation completed according to specifications
- [ ] Code reviewed and approved by team lead
- [ ] Unit tests written and passing
- [ ] Integration tests completed
- [ ] Documentation updated
- [ ] Performance validated
- [ ] Security review completed

## Dependencies

- Previous tasks in story completed
- Required tools and resources available
- Team member availability and expertise

## Resources

- Technical documentation and API references
- Design specifications and mockups
- Previous implementation examples

---

**Task Owner**: Developer
**Reviewer**: Senior Developer
**Due Date**: Sprint Day {task["hours"] // 4 + 1}
"""

    with open(task_filepath, "w") as f:
        f.write(task_content)


def generate_acceptance_criteria(story_title):
    """Generate acceptance criteria based on story title"""
    criteria = [
        "- [ ] Functionality works as specified",
        "- [ ] User interface is intuitive and responsive",
        "- [ ] Performance meets defined requirements",
        "- [ ] Security requirements are implemented",
        "- [ ] Error handling is comprehensive",
        "- [ ] Integration with other services works correctly",
    ]

    if "AI" in story_title:
        criteria.append("- [ ] AI responses are accurate and relevant")
    if "Mobile" in story_title:
        criteria.append("- [ ] Works on both iOS and Android platforms")
    if "Database" in story_title:
        criteria.append("- [ ] Data integrity and backup systems functional")

    return "\n".join(criteria)


def generate_technical_requirements(story_title):
    """Generate technical requirements based on story title"""
    return """### Core Requirements
- Implementation follows established architecture patterns
- Code quality meets team standards
- Security best practices implemented
- Performance optimization applied
- Documentation comprehensive and up-to-date

### Integration Requirements
- APIs properly designed and documented
- Database schema changes managed
- Service dependencies handled correctly
- Error handling and logging implemented"""


def generate_task_description(task_title):
    """Generate task description based on title"""
    descriptions = {
        "Requirements Analysis and Design": "Analyze requirements, create technical design, and plan implementation approach for the story deliverables.",
        "Core Implementation": "Implement the primary functionality and features as specified in the story requirements.",
        "Testing and Validation": "Create and execute comprehensive tests to validate functionality and ensure quality standards.",
        "Integration and Documentation": "Integrate with existing systems and create comprehensive documentation for the implementation.",
        "Security and Performance Review": "Conduct security assessment and performance optimization to meet production standards.",
        "Deployment and Verification": "Deploy to appropriate environments and verify successful implementation in production-like conditions.",
    }
    return descriptions.get(
        task_title,
        f"Complete the {task_title.lower()} task as specified in the story requirements.",
    )


def generate_task_acceptance_criteria(task_title):
    """Generate acceptance criteria for tasks"""
    return """- [ ] Task requirements fully implemented
- [ ] Code follows team coding standards
- [ ] All tests passing
- [ ] Documentation updated
- [ ] Code review completed
- [ ] Integration verified"""


def generate_technical_implementation(task_title):
    """Generate technical implementation section"""
    return """### Implementation Approach
- Follow established patterns and conventions
- Use appropriate frameworks and libraries
- Implement proper error handling
- Add comprehensive logging
- Ensure security best practices

### Code Structure
- Maintain clean, readable code
- Follow SOLID principles
- Implement proper separation of concerns
- Use dependency injection where appropriate"""


def generate_subtasks(task_title, hours):
    """Generate subtasks based on task title and hours"""
    num_subtasks = max(2, hours // 2)
    subtasks = []

    for i in range(num_subtasks):
        subtask_hours = hours // num_subtasks
        if i == 0:
            subtasks.append(
                f"### {i + 1}. Analysis and Planning ({subtask_hours} hour{'s' if subtask_hours > 1 else ''})"
            )
        elif i == num_subtasks - 1:
            subtasks.append(
                f"### {i + 1}. Testing and Documentation ({subtask_hours} hour{'s' if subtask_hours > 1 else ''})"
            )
        else:
            subtasks.append(
                f"### {i + 1}. Implementation Phase {i} ({subtask_hours} hour{'s' if subtask_hours > 1 else ''})"
            )

        subtasks.append("- Define specific requirements and approach")
        subtasks.append("- Implement functionality according to specifications")
        subtasks.append("- Test and validate implementation")
        subtasks.append("")

    return "\n".join(subtasks)


def main():
    """Main function to generate all EPIC/Story/Task structure"""
    print("Generating EPIC/Story/Task structure for Azure DevOps import...")

    # Create stories for EPICs 1-6 (EPICs 7-13 already exist)
    for epic_key, epic_data in EPICS_STRUCTURE.items():
        epic_folder = os.path.join(STORIES_BASE, epic_key)

        # Skip if epic folder already exists with content
        if os.path.exists(epic_folder) and len(os.listdir(epic_folder)) > 1:
            print(f"Skipping {epic_key} - already exists with content")
            continue

        print(f"Processing {epic_key}: {epic_data['title']}")

        for story_id in epic_data["stories"]:
            story_title = story_id.replace("STORY-", "").replace("-", " ").title()
            story_title = (
                story_title.replace("01 01", "")
                .replace("02 01", "")
                .replace("03 01", "")
                .replace("04 01", "")
                .replace("05 01", "")
                .replace("06 01", "")
            )  # Clean up numbering
            story_title = " ".join(story_title.split()[2:])  # Remove number prefix

            print(f"  Creating {story_id}: {story_title}")
            create_story_structure(epic_folder, story_id, story_title)

    print("\\nEPIC/Story/Task structure generation completed!")
    print("Structure is ready for Azure DevOps import using the import scripts.")


if __name__ == "__main__":
    main()
