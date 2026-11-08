#!/usr/bin/env python3
"""
Simplified Azure DevOps Importer
Quick setup version for importing EPICs and User Stories
"""

import base64
import os

import requests
from dotenv import load_dotenv

# Load environment variables
load_dotenv()

# Configuration (Update these or use .env file)
ORGANIZATION = os.getenv("AZURE_DEVOPS_ORGANIZATION", "Onwuzulike-Emeka")
PROJECT = os.getenv("AZURE_DEVOPS_PROJECT", "Interview-Prep")
PAT_TOKEN = os.getenv("AZURE_DEVOPS_PAT_TOKEN", "")


def create_work_item(
    title,
    work_item_type,
    description="",
    story_points=0,
    priority=2,
    acceptance_criteria="",
    tags="",
):
    """Create a work item in Azure DevOps"""

    # Base URL for Azure DevOps REST API
    base_url = f"https://dev.azure.com/{ORGANIZATION}/{PROJECT}/_apis"

    # Authorization header
    credentials = base64.b64encode(f":{PAT_TOKEN}".encode()).decode()
    headers = {
        "Authorization": f"Basic {credentials}",
        "Content-Type": "application/json-patch+json",
    }

    # Work item data
    work_item_data = [
        {"op": "add", "path": "/fields/System.Title", "value": title},
        {"op": "add", "path": "/fields/System.Description", "value": description},
        {
            "op": "add",
            "path": "/fields/Microsoft.VSTS.Common.Priority",
            "value": priority,
        },
        {"op": "add", "path": "/fields/System.Tags", "value": tags},
    ]

    # Add story points for User Stories
    if work_item_type == "User Story" and story_points > 0:
        work_item_data.append(
            {
                "op": "add",
                "path": "/fields/Microsoft.VSTS.Scheduling.StoryPoints",
                "value": story_points,
            }
        )

    # Add acceptance criteria if provided
    if acceptance_criteria:
        work_item_data.append(
            {
                "op": "add",
                "path": "/fields/Microsoft.VSTS.Common.AcceptanceCriteria",
                "value": acceptance_criteria,
            }
        )

    # API endpoint
    url = f"{base_url}/wit/workitems/${work_item_type}?api-version=7.1-preview.3"

    try:
        response = requests.post(url, headers=headers, json=work_item_data)
        response.raise_for_status()

        work_item_id = response.json()["id"]
        print(f"✅ Created {work_item_type}: {title} (ID: {work_item_id})")
        return work_item_id

    except requests.exceptions.RequestException as e:
        print(f"❌ Failed to create {work_item_type}: {title}")
        print(f"   Error: {e}")
        if hasattr(e, "response") and e.response:
            print(f"   Response: {e.response.text}")
        return None


def link_work_items(child_id, parent_id):
    """Link a child work item to a parent work item"""

    base_url = f"https://dev.azure.com/{ORGANIZATION}/{PROJECT}/_apis"
    credentials = base64.b64encode(f":{PAT_TOKEN}".encode()).decode()
    headers = {
        "Authorization": f"Basic {credentials}",
        "Content-Type": "application/json-patch+json",
    }

    link_data = [
        {
            "op": "add",
            "path": "/relations/-",
            "value": {
                "rel": "System.LinkTypes.Hierarchy-Reverse",
                "url": f"{base_url}/wit/workItems/{parent_id}",
            },
        }
    ]

    url = f"{base_url}/wit/workitems/{child_id}?api-version=7.1-preview.3"

    try:
        response = requests.patch(url, headers=headers, json=link_data)
        response.raise_for_status()
        print(f"  🔗 Linked story {child_id} to epic {parent_id}")

    except requests.exceptions.RequestException as e:
        print(f"  ❌ Failed to link {child_id} to {parent_id}: {e}")


def map_priority(priority_text):
    """Convert text priority to Azure DevOps priority number"""
    priority_map = {"Critical": 1, "High": 2, "Medium": 3, "Low": 4}
    return priority_map.get(priority_text, 3)


