using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ScreenRecorderLib;

namespace GoogleMeetRecorder
{
    public class RecordingManager
    {
        // Settings
        public string TargetFolder { get; set; } = "";
        public string FinalFileName { get; set; } = "";
        public IntPtr? WindowHandleToRecord { get; set; } = null;
        public bool CaptureMicrophone { get; set; } = true;
        public bool CaptureSystemAudio { get; set; } = true;
        public int SegmentDurationMinutes { get; set; } = 10;
        public Rectangle? CropRectWindow { get; set; } = null;
        public Rectangle? CropRectScreen { get; set; } = null;

        // State
        private Recorder? _currentRecorder;
        private RecordingSourceBase? _activeSource;
        private string? _tempFolder;
        private List<string> _segmentFiles = new List<string>();
        private int _currentSegmentIndex = 1;
        private bool _isRecording = false;
        private bool _isRotating = false;
        private System.Windows.Forms.Timer? _segmentTimer;
        private readonly object _lock = new object();

        // Startup watchdog: detects a segment whose capture session silently never starts
        // producing frames (observed with WindowsGraphicsCapture when a new session is requested
        // for the same window right after the previous one closed). Without this, a stalled
        // segment would sit idle undetected until the next full SegmentDurationMinutes tick,
        // wasting an entire segment's worth of recording time.
        private const int StartupWatchdogTimeoutMs = 10000;
        private System.Threading.Timer? _startupWatchdogTimer;
        private bool _currentSegmentReachedRecording = false;

        // Events
        public event EventHandler<string>? OnStatusMessage;
        public event EventHandler? OnRecordingStarted;
        public event EventHandler? OnRecordingStopped;
        public event EventHandler<string>? OnRecordingError;

        public bool IsRecording => _isRecording;

        private static readonly string DebugLogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "recorder_debug.log");
        private static readonly object _logFileLock = new object();

        private void Log(string message)
        {
            string line = $"[{DateTime.Now:HH:mm:ss}] {message}";
            OnStatusMessage?.Invoke(this, line);

            // Also persist to a file next to the exe so segment-rotation failures can be diagnosed
            // after the fact - the in-app log box is lost once the process exits or is closed.
            try
            {
                lock (_logFileLock)
                {
                    File.AppendAllText(DebugLogPath, line + Environment.NewLine);
                }
            }
            catch { /* Diagnostic logging must never break recording. */ }
        }

        public void Start()
        {
            lock (_lock)
            {
                if (_isRecording)
                {
                    Log("Recording is already in progress.");
                    return;
                }

                if (string.IsNullOrWhiteSpace(TargetFolder))
                {
                    OnRecordingError?.Invoke(this, "Target folder path is not set.");
                    return;
                }

                if (string.IsNullOrWhiteSpace(FinalFileName))
                {
                    OnRecordingError?.Invoke(this, "Final file name is not set.");
                    return;
                }

                if (!Directory.Exists(TargetFolder))
                {
                    try
                    {
                        Directory.CreateDirectory(TargetFolder);
                    }
                    catch (Exception ex)
                    {
                        OnRecordingError?.Invoke(this, $"Failed to create target directory: {ex.Message}");
                        return;
                    }
                }

                _isRecording = true;
                _isRotating = false;
                _currentSegmentIndex = 1;
                _segmentFiles.Clear();

                // Create a unique temp folder inside TargetFolder
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                _tempFolder = Path.Combine(TargetFolder, $".tmp_rec_{timestamp}");
                try
                {
                    Directory.CreateDirectory(_tempFolder);
                }
                catch (Exception ex)
                {
                    _isRecording = false;
                    OnRecordingError?.Invoke(this, $"Failed to create temporary directory: {ex.Message}");
                    return;
                }

                Log($"Started recording session. Temp directory: {_tempFolder}");

                // Start the first segment
                StartSegment();

                // Start segment rotation timer
                _segmentTimer = new System.Windows.Forms.Timer();
                _segmentTimer.Interval = SegmentDurationMinutes * 60 * 1000;
                _segmentTimer.Tick += SegmentTimer_Tick;
                _segmentTimer.Start();

                OnRecordingStarted?.Invoke(this, EventArgs.Empty);
            }
        }

