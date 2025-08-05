# Azure DevOps Import Guide

## 🎯 Methods to Import EPICs and Stories to Azure DevOps

### **Method 1: Automated Python Script (Recommended)**

The Python script I've created will automatically parse your markdown files and create work items in Azure DevOps.

#### **Prerequisites:**

1. **Azure DevOps Personal Access Token (PAT)**

   - Go to Azure DevOps → User Settings → Personal Access Tokens
   - Create token with "Work Items (Read & Write)" permissions
   - Copy the token securely

2. **Python Environment**
   ```bash
   cd /Users/texy/Documents/Work/interview-preper/GenAI/scripts
   python3 -m venv venv
   source venv/bin/activate  # On macOS/Linux
   pip install -r requirements.txt
   ```

#### **Setup Steps:**

1. **Configure your environment:**

   ```bash
   cp .env.example .env
   # Edit .env with your actual values
   ```

2. **Update the script variables:**

   - Open `import-to-azure-devops.py`
   - Update `ORGANIZATION`, `PROJECT`, and `PAT_TOKEN` variables
   - Or use environment variables from `.env`

3. **Run the import:**
   ```bash
   python import-to-azure-devops.py
   ```

#### **What it creates:**

- ✅ 8 Epic work items with full descriptions
- ✅ 72 User Story work items linked to their Epics
- ✅ Story points, priorities, and acceptance criteria
- ✅ Proper parent-child relationships

---

### **Method 2: Azure DevOps Excel Add-in**

#### **Steps:**

1. **Install Azure DevOps Office Integration**

   - Download from Microsoft
   - Requires Excel/Word with Azure DevOps account

2. **Export to Excel format:**

   ```bash
   # Create Excel-compatible CSV
   python scripts/export-to-excel.py
   ```

3. **Import via Excel:**
   - Open Excel with Azure DevOps add-in
   - Connect to your project
   - Import work items from CSV
   - Publish to Azure DevOps

---

### **Method 3: Azure DevOps CLI**

#### **Prerequisites:**

```bash
# Install Azure CLI
brew install azure-cli

# Install Azure DevOps extension
az extension add --name azure-devops

# Login to Azure
az login
```

#### **Create work items via CLI:**

```bash
# Set default organization and project
az devops configure --defaults organization=https://dev.azure.com/YourOrg project=YourProject

# Create Epic example
az boards work-item create \
  --title "EPIC 01: Foundation Infrastructure" \
  --type Epic \
  --description "Implement core infrastructure..." \
  --area "YourProject" \
  --iteration "YourProject\Sprint 1"

# Create User Story linked to Epic
az boards work-item create \
  --title "Story 1.1: Azure Infrastructure Setup" \
  --type "User Story" \
  --description "As a DevOps Engineer..." \
  --area "YourProject" \
  --iteration "YourProject\Sprint 1" \
  --fields "Microsoft.VSTS.Scheduling.StoryPoints=13" \
  --parent-id <epic-id>
```

---

### **Method 4: Manual Import via Azure DevOps Web Interface**

#### **Steps:**

1. **Access Azure DevOps:**

   - Go to `https://dev.azure.com/YourOrganization/YourProject`
   - Navigate to Boards → Work Items

2. **Create Epics:**

   - New Work Item → Epic
   - Copy title and description from markdown files
   - Set priority, business value, acceptance criteria

3. **Create User Stories:**
   - New Work Item → User Story
   - Link to parent Epic
   - Set story points, priority, acceptance criteria
   - Copy technical tasks to description or tasks

---

### **Method 5: Azure DevOps REST API (Custom Integration)**

#### **Direct API calls:**

```bash
# Create Epic via REST API
curl -X POST \
  'https://dev.azure.com/{organization}/{project}/_apis/wit/workitems/$Epic?api-version=7.1-preview.3' \
  -H 'Authorization: Basic {base64-encoded-pat}' \
  -H 'Content-Type: application/json-patch+json' \
  -d '[
    {"op": "add", "path": "/fields/System.Title", "value": "EPIC 01: Foundation Infrastructure"},
    {"op": "add", "path": "/fields/System.Description", "value": "Epic description..."},
    {"op": "add", "path": "/fields/Microsoft.VSTS.Common.Priority", "value": 1}
  ]'
```

---

## 🚀 **Recommended Approach: Python Script**

The Python script is the most efficient method because it:

✅ **Automates everything** - Creates all 8 EPICs and 72 User Stories
✅ **Preserves structure** - Maintains parent-child relationships
✅ **Includes metadata** - Story points, priorities, acceptance criteria
✅ **Handles formatting** - Converts markdown to proper work item fields
✅ **Error handling** - Reports success/failure for each item
✅ **Extensible** - Easy to modify for custom fields or additional metadata

---

## 📋 **Pre-Import Checklist**

Before running the import:

- [ ] Azure DevOps project created
- [ ] Personal Access Token with work item permissions
- [ ] Project areas and iterations configured
- [ ] Team and users added to project
- [ ] Work item process template configured (Agile/Scrum/Basic)
- [ ] Custom fields created if needed

---

## 🎯 **Post-Import Tasks**

After successful import:

1. **Review Work Items:**

   - Verify all EPICs and stories created correctly
   - Check parent-child relationships
   - Validate story points and priorities

2. **Configure Sprints:**

   - Set up iteration paths
   - Assign stories to appropriate sprints
   - Configure sprint dates and capacity

3. **Assign Team Members:**

   - Assign EPICs to Epic owners
   - Assign User Stories to developers
   - Set up notification rules

4. **Set Up Boards:**
   - Configure Kanban boards
   - Set up swim lanes and columns
   - Configure work item rules and automation

---

## 🔧 **Troubleshooting**

**Common Issues:**

1. **Authentication Errors:**

   - Verify PAT token has correct permissions
   - Check organization and project names
   - Ensure token hasn't expired

2. **Work Item Creation Failures:**

   - Verify work item types exist in your process
   - Check required fields are populated
   - Validate field value constraints

3. **Linking Errors:**
   - Ensure parent work items exist before creating children
   - Verify link type relationships are allowed
   - Check work item hierarchy rules

**Need Help?**

- Check Azure DevOps documentation
- Use Azure DevOps REST API Explorer
- Test with single work item first
- Enable verbose logging in script

---

## 📊 **Expected Results**

After running the Python script, you'll have:

- **8 Epic work items** representing your major features
- **72 User Story work items** with detailed acceptance criteria
- **Proper hierarchy** with stories linked to their parent epics
- **Complete metadata** including story points, priorities, and descriptions
- **Ready-to-use backlog** for sprint planning and development

The import should take 2-5 minutes depending on API response times and network connectivity.
