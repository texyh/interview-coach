#!/usr/bin/env python3
"""
EPIC and Story Organizer
Moves each EPIC into its own folder and extracts individual stories into separate files
"""

import os
import re
import shutil
from typing import Dict, List

class EpicOrganizer:
    def __init__(self, stories_directory: str):
        self.stories_directory = stories_directory
        self.epic_folders: List[str] = []
    
    def reorganize_epics(self):
        """Main function to reorganize EPICs and extract stories"""
        
        print("🚀 Starting EPIC and Story Organization...")
        print(f"Stories Directory: {self.stories_directory}")
        print("=" * 60)
        
        # Get all Epic files
        epic_files = [f for f in os.listdir(self.stories_directory) 
                      if f.startswith("EPIC-") and f.endswith(".md") and f != "000-EPIC-Overview-Roadmap.md"]
        epic_files.sort()
        
        if not epic_files:
            print("❌ No EPIC files found!")
            return
        
        print(f"📋 Found {len(epic_files)} EPIC files:")
        for epic_file in epic_files:
            print(f"  - {epic_file}")
        print("-" * 60)
        
        # Process each EPIC
        for i, epic_file in enumerate(epic_files, 1):
            print(f"\n📁 Processing {i}/{len(epic_files)}: {epic_file}")
            self.process_epic(epic_file)
        
        print("\n" + "=" * 60)
        print("✅ Organization completed!")
        print(f"📊 Created {len(self.epic_folders)} EPIC folders:")
        for folder in self.epic_folders:
            print(f"  - {folder}")
    
    def process_epic(self, epic_file: str):
        """Process a single EPIC file"""
        
        epic_path = os.path.join(self.stories_directory, epic_file)
        
        # Read the EPIC file
        with open(epic_path, 'r', encoding='utf-8') as file:
            content = file.read()
        
        # Extract EPIC metadata
        epic_data = self.extract_epic_metadata(content)
        epic_folder_name = self.create_epic_folder_name(epic_data['epic_id'], epic_data['title'])
        
        # Create EPIC folder
        epic_folder_path = os.path.join(self.stories_directory, epic_folder_name)
        if not os.path.exists(epic_folder_path):
            os.makedirs(epic_folder_path)
            print(f"  📂 Created folder: {epic_folder_name}")
        
        self.epic_folders.append(epic_folder_name)
        
        # Move EPIC file to its folder and rename it
        epic_readme_path = os.path.join(epic_folder_path, "README.md")
        shutil.copy2(epic_path, epic_readme_path)
        print(f"  📄 Moved EPIC file to: {epic_folder_name}/README.md")
        
        # Extract and save individual stories
        stories = self.extract_user_stories(content)
        print(f"  🔍 Found {len(stories)} user stories")
        
        for j, story in enumerate(stories, 1):
            story_file_name = self.create_story_file_name(story['story_id'], story['title'])
            story_file_path = os.path.join(epic_folder_path, story_file_name)
            
            # Create story markdown content
            story_content = self.create_story_markdown(story, epic_data)
            
            # Save story file
            with open(story_file_path, 'w', encoding='utf-8') as story_file:
                story_file.write(story_content)
            
            print(f"    📝 Created story: {story_file_name}")
        
        # Create EPIC summary file
        self.create_epic_summary(epic_folder_path, epic_data, stories)
        
        # Remove original EPIC file from root
        os.remove(epic_path)
        print(f"  🗑️  Removed original file: {epic_file}")
    
    def extract_epic_metadata(self, content: str) -> Dict:
        """Extract EPIC metadata from markdown content"""
        
        title_match = re.search(r'^# (.+)$', content, re.MULTILINE)
        epic_id_match = re.search(r'\*\*EPIC ID\*\*:\s*(.+)$', content, re.MULTILINE)
        priority_match = re.search(r'\*\*Priority\*\*:\s*(.+)$', content, re.MULTILINE)
        phase_match = re.search(r'\*\*Phase\*\*:\s*(.+)$', content, re.MULTILINE)
        business_value_match = re.search(r'\*\*Business Value\*\*:\s*(.+)$', content, re.MULTILINE)
        story_points_match = re.search(r'\*\*Estimated Story Points\*\*:\s*(.+)$', content, re.MULTILINE)
        
        # Extract sections
        desc_match = re.search(r'## EPIC Description\s*\n(.+?)(?=\n##)', content, re.DOTALL)
        objectives_match = re.search(r'## Business Objectives\s*\n(.+?)(?=\n##)', content, re.DOTALL)
        acceptance_match = re.search(r'## Acceptance Criteria\s*\n(.+?)(?=\n##)', content, re.DOTALL)
        dependencies_match = re.search(r'## Dependencies\s*\n(.+?)(?=\n---|\n##)', content, re.DOTALL)
        
        return {
            "title": title_match.group(1).strip() if title_match else "Unknown Epic",
            "epic_id": epic_id_match.group(1).strip() if epic_id_match else "",
            "priority": priority_match.group(1).strip() if priority_match else "Medium",
            "phase": phase_match.group(1).strip() if phase_match else "",
            "business_value": business_value_match.group(1).strip() if business_value_match else "",
            "story_points": story_points_match.group(1).strip() if story_points_match else "",
            "description": self.clean_text(desc_match.group(1).strip() if desc_match else ""),
            "business_objectives": self.clean_text(objectives_match.group(1).strip() if objectives_match else ""),
            "acceptance_criteria": self.clean_text(acceptance_match.group(1).strip() if acceptance_match else ""),
            "dependencies": self.clean_text(dependencies_match.group(1).strip() if dependencies_match else "")
        }
    
    def extract_user_stories(self, content: str) -> List[Dict]:
        """Extract User Stories from markdown content"""
        
        stories = []
        
        # Pattern to match complete user stories
        story_pattern = r'### (Story \d+\.\d+:.+?)\n\*\*Story ID\*\*:\s*(.+?)\n\*\*Story Points\*\*:\s*(\d+)\n\*\*Priority\*\*:\s*(.+?)\n\*\*Sprint\*\*:\s*(.+?)\n\n\*\*As a\*\* (.+?)\n\*\*I want to\*\* (.+?)\n\*\*So that\*\* (.+?)\n\n#### Acceptance Criteria\s*\n(.+?)(?=#### Technical Tasks|\n---|\n### |$)'
        
        for match in re.finditer(story_pattern, content, re.DOTALL):
            # Extract technical tasks if they exist
            technical_tasks = ""
            story_end = match.end()
            remaining_content = content[story_end:]
            
            # Look for technical tasks section
            tech_tasks_match = re.search(r'#### Technical Tasks\s*\n(.+?)(?=\n---|\n### |$)', remaining_content, re.DOTALL)
            if tech_tasks_match:
                technical_tasks = self.clean_text(tech_tasks_match.group(1).strip())
            
            story_data = {
                "title": match.group(1).strip(),
                "story_id": match.group(2).strip(),
                "story_points": int(match.group(3)),
                "priority": match.group(4).strip(),
                "sprint": match.group(5).strip(),
                "user_role": match.group(6).strip(),
                "user_want": match.group(7).strip(),
                "user_so_that": match.group(8).strip(),
                "acceptance_criteria": self.clean_text(match.group(9).strip()),
                "technical_tasks": technical_tasks
            }
            stories.append(story_data)
        
        return stories
    
    def create_epic_folder_name(self, epic_id: str, title: str) -> str:
        """Create a clean folder name for the EPIC"""
        
        # Extract EPIC number from ID
        epic_num_match = re.search(r'EPIC-(\d+)', epic_id)
        epic_num = epic_num_match.group(1) if epic_num_match else "00"
        
        # Clean title for folder name
        clean_title = re.sub(r'[^\w\s-]', '', title)
        clean_title = re.sub(r'[-\s]+', '-', clean_title)
        clean_title = clean_title.strip('-')
        
        return f"EPIC-{epic_num}-{clean_title}"
    
    def create_story_file_name(self, story_id: str, title: str) -> str:
        """Create a clean file name for the story"""
        
        # Extract story number from ID
        story_num_match = re.search(r'STORY-(\d+-\d+)', story_id)
        story_num = story_num_match.group(1) if story_num_match else "00-00"
        
        # Clean title for file name
        clean_title = re.sub(r'[^\w\s-]', '', title)
        clean_title = re.sub(r'[-\s]+', '-', clean_title)
        clean_title = clean_title.strip('-')
        
        # Limit length
        if len(clean_title) > 50:
            clean_title = clean_title[:50].rstrip('-')
        
        return f"STORY-{story_num}-{clean_title}.md"
    
    def create_story_markdown(self, story: Dict, epic_data: Dict) -> str:
        """Create markdown content for individual story"""
        
        content = f"""# {story['title']}

**Story ID**: {story['story_id']}  
**Epic**: {epic_data['epic_id']} - {epic_data['title']}  
**Story Points**: {story['story_points']}  
**Priority**: {story['priority']}  
**Sprint**: {story['sprint']}  

## User Story

**As a** {story['user_role']}  
**I want to** {story['user_want']}  
**So that** {story['user_so_that']}  

## Acceptance Criteria

{story['acceptance_criteria']}

## Technical Tasks

{story['technical_tasks'] if story['technical_tasks'] else "No technical tasks specified."}

---

## Metadata

- **Created**: Auto-generated from EPIC markdown
- **Epic Folder**: {epic_data['epic_id']}
- **Story File**: {self.create_story_file_name(story['story_id'], story['title'])}

## Links

- [Back to Epic](./README.md)
- [Epic Overview](../000-EPIC-Overview-Roadmap.md)
"""
        
        return content
    
    def create_epic_summary(self, epic_folder_path: str, epic_data: Dict, stories: List[Dict]):
        """Create a summary file for the EPIC"""
        
        summary_content = f"""# {epic_data['title']} - Summary

## Overview

**EPIC ID**: {epic_data['epic_id']}  
**Priority**: {epic_data['priority']}  
**Phase**: {epic_data['phase']}  
**Business Value**: {epic_data['business_value']}  
**Estimated Story Points**: {epic_data['story_points']}  

## Stories Summary

| Story ID | Title | Points | Priority | Sprint |
|----------|-------|--------|----------|--------|
"""
        
        for story in stories:
            story_file = self.create_story_file_name(story['story_id'], story['title'])
            summary_content += f"| {story['story_id']} | [{story['title']}](./{story_file}) | {story['story_points']} | {story['priority']} | {story['sprint']} |\n"
        
        total_points = sum(story['story_points'] for story in stories)
        
        summary_content += f"""
## Statistics

- **Total Stories**: {len(stories)}
- **Total Story Points**: {total_points}
- **Average Points per Story**: {total_points / len(stories):.1f}

## Quick Links

- [Full EPIC Details](./README.md)
- [Project Overview](../000-EPIC-Overview-Roadmap.md)

---

*Auto-generated summary - {len(stories)} stories extracted from EPIC markdown*
"""
        
        summary_path = os.path.join(epic_folder_path, "SUMMARY.md")
        with open(summary_path, 'w', encoding='utf-8') as file:
            file.write(summary_content)
        
        print("    📊 Created summary: SUMMARY.md")
    
    def clean_text(self, text: str) -> str:
        """Clean markdown text"""
        if not text:
            return ""
        
        # Remove excessive whitespace and normalize line breaks
        text = re.sub(r'\n\s*\n\s*\n', '\n\n', text)
        text = re.sub(r'^\s+|\s+$', '', text, flags=re.MULTILINE)
        
        return text.strip()