        public void Stop()
        {
            bool shouldStopRecorder = false;
            lock (_lock)
            {
                if (!_isRecording) return;

                Log("Stopping recording. Merging segments, please wait...");
                _isRecording = false;

                if (_segmentTimer != null)
                {
                    _segmentTimer.Stop();
                    _segmentTimer.Dispose();
                    _segmentTimer = null;
                }

                if (_currentRecorder != null)
                {
                    _isRotating = false;
                    shouldStopRecorder = true;
                }
                else
                {
                    Task.Run(() => MergeAndCleanup());
                }
            }

            if (shouldStopRecorder && _currentRecorder != null)
            {
                try
                {
                    _currentRecorder.Stop();
                }
                catch (Exception ex)
                {
                    Log($"Error stopping recorder: {ex.Message}");
                    Task.Run(() => MergeAndCleanup());
                }
            }
        }

        private void StartSegment()
        {
            string? segmentPath = null;
            Recorder? recorder = null;
            try
            {
                lock (_lock)
                {
                    if (_tempFolder == null || !_isRecording) return;
                    segmentPath = Path.Combine(_tempFolder, $"part_{_currentSegmentIndex:D4}.mp4");
                }

                Log($"Initializing segment {_currentSegmentIndex}: {Path.GetFileName(segmentPath)}");

                RecorderOptions options = ConfigureOptions();

                // Create Recorder OUTSIDE the lock
                recorder = Recorder.CreateRecorder(options);
                recorder.OnRecordingComplete += Recorder_OnRecordingComplete;
                recorder.OnRecordingFailed += Recorder_OnRecordingFailed;
                recorder.OnStatusChanged += Recorder_OnStatusChanged;

                lock (_lock)
                {
                    if (!_isRecording)
                    {
                        FireAndForgetDispose(recorder);
                        return;
                    }
                    _currentRecorder = recorder;
                }

                // Start recording OUTSIDE the lock
                recorder.Record(segmentPath);

                // Arm the watchdog so a session that never reports Recording (silently stalled)
                // gets caught and retried in seconds instead of after the full segment duration.
                ArmStartupWatchdog(recorder);
            }
            catch (Exception ex)
            {
                Log($"Failed to start segment {_currentSegmentIndex}: {ex.Message}");
                OnRecordingError?.Invoke(this, $"Start failed: {ex.Message}");

                if (recorder != null)
                {
                    FireAndForgetDispose(recorder);
                }

                lock (_lock)
                {
                    if (_currentRecorder == recorder)
                    {
                        _currentRecorder = null;
                        _activeSource = null;
                    }
                }

                // Auto-recover after a short delay
                bool proceed = false;
                lock (_lock)
                {
                    if (_isRecording) proceed = true;
                }
                if (proceed)
                {
                    Task.Run(async () =>
                    {
                        await Task.Delay(2000);
                        bool proceedAgain = false;
                        lock (_lock)
                        {
                            if (_isRecording) proceedAgain = true;
                        }
                        if (proceedAgain)
                        {
                            lock (_lock)
                            {
                                _currentSegmentIndex++;
                            }
                            StartSegment();
                        }
                    });
                }
            }
        }

