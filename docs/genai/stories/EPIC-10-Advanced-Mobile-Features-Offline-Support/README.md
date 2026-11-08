# EPIC 10: Advanced Mobile Features & Offline Support

**Epic Priority**: 10 (Phase 2 - Platform Enhancement)
**Phase**: 2 - Enhanced Features
**Estimated Duration**: 5-6 weeks
**Team Dependencies**: Mobile Developer, UX Designer, Backend, DevOps

## Epic Overview

Enhance the mobile application with advanced native features, comprehensive offline capabilities, and mobile-optimized AI processing to provide a premium mobile interview preparation experience. This epic focuses on leveraging device capabilities, ensuring seamless offline functionality, and optimizing mobile-specific user interactions.

## Business Value

- **Mobile-First Users**: Captures users who prefer mobile-only interview preparation
- **Accessibility**: Enables practice in various environments (commuting, traveling, limited connectivity)
- **User Retention**: Offline capabilities increase app usage frequency and session length
- **Market Differentiation**: Advanced mobile features distinguish from web-only competitors
- **Global Reach**: Offline support enables users in areas with poor internet connectivity

## Technical Scope

### Offline-First Architecture

- **Local Data Synchronization**: Complete offline question banks and user progress
- **Offline AI Processing**: Local speech-to-text and basic response analysis
- **Smart Sync**: Intelligent background synchronization when connectivity is available
- **Conflict Resolution**: Handles data conflicts between offline and online states
- **Progressive Download**: Adaptive content downloading based on storage and connectivity

### Advanced Mobile Features

- **Native Device Integration**: Camera, microphone, haptic feedback, and biometric authentication
- **Voice-Only Interviews**: Hands-free interview sessions with voice navigation
- **Mobile-Optimized UI**: Touch-first interface with gesture-based navigation
- **Background Processing**: Continue processing even when app is backgrounded
- **Push Notifications**: Intelligent reminders and coaching notifications

### Mobile Performance Optimization

- **Edge AI Processing**: On-device machine learning for instant feedback
- **Adaptive Quality**: Dynamic adjustment of features based on device capabilities
- **Battery Optimization**: Efficient power management for extended practice sessions
- **Storage Management**: Intelligent caching and data cleanup strategies
- **Network Optimization**: Minimal bandwidth usage with smart prefetching

## Success Criteria

- [ ] Mobile app functions completely offline for core interview features
- [ ] Voice-only interviews provide full feature parity with visual interface
- [ ] App performance maintains 60fps on devices as old as 3 years
- [ ] Offline sync resolves conflicts without data loss in 99.9% of cases
- [ ] Mobile app ratings improve to 4.6+ stars across app stores
- [ ] Mobile user session length increases by 40% with offline features
- [ ] Battery usage remains under 5% per hour during typical usage
- [ ] App size stays under 150MB including offline content

## Stories in this Epic

1. **[STORY-10-01] Offline Data Architecture & Local Database**
2. **[STORY-10-02] Smart Synchronization Engine**
3. **[STORY-10-03] Offline AI Processing & Speech Recognition**
4. **[STORY-10-04] Voice-Only Interview Experience**
5. **[STORY-10-05] Advanced Camera & Video Recording Features**
6. **[STORY-10-06] Haptic Feedback & Gesture Controls**
7. **[STORY-10-07] Biometric Authentication Integration**
8. **[STORY-10-08] Intelligent Push Notifications**
9. **[STORY-10-09] Background Processing & State Management**
10. **[STORY-10-10] Mobile Performance Optimization**
11. **[STORY-10-11] Adaptive Content Downloading**
12. **[STORY-10-12] Mobile-Specific Analytics & Crash Reporting**

## Dependencies & Risks

### Dependencies

- **Device Permissions**: Camera, microphone, storage, and notification access
- **Platform APIs**: iOS and Android native API integrations
- **Edge AI Models**: Optimized ML models for mobile inference
- **App Store Compliance**: Review guidelines for both iOS and Android stores
- **Push Notification Services**: Firebase Cloud Messaging and Apple Push Notifications

### Risks & Mitigations

- **Risk**: Offline AI processing accuracy lower than server-side processing
  - **Mitigation**: Hybrid approach with basic offline analysis and full cloud analysis when online
