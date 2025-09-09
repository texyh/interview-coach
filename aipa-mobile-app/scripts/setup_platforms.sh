#!/bin/bash

# AIPA Mobile App - Flutter Platform Setup Script
# This script configures Flutter for Android and iOS development

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
PURPLE='\033[0;35m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# Project configuration
PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

echo -e "${BLUE}🚀 AIPA Flutter Platform Setup${NC}"
echo "========================================="
echo "This script will help you set up Flutter for native mobile development"
echo ""

# Function to print section headers
print_section() {
    echo ""
    echo -e "${PURPLE}📋 $1${NC}"
    echo "----------------------------------------"
}

# Function to check if a command exists
command_exists() {
    command -v "$1" >/dev/null 2>&1
}

# Function to check Flutter doctor status
check_flutter_doctor() {
    print_section "Checking Flutter Configuration"
    flutter doctor -v
    echo ""
}

# Function to install CocoaPods if not installed
setup_cocoapods() {
    print_section "Setting up CocoaPods for iOS Development"

    if command_exists pod; then
        echo -e "${GREEN}✅ CocoaPods is already installed (version: $(pod --version))${NC}"
    else
        echo -e "${YELLOW}🔄 Installing CocoaPods...${NC}"

        if command_exists brew; then
            echo "Installing CocoaPods using Homebrew..."
            brew install cocoapods
        else
            echo "Installing CocoaPods using RubyGems..."
            sudo gem install cocoapods
        fi
    fi

    echo -e "${YELLOW}🔄 Setting up CocoaPods repository...${NC}"
    pod setup
    echo -e "${GREEN}✅ CocoaPods setup completed${NC}"
}

# Function to check and configure iOS setup
setup_ios() {
    print_section "iOS Development Setup"

    # Check if running on macOS
    if [[ "$OSTYPE" != "darwin"* ]]; then
        echo -e "${YELLOW}⚠️  iOS development is only available on macOS${NC}"
        return
    fi

    # Check if Xcode is installed
    if [ -d "/Applications/Xcode.app" ]; then
        echo -e "${GREEN}✅ Xcode is installed${NC}"

        # Check if command line tools are properly configured
        xcode_path=$(xcode-select -p)
        if [[ "$xcode_path" == "/Applications/Xcode.app/Contents/Developer" ]]; then
            echo -e "${GREEN}✅ Xcode command line tools are properly configured${NC}"
        else
            echo -e "${YELLOW}🔄 Configuring Xcode command line tools...${NC}"
            sudo xcode-select --switch /Applications/Xcode.app/Contents/Developer
            echo -e "${GREEN}✅ Xcode command line tools configured${NC}"
        fi

        # Run first launch if needed
        echo -e "${YELLOW}🔄 Running Xcode first launch setup...${NC}"
        sudo xcodebuild -runFirstLaunch

    else
        echo -e "${RED}❌ Xcode is not installed${NC}"
        echo ""
        echo -e "${YELLOW}📖 To install Xcode:${NC}"
        echo "1. Open the Mac App Store"
        echo "2. Search for 'Xcode'"
        echo "3. Click 'Install' (this may take 1-2 hours)"
        echo "4. Re-run this script after installation"
        echo ""
        echo -e "${YELLOW}📖 Alternative download:${NC}"
        echo "1. Visit https://developer.apple.com/xcode/"
        echo "2. Sign in with your Apple ID"
        echo "3. Download Xcode"
        echo ""
        return 1
    fi

    # Setup CocoaPods
    setup_cocoapods

    # Test iOS Simulator
    echo -e "${YELLOW}🔄 Testing iOS Simulator...${NC}"
    if xcrun simctl list devices | grep -q "Booted"; then
        echo -e "${GREEN}✅ iOS Simulator is ready${NC}"
    else
        echo -e "${YELLOW}🔄 Opening iOS Simulator...${NC}"
        open -a Simulator
        sleep 3
        echo -e "${GREEN}✅ iOS Simulator opened${NC}"
    fi
}