        private RecorderOptions ConfigureOptions()
        {
            var options = new RecorderOptions();

            // Audio configuration
            options.AudioOptions = new AudioOptions
            {
                IsAudioEnabled = CaptureSystemAudio || CaptureMicrophone,
                IsOutputDeviceEnabled = CaptureSystemAudio,
                IsInputDeviceEnabled = CaptureMicrophone,
                AudioOutputDevice = null, // System default
                AudioInputDevice = null,  // System default
                OutputVolume = 1.0f,
                InputVolume = 1.0f,
                Bitrate = AudioBitrate.bitrate_128kbps,
                Channels = AudioChannels.Stereo
            };

            // Video configuration
            options.VideoEncoderOptions = new VideoEncoderOptions
            {
                Bitrate = 3000000, // 3 Mbps for decent 1080p/720p quality
                Framerate = 30,
                IsHardwareEncodingEnabled = true
            };

            // Mouse cursor configuration (Hide cursor and clicks from recorded video)
            options.MouseOptions = new MouseOptions
            {
                IsMousePointerEnabled = false,
                IsMouseClicksDetected = false
            };

            // Output layout configuration
            options.OutputOptions = new OutputOptions
            {
                OutputFrameSize = new ScreenSize(1920, 1080),
                Stretch = StretchMode.Uniform
            };

            // Source configuration: prefer capturing the app window itself (so recording follows
            // Meet's content even if the window is covered by another program), falling back to
            // display capture only if the window can't be found. Cropping is applied via
            // SourceRect on whichever source is chosen.
            options.SourceOptions = new SourceOptions();
            RecordingSourceBase? source = null;

            if (WindowHandleToRecord.HasValue && WindowHandleToRecord.Value != IntPtr.Zero)
            {
                var windows = Recorder.GetWindows();
                var targetWindow = windows.FirstOrDefault(w => w.Handle == WindowHandleToRecord.Value);
                if (targetWindow != null)
                {
                    // WindowRecordingSource.RecorderApi is read-only in the installed ScreenRecorderLib
                    // version - it is always WindowsGraphicsCapture for window sources (the only API
                    // that supports capturing a window's own content; Desktop Duplication only
                    // supports screens), so there is nothing to set here explicitly.

                    if (CropRectWindow.HasValue)
                    {
                        var crop = CropRectWindow.Value;
                        if (crop.Width > 0 && crop.Height > 0)
                        {
                            targetWindow.SourceRect = new ScreenRect(crop.X, crop.Y, crop.Width, crop.Height);
                            Log($"視窗錄影範圍已裁剪至瀏覽器區域: X={crop.X}, Y={crop.Y}, {crop.Width}x{crop.Height}");
                        }
                    }
                    Log($"Recording target window: '{targetWindow.Title}' (API: {targetWindow.RecorderApi})");
                    source = targetWindow;
                }
                else
                {
                    Log("Warning: Target window handle not found in recordable windows. Defaulting to full screen.");
                }
            }

            if (source == null)
            {
                Log("Recording primary monitor (full screen).");
                var displays = Recorder.GetDisplays();
                var primaryDisplay = displays.FirstOrDefault();
                if (primaryDisplay != null)
                {
                    if (CropRectScreen.HasValue)
                    {
                        var crop = CropRectScreen.Value;
                        if (crop.Width > 0 && crop.Height > 0)
                        {
                            primaryDisplay.SourceRect = new ScreenRect(crop.X, crop.Y, crop.Width, crop.Height);
                            Log($"全螢幕錄影範圍已裁剪至瀏覽器區域: X={crop.X}, Y={crop.Y}, {crop.Width}x{crop.Height}");
                        }
                    }
                    Log($"Selected display for recording: {primaryDisplay.DeviceName}");
                    source = primaryDisplay;
                }
                else
                {
                    Log("Warning: No displays found to record.");
                }
            }

            if (source != null)
            {
                options.SourceOptions.RecordingSources = new List<RecordingSourceBase> { source };
                _activeSource = source;
            }

            return options;
        }

        private void SegmentTimer_Tick(object? sender, EventArgs e)
        {
            bool shouldStop = false;
            lock (_lock)
            {
                if (!_isRecording) return;

                Log("Segment duration reached. Rotating segment...");
                _isRotating = true;

                if (_currentRecorder != null)
                {
                    shouldStop = true;
                }
                else
                {
                    _isRotating = false;
                    _currentSegmentIndex++;
                    StartSegment();
                }
            }

            if (shouldStop && _currentRecorder != null)
            {
                try
                {
                    _currentRecorder.Stop();
                }
                catch (Exception ex)
                {
                    Log($"Error during segment rotation stop: {ex.Message}");
                    HandleRecordingFailure("Rotation stop failed");
                }
            }
        }

        private void Recorder_OnStatusChanged(object? sender, RecordingStatusEventArgs e)
        {
            Log($"Recorder status: {e.Status}");

            if (e.Status == RecorderStatus.Recording)
            {
                lock (_lock)
                {
                    _currentSegmentReachedRecording = true;
                }
                DisarmStartupWatchdog();
            }
        }