- **Risk**: Complex synchronization leads to data inconsistencies
  - **Mitigation**: Robust conflict resolution with user-controlled merge strategies
- **Risk**: Battery drain from intensive AI processing
  - **Mitigation**: Adaptive processing based on battery level and device performance
- **Risk**: App store rejection due to feature complexity
  - **Mitigation**: Thorough compliance review and phased feature rollout

## Technical Architecture

### Offline-First Data Layer

```dart
// Flutter offline data management
class OfflineDataManager {
  late Database _database;
  late Box<dynamic> _hivebox;
  final SyncManager _syncManager = SyncManager();

  Future<void> initialize() async {
    // Initialize SQLite for structured data
    _database = await openDatabase(
      'aipa_offline.db',
      version: 1,
      onCreate: _createTables,
    );

    // Initialize Hive for documents and media
    await Hive.initFlutter();
    _hivebox = await Hive.openBox('offline_content');

    // Start background sync
    _syncManager.startPeriodicSync();
  }

  Future<void> _createTables(Database db, int version) async {
    // Questions table
    await db.execute('''
      CREATE TABLE questions (
        id TEXT PRIMARY KEY,
        category TEXT NOT NULL,
        difficulty TEXT NOT NULL,
        question_text TEXT NOT NULL,
        expected_keywords TEXT,
        industry TEXT,
        role TEXT,
        last_updated INTEGER,
        sync_status TEXT DEFAULT 'synced'
      )
    ''');

    // User progress table
    await db.execute('''
      CREATE TABLE user_progress (
        id TEXT PRIMARY KEY,
        user_id TEXT NOT NULL,
        question_id TEXT NOT NULL,
        response_text TEXT,
        score REAL,
        completed_at INTEGER,
        sync_status TEXT DEFAULT 'pending',
        FOREIGN KEY (question_id) REFERENCES questions (id)
      )
    ''');

    // Sync queue table
    await db.execute('''
      CREATE TABLE sync_queue (
        id INTEGER PRIMARY KEY AUTOINCREMENT,
        table_name TEXT NOT NULL,
        record_id TEXT NOT NULL,
        operation TEXT NOT NULL,
        data TEXT,
        created_at INTEGER,
        retry_count INTEGER DEFAULT 0
      )
    ''');
  }

  // Offline question retrieval
  Future<List<Question>> getQuestionsOffline({
    String? category,
    String? difficulty,
    int limit = 20,
  }) async {
    String sql = 'SELECT * FROM questions WHERE 1=1';
    List<dynamic> args = [];

    if (category != null) {
      sql += ' AND category = ?';
      args.add(category);
    }

    if (difficulty != null) {
      sql += ' AND difficulty = ?';
      args.add(difficulty);
    }

    sql += ' ORDER BY RANDOM() LIMIT ?';
    args.add(limit);

    final List<Map<String, dynamic>> maps = await _database.query(
      'questions',
      where: category != null || difficulty != null ? sql.split('WHERE')[1] : null,
      whereArgs: args.isNotEmpty ? args : null,
      limit: limit,
    );

    return maps.map((map) => Question.fromMap(map)).toList();
  }

  // Save progress offline
  Future<void> saveProgressOffline(UserProgress progress) async {
    await _database.insert(
      'user_progress',
      {
        ...progress.toMap(),
        'sync_status': 'pending',
      },
      conflictAlgorithm: ConflictAlgorithm.replace,
    );

    // Add to sync queue
    await _addToSyncQueue('user_progress', progress.id, 'INSERT', progress.toJson());
  }

  Future<void> _addToSyncQueue(String tableName, String recordId, String operation, String data) async {
    await _database.insert('sync_queue', {
      'table_name': tableName,
      'record_id': recordId,
      'operation': operation,
      'data': data,
      'created_at': DateTime.now().millisecondsSinceEpoch,
    });
  }
}
```

### Smart Synchronization System