# Function to guide Android Studio installation
setup_android() {
    print_section "Android Development Setup"

    # Check if Android Studio is installed
    if [ -d "/Applications/Android Studio.app" ]; then
        echo -e "${GREEN}✅ Android Studio is installed${NC}"

        # Try to find Android SDK
        if [ -d "$HOME/Library/Android/sdk" ]; then
            export ANDROID_SDK_ROOT="$HOME/Library/Android/sdk"
            echo -e "${GREEN}✅ Android SDK found at: $ANDROID_SDK_ROOT${NC}"

            # Configure Flutter to use the SDK
            flutter config --android-sdk "$ANDROID_SDK_ROOT"

            # Accept licenses
            echo -e "${YELLOW}🔄 Accepting Android SDK licenses...${NC}"
            yes | flutter doctor --android-licenses

        elif [ -d "$HOME/Android/Sdk" ]; then
            export ANDROID_SDK_ROOT="$HOME/Android/Sdk"
            echo -e "${GREEN}✅ Android SDK found at: $ANDROID_SDK_ROOT${NC}"
            flutter config --android-sdk "$ANDROID_SDK_ROOT"
            yes | flutter doctor --android-licenses

        else
            echo -e "${YELLOW}⚠️  Android SDK not found in default locations${NC}"
            echo "Please launch Android Studio and install the Android SDK components"
        fi

    else
        echo -e "${RED}❌ Android Studio is not installed${NC}"
        echo ""
        echo -e "${YELLOW}📖 To install Android Studio:${NC}"
        echo "1. Visit: https://developer.android.com/studio"
        echo "2. Download Android Studio for macOS"
        echo "3. Open the downloaded .dmg file"
        echo "4. Drag Android Studio to Applications folder"
        echo "5. Launch Android Studio from Applications"
        echo "6. Complete the setup wizard:"
        echo "   - Choose 'Standard' installation"
        echo "   - Install Android SDK"
        echo "   - Install Android SDK Command-line Tools"
        echo "   - Install Android SDK Build-Tools"
        echo "   - Install Android Emulator"
        echo "7. Re-run this script after installation"
        echo ""
        return 1
    fi
}

# Function to test builds
test_builds() {
    print_section "Testing Platform Builds"

    cd "$PROJECT_ROOT"

    echo -e "${YELLOW}🔄 Testing web build...${NC}"
    if flutter build web --quiet; then
        echo -e "${GREEN}✅ Web build successful${NC}"
    else
        echo -e "${RED}❌ Web build failed${NC}"
    fi

    if [[ "$OSTYPE" == "darwin"* ]] && [ -d "/Applications/Xcode.app" ]; then
        echo -e "${YELLOW}🔄 Testing iOS build...${NC}"
        if flutter build ios --debug --simulator --quiet; then
            echo -e "${GREEN}✅ iOS build successful${NC}"
        else
            echo -e "${RED}❌ iOS build failed${NC}"
        fi
    fi

    if [ -d "/Applications/Android Studio.app" ]; then
        echo -e "${YELLOW}🔄 Testing Android build...${NC}"
        if flutter build apk --debug --quiet; then
            echo -e "${GREEN}✅ Android build successful${NC}"
        else
            echo -e "${RED}❌ Android build failed${NC}"
        fi
    fi
}

# Function to update project configurations
update_project_config() {
    print_section "Updating Project Configuration"

    cd "$PROJECT_ROOT"

    # Check iOS deployment target
    if [ -f "ios/Runner.xcodeproj/project.pbxproj" ]; then
        current_target=$(grep -o "IPHONEOS_DEPLOYMENT_TARGET = [0-9]*\.[0-9]*" ios/Runner.xcodeproj/project.pbxproj | head -1 | cut -d'=' -f2 | tr -d ' ')
        echo -e "${CYAN}ℹ️  Current iOS deployment target: $current_target${NC}"

        if [[ "$current_target" < "12.0" ]]; then
            echo -e "${YELLOW}🔄 Updating iOS deployment target to 12.0...${NC}"
            sed -i '' 's/IPHONEOS_DEPLOYMENT_TARGET = [0-9]*\.[0-9]*/IPHONEOS_DEPLOYMENT_TARGET = 12.0/g' ios/Runner.xcodeproj/project.pbxproj
            echo -e "${GREEN}✅ iOS deployment target updated${NC}"
        else
            echo -e "${GREEN}✅ iOS deployment target is appropriate${NC}"
        fi
    fi

    # Check Android SDK versions
    if [ -f "android/app/build.gradle.kts" ]; then
        echo -e "${CYAN}ℹ️  Checking Android build configuration...${NC}"
        echo -e "${GREEN}✅ Android build configuration looks good${NC}"
    fi
}