        private void ArmStartupWatchdog(Recorder recorder)
        {
            DisarmStartupWatchdog();

            lock (_lock)
            {
                _currentSegmentReachedRecording = false;
                _startupWatchdogTimer = new System.Threading.Timer(
                    _ => OnStartupWatchdogElapsed(recorder),
                    null,
                    StartupWatchdogTimeoutMs,
                    System.Threading.Timeout.Infinite);
            }
        }

        private void DisarmStartupWatchdog()
        {
            System.Threading.Timer? old;
            lock (_lock)
            {
                old = _startupWatchdogTimer;
                _startupWatchdogTimer = null;
            }
            old?.Dispose();
        }

        private void OnStartupWatchdogElapsed(Recorder recorder)
        {
            bool isStillStalled;
            lock (_lock)
            {
                isStillStalled = _isRecording && _currentRecorder == recorder && !_currentSegmentReachedRecording;
            }

            if (isStillStalled)
            {
                Log("偵測到本段錄影逾時仍未真正開始擷取畫面（可能是視窗擷取工作階段尚未釋放），強制中止並立即重試，避免浪費整段錄影時間。");
                HandleRecordingFailure("Segment failed to reach Recording status within timeout");
            }
        }

        private void Recorder_OnRecordingFailed(object? sender, RecordingFailedEventArgs e)
        {
            HandleRecordingFailure(e.Error);
        }

        private void HandleRecordingFailure(string error)
        {
            Log($"Recorder encountered an error: {error}");
            OnRecordingError?.Invoke(this, $"Recording error: {error}");

            bool shouldResume = false;
            lock (_lock)
            {
                if (_isRecording)
                {
                    shouldResume = true;
                    if (_segmentFiles.Count > 0)
                    {
                        _currentSegmentIndex++;
                    }
                }
            }

            CleanupCurrentRecorder();

            if (shouldResume)
            {
                Log("Attempting to auto-recover and resume recording...");
                Task.Run(async () =>
                {
                    await Task.Delay(1000);
                    bool proceed = false;
                    lock (_lock)
                    {
                        if (_isRecording) proceed = true;
                    }
                    if (proceed)
                    {
                        StartSegment();
                    }
                });
            }
        }

        private void Recorder_OnRecordingComplete(object? sender, RecordingCompleteEventArgs e)
        {
            string completedPath = e.FilePath;
            Log($"Segment completed: {Path.GetFileName(completedPath)}");

            bool shouldStartNext = false;
            bool shouldMerge = false;

            lock (_lock)
            {
                if (File.Exists(completedPath))
                {
                    _segmentFiles.Add(completedPath);
                }
                else
                {
                    Log($"Warning: Completed segment file {Path.GetFileName(completedPath)} is missing.");
                }

                if (_isRecording && _isRotating)
                {
                    _isRotating = false;
                    _currentSegmentIndex++;
                    shouldStartNext = true;
                }
                else if (!_isRecording)
                {
                    shouldMerge = true;
                }
            }

            CleanupCurrentRecorder();

            if (shouldStartNext)
            {
                Log($"Initializing segment {_currentSegmentIndex}: part_{_currentSegmentIndex:D4}.mp4");
                StartSegment();
            }
            else if (shouldMerge)
            {
                Task.Run(() => MergeAndCleanup());
            }
        }

        private void CleanupCurrentRecorder()
        {
            Recorder? recorderToDispose = null;
            lock (_lock)
            {
                if (_currentRecorder != null)
                {
                    _currentRecorder.OnRecordingComplete -= Recorder_OnRecordingComplete;
                    _currentRecorder.OnRecordingFailed -= Recorder_OnRecordingFailed;
                    _currentRecorder.OnStatusChanged -= Recorder_OnStatusChanged;
                    recorderToDispose = _currentRecorder;
                    _currentRecorder = null;
                }
                _activeSource = null;
            }

            if (recorderToDispose != null)
            {
                FireAndForgetDispose(recorderToDispose);
            }
        }

