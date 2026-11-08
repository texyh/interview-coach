namespace AIPA.Domain.Interviews.ValueObjects;

public class SessionRecordingMetadata
{
    public string? AudioFilePath { get; private set; }
    public string? VideoFilePath { get; private set; }
    public string? TranscriptFilePath { get; private set; }
    public long AudioFileSize { get; private set; }
    public long VideoFileSize { get; private set; }
    public TimeSpan RecordingDuration { get; private set; }
    public string AudioFormat { get; private set; } = "wav";
    public string VideoFormat { get; private set; } = "mp4";
    public DateTime? RecordingStartedAt { get; private set; }
    public DateTime? RecordingCompletedAt { get; private set; }
    public bool IsEncrypted { get; private set; }
    public string? StorageProvider { get; private set; }
    public Dictionary<string, string> Metadata { get; private set; } = new();

    private SessionRecordingMetadata() { }

    public static SessionRecordingMetadata Create()
    {
        return new SessionRecordingMetadata();
    }

    public void StartRecording()
    {
        RecordingStartedAt = DateTime.UtcNow;
    }

    public void CompleteRecording(TimeSpan duration)
    {
        RecordingCompletedAt = DateTime.UtcNow;
        RecordingDuration = duration;
    }

    public void SetAudioFile(string filePath, long fileSize, string format = "wav")
    {
        AudioFilePath = filePath;
        AudioFileSize = fileSize;
        AudioFormat = format;
    }

    public void SetVideoFile(string filePath, long fileSize, string format = "mp4")
    {
        VideoFilePath = filePath;
        VideoFileSize = fileSize;
        VideoFormat = format;
    }

    public void SetTranscriptFile(string filePath)
    {
        TranscriptFilePath = filePath;
    }

    public void SetStorageProvider(string provider)
    {
        StorageProvider = provider;
    }

    public void SetEncryption(bool isEncrypted)
    {
        IsEncrypted = isEncrypted;
    }

    public void AddMetadata(string key, string value)
    {
        Metadata[key] = value;
    }
}
