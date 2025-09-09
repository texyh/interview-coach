# Flutter Platform Setup Guide

This guide helps you complete the Flutter platform setup for native mobile development on Android and iOS.

## Current Status

✅ **Flutter SDK**: Installed and working (v3.32.8)
✅ **Web Platform**: Fully configured and tested
✅ **CocoaPods**: Installed and configured (v1.16.2)
❌ **Android Toolchain**: Requires Android Studio installation
❌ **iOS Development**: Requires Xcode installation

## Quick Setup

Run our automated setup script:

```bash
./scripts/setup_platforms.sh
```

For testing all platforms after installation:

```bash
./scripts/setup_platforms.sh --test-builds
```

## Manual Installation Guide

### 1. Android Development Setup

#### Step 1: Install Android Studio

1. **Download Android Studio**: Visit https://developer.android.com/studio
2. **Install**: Open the downloaded file and drag Android Studio to Applications
3. **Launch**: Open Android Studio from Applications folder
4. **Complete Setup Wizard**:
   - Choose "Standard" installation
   - Install Android SDK (latest version)
   - Install Android SDK Command-line Tools
   - Install Android SDK Build-Tools
   - Install Android Emulator
   - Accept all license agreements

#### Step 2: Configure Flutter for Android

```bash
# Set Android SDK path (adjust path if different)
flutter config --android-sdk ~/Library/Android/sdk

# Accept Android licenses
flutter doctor --android-licenses

# Verify setup
flutter doctor
```

#### Step 3: Create Android Emulator

1. Open Android Studio
2. Go to **Tools > AVD Manager**
3. Click **Create Virtual Device**
4. Select a device (e.g., Pixel 4)
5. Choose system image (latest API level)
6. Click **Finish**

### 2. iOS Development Setup (macOS only)

#### Step 1: Install Xcode

1. **Option A - Mac App Store**:

   - Open Mac App Store
   - Search for "Xcode"
   - Click "Install" (1-2 hours download)

2. **Option B - Apple Developer**:
   - Visit https://developer.apple.com/xcode/
   - Sign in with Apple ID
   - Download latest Xcode

#### Step 2: Configure Xcode

```bash
# Set Xcode command line tools path
sudo xcode-select --switch /Applications/Xcode.app/Contents/Developer

# Run first launch setup
sudo xcodebuild -runFirstLaunch

# Verify setup
flutter doctor
```

#### Step 3: CocoaPods (Already Installed ✅)

CocoaPods is already installed and configured on your system.

## Verification Steps

### 1. Check Flutter Doctor

```bash
flutter doctor -v
```

Expected output should show:

- ✅ Flutter
- ✅ Android toolchain (after Android Studio setup)
- ✅ Xcode (after Xcode installation)
- ✅ Chrome
- ✅ VS Code
- ✅ Connected device

### 2. Test Platform Builds

```bash
# Test web build (should work now)
flutter build web

# Test Android build (after Android Studio setup)
flutter build apk --debug

# Test iOS build (after Xcode installation)
flutter build ios --debug --simulator
```

## Development Workflow

### Running the App

```bash
# Run on web
flutter run -d chrome

# Run on Android emulator
flutter run -d android

# Run on iOS simulator
flutter run -d ios

# List available devices
flutter devices
```

### Building for Release

```bash
# Web release
flutter build web --release

# Android APK
flutter build apk --release

# iOS release (requires Apple Developer account)
flutter build ios --release
```

## Project Configuration

### Android Configuration

The project is already configured for Android development:

- **Min SDK**: 21 (Android 5.0+)
- **Target SDK**: Latest
- **Compile SDK**: Latest
- **Build Tools**: Latest

### iOS Configuration

The project is already configured for iOS development:

- **Deployment Target**: iOS 12.0+
- **CocoaPods**: Configured
- **Architecture**: Universal (arm64 + x86_64)

## Troubleshooting

### Common Android Issues

1. **"Unable to locate Android SDK"**:

   ```bash
   # Set SDK path manually
   flutter config --android-sdk ~/Library/Android/sdk
   ```

2. **License agreements not accepted**:

   ```bash
   flutter doctor --android-licenses
   ```

3. **Build tools not found**:
   - Open Android Studio
   - Go to SDK Manager
   - Install latest Build-Tools

### Common iOS Issues

1. **"Xcode installation is incomplete"**:

   - Ensure Xcode is fully installed from App Store
   - Run: `sudo xcodebuild -runFirstLaunch`

2. **CocoaPods issues**:

   ```bash
   # Update CocoaPods
   brew upgrade cocoapods

   # Clear cache
   pod cache clean --all

   # Reinstall pods
   cd ios && pod install
   ```

3. **Simulator not available**:

   ```bash
   # Open iOS Simulator
   open -a Simulator

   # List available simulators
   xcrun simctl list devices
   ```

### Flutter Issues

1. **Clean and rebuild**:

   ```bash
   flutter clean
   flutter pub get
   flutter build <platform>
   ```

2. **Update Flutter**:

   ```bash
   flutter upgrade
   ```

3. **Reset Flutter configuration**:
   ```bash
   flutter doctor --android-licenses
   flutter config --clear-features
   ```

## Getting Help

- **Flutter Documentation**: https://flutter.dev/docs
- **Platform Setup Guides**: https://flutter.dev/docs/get-started/install
- **Community Support**: https://github.com/flutter/flutter/discussions
- **Stack Overflow**: https://stackoverflow.com/questions/tagged/flutter

## Next Steps

After completing the platform setup:

1. ✅ **Verify** all platforms with `flutter doctor`
2. ✅ **Test builds** for each platform
3. ✅ **Create emulators/simulators** for testing
4. ✅ **Start developing** your Flutter app!

---

_This guide was generated as part of TASK-01-01-03: Flutter Platform Setup and Toolchain Configuration_