```dart
// Intelligent sync manager
class SyncManager {
  final ConnectivityService _connectivity = ConnectivityService();
  final ApiService _apiService = ApiService();
  final OfflineDataManager _offlineData = OfflineDataManager();

  Timer? _periodicSyncTimer;
  bool _isSyncing = false;

  void startPeriodicSync() {
    _periodicSyncTimer = Timer.periodic(
      Duration(minutes: 5),
      (_) => _performSmartSync(),
    );
  }

  Future<void> _performSmartSync() async {
    if (_isSyncing) return;

    final connectivity = await _connectivity.checkConnectivity();
    if (connectivity == ConnectivityResult.none) return;

    _isSyncing = true;

    try {
      // Sync in priority order
      await _syncUserProgress();
      await _syncQuestions();
      await _syncMediaFiles();
      await _cleanupSyncQueue();
    } catch (e) {
      print('Sync error: $e');
    } finally {
      _isSyncing = false;
    }
  }

  Future<void> _syncUserProgress() async {
    final pendingProgress = await _offlineData.getPendingProgress();

    for (final progress in pendingProgress) {
      try {
        await _apiService.syncUserProgress(progress);
        await _offlineData.markProgressAsSynced(progress.id);
      } catch (e) {
        // Handle conflicts
        if (e is ConflictException) {
          await _handleSyncConflict(progress, e.serverData);
        }
      }
    }
  }

  Future<void> _handleSyncConflict(UserProgress localData, Map<String, dynamic> serverData) async {
    // Conflict resolution strategy
    final resolution = await _showConflictResolutionDialog(localData, serverData);

    switch (resolution) {
      case ConflictResolution.useLocal:
        await _apiService.forceUpdateProgress(localData);
        break;
      case ConflictResolution.useServer:
        await _offlineData.updateProgress(UserProgress.fromMap(serverData));
        break;
      case ConflictResolution.merge:
        final merged = _mergeProgressData(localData, serverData);
        await _apiService.updateProgress(merged);
        await _offlineData.updateProgress(merged);
        break;
    }
  }

  Future<void> _syncQuestions() async {
    final lastSyncTime = await _offlineData.getLastQuestionSyncTime();
    final updatedQuestions = await _apiService.getUpdatedQuestions(lastSyncTime);

    for (final question in updatedQuestions) {
      await _offlineData.upsertQuestion(question);
    }

    await _offlineData.setLastQuestionSyncTime(DateTime.now());
  }
}
```

### Offline AI Processing

```dart
// On-device speech recognition and analysis
class OfflineAIProcessor {
  late SpeechToText _speechToText;
  late TensorFlowLite _tflite;
  bool _isInitialized = false;

  Future<void> initialize() async {
    // Initialize speech recognition
    _speechToText = SpeechToText();
    bool available = await _speechToText.initialize();

    if (!available) {
      throw Exception('Speech recognition not available');
    }

    // Load pre-trained TensorFlow Lite models
    await _loadTFLiteModels();

    _isInitialized = true;
  }

  Future<void> _loadTFLiteModels() async {
    // Load content analysis model
    await Tflite.loadModel(
      model: "assets/models/content_analyzer.tflite",
      labels: "assets/models/content_labels.txt",
    );
  }

  Future<OfflineAnalysisResult> processInterviewResponse(String audioPath) async {
    if (!_isInitialized) await initialize();

    // Convert speech to text offline
    final transcript = await _processSpeechOffline(audioPath);

    // Analyze content quality offline
    final contentAnalysis = await _analyzeContentOffline(transcript);

    // Basic speech quality metrics
    final speechMetrics = await _analyzeSpeechMetricsOffline(audioPath);

    return OfflineAnalysisResult(
      transcript: transcript,
      contentScore: contentAnalysis.score,
      speechMetrics: speechMetrics,
      recommendations: _generateOfflineRecommendations(contentAnalysis, speechMetrics),
      processingTime: DateTime.now(),
      requiresOnlineRefinement: true,
    );
  }

  Future<String> _processSpeechOffline(String audioPath) async {
    // Use device's built-in speech recognition
    final completer = Completer<String>();

    await _speechToText.listen(
      onResult: (result) {
        if (result.finalResult) {
          completer.complete(result.recognizedWords);
        }
      },
      listenFor: Duration(minutes: 5),
      pauseFor: Duration(seconds: 3),
    );

    return completer.future;
  }

  Future<ContentAnalysis> _analyzeContentOffline(String transcript) async {
    // Use TensorFlow Lite for basic content analysis
    final recognition = await Tflite.runModelOnText(
      text: transcript,
      numResults: 5,
      threshold: 0.1,
    );

    if (recognition == null || recognition.isEmpty) {
      return ContentAnalysis(score: 50.0, confidence: 0.3);
    }

    // Process TFLite results
    final relevanceScore = recognition.first['confidence'] * 100;

    return ContentAnalysis(
      score: relevanceScore,
      confidence: recognition.first['confidence'],
      keyTopics: recognition.map((r) => r['label'] as String).toList(),
    );
  }

  Future<SpeechMetrics> _analyzeSpeechMetricsOffline(String audioPath) async {
    // Basic audio analysis using native audio processing
    final audioFile = File(audioPath);
    final audioBytes = await audioFile.readAsBytes();

    // Simple metrics calculation
    final duration = await _getAudioDuration(audioPath);
    final wordsPerMinute = await _calculateWPM(audioPath);
    final averageVolume = await _calculateAverageVolume(audioBytes);

    return SpeechMetrics(
      duration: duration,
      wordsPerMinute: wordsPerMinute,
      averageVolume: averageVolume,
      clarityScore: _estimateClarityFromVolume(averageVolume),
    );
  }
}
```

