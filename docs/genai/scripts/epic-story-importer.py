#!/usr/bin/env python3
"""
Azure DevOps Epic and Story Importer
Parses all EPIC markdown files and imports EPICs and User Stories with full details
"""

import base64
import os
import re
from typing import Dict, List, Optional, Tuple

import requests


class AzureDevOpsImporter:
    def __init__(self, organization: str, project: str, pat_token: str):
        self.organization = organization
        self.project = project
        self.base_url = f"https://dev.azure.com/{organization}/{project}/_apis"

        # Create authorization header
        credentials = base64.b64encode(f":{pat_token}".encode()).decode()
        self.headers = {
            "Authorization": f"Basic {credentials}",
            "Content-Type": "application/json-patch+json",
        }

    def create_work_item(
        self,
        work_item_type: str,
        title: str,
        description: str = "",
        story_points: int = 0,
        priority: int = 2,
        acceptance_criteria: str = "",
        tags: str = "",
        area_path: str = "",
        iteration_path: str = "",
    ) -> Optional[int]:
        """Create a work item in Azure DevOps"""

        work_item_data = [
            {"op": "add", "path": "/fields/System.Title", "value": title},
            {"op": "add", "path": "/fields/System.Description", "value": description},
            {
                "op": "add",
                "path": "/fields/Microsoft.VSTS.Common.Priority",
                "value": priority,
            },
        ]

        # Add optional fields
        if tags:
            work_item_data.append(
                {"op": "add", "path": "/fields/System.Tags", "value": tags}
            )

        if area_path:
            work_item_data.append(
                {"op": "add", "path": "/fields/System.AreaPath", "value": area_path}
            )

        if iteration_path:
            work_item_data.append(
                {
                    "op": "add",
                    "path": "/fields/System.IterationPath",
                    "value": iteration_path,
                }
            )

        # Add story points for User Stories
        if work_item_type == "User Story" and story_points > 0:
            work_item_data.append(
                {
                    "op": "add",
                    "path": "/fields/Microsoft.VSTS.Scheduling.StoryPoints",
                    "value": story_points,
                }
            )

        # Add acceptance criteria
        if acceptance_criteria:
            work_item_data.append(
                {
                    "op": "add",
                    "path": "/fields/Microsoft.VSTS.Common.AcceptanceCriteria",
                    "value": acceptance_criteria,
                }
            )

        url = (
            f"{self.base_url}/wit/workitems/${work_item_type}?api-version=7.1-preview.3"
        )

        try:
            response = requests.post(url, headers=self.headers, json=work_item_data)
            response.raise_for_status()

            work_item_id = response.json()["id"]
            print(f"✅ Created {work_item_type}: {title} (ID: {work_item_id})")
            return work_item_id

        except requests.exceptions.RequestException as e:
            print(f"❌ Failed to create {work_item_type}: {title}")
            print(f"   Error: {e}")
            if hasattr(e, "response") and e.response and e.response.text:
                print(f"   Response: {e.response.text}")
            return None

    def link_work_items(self, child_id: int, parent_id: int) -> bool:
        """Link a child work item to a parent work item"""

        link_data = [
            {
                "op": "add",
                "path": "/relations/-",
                "value": {
                    "rel": "System.LinkTypes.Hierarchy-Reverse",
                    "url": f"{self.base_url}/wit/workItems/{parent_id}",
                },
            }
        ]

        url = f"{self.base_url}/wit/workitems/{child_id}?api-version=7.1-preview.3"

        try:
            response = requests.patch(url, headers=self.headers, json=link_data)
            response.raise_for_status()
            print(f"  🔗 Linked Story {child_id} to Epic {parent_id}")
            return True

        except requests.exceptions.RequestException as e:
            print(f"  ❌ Failed to link {child_id} to {parent_id}: {e}")
            return False