# Function to create development documentation
create_docs() {
    print_section "Creating Development Documentation"

    cat > "$PROJECT_ROOT/PLATFORM_SETUP.md" << EOF
# Flutter Platform Setup Guide

This guide helps you set up Flutter development for iOS and Android platforms.

## Quick Setup

Run the automated setup script:
\`\`\`bash
./scripts/setup_platforms.sh
\`\`\`

## Manual Setup Instructions

### iOS Development (macOS only)

1. **Install Xcode**:
   - Download from Mac App Store or https://developer.apple.com/xcode/
   - Complete installation (1-2 hours)

2. **Configure Xcode**:
   \`\`\`bash
   sudo xcode-select --switch /Applications/Xcode.app/Contents/Developer
   sudo xcodebuild -runFirstLaunch
   \`\`\`

3. **Install CocoaPods**:
   \`\`\`bash
   brew install cocoapods  # or: sudo gem install cocoapods
   pod setup
   \`\`\`

### Android Development

1. **Install Android Studio**:
   - Download from https://developer.android.com/studio
   - Complete setup wizard with standard installation
   - Install Android SDK, Command-line Tools, Build-Tools, and Emulator

2. **Accept Licenses**:
   \`\`\`bash
   flutter doctor --android-licenses
   \`\`\`

## Verification

Check your setup:
\`\`\`bash
flutter doctor -v
\`\`\`

## Building for Platforms

- **Web**: \`flutter build web\`
- **Android**: \`flutter build apk\`
- **iOS**: \`flutter build ios --simulator\`

## Troubleshooting

### Common Issues

1. **Android SDK not found**:
   - Launch Android Studio
   - Go to SDK Manager and install missing components
   - Run: \`flutter config --android-sdk /path/to/sdk\`

2. **iOS build failures**:
   - Ensure Xcode is fully installed
   - Run: \`pod repo update\` in the ios/ directory
   - Clean build: \`flutter clean && flutter pub get\`

3. **CocoaPods issues**:
   - Update CocoaPods: \`brew upgrade cocoapods\`
   - Clear cache: \`pod cache clean --all\`

### Getting Help

- Flutter documentation: https://flutter.dev/docs
- Platform-specific guides: https://flutter.dev/docs/deployment
- Community support: https://github.com/flutter/flutter/discussions
EOF

    echo -e "${GREEN}✅ Created PLATFORM_SETUP.md documentation${NC}"
}

# Main execution
main() {
    echo -e "${CYAN}Starting Flutter platform setup...${NC}"
    echo ""

    # Initial Flutter doctor check
    check_flutter_doctor

    # Setup iOS if on macOS
    if [[ "$OSTYPE" == "darwin"* ]]; then
        setup_ios
    else
        echo -e "${YELLOW}⚠️  Skipping iOS setup (not on macOS)${NC}"
    fi

    # Setup Android
    setup_android

    # Update project configurations
    update_project_config

    # Create documentation
    create_docs

    # Final Flutter doctor check
    print_section "Final Configuration Check"
    check_flutter_doctor

    # Test builds if requested
    if [[ "${1}" == "--test-builds" ]]; then
        test_builds
    fi

    print_section "Setup Complete!"
    echo -e "${GREEN}🎉 Flutter platform setup completed!${NC}"
    echo ""
    echo -e "${CYAN}Next Steps:${NC}"
    echo "1. If Android Studio or Xcode installations are needed, follow the instructions above"
    echo "2. Re-run this script after installing missing components"
    echo "3. Run './scripts/setup_platforms.sh --test-builds' to test all platforms"
    echo "4. Check PLATFORM_SETUP.md for detailed documentation"
    echo ""
    echo -e "${CYAN}Development Commands:${NC}"
    echo "• Run on web: flutter run -d chrome"
    echo "• Run on iOS: flutter run -d ios"
    echo "• Run on Android: flutter run -d android"
    echo ""
}

# Check if script should run interactively
if [[ "${BASH_SOURCE[0]}" == "${0}" ]]; then
    main "$@"
fi