### Voice-Only Interview Experience

```dart
// Voice navigation and hands-free operation
class VoiceOnlyInterviewController {
  late SpeechToText _speechToText;
  late FlutterTts _textToSpeech;
  late VoiceCommandProcessor _commandProcessor;

  InterviewSession? _currentSession;
  bool _isListeningForCommands = false;
  bool _isRecordingResponse = false;

  Future<void> startVoiceOnlySession(String sessionId) async {
    _currentSession = await InterviewService.getSession(sessionId);

    await _initializeVoiceServices();
    await _startVoiceNavigation();
  }

  Future<void> _initializeVoiceServices() async {
    // Initialize text-to-speech
    _textToSpeech = FlutterTts();
    await _textToSpeech.setLanguage("en-US");
    await _textToSpeech.setSpeechRate(0.8);
    await _textToSpeech.setPitch(1.0);

    // Initialize speech recognition
    _speechToText = SpeechToText();
    await _speechToText.initialize();

    // Initialize command processor
    _commandProcessor = VoiceCommandProcessor();
  }

  Future<void> _startVoiceNavigation() async {
    await _speakText("Welcome to your voice-only interview session. I'll guide you through each question. You can say 'next question', 'repeat question', 'pause session', or 'end session' at any time.");

    await _presentNextQuestion();
    await _listenForCommands();
  }

  Future<void> _listenForCommands() async {
    _isListeningForCommands = true;

    await _speechToText.listen(
      onResult: _processVoiceCommand,
      listenFor: Duration(hours: 1), // Long session
      pauseFor: Duration(seconds: 2),
      partialResults: false,
      listenMode: ListenMode.dictation,
    );
  }

  void _processVoiceCommand(SpeechRecognitionResult result) async {
    if (!result.finalResult) return;

    final command = _commandProcessor.parseCommand(result.recognizedWords);

    switch (command.type) {
      case VoiceCommandType.nextQuestion:
        await _presentNextQuestion();
        break;
      case VoiceCommandType.repeatQuestion:
        await _repeatCurrentQuestion();
        break;
      case VoiceCommandType.startRecording:
        await _startResponseRecording();
        break;
      case VoiceCommandType.stopRecording:
        await _stopResponseRecording();
        break;
      case VoiceCommandType.pauseSession:
        await _pauseSession();
        break;
      case VoiceCommandType.endSession:
        await _endSession();
        break;
      default:
        if (_isRecordingResponse) {
          await _processInterviewResponse(result.recognizedWords);
        } else {
          await _speakText("I didn't understand that command. Please try again or say 'help' for available commands.");
        }
    }
  }

  Future<void> _presentNextQuestion() async {
    final question = await _currentSession!.getNextQuestion();

    if (question == null) {
      await _speakText("Congratulations! You've completed all questions in this interview session.");
      await _endSession();
      return;
    }

    await _speakText("Here's your next question:");
    await Future.delayed(Duration(milliseconds: 500));
    await _speakText(question.text);
    await Future.delayed(Duration(milliseconds: 500));
    await _speakText("Take your time to think, then say 'start recording' when you're ready to answer.");
  }

  Future<void> _startResponseRecording() async {
    _isRecordingResponse = true;
    await _speakText("Recording your response now. Speak naturally and say 'stop recording' when you're finished.");

    HapticFeedback.lightImpact(); // Haptic feedback for recording start

    // Start recording with visual indicator
    Get.find<InterviewController>().startRecordingWithoutUI();
  }

  Future<void> _stopResponseRecording() async {
    _isRecordingResponse = false;

    HapticFeedback.mediumImpact(); // Haptic feedback for recording stop

    await _speakText("Recording stopped. Processing your response...");

    final audioPath = await Get.find<InterviewController>().stopRecordingWithoutUI();
    await _processRecordedResponse(audioPath);
  }

  Future<void> _processRecordedResponse(String audioPath) async {
    // Process offline first for immediate feedback
    final offlineResult = await OfflineAIProcessor().processInterviewResponse(audioPath);

    await _speakText("Thank you for your response. Here's some quick feedback:");
    await _provideFeedback(offlineResult);

    // Queue for online processing when available
    await SyncManager().queueResponseForProcessing(audioPath, _currentSession!.currentQuestionId);
  }

  Future<void> _provideFeedback(OfflineAnalysisResult result) async {
    String feedback = "Your response scored ${result.contentScore.toInt()} out of 100. ";

    if (result.contentScore >= 80) {
      feedback += "Excellent work! Your answer was comprehensive and well-structured.";
    } else if (result.contentScore >= 60) {
      feedback += "Good response! Consider adding more specific examples to strengthen your answer.";
    } else {
      feedback += "Your response could be improved. Try to be more specific and provide concrete examples.";
    }

    await _speakText(feedback);

    if (result.recommendations.isNotEmpty) {
      await _speakText("Here's a quick tip: ${result.recommendations.first}");
    }

    await Future.delayed(Duration(seconds: 1));
    await _speakText("Ready for the next question? Say 'next question' to continue or 'repeat feedback' to hear this again.");
  }
}
```