class EpicParser:
    @staticmethod
    def parse_epic_file(file_path: str) -> Tuple[Dict, List[Dict]]:
        """Parse an EPIC markdown file and extract epic and story data"""

        with open(file_path, "r", encoding="utf-8") as file:
            content = file.read()

        # Extract Epic metadata
        epic_data = EpicParser._extract_epic_metadata(content)

        # Extract User Stories
        stories = EpicParser._extract_user_stories(content)

        return epic_data, stories

    @staticmethod
    def _extract_epic_metadata(content: str) -> Dict:
        """Extract Epic metadata from markdown content"""

        # Extract basic Epic information
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
        desc_match = re.search(
            r"## EPIC Description\s*\n(.+?)(?=\n##)", content, re.DOTALL
        )

        # Extract business objectives
        objectives_match = re.search(
            r"## Business Objectives\s*\n(.+?)(?=\n##)", content, re.DOTALL
        )

        # Extract acceptance criteria
        acceptance_match = re.search(
            r"## Acceptance Criteria\s*\n(.+?)(?=\n##)", content, re.DOTALL
        )

        epic_data = {
            "title": title_match.group(1).strip() if title_match else "Unknown Epic",
            "epic_id": epic_id_match.group(1).strip() if epic_id_match else "",
            "priority": EpicParser._map_priority(
                priority_match.group(1).strip() if priority_match else "Medium"
            ),
            "phase": phase_match.group(1).strip() if phase_match else "",
            "business_value": business_value_match.group(1).strip()
            if business_value_match
            else "",
            "story_points": story_points_match.group(1).strip()
            if story_points_match
            else "",
            "description": EpicParser._clean_text(
                desc_match.group(1).strip() if desc_match else ""
            ),
            "business_objectives": EpicParser._clean_text(
                objectives_match.group(1).strip() if objectives_match else ""
            ),
            "acceptance_criteria": EpicParser._clean_text(
                acceptance_match.group(1).strip() if acceptance_match else ""
            ),
            "tags": f"Epic;{epic_id_match.group(1).strip() if epic_id_match else ''}",
        }

        return epic_data

    @staticmethod
    def _extract_user_stories(content: str) -> List[Dict]:
        """Extract User Stories from markdown content"""

        stories = []

        # Pattern to match complete user stories
        story_pattern = r"### (Story \d+\.\d+:.+?)\n\*\*Story ID\*\*:\s*(.+?)\n\*\*Story Points\*\*:\s*(\d+)\n\*\*Priority\*\*:\s*(.+?)\n\*\*Sprint\*\*:\s*(.+?)\n\n\*\*As a\*\* (.+?)\n\*\*I want to\*\* (.+?)\n\*\*So that\*\* (.+?)\n\n#### Acceptance Criteria\s*\n(.+?)(?=#### Technical Tasks|\n---|\n### |$)"

        for match in re.finditer(story_pattern, content, re.DOTALL):
            # Extract technical tasks if they exist
            technical_tasks = ""
            story_end = match.end()
            remaining_content = content[story_end:]

            # Look for technical tasks section
            tech_tasks_match = re.search(
                r"#### Technical Tasks\s*\n(.+?)(?=\n---|\n### |$)",
                remaining_content,
                re.DOTALL,
            )
            if tech_tasks_match:
                technical_tasks = EpicParser._clean_text(
                    tech_tasks_match.group(1).strip()
                )

            story_data = {
                "title": match.group(1).strip(),
                "story_id": match.group(2).strip(),
                "story_points": int(match.group(3)),
                "priority": EpicParser._map_priority(match.group(4).strip()),
                "sprint": match.group(5).strip(),
                "user_role": match.group(6).strip(),
                "user_want": match.group(7).strip(),
                "user_so_that": match.group(8).strip(),
                "acceptance_criteria": EpicParser._clean_text(match.group(9).strip()),
                "technical_tasks": technical_tasks,
                "description": f"**User Story**: As a {match.group(6).strip()}, I want to {match.group(7).strip()} so that {match.group(8).strip()}.\n\n**Technical Tasks**:\n{technical_tasks}"
                if technical_tasks
                else f"As a {match.group(6).strip()}, I want to {match.group(7).strip()} so that {match.group(8).strip()}.",
                "tags": f"UserStory;{match.group(2).strip()};{match.group(5).strip()}",
            }
            stories.append(story_data)

        return stories

    @staticmethod
    def _map_priority(priority_text: str) -> int:
        """Map text priority to Azure DevOps priority number"""
        priority_map = {"Critical": 1, "High": 2, "Medium": 3, "Low": 4}
        return priority_map.get(priority_text, 3)

    @staticmethod
    def _clean_text(text: str) -> str:
        """Clean markdown text for Azure DevOps"""
        if not text:
            return ""

        # Remove excessive whitespace and normalize line breaks
        text = re.sub(r"\n\s*\n\s*\n", "\n\n", text)
        text = re.sub(r"^\s+|\s+$", "", text, flags=re.MULTILINE)

        return text.strip()