        // Recorder.Dispose() has been observed to hang indefinitely for a WindowRecordingSource
        // (self-window WindowsGraphicsCapture) session once a segment finishes recording - this
        // reproduces on every rotation, on every thread tried (background callback thread as well
        // as the UI thread via Control.Invoke), so it is not a thread-affinity issue but an actual
        // hang inside the library/native capture teardown. Blocking on it stalls segment rotation
        // forever (or freezes the whole UI if done on that thread), so instead of awaiting it,
        // fire it off and move on - Stop() has already completed by this point (status reached
        // Idle), so the capture resources are presumably already released in practice even though
        // the managed Dispose() call itself never returns.
        private void FireAndForgetDispose(Recorder recorder)
        {
            Task.Run(() =>
            {
                try
                {
                    recorder.Dispose();
                }
                catch (Exception ex)
                {
                    Log($"Error disposing recorder (background): {ex.Message}");
                }
            });
        }

        private void MergeAndCleanup()
        {
            string finalPath = Path.Combine(TargetFolder, FinalFileName);
            if (!finalPath.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase))
            {
                finalPath += ".mp4";
            }

            Log("Processing recorded segments...");

            List<string> filesToMerge;
            string? tempDir;
            lock (_lock)
            {
                // Filter files that actually exist and have content now that the recorder is disposed
                filesToMerge = _segmentFiles.Where(f => File.Exists(f) && new FileInfo(f).Length > 0).ToList();
                tempDir = _tempFolder;
            }

            if (filesToMerge.Count == 0)
            {
                Log("No valid segments recorded. Nothing to merge.");
                OnRecordingStopped?.Invoke(this, EventArgs.Empty);
                return;
            }