### Advanced Camera Features

```dart
// Enhanced video recording with analysis
class AdvancedCameraController {
  late CameraController _cameraController;
  late VideoRecorder _videoRecorder;
  bool _isRecording = false;
  bool _isFrontCamera = true;

  // AI-powered features
  bool _bodyLanguageAnalysisEnabled = true;
  bool _eyeContactTrackingEnabled = true;
  bool _expressionAnalysisEnabled = true;

  Future<void> initializeCamera() async {
    final cameras = await availableCameras();
    final frontCamera = cameras.firstWhere(
      (camera) => camera.lensDirection == CameraLensDirection.front,
      orElse: () => cameras.first,
    );

    _cameraController = CameraController(
      frontCamera,
      ResolutionPreset.high,
      enableAudio: true,
      imageFormatGroup: ImageFormatGroup.jpeg,
    );

    await _cameraController.initialize();

    // Initialize video recorder with advanced features
    _videoRecorder = VideoRecorder(_cameraController);
  }

  Future<String> startAdvancedRecording() async {
    if (_isRecording) return '';

    _isRecording = true;

    // Start video recording
    final videoPath = await _videoRecorder.startRecording();

    // Start real-time analysis if enabled
    if (_bodyLanguageAnalysisEnabled) {
      _startRealTimeBodyLanguageAnalysis();
    }

    if (_eyeContactTrackingEnabled) {
      _startEyeContactTracking();
    }

    return videoPath;
  }

  void _startRealTimeBodyLanguageAnalysis() {
    Timer.periodic(Duration(seconds: 2), (timer) {
      if (!_isRecording) {
        timer.cancel();
        return;
      }

      _analyzeCurrentFrame();
    });
  }

  Future<void> _analyzeCurrentFrame() async {
    if (!_cameraController.value.isInitialized) return;

    try {
      // Capture current frame
      final image = await _cameraController.takePicture();

      // Analyze body language using ML Kit or custom model
      final analysis = await BodyLanguageAnalyzer.analyzeFrame(image.path);

      // Provide real-time feedback
      if (analysis.postureScore < 70) {
        HapticFeedback.lightImpact();
        _showPostureReminder();
      }

      if (analysis.eyeContactScore < 50) {
        _showEyeContactReminder();
      }

    } catch (e) {
      print('Frame analysis error: $e');
    }
  }

  Future<void> _startEyeContactTracking() async {
    // Use device's face detection capabilities
    final faceDetector = GoogleMlKit.vision.faceDetector(
      FaceDetectorOptions(
        enableLandmarks: true,
        enableClassification: true,
        enableTracking: true,
      ),
    );

    Timer.periodic(Duration(seconds: 1), (timer) async {
      if (!_isRecording) {
        timer.cancel();
        return;
      }

      try {
        final image = await _cameraController.takePicture();
        final inputImage = InputImage.fromFilePath(image.path);
        final faces = await faceDetector.processImage(inputImage);

        for (final face in faces) {
          _analyzeEyeContact(face);
        }
      } catch (e) {
        print('Eye contact tracking error: $e');
      }
    });
  }

  void _analyzeEyeContact(Face face) {
    // Calculate eye contact based on head pose
    final rotX = face.headEulerAngleX ?? 0;
    final rotY = face.headEulerAngleY ?? 0;
    final rotZ = face.headEulerAngleZ ?? 0;

    // Good eye contact: head relatively straight
    final isGoodEyeContact = rotX.abs() < 15 && rotY.abs() < 20 && rotZ.abs() < 15;

    if (!isGoodEyeContact) {
      _provideLiveEyeContactFeedback();
    }
  }

  void _provideLiveEyeContactFeedback() {
    // Subtle vibration feedback
    HapticFeedback.selectionClick();

    // Optional: Show brief visual indicator
    Get.find<InterviewController>().showBriefFeedback("👁️ Eye contact");
  }

  Future<VideoAnalysisResult> stopRecordingAndAnalyze() async {
    if (!_isRecording) return VideoAnalysisResult.empty();

    _isRecording = false;

    final videoPath = await _videoRecorder.stopRecording();

    // Perform comprehensive video analysis
    final analysisResult = await VideoAnalyzer.analyzeFullVideo(
      videoPath,
      includeBodyLanguage: _bodyLanguageAnalysisEnabled,
      includeEyeContact: _eyeContactTrackingEnabled,
      includeExpressions: _expressionAnalysisEnabled,
    );

    return analysisResult;
  }
}
```