def main():
    """Main import function"""

    # Configuration
    ORGANIZATION = "Onwuzulike-Emeka"
    PROJECT = "Interview-Prep"
    PAT_TOKEN = os.getenv(
        "AZURE_DEVOPS_PAT",
        "BfkZMi1BDZq6jNKskpSc1dALkEXZLRhxFcxnBAkA26AaJuPe4iT0JQQJ99BHACAAAAAAAAAAAAAGAZDO",
    )

    # Directory containing Epic markdown files
    STORIES_DIRECTORY = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "stories")

    print("🚀 Azure DevOps Epic and Story Import Starting...")
    print(f"Organization: {ORGANIZATION}")
    print(f"Project: {PROJECT}")
    print(f"Stories Directory: {STORIES_DIRECTORY}")
    print("=" * 60)

    # Initialize importer
    importer = AzureDevOpsImporter(ORGANIZATION, PROJECT, PAT_TOKEN)

    # Get all Epic files (skip overview file)
    epic_files = [
        f
        for f in os.listdir(STORIES_DIRECTORY)
        if f.startswith("EPIC-")
        and f.endswith(".md")
        and f != "000-EPIC-Overview-Roadmap.md"
    ]
    epic_files.sort()  # Process in order

    if not epic_files:
        print("❌ No EPIC files found in the stories directory!")
        return

    print(f"📋 Found {len(epic_files)} EPIC files to process:")
    for epic_file in epic_files:
        print(f"  - {epic_file}")
    print("-" * 60)

    total_epics_created = 0
    total_stories_created = 0

    # Process each Epic file
    for i, epic_file in enumerate(epic_files, 1):
        file_path = os.path.join(STORIES_DIRECTORY, epic_file)
        print(f"\n📁 Processing {i}/{len(epic_files)}: {epic_file}")

        try:
            # Parse Epic and Stories
            epic_data, stories = EpicParser.parse_epic_file(file_path)

            print(f"  Epic: {epic_data['title']}")
            print(f"  Stories: {len(stories)} user stories found")

            # Create Epic description combining multiple sections
            epic_description = f"""**EPIC ID**: {epic_data["epic_id"]}
**Phase**: {epic_data["phase"]}
**Business Value**: {epic_data["business_value"]}
**Story Points**: {epic_data["story_points"]}

**Description**:
{epic_data["description"]}

**Business Objectives**:
{epic_data["business_objectives"]}"""

            # Create Epic
            epic_id = importer.create_work_item(
                work_item_type="Epic",
                title=epic_data["title"],
                description=epic_description,
                priority=epic_data["priority"],
                acceptance_criteria=epic_data["acceptance_criteria"],
                tags=epic_data["tags"],
            )

            if epic_id:
                total_epics_created += 1

                # Create User Stories for this Epic
                for j, story in enumerate(stories, 1):
                    print(f"  Creating Story {j}/{len(stories)}: {story['title']}")

                    story_id = importer.create_work_item(
                        work_item_type="User Story",
                        title=story["title"],
                        description=story["description"],
                        story_points=story["story_points"],
                        priority=story["priority"],
                        acceptance_criteria=story["acceptance_criteria"],
                        tags=story["tags"],
                    )

                    if story_id:
                        total_stories_created += 1
                        # Link story to epic
                        importer.link_work_items(story_id, epic_id)

        except Exception as e:
            print(f"❌ Error processing {epic_file}: {e}")
            import traceback

            traceback.print_exc()

    print("\n" + "=" * 60)
    print("✅ Import completed!")
    print("📊 Summary:")
    print(f"  - EPICs created: {total_epics_created}/{len(epic_files)}")
    print(f"  - User Stories created: {total_stories_created}")
    print(f"  - Total work items: {total_epics_created + total_stories_created}")

    print("\n🎯 Next steps:")
    print("1. Review work items in Azure DevOps:")
    print(f"   https://dev.azure.com/{ORGANIZATION}/{PROJECT}/_workitems")
    print("2. Set up Area Paths and Iteration Paths")
    print("3. Assign work items to team members")
    print("4. Configure Kanban boards and sprint planning")
    print("5. Set up work item queries and dashboards")


if __name__ == "__main__":
    main()
