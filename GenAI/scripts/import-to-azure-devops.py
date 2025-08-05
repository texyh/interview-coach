#!/usr/bin/env python3
"""
Azure DevOps Work Items Import Script
Imports EPICs and User Stories from markdown files to Azure DevOps
"""

import base64
import os
import re
from typing import Dict, Optional

import requests


class AzureDevOpsImporter:
    def __init__(self, organization: str, project: str, personal_access_token: str):
        """
        Initialize Azure DevOps importer

        Args:
            organization: Your Azure DevOps organization name
            project: Your project name
            personal_access_token: Your PAT token with work items write permissions
        """
        self.organization = organization
        self.project = project
        self.base_url = f"https://dev.azure.com/{organization}/{project}/_apis"

        # Create authorization header
        credentials = base64.b64encode(f":{personal_access_token}".encode()).decode()
        self.headers = {
            "Authorization": f"Basic {credentials}",
            "Content-Type": "application/json-patch+json",
        }

    def create_epic(self, epic_data: Dict) -> Optional[int]:
        """Create an Epic work item in Azure DevOps"""

        # Epic work item structure
        work_item = [
            {"op": "add", "path": "/fields/System.Title", "value": epic_data["title"]},
            {
                "op": "add",
                "path": "/fields/System.Description",
                "value": epic_data["description"],
            },
            {"op": "add", "path": "/fields/System.WorkItemType", "value": "Epic"},
            {
                "op": "add",
                "path": "/fields/Microsoft.VSTS.Common.Priority",
                "value": epic_data["priority"],
            },
            {
                "op": "add",
                "path": "/fields/System.Tags",
                "value": epic_data.get("tags", ""),
            },
        ]

        # Add custom fields if needed
        if epic_data.get("business_value"):
            work_item.append(
                {
                    "op": "add",
                    "path": "/fields/Microsoft.VSTS.Common.BusinessValue",
                    "value": epic_data["business_value"],
                }
            )

        if epic_data.get("acceptance_criteria"):
            work_item.append(
                {
                    "op": "add",
                    "path": "/fields/Microsoft.VSTS.Common.AcceptanceCriteria",
                    "value": epic_data["acceptance_criteria"],
                }
            )

        url = f"{self.base_url}/wit/workitems/$Epic?api-version=7.1-preview.3"

        try:
            response = requests.post(url, headers=self.headers, json=work_item)
            response.raise_for_status()

            epic_id = response.json()["id"]
            print(f"✅ Created Epic: {epic_data['title']} (ID: {epic_id})")
            return epic_id

        except requests.exceptions.RequestException as e:
            print(f"❌ Failed to create Epic {epic_data['title']}: {e}")
            return None

    def create_user_story(
        self, story_data: Dict, epic_id: Optional[int] = None
    ) -> Optional[int]:
        """Create a User Story work item in Azure DevOps"""

        work_item = [
            {"op": "add", "path": "/fields/System.Title", "value": story_data["title"]},
            {
                "op": "add",
                "path": "/fields/System.Description",
                "value": story_data["description"],
            },
            {"op": "add", "path": "/fields/System.WorkItemType", "value": "User Story"},
            {
                "op": "add",
                "path": "/fields/Microsoft.VSTS.Scheduling.StoryPoints",
                "value": story_data.get("story_points", 0),
            },
            {
                "op": "add",
                "path": "/fields/Microsoft.VSTS.Common.Priority",
                "value": story_data.get("priority", 2),
            },
            {
                "op": "add",
                "path": "/fields/System.Tags",
                "value": story_data.get("tags", ""),
            },
        ]

        # Add acceptance criteria
        if story_data.get("acceptance_criteria"):
            work_item.append(
                {
                    "op": "add",
                    "path": "/fields/Microsoft.VSTS.Common.AcceptanceCriteria",
                    "value": story_data["acceptance_criteria"],
                }
            )

        url = f"{self.base_url}/wit/workitems/$User Story?api-version=7.1-preview.3"

        try:
            response = requests.post(url, headers=self.headers, json=work_item)
            response.raise_for_status()

            story_id = response.json()["id"]
            print(f"  ✅ Created User Story: {story_data['title']} (ID: {story_id})")

            # Link to Epic if provided
            if epic_id:
                self.link_story_to_epic(story_id, epic_id)

            return story_id

        except requests.exceptions.RequestException as e:
            print(f"  ❌ Failed to create User Story {story_data['title']}: {e}")
            return None

    def link_story_to_epic(self, story_id: int, epic_id: int):
        """Create a parent-child link between Epic and User Story"""

        link_data = [
            {
                "op": "add",
                "path": "/relations/-",
                "value": {
                    "rel": "System.LinkTypes.Hierarchy-Reverse",
                    "url": f"{self.base_url}/wit/workItems/{epic_id}",
                },
            }
        ]

        url = f"{self.base_url}/wit/workitems/{story_id}?api-version=7.1-preview.3"

        try:
            response = requests.patch(url, headers=self.headers, json=link_data)
            response.raise_for_status()
            print(f"    🔗 Linked Story {story_id} to Epic {epic_id}")

        except requests.exceptions.RequestException as e:
            print(f"    ❌ Failed to link Story {story_id} to Epic {epic_id}: {e}")