### Intelligent Push Notifications

```dart
// Smart notification system
class IntelligentNotificationService {
  late FlutterLocalNotificationsPlugin _notifications;
  final UserBehaviorAnalyzer _behaviorAnalyzer = UserBehaviorAnalyzer();

  Future<void> initialize() async {
    _notifications = FlutterLocalNotificationsPlugin();

    const initializationSettingsAndroid = AndroidInitializationSettings('@mipmap/ic_launcher');
    const initializationSettingsIOS = DarwinInitializationSettings();

    const initializationSettings = InitializationSettings(
      android: initializationSettingsAndroid,
      iOS: initializationSettingsIOS,
    );

    await _notifications.initialize(initializationSettings);

    // Start intelligent scheduling
    _scheduleSmartNotifications();
  }

  Future<void> _scheduleSmartNotifications() async {
    final userPreferences = await UserPreferences.getNotificationSettings();
    final behaviorPattern = await _behaviorAnalyzer.analyzeUserBehavior();

    // Schedule practice reminders based on user behavior
    _schedulePracticeReminders(behaviorPattern);

    // Schedule motivational notifications
    _scheduleMotivationalNotifications(behaviorPattern);

    // Schedule achievement notifications
    _scheduleAchievementNotifications();
  }

  Future<void> _schedulePracticeReminders(UserBehaviorPattern pattern) async {
    // Find optimal times based on user's most active periods
    final optimalTimes = pattern.mostActiveTimeSlots;

    for (int i = 0; i < optimalTimes.length && i < 3; i++) {
      final time = optimalTimes[i];

      await _notifications.zonedSchedule(
        i,
        'Time to Practice! 🎯',
        _generatePersonalizedPracticeMessage(pattern),
        _nextInstanceOfTime(time),
        const NotificationDetails(
          android: AndroidNotificationDetails(
            'practice_reminders',
            'Practice Reminders',
            channelDescription: 'Reminders to practice interview skills',
            importance: Importance.defaultImportance,
            priority: Priority.defaultPriority,
          ),
          iOS: DarwinNotificationDetails(),
        ),
        androidAllowWhileIdle: true,
        uiLocalNotificationDateInterpretation: UILocalNotificationDateInterpretation.absoluteTime,
        matchDateTimeComponents: DateTimeComponents.time,
      );
    }
  }

  String _generatePersonalizedPracticeMessage(UserBehaviorPattern pattern) {
    if (pattern.strugglingSkills.isNotEmpty) {
      final skill = pattern.strugglingSkills.first;
      return 'Ready to improve your $skill skills? A quick 10-minute session can make a difference!';
    }

    if (pattern.daysSinceLastSession > 3) {
      return 'You\'ve been away for ${pattern.daysSinceLastSession} days. Let\'s get back on track with a practice session!';
    }

    if (pattern.averageSessionLength < 300) { // Less than 5 minutes
      return 'Try a longer practice session today. Even 15 minutes can significantly improve your skills!';
    }

    return 'You\'re doing great! Keep the momentum going with another practice session.';
  }

  Future<void> scheduleStreakReminder() async {
    const streakMessages = [
      'Don\'t break your streak! 🔥 Quick practice session?',
      'Your streak is looking great! Keep it going! 💪',
      'Streak protection mode! One quick session to maintain momentum.',
    ];

    final randomMessage = streakMessages[Random().nextInt(streakMessages.length)];

    await _notifications.show(
      99,
      'Streak Alert!',
      randomMessage,
      const NotificationDetails(
        android: AndroidNotificationDetails(
          'streak_reminders',
          'Streak Reminders',
          channelDescription: 'Notifications to maintain practice streaks',
          importance: Importance.high,
          priority: Priority.high,
        ),
      ),
    );
  }

  Future<void> scheduleAchievementNotification(Achievement achievement) async {
    await _notifications.show(
      achievement.id.hashCode,
      'New Achievement Unlocked! 🏆',
      achievement.description,
      NotificationDetails(
        android: AndroidNotificationDetails(
          'achievements',
          'Achievements',
          channelDescription: 'Achievement notifications',
          importance: Importance.high,
          priority: Priority.high,
          styleInformation: BigTextStyleInformation(
            achievement.description,
            contentTitle: 'Achievement Unlocked!',
            summaryText: 'Tap to view your progress',
          ),
        ),
      ),
    );
  }
}
```