def main():
    """Main function"""
    
    # Configuration
    STORIES_DIRECTORY = "/Users/texy/Documents/Work/interview-preper/GenAI/stories"
    
    # Check if directory exists
    if not os.path.exists(STORIES_DIRECTORY):
        print(f"❌ Stories directory not found: {STORIES_DIRECTORY}")
        return
    
    # Create organizer and run
    organizer = EpicOrganizer(STORIES_DIRECTORY)
    organizer.reorganize_epics()
    
    print("\n🎯 Organization complete! Your stories are now organized as:")
    print("stories/")
    print("├── EPIC-01-Foundation-Infrastructure/")
    print("│   ├── README.md (original EPIC file)")
    print("│   ├── SUMMARY.md (stories overview)")
    print("│   ├── STORY-01-01-Azure-Infrastructure-Setup.md")
    print("│   ├── STORY-01-02-CI-CD-Pipeline-Implementation.md")
    print("│   └── ... (all stories for this EPIC)")
    print("├── EPIC-02-Content-Management/")
    print("│   └── ... (similar structure)")
    print("└── ... (all other EPICs)")
    
    print("\n📋 Next steps:")
    print("1. Review the organized structure")
    print("2. Edit individual story files as needed")
    print("3. Use the Azure DevOps importer to import organized stories")

if __name__ == "__main__":
    main()
