#!/bin/bash

# AIPA Epic Tasks Distribution Script
# This script copies the epic task files to their respective application directories

echo "🚀 AIPA Epic Tasks Distribution Script"
echo "======================================"
echo

# Define source and destination paths
TASKS_DIR="/Users/texy/Documents/Work/interview-preper/GenAI/tasks"
BASE_DIR="/Users/texy/Documents/Work/interview-preper"

# Check if we're in the correct directory
if [ ! -d "$TASKS_DIR" ]; then
    echo "❌ Error: Tasks directory not found at $TASKS_DIR"
    echo "Please run this script from the correct location."
    exit 1
fi

echo "📂 Source directory: $TASKS_DIR"
echo "📂 Base directory: $BASE_DIR"
echo

# Function to copy file with verification
copy_task_file() {
    local source_file="$1"
    local dest_file="$2"
    local component_name="$3"

    echo "📋 Copying $component_name tasks..."

    if [ -f "$source_file" ]; then
        if [ -d "$(dirname "$dest_file")" ]; then
            cp "$source_file" "$dest_file"
            if [ $? -eq 0 ]; then
                echo "   ✅ Successfully copied to: $dest_file"
            else
                echo "   ❌ Failed to copy $source_file"
                return 1
            fi
        else
            echo "   ⚠️  Destination directory not found: $(dirname "$dest_file")"
            echo "   Creating directory..."
            mkdir -p "$(dirname "$dest_file")"
            cp "$source_file" "$dest_file"
            if [ $? -eq 0 ]; then
                echo "   ✅ Successfully copied to: $dest_file"
            else
                echo "   ❌ Failed to copy after creating directory"
                return 1
            fi
        fi
    else
        echo "   ❌ Source file not found: $source_file"
        return 1
    fi
    echo
}

# Copy files to respective directories
echo "🔄 Starting file distribution..."
echo

# Backend tasks
copy_task_file \
    "$TASKS_DIR/AIPA_BACKEND_TASKS.md" \
    "$BASE_DIR/aipa-backend/EPIC_TASKS.md" \
    "Backend (.NET 9)"

# AI Service tasks
copy_task_file \
    "$TASKS_DIR/AIPA_AI_SERVICE_TASKS.md" \
    "$BASE_DIR/aipa-ai-service/EPIC_TASKS.md" \
    "AI Service (NestJS)"

# Web App tasks
copy_task_file \
    "$TASKS_DIR/AIPA_WEB_APP_TASKS.md" \
    "$BASE_DIR/aipa-web-app/EPIC_TASKS.md" \
    "Web App (Next.js)"

# Mobile App tasks
copy_task_file \
    "$TASKS_DIR/AIPA_MOBILE_APP_TASKS.md" \
    "$BASE_DIR/aipa-mobile-app/EPIC_TASKS.md" \
    "Mobile App (Flutter)"

echo "🎉 Epic task distribution completed!"
echo
echo "📋 Summary:"
echo "   - Backend tasks: $BASE_DIR/aipa-backend/EPIC_TASKS.md"
echo "   - AI Service tasks: $BASE_DIR/aipa-ai-service/EPIC_TASKS.md"
echo "   - Web App tasks: $BASE_DIR/aipa-web-app/EPIC_TASKS.md"
echo "   - Mobile App tasks: $BASE_DIR/aipa-mobile-app/EPIC_TASKS.md"
echo
echo "🚀 Next Steps:"
echo "   1. Each development team should review their EPIC_TASKS.md file"
echo "   2. Create detailed sprint planning based on epic tasks"
echo "   3. Begin Phase 1 development with Foundation & Infrastructure"
echo "   4. Follow the development phase recommendations in the task files"
echo
echo "📞 For questions, contact the respective development teams:"
echo "   - Backend Team: .NET 9, Entity Framework, PostgreSQL"
echo "   - AI Service Team: NestJS, TypeScript, OpenAI, Azure Speech"
echo "   - Web App Team: Next.js, React, TypeScript, Ant Design"
echo "   - Mobile App Team: Flutter, Dart, iOS, Android"
echo