### Background Processing & State Management

```dart
// Background task management
class BackgroundTaskManager {
  static const _taskPort = 'background_tasks';
  late ReceivePort _receivePort;

  Future<void> initialize() async {
    // Register background task
    await Workmanager().initialize(callbackDispatcher, isInDebugMode: false);

    // Set up receive port for communication
    _receivePort = ReceivePort();
    IsolateNameServer.registerPortWithName(_receivePort.sendPort, _taskPort);

    _receivePort.listen(_handleBackgroundMessage);
  }

  void _handleBackgroundMessage(dynamic message) {
    if (message is Map<String, dynamic>) {
      switch (message['type']) {
        case 'sync_complete':
          _handleSyncComplete(message['data']);
          break;
        case 'processing_complete':
          _handleProcessingComplete(message['data']);
          break;
      }
    }
  }

  Future<void> schedulePeriodicSync() async {
    await Workmanager().registerPeriodicTask(
      'sync_task',
      'syncUserData',
      frequency: Duration(minutes: 15),
      constraints: Constraints(
        networkType: NetworkType.connected,
        requiresBatteryNotLow: true,
      ),
    );
  }

  Future<void> scheduleResponseProcessing(String audioPath, String questionId) async {
    await Workmanager().registerOneOffTask(
      'process_${DateTime.now().millisecondsSinceEpoch}',
      'processInterviewResponse',
      inputData: {
        'audioPath': audioPath,
        'questionId': questionId,
        'userId': await UserService.getCurrentUserId(),
      },
    );
  }
}

// Background task callback
void callbackDispatcher() {
  Workmanager().executeTask((task, inputData) async {
    switch (task) {
      case 'syncUserData':
        return await _performBackgroundSync();
      case 'processInterviewResponse':
        return await _processResponseInBackground(inputData!);
      default:
        return Future.value(true);
    }
  });
}

Future<bool> _performBackgroundSync() async {
  try {
    final syncManager = SyncManager();
    await syncManager.performSmartSync();

    // Notify UI
    final sendPort = IsolateNameServer.lookupPortByName('background_tasks');
    sendPort?.send({
      'type': 'sync_complete',
      'data': {'success': true, 'timestamp': DateTime.now().toIso8601String()}
    });

    return true;
  } catch (e) {
    return false;
  }
}
```