def main():
    """Import EPICs and User Stories"""

    print("🚀 Azure DevOps Import Starting...")
    print(f"Organization: {ORGANIZATION}")
    print(f"Project: {PROJECT}")
    print("-" * 50)

    # Epic definitions with their stories
    epics = [
        {
            "title": "EPIC 01: Foundation Infrastructure",
            "description": "Establish core Azure infrastructure, CI/CD pipelines, authentication services, and foundational architecture to enable all subsequent development phases.",
            "priority": 1,
            "stories": [
                {
                    "title": "Story 1.1: Azure Infrastructure Setup",
                    "points": 21,
                    "priority": 1,
                },
                {
                    "title": "Story 1.2: CI/CD Pipeline Implementation",
                    "points": 13,
                    "priority": 1,
                },
                {
                    "title": "Story 1.3: Authentication Service Development",
                    "points": 21,
                    "priority": 1,
                },
                {
                    "title": "Story 1.4: User Management System",
                    "points": 13,
                    "priority": 1,
                },
                {
                    "title": "Story 1.5: API Gateway and Security",
                    "points": 13,
                    "priority": 2,
                },
                {
                    "title": "Story 1.6: Monitoring and Logging Infrastructure",
                    "points": 8,
                    "priority": 2,
                },
                {
                    "title": "Story 1.7: Database Design and Implementation",
                    "points": 13,
                    "priority": 1,
                },
                {
                    "title": "Story 1.8: Email and Notification Services",
                    "points": 8,
                    "priority": 2,
                },
                {
                    "title": "Story 1.9: Development Environment Setup",
                    "points": 8,
                    "priority": 2,
                },
            ],
        },
        {
            "title": "EPIC 02: Content Management & Question Bank System",
            "description": "Implement comprehensive content management system for interview questions, educational resources, and expert contributions with multi-language support.",
            "priority": 1,
            "stories": [
                {
                    "title": "Story 2.1: Question Bank Data Model and Architecture",
                    "points": 13,
                    "priority": 1,
                },
                {
                    "title": "Story 2.2: Content Management Service Implementation",
                    "points": 21,
                    "priority": 1,
                },
                {
                    "title": "Story 2.3: Search and Discovery Engine",
                    "points": 13,
                    "priority": 2,
                },
                {
                    "title": "Story 2.4: Multi-Language Content Support",
                    "points": 21,
                    "priority": 2,
                },
                {
                    "title": "Story 2.5: Content Contributor Platform",
                    "points": 21,
                    "priority": 3,
                },
                {
                    "title": "Story 2.6: Educational Resource Management",
                    "points": 13,
                    "priority": 3,
                },
                {
                    "title": "Story 2.7: Content Quality Assurance System",
                    "points": 13,
                    "priority": 2,
                },
                {
                    "title": "Story 2.8: Content Analytics and Insights",
                    "points": 8,
                    "priority": 3,
                },
                {
                    "title": "Story 2.9: Content Administration Interface",
                    "points": 13,
                    "priority": 3,
                },
            ],
        },
        {
            "title": "EPIC 03: AI-Powered Mock Interview System",
            "description": "Develop core AI-powered mock interview system with text, voice, and video interactions, intelligent question generation, and real-time conversation management.",
            "priority": 1,
            "stories": [
                {
                    "title": "Story 3.1: Interview Simulation Service Architecture",
                    "points": 13,
                    "priority": 1,
                },
                {
                    "title": "Story 3.2: Text-Based Mock Interview Engine",
                    "points": 21,
                    "priority": 1,
                },
                {
                    "title": "Story 3.3: OpenAI Integration for Intelligent Responses",
                    "points": 21,
                    "priority": 1,
                },
                {
                    "title": "Story 3.4: Azure Speech Services Integration",
                    "points": 21,
                    "priority": 2,
                },
                {
                    "title": "Story 3.5: Video Interview Capabilities",
                    "points": 21,
                    "priority": 3,
                },
                {
                    "title": "Story 3.6: Industry-Specific Interview Templates",
                    "points": 13,
                    "priority": 2,
                },
                {
                    "title": "Story 3.7: Real-Time Interview State Management",
                    "points": 13,
                    "priority": 2,
                },
                {
                    "title": "Story 3.8: Interview Recording and Playback System",
                    "points": 13,
                    "priority": 3,
                },
                {
                    "title": "Story 3.9: Interview Analytics and Metrics",
                    "points": 8,
                    "priority": 3,
                },
            ],
        },
        {
            "title": "EPIC 04: Mobile Application Development",
            "description": "Develop native mobile applications for iOS and Android using Flutter with full feature parity, offline capabilities, and mobile-specific features.",
            "priority": 2,
            "stories": [
                {
                    "title": "Story 4.1: Flutter Application Architecture Setup",
                    "points": 13,
                    "priority": 1,
                },
                {
                    "title": "Story 4.2: User Authentication and Profile Management",
                    "points": 13,
                    "priority": 1,
                },
                {
                    "title": "Story 4.3: Offline Data Synchronization System",
                    "points": 21,
                    "priority": 2,
                },
                {
                    "title": "Story 4.4: Mobile-Optimized Interview Experience",
                    "points": 21,
                    "priority": 2,
                },
                {
                    "title": "Story 4.5: Push Notifications System",
                    "points": 13,
                    "priority": 3,
                },
                {
                    "title": "Story 4.6: Mobile Performance Optimization",
                    "points": 13,
                    "priority": 3,
                },
                {
                    "title": "Story 4.7: Device Integration Features",
                    "points": 13,
                    "priority": 3,
                },
                {
                    "title": "Story 4.8: Cross-Platform Data Synchronization",
                    "points": 13,
                    "priority": 2,
                },
                {
                    "title": "Story 4.9: App Store Deployment and Distribution",
                    "points": 8,
                    "priority": 3,
                },
            ],
        },
        {
            "title": "EPIC 05: Analytics and Business Intelligence",
            "description": "Build comprehensive analytics and business intelligence capabilities for user performance tracking, predictive insights, and data-driven business decisions.",
            "priority": 2,
            "stories": [
                {
                    "title": "Story 5.1: Analytics Data Model and Infrastructure",
                    "points": 21,
                    "priority": 1,
                },
                {
                    "title": "Story 5.2: User Performance Analytics Dashboard",
                    "points": 21,
                    "priority": 2,
                },
                {
                    "title": "Story 5.3: Predictive Analytics Engine",
                    "points": 21,
                    "priority": 2,
                },
                {
                    "title": "Story 5.4: Business Intelligence Dashboards",
                    "points": 13,
                    "priority": 3,
                },
                {
                    "title": "Story 5.5: Real-Time Analytics and Monitoring",
                    "points": 13,
                    "priority": 3,
                },
                {
                    "title": "Story 5.6: Personalized Insights and Recommendations",
                    "points": 13,
                    "priority": 2,
                },
                {
                    "title": "Story 5.7: Advanced Reporting System",
                    "points": 8,
                    "priority": 3,
                },
                {
                    "title": "Story 5.8: Data Privacy and Compliance Analytics",
                    "points": 8,
                    "priority": 3,
                },
                {
                    "title": "Story 5.9: Analytics API and Third-Party Integrations",
                    "points": 8,
                    "priority": 4,
                },
            ],
        },
        {
            "title": "EPIC 06: Third-Party Integrations and External Services",
            "description": "Develop comprehensive third-party integrations with LinkedIn, job boards, calendar systems, and payment gateways to enhance user experience.",
            "priority": 3,
            "stories": [
                {
                    "title": "Story 6.1: LinkedIn Integration Architecture",
                    "points": 13,
                    "priority": 2,
                },
                {
                    "title": "Story 6.2: Calendar Integration System",
                    "points": 13,
                    "priority": 3,
                },
                {
                    "title": "Story 6.3: Job Board Integration Platform",
                    "points": 21,
                    "priority": 2,
                },
                {
                    "title": "Story 6.4: HR System Connectors for Enterprise",
                    "points": 21,
                    "priority": 3,
                },
                {
                    "title": "Story 6.5: Social Media and Professional Networking",
                    "points": 13,
                    "priority": 4,
                },
                {
                    "title": "Story 6.6: Email Service Integration Hub",
                    "points": 8,
                    "priority": 3,
                },
                {
                    "title": "Story 6.7: Payment Gateway Integrations",
                    "points": 13,
                    "priority": 2,
                },
                {
                    "title": "Story 6.8: Communication Platform Integrations",
                    "points": 8,
                    "priority": 4,
                },
                {
                    "title": "Story 6.9: Integration Management and Monitoring",
                    "points": 8,
                    "priority": 3,
                },
            ],
        },
        {
            "title": "EPIC 07: Enterprise Features and Team Management",
            "description": "Develop enterprise features including team management, SSO, custom branding, and administrative controls for large organizations.",
            "priority": 3,
            "stories": [
                {
                    "title": "Story 7.1: Multi-Tenant Team Management Architecture",
                    "points": 21,
                    "priority": 1,
                },
                {
                    "title": "Story 7.2: Enterprise Single Sign-On (SSO) Integration",
                    "points": 21,
                    "priority": 1,
                },
                {
                    "title": "Story 7.3: Role-Based Access Control (RBAC) System",
                    "points": 13,
                    "priority": 2,
                },
                {
                    "title": "Story 7.4: Team Performance Dashboard and Reporting",
                    "points": 21,
                    "priority": 2,
                },
                {
                    "title": "Story 7.5: Custom Branding and White-Label Solution",
                    "points": 13,
                    "priority": 3,
                },
                {
                    "title": "Story 7.6: Advanced User Management and Provisioning",
                    "points": 13,
                    "priority": 3,
                },
                {
                    "title": "Story 7.7: Enterprise Security and Compliance Features",
                    "points": 13,
                    "priority": 2,
                },
                {
                    "title": "Story 7.8: Enterprise Integration and API Management",
                    "points": 13,
                    "priority": 3,
                },
                {
                    "title": "Story 7.9: Enterprise Support and Training System",
                    "points": 8,
                    "priority": 4,
                },
            ],
        },
        {
            "title": "EPIC 08: Global Expansion and Localization",
            "description": "Enable global expansion through localization, multi-language support, regional compliance, and multi-region deployment capabilities.",
            "priority": 4,
            "stories": [
                {
                    "title": "Story 8.1: Internationalization (i18n) Infrastructure",
                    "points": 21,
                    "priority": 1,
                },
                {
                    "title": "Story 8.2: Content Translation and Management System",
                    "points": 13,
                    "priority": 2,
                },
                {
                    "title": "Story 8.3: Regional Compliance and Legal Framework",
                    "points": 13,
                    "priority": 2,
                },
                {
                    "title": "Story 8.4: Multi-Language Interview Content Localization",
                    "points": 21,
                    "priority": 2,
                },
                {
                    "title": "Story 8.5: Multi-Region Infrastructure Deployment",
                    "points": 21,
                    "priority": 3,
                },
                {
                    "title": "Story 8.6: Local Payment Methods and Currency Support",
                    "points": 13,
                    "priority": 3,
                },
                {
                    "title": "Story 8.7: Cultural Adaptation and User Experience",
                    "points": 8,
                    "priority": 3,
                },
                {
                    "title": "Story 8.8: Global Support and Customer Success",
                    "points": 8,
                    "priority": 4,
                },
                {
                    "title": "Story 8.9: Global Launch and Market Entry Strategy",
                    "points": 8,
                    "priority": 4,
                },
            ],
        },
    ]

    # Create EPICs and User Stories
    total_epics = len(epics)
    total_stories = sum(len(epic["stories"]) for epic in epics)

    print(f"📊 Will create {total_epics} EPICs and {total_stories} User Stories")
    print("-" * 50)

    for i, epic in enumerate(epics, 1):
        print(f"\n📋 Creating Epic {i}/{total_epics}: {epic['title']}")

        # Create Epic
        epic_id = create_work_item(
            title=epic["title"],
            work_item_type="Epic",
            description=epic["description"],
            priority=epic["priority"],
            tags=f"Epic;Phase-{i}",
        )

        if epic_id:
            # Create User Stories for this Epic
            for j, story in enumerate(epic["stories"], 1):
                print(f"  Creating Story {j}/{len(epic['stories'])}: {story['title']}")

                story_id = create_work_item(
                    title=story["title"],
                    work_item_type="User Story",
                    description=f"User story for {epic['title']}",
                    story_points=story["points"],
                    priority=story["priority"],
                    tags=f"UserStory;Epic-{i:02d}",
                )

                if story_id and epic_id:
                    link_work_items(story_id, epic_id)

    print("\n" + "=" * 50)
    print("✅ Import completed successfully!")
    print(f"Created {total_epics} EPICs and {total_stories} User Stories")
    print("\n🎯 Next steps:")
    print("1. Review work items in Azure DevOps")
    print("2. Set up sprints and assign stories")
    print("3. Configure team assignments")
    print("4. Set up Kanban boards")


if __name__ == "__main__":
    # Validate configuration
    if (
        ORGANIZATION == "your-organization"
        or PROJECT == "your-project"
        or PAT_TOKEN == "your-pat-token"
    ):
        print("❌ Please update the configuration variables:")
        print("   - ORGANIZATION")
        print("   - PROJECT")
        print("   - PAT_TOKEN")
        print("\nOr create a .env file with your values.")
        exit(1)

    main()
