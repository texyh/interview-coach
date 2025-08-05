# 🚀 Quick Start: Push EPICs to Azure DevOps

## **Option 1: Simple Python Script (5 minutes)**

### **Step 1: Get Your Azure DevOps Information**

1. **Organization Name**: Your Azure DevOps URL is `https://dev.azure.com/YOUR-ORG`
2. **Project Name**: The project where you want to import
3. **Personal Access Token**:
   - Go to Azure DevOps → User Settings → Personal Access Tokens
   - Create new token with "Work Items (Read & Write)" permissions
   - Copy the token (save it securely!)

### **Step 2: Run the Import**

```bash
cd /Users/texy/Documents/Work/interview-preper/GenAI/scripts

# Install dependencies
pip3 install requests python-dotenv

# Edit the configuration in simple-import.py
# Update these 3 lines:
ORGANIZATION = 'your-organization-name'  # e.g., 'contoso'
PROJECT = 'your-project-name'           # e.g., 'interview-assistant'
PAT_TOKEN = 'your-personal-access-token' # Your PAT token

# Run the import
python3 simple-import.py
```

**That's it!** ✅ You'll have all 8 EPICs and 72 User Stories in Azure DevOps.

---

## **Option 2: Manual Import (30 minutes)**

If you prefer to create work items manually:

### **Create EPICs First:**

1. Go to Azure DevOps → Boards → Work Items
2. New Work Item → Epic
3. Copy from your markdown files:

**EPIC 01**: Foundation Infrastructure

- **Title**: EPIC 01: Foundation Infrastructure
- **Description**: Copy from EPIC-01-Foundation-Infrastructure.md
- **Priority**: 1 (Critical)
- **Business Value**: Core foundation enabling all development

**EPIC 02**: Content Management

- **Title**: EPIC 02: Content Management & Question Bank System
- **Description**: Copy from EPIC-02-Content-Management.md
- **Priority**: 1 (Critical)
- **Business Value**: Core content foundation

_Continue for all 8 EPICs..._

### **Create User Stories:**

1. New Work Item → User Story
2. Link to Parent Epic using "Add Link" → Parent
3. Set Story Points and Priority
4. Copy Acceptance Criteria from markdown

---

## **Option 3: Azure DevOps Excel Add-in**

1. Install Azure DevOps Office Integration
2. Open Excel, connect to your Azure DevOps project
3. Import work items from a CSV export of your EPICs/Stories
4. Publish to Azure DevOps

---

## **What Gets Created:**

### **8 Epic Work Items:**

1. EPIC 01: Foundation Infrastructure (120-150 points)
2. EPIC 02: Content Management (100-130 points)
3. EPIC 03: AI Mock Interview System (150-180 points)
4. EPIC 04: Mobile Application (130-160 points)
5. EPIC 05: Analytics & BI (110-140 points)
6. EPIC 06: Third-Party Integrations (100-130 points)
7. EPIC 07: Enterprise Features (120-150 points)
8. EPIC 08: Global Expansion (90-120 points)

### **72 User Story Work Items:**

- 9 stories per EPIC
- Story points from 8-21 each
- Proper parent-child linking
- Priorities and acceptance criteria

---

## **Post-Import Checklist:**

After successful import:

- [ ] **Review Work Items**: Check all EPICs and stories created correctly
- [ ] **Set Up Sprints**: Create iteration paths and assign stories
- [ ] **Assign Team Members**: Add developers to stories
- [ ] **Configure Boards**: Set up Kanban columns and swim lanes
- [ ] **Set Up Notifications**: Configure alerts for work item changes

---

## **Troubleshooting:**

**❌ Authentication Error**

- Verify your PAT token has "Work Items (Read & Write)" permissions
- Check organization and project names are correct

**❌ Work Item Creation Failed**

- Verify your project uses "Agile" or "Scrum" process template
- Check if Epic and User Story work item types exist

**❌ Linking Errors**

- Ensure Epic is created before trying to link User Stories
- Verify hierarchy relationships are allowed in your process

---

## **Need Help?**

1. **Test First**: Run with just one EPIC to verify everything works
2. **Check Permissions**: Make sure you have project contributor access
3. **Verify Process**: Confirm your project supports Epic → User Story hierarchy
4. **Contact Support**: Use Azure DevOps community forums for complex issues

---

## **Expected Timeline:**

- **Python Script**: 2-5 minutes for complete import
- **Manual Import**: 30-60 minutes
- **Excel Import**: 15-30 minutes

The automated script is highly recommended as it preserves all relationships and metadata perfectly! 🎯