### Performance Optimization

```dart
// Mobile performance optimization
class MobilePerformanceOptimizer {
  static const int _maxCacheSize = 100 * 1024 * 1024; // 100MB
  static const int _maxAudioFiles = 50;

  Future<void> optimizeAppPerformance() async {
    await Future.wait([
      _optimizeImageCache(),
      _optimizeAudioCache(),
      _optimizeDatabase(),
      _precomputeFrequentQueries(),
    ]);
  }

  Future<void> _optimizeImageCache() async {
    final cacheManager = DefaultCacheManager();
    await cacheManager.emptyCache();

    // Pre-cache essential images
    final essentialImages = [
      'assets/images/interview_background.jpg',
      'assets/images/success_celebration.gif',
    ];

    for (final image in essentialImages) {
      await precacheImage(AssetImage(image), Get.context!);
    }
  }

  Future<void> _optimizeAudioCache() async {
    final audioDir = await getApplicationDocumentsDirectory();
    final audioFiles = audioDir.listSync()
        .where((file) => file.path.endsWith('.wav') || file.path.endsWith('.m4a'))
        .toList();

    // Sort by last modified time
    audioFiles.sort((a, b) =>
        File(b.path).lastModifiedSync().compareTo(File(a.path).lastModifiedSync()));

    // Keep only the most recent files
    if (audioFiles.length > _maxAudioFiles) {
      for (int i = _maxAudioFiles; i < audioFiles.length; i++) {
        await File(audioFiles[i].path).delete();
      }
    }
  }

  Future<void> _optimizeDatabase() async {
    final database = await OfflineDataManager().getDatabase();

    // Run VACUUM to reclaim space
    await database.execute('VACUUM');

    // Update statistics
    await database.execute('ANALYZE');

    // Clean old sync queue entries
    await database.execute('''
      DELETE FROM sync_queue
      WHERE created_at < ? AND retry_count > 5
    ''', [DateTime.now().subtract(Duration(days: 7)).millisecondsSinceEpoch]);
  }

  Future<void> _precomputeFrequentQueries() async {
    // Pre-compute and cache frequent analytics queries
    final commonQueries = [
      'user_recent_progress',
      'question_categories',
      'user_achievements',
    ];

    for (final query in commonQueries) {
      await _executeAndCacheQuery(query);
    }
  }

  Future<void> adaptToDeviceCapabilities() async {
    final deviceInfo = await DeviceInfoPlugin().androidInfo;
    final totalMemory = deviceInfo.totalMemory;

    // Adjust processing based on device capabilities
    if (totalMemory < 2 * 1024 * 1024 * 1024) { // Less than 2GB RAM
      // Reduce background processing
      BackgroundTaskManager.setLowMemoryMode(true);

      // Disable real-time analysis features
      OfflineAIProcessor.setPerformanceMode(PerformanceMode.batteryOptimized);

      // Reduce cache sizes
      _adjustCacheSizes(0.5);
    } else if (totalMemory > 6 * 1024 * 1024 * 1024) { // More than 6GB RAM
      // Enable all premium features
      OfflineAIProcessor.setPerformanceMode(PerformanceMode.highPerformance);

      // Increase cache sizes
      _adjustCacheSizes(1.5);
    }
  }
}
```

## Definition of Done

- [ ] Complete offline functionality for core interview features without internet connection
- [ ] Smart synchronization handles data conflicts without loss in 99.9% of cases
- [ ] Voice-only interviews provide full feature parity with visual interface
- [ ] Mobile app maintains 60fps performance on devices released in last 3 years
- [ ] Battery usage remains under 5% per hour during typical interview sessions
- [ ] Offline AI processing provides basic analysis within 10 seconds of response completion
- [ ] Advanced camera features accurately track eye contact and body language
- [ ] Intelligent notifications improve user engagement by 35%+
- [ ] App size including offline content remains under 150MB
- [ ] Crash rate stays below 0.1% across all supported devices
- [ ] Mobile app store ratings improve to 4.6+ stars
- [ ] Background processing continues seamlessly when app is not in foreground

---

**Epic Owner**: Senior Mobile Developer & UX Designer
**Stakeholders**: Product Owner, AI/ML Engineer, DevOps Team
**Next Epic**: EPIC-11 Global Expansion & Localization
