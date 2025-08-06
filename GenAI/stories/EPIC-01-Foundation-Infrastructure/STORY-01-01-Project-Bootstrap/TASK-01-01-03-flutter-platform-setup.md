# TASK-01-01-03: Flutter Platform Setup and Toolchain Configuration

**Epic**: EPIC-01 Foundation Infrastructure
**Story**: STORY-01-01 Development Environment Setup
**Priority**: Medium
**Estimate**: 3-4 hours
**Status**: TODO

## Overview

Configure Flutter development environment for native mobile app development by installing and setting up Android SDK, Xcode, and CocoaPods to enable building and testing on Android and iOS platforms.

## Current State

Flutter SDK is installed and web platform is working, but native mobile platforms are not configured:

```
[✗] Android toolchain - develop for Android devices
    ✗ Unable to locate Android SDK.
[✗] Xcode - develop for iOS and macOS
    ✗ Xcode installation is incomplete
    ✗ CocoaPods not installed
```

## Acceptance Criteria

### Android Setup

- [ ] **Install Android Studio**

  - Download from https://developer.android.com/studio/index.html
  - Complete initial setup wizard
  - Install Android SDK components
  - Configure Android SDK path in Flutter

- [ ] **Configure Android Toolchain**

  - Accept Android SDK licenses: `flutter doctor --android-licenses`
  - Verify Android toolchain: `flutter doctor` shows ✓ for Android

- [ ] **Test Android Build**
  - Create and run Android emulator
  - Build APK: `flutter build apk --debug`
  - Test app deployment to emulator

### iOS Setup (macOS only)

- [ ] **Install Xcode**

  - Download from Mac App Store or Apple Developer
  - Complete Xcode installation
  - Run: `sudo xcode-select --switch /Applications/Xcode.app/Contents/Developer`
  - Run: `sudo xcodebuild -runFirstLaunch`

- [ ] **Install CocoaPods**

  - Install CocoaPods: `sudo gem install cocoapods`
  - Setup CocoaPods: `pod setup`
  - Verify installation: `pod --version`

- [ ] **Configure iOS Toolchain**

  - Verify Xcode toolchain: `flutter doctor` shows ✓ for Xcode
  - Test iOS Simulator launch

- [ ] **Test iOS Build**
  - Open iOS Simulator
  - Build iOS app: `flutter build ios --debug --simulator`
  - Test app deployment to iOS Simulator

### Verification

- [ ] **Flutter Doctor Check**

  - Run `flutter doctor -v`
  - All platforms show ✓ (Web, Android, iOS)
  - No blocking issues reported

- [ ] **Multi-Platform Build Test**
  - `flutter build web` - Success ✓
  - `flutter build apk` - Success ✓
  - `flutter build ios --simulator` - Success ✓

## Implementation Steps

### 1. Android Development Setup

```bash
# 1. Download and install Android Studio
# Visit: https://developer.android.com/studio/index.html

# 2. Launch Android Studio and complete setup wizard
# - Install Android SDK
# - Install Android SDK Command-line Tools
# - Install Android SDK Build-Tools
# - Install Android Emulator

# 3. Accept Android licenses
flutter doctor --android-licenses

# 4. Verify Android setup
flutter doctor

# 5. Create and test Android emulator
flutter emulators --launch <emulator_name>
cd aipa-mobile-app && flutter run
```

### 2. iOS Development Setup (macOS)

```bash
# 1. Install Xcode from Mac App Store
# 2. Set up Xcode command line tools
sudo xcode-select --switch /Applications/Xcode.app/Contents/Developer
sudo xcodebuild -runFirstLaunch

# 3. Install CocoaPods
sudo gem install cocoapods
pod setup

# 4. Verify iOS setup
flutter doctor

# 5. Test iOS Simulator
open -a Simulator
cd aipa-mobile-app && flutter run
```

### 3. Project Configuration Updates

```bash
# Update iOS deployment target if needed
# Edit ios/Runner.xcodeproj/project.pbxproj
# Set IPHONEOS_DEPLOYMENT_TARGET = 12.0

# Update Android configuration if needed
# Edit android/app/build.gradle
# Set minSdkVersion 21
# Set compileSdkVersion 34
# Set targetSdkVersion 34
```

## Technical Considerations

### Platform Requirements

- **Android**: API level 21+ (Android 5.0+)
- **iOS**: iOS 12.0+
- **Xcode**: Version 12.0+
- **Android Studio**: Latest stable version

### Performance Optimization

- Enable hardware acceleration for Android emulator
- Use iOS Simulator for faster iOS testing
- Configure appropriate emulator RAM and storage

### Development Workflow

- Set up device debugging for physical devices
- Configure code signing for iOS development
- Test hot reload functionality on both platforms

## Testing Strategy

- Verify app launches on both Android and iOS
- Test hot reload and hot restart
- Validate platform-specific UI elements
- Check native plugin functionality

## Documentation Updates

- [ ] Update README.md with platform setup instructions
- [ ] Add troubleshooting section for common platform issues
- [ ] Document emulator/simulator setup procedures
- [ ] Add platform-specific build commands

## Success Metrics

- `flutter doctor` shows all green checkmarks
- App successfully builds for all three platforms (web, Android, iOS)
- Development workflow is smooth across platforms
- Team can develop and test on target platforms

## Risks and Mitigation

- **Risk**: Xcode/Android Studio installation issues
  - **Mitigation**: Follow official installation guides, check system requirements
- **Risk**: SDK license acceptance issues
  - **Mitigation**: Run `flutter doctor --android-licenses` and accept all
- **Risk**: CocoaPods installation conflicts
  - **Mitigation**: Use `sudo gem install` or consider using Homebrew

## Dependencies

- Completed: TASK-01-01-02 (Flutter project structure)
- Requires: macOS for iOS development
- Requires: Sufficient disk space (10+ GB for Android Studio + Xcode)

## Definition of Done

- [ ] Android toolchain fully configured and functional
- [ ] iOS toolchain fully configured and functional (macOS)
- [ ] All platforms pass `flutter doctor` checks
- [ ] App builds successfully on all platforms
- [ ] Documentation updated with setup instructions
- [ ] Team can develop and test on native mobile platforms