            try
            {
                if (filesToMerge.Count == 1)
                {
                    Log("Single segment recorded. Moving file directly...");
                    if (File.Exists(finalPath))
                    {
                        File.Delete(finalPath);
                    }
                    File.Move(filesToMerge[0], finalPath);
                    Log($"Recording saved successfully to: {finalPath}");
                }
                else
                {
                    Log($"Merging {filesToMerge.Count} segments into: {finalPath}");
                    bool success = ConcatSegmentsFFmpeg(filesToMerge, tempDir, finalPath);

                    if (success)
                    {
                        Log($"Merged file saved successfully to: {finalPath}");
                    }
                    else
                    {
                        Log("Failed to merge segments using FFmpeg. Segments have been preserved in the temp folder.");
                        OnRecordingError?.Invoke(this, "FFmpeg merge failed. Raw segments preserved.");
                    }
                }

                // Delete temp folder if all operations completed successfully
                if (tempDir != null && Directory.Exists(tempDir))
                {
                    try
                    {
                        Directory.Delete(tempDir, true);
                        Log("Cleaned up temporary segment files.");
                    }
                    catch (Exception ex)
                    {
                        Log($"Failed to delete temporary directory: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"Error during post-processing: {ex.Message}");
                OnRecordingError?.Invoke(this, $"Post-processing error: {ex.Message}");
            }
            finally
            {
                OnRecordingStopped?.Invoke(this, EventArgs.Empty);
            }
        }

        private bool ConcatSegmentsFFmpeg(List<string> segments, string? tempDir, string outputFilePath)
        {
            if (tempDir == null || !Directory.Exists(tempDir)) return false;

            string listFilePath = Path.Combine(tempDir, "concat_list.txt");
            try
            {
                // Write segment paths to text file for FFmpeg concat demuxer
                using (var writer = new StreamWriter(listFilePath))
                {
                    foreach (var segment in segments)
                    {
                        // FFmpeg requires forward slashes or escaped backslashes in concat files
                        string formattedPath = segment.Replace("\\", "/");
                        writer.WriteLine($"file '{formattedPath}'");
                    }
                }

                Log("Running FFmpeg concat demuxer...");
                
                var startInfo = new ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    Arguments = $"-y -f concat -safe 0 -i \"{listFilePath}\" -c copy \"{outputFilePath}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true
                };

                using (var process = Process.Start(startInfo))
                {
                    if (process == null)
                    {
                        Log("Could not start FFmpeg process.");
                        return false;
                    }

                    string errorOutput = process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    if (process.ExitCode == 0)
                    {
                        return true;
                    }
                    else
                    {
                        Log($"FFmpeg exited with code {process.ExitCode}. Error details:");
                        Log(errorOutput);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"FFmpeg execution exception: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Updates the crop rectangle of the currently running recorder in place, without
        /// stopping/restarting the segment. Used to react to window moves/resizes. Safe to call
        /// frequently since it never touches segment state.
        /// </summary>
        public bool UpdateCropRectLive(Rectangle? cropWindow, Rectangle? cropScreen)
        {
            Recorder? recorder;
            RecordingSourceBase? source;
            lock (_lock)
            {
                if (!_isRecording || _currentRecorder == null || _activeSource == null) return false;
                recorder = _currentRecorder;
                source = _activeSource;
            }

            Rectangle? crop = source is WindowRecordingSource ? cropWindow : cropScreen;
            if (!crop.HasValue || crop.Value.Width <= 0 || crop.Value.Height <= 0) return false;

            try
            {
                var rect = crop.Value;
                source.SourceRect = new ScreenRect(rect.X, rect.Y, rect.Width, rect.Height);
                return recorder.GetDynamicOptionsBuilder()
                    .SetUpdatedRecordingSource(source)
                    .Apply();
            }
            catch (Exception ex)
            {
                Log($"Live crop update failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Attempts to recover and merge segments from a previously crashed recording session.
        /// </summary>
        public static bool RecoverCrashedSession(string tmpFolder, string targetFolder, string finalFileName, Action<string> logAction)
        {
            try
            {
                if (!Directory.Exists(tmpFolder))
                {
                    logAction($"Directory {tmpFolder} does not exist.");
                    return false;
                }

                // Find all part_*.mp4 files in alphabetical order
                var segments = Directory.GetFiles(tmpFolder, "part_*.mp4")
                                        .OrderBy(f => f)
                                        .ToList();

                if (segments.Count == 0)
                {
                    logAction("No mp4 segments found in the directory.");
                    return false;
                }

                string finalPath = Path.Combine(targetFolder, finalFileName);
                if (!finalPath.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase))
                {
                    finalPath += ".mp4";
                }

                logAction($"Found {segments.Count} segments for recovery.");

                if (segments.Count == 1)
                {
                    if (File.Exists(finalPath))
                    {
                        File.Delete(finalPath);
                    }
                    File.Move(segments[0], finalPath);
                    logAction($"Recovered file saved directly to: {finalPath}");
                }
                else
                {
                    string listFilePath = Path.Combine(tmpFolder, "concat_list.txt");
                    using (var writer = new StreamWriter(listFilePath))
                    {
                        foreach (var segment in segments)
                        {
                            string formattedPath = segment.Replace("\\", "/");
                            writer.WriteLine($"file '{formattedPath}'");
                        }
                    }

                    logAction("Executing FFmpeg recovery concat...");
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = "ffmpeg",
                        Arguments = $"-y -f concat -safe 0 -i \"{listFilePath}\" -c copy \"{finalPath}\"",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardError = true,
                        RedirectStandardOutput = true
                    };

                    using (var process = Process.Start(startInfo))
                    {
                        if (process == null) return false;
                        string errorOutput = process.StandardError.ReadToEnd();
                        process.WaitForExit();

                        if (process.ExitCode != 0)
                        {
                            logAction($"FFmpeg recovery failed with exit code {process.ExitCode}. Error details:");
                            logAction(errorOutput);
                            return false;
                        }
                    }
                    logAction($"Successfully recovered and merged file: {finalPath}");
                }

                // Clean up temp folder
                try
                {
                    Directory.Delete(tmpFolder, true);
                    logAction("Cleaned up temporary files after successful recovery.");
                }
                catch (Exception ex)
                {
                    logAction($"Warning: Clean up of temporary folder failed: {ex.Message}");
                }

                return true;
            }
            catch (Exception ex)
            {
                logAction($"Recovery exception: {ex.Message}");
                return false;
            }
        }
    }
}