def parse_epic_markdown(file_path: str) -> Dict:
    """Parse Epic markdown file and extract structured data"""

    with open(file_path, "r", encoding="utf-8") as file:
        content = file.read()

    # Extract Epic metadata
    title_match = re.search(r"^# (.+)$", content, re.MULTILINE)
    epic_id_match = re.search(r"\*\*EPIC ID\*\*:\s*(.+)$", content, re.MULTILINE)
    priority_match = re.search(r"\*\*Priority\*\*:\s*(.+)$", content, re.MULTILINE)
    phase_match = re.search(r"\*\*Phase\*\*:\s*(.+)$", content, re.MULTILINE)
    business_value_match = re.search(
        r"\*\*Business Value\*\*:\s*(.+)$", content, re.MULTILINE
    )
    story_points_match = re.search(
        r"\*\*Estimated Story Points\*\*:\s*(.+)$", content, re.MULTILINE
    )

    # Extract Epic description
    desc_match = re.search(r"## EPIC Description\n(.+?)(?=\n##)", content, re.DOTALL)

    # Extract acceptance criteria
    acceptance_match = re.search(
        r"## Acceptance Criteria\n(.+?)(?=\n##)", content, re.DOTALL
    )

    epic_data = {
        "title": title_match.group(1).strip() if title_match else "Unknown Epic",
        "epic_id": epic_id_match.group(1).strip() if epic_id_match else "",
        "priority": map_priority(
            priority_match.group(1).strip() if priority_match else "Medium"
        ),
        "phase": phase_match.group(1).strip() if phase_match else "",
        "business_value": business_value_match.group(1).strip()
        if business_value_match
        else "",
        "description": desc_match.group(1).strip() if desc_match else "",
        "acceptance_criteria": acceptance_match.group(1).strip()
        if acceptance_match
        else "",
        "tags": f"Epic;{epic_id_match.group(1).strip() if epic_id_match else ''}",
    }

    # Extract User Stories
    stories = []
    story_pattern = r"### (Story \d+\.\d+:.+?)\n\*\*Story ID\*\*:\s*(.+?)\n\*\*Story Points\*\*:\s*(\d+)\n\*\*Priority\*\*:\s*(.+?)\n\*\*Sprint\*\*:\s*(.+?)\n\n\*\*As a\*\* (.+?)\n\*\*I want to\*\* (.+?)\n\*\*So that\*\* (.+?)\n\n#### Acceptance Criteria\n(.+?)(?=#### Technical Tasks|\n---|\n### |$)"

    for match in re.finditer(story_pattern, content, re.DOTALL):
        story_data = {
            "title": match.group(1).strip(),
            "story_id": match.group(2).strip(),
            "story_points": int(match.group(3)),
            "priority": map_priority(match.group(4).strip()),
            "sprint": match.group(5).strip(),
            "user_role": match.group(6).strip(),
            "user_want": match.group(7).strip(),
            "user_so_that": match.group(8).strip(),
            "acceptance_criteria": match.group(9).strip(),
            "description": f"As a {match.group(6).strip()}, I want to {match.group(7).strip()} so that {match.group(8).strip()}.",
            "tags": f"UserStory;{match.group(2).strip()};{match.group(5).strip()}",
        }
        stories.append(story_data)

    return epic_data, stories


def map_priority(priority_text: str) -> int:
    """Map text priority to Azure DevOps priority number"""
    priority_map = {"Critical": 1, "High": 2, "Medium": 3, "Low": 4}
    return priority_map.get(priority_text, 3)


def main():
    """Main import function"""

    # Configuration - UPDATE THESE VALUES
    ORGANIZATION = "your-organization"  # Replace with your Azure DevOps organization
    PROJECT = "your-project"  # Replace with your project name
    PAT_TOKEN = "your-pat-token"  # Replace with your Personal Access Token

    # Directory containing Epic markdown files
    EPICS_DIRECTORY = "/Users/texy/Documents/Work/interview-preper/GenAI/stories"

    # Initialize importer
    importer = AzureDevOpsImporter(ORGANIZATION, PROJECT, PAT_TOKEN)

    # Get all Epic files
    epic_files = [
        f
        for f in os.listdir(EPICS_DIRECTORY)
        if f.startswith("EPIC-") and f.endswith(".md")
    ]
    epic_files.sort()  # Process in order

    print(f"🚀 Starting import of {len(epic_files)} EPICs to Azure DevOps")
    print(f"Organization: {ORGANIZATION}")
    print(f"Project: {PROJECT}")
    print("-" * 60)

    for epic_file in epic_files:
        if epic_file == "000-EPIC-Overview-Roadmap.md":
            continue  # Skip overview file

        file_path = os.path.join(EPICS_DIRECTORY, epic_file)
        print(f"\n📁 Processing {epic_file}")

        try:
            # Parse Epic and Stories
            epic_data, stories = parse_epic_markdown(file_path)

            # Create Epic
            epic_id = importer.create_epic(epic_data)

            if epic_id:
                # Create User Stories
                for story in stories:
                    importer.create_user_story(story, epic_id)

        except Exception as e:
            print(f"❌ Error processing {epic_file}: {e}")

    print("\n" + "=" * 60)
    print("✅ Import completed!")
    print("\nNext steps:")
    print("1. Review work items in Azure DevOps")
    print("2. Assign team members to stories")
    print("3. Set up sprints and iterations")
    print("4. Configure work item templates if needed")


if __name__ == "__main__":
    main()
