using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using Microsoft.Web.WebView2.Core;

namespace GoogleMeetRecorder
{
    public partial class Form1 : Form
    {
        // ffmpeg.exe (~90MB) is not checked into source control - it exceeds GitHub's web-upload
        // limit - so it's fetched automatically on first run instead. See EnsureFfmpegAvailableAsync.
        private const string FfmpegDownloadUrl = "https://www.gyan.dev/ffmpeg/builds/ffmpeg-release-essentials.zip";

        private RecordingManager _recordingManager;
        private System.Windows.Forms.Timer _schedulerTimer;
        private DateTime _recordingStartTime;
        private System.Windows.Forms.Timer _resizeDebounceTimer;
        private bool _closingAfterRecordingStop = false;
        private bool _ffmpegReady = false;

        public Form1()
        {
            InitializeComponent();
            _recordingManager = new RecordingManager();
            
            // Wire up RecordingManager events
            _recordingManager.OnStatusMessage += RecordingManager_OnStatusMessage;
            _recordingManager.OnRecordingStarted += RecordingManager_OnRecordingStarted;
            _recordingManager.OnRecordingStopped += RecordingManager_OnRecordingStopped;
            _recordingManager.OnRecordingError += RecordingManager_OnRecordingError;

            // Wire up scheduler timer
            _schedulerTimer = new System.Windows.Forms.Timer();
            _schedulerTimer.Interval = 1000; // Check every second
            _schedulerTimer.Tick += SchedulerTimer_Tick;
            _schedulerTimer.Start();

            // Set up resize debounce timer and wire window change events
            _resizeDebounceTimer = new System.Windows.Forms.Timer();
            _resizeDebounceTimer.Interval = 500; // Wait 500ms after last layout change before recalculating crop
            _resizeDebounceTimer.Tick += ResizeDebounceTimer_Tick;

            this.Resize += Form1_Resize;
            this.LocationChanged += Form1_LocationChanged;

            // Set up key preview for hotkey support
            this.KeyPreview = true;
            this.KeyDown += Form1_KeyDown;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Recording must not be allowed to start until ffmpeg.exe is confirmed present -
            // otherwise any recording longer than one segment (10 min) would fail silently at
            // merge time. Re-enabled once EnsureFfmpegAvailableAsync confirms/downloads it.
            btnStartRecord.Enabled = false;

            // Set default output directory to _Record folder under executable directory
            string defaultPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "_Record");
            txtSavePath.Text = defaultPath;

            // Set default scheduled times
            dtpStart.Value = DateTime.Now;
            dtpEnd.Value = DateTime.Now.AddHours(1);

            // Load last meeting ID from file if exists
            string lastMeetPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "last_meet_id.txt");
            if (File.Exists(lastMeetPath))
            {
                try
                {
                    string lastMeetId = File.ReadAllText(lastMeetPath).Trim();
                    if (!string.IsNullOrEmpty(lastMeetId))
                    {
                        txtUrl.Text = lastMeetId;
                        Log($"已載入上一次使用的會議 ID/網址: {lastMeetId}");
                    }
                }
                catch (Exception ex)
                {
                    Log($"載入上次會議 ID 失敗: {ex.Message}");
                }
            }

            // Initialize WebView2
            InitializeWebView();

            Log("Google Meet Backup Recorder loaded.");
            Log("快速鍵提示: 按下 Ctrl + Alt + C 可以隨時顯示/隱藏控制台");

            _ = EnsureFfmpegAvailableAsync();
        }

        // Segments longer than SegmentDurationMinutes need to be merged via ffmpeg, which the app
        // invokes as a bare "ffmpeg" command (relies on it being found next to the exe or on PATH).
        // ffmpeg.exe itself is not checked into source control (~90MB, over GitHub's web-upload
        // limit), so if it's missing here it's downloaded automatically. The Start Recording button
        // stays disabled the whole time so nobody can start a recording that would silently fail to
        // merge once it grows past one segment.
        private async Task EnsureFfmpegAvailableAsync()
        {
            string targetPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg.exe");

            string? foundPath = FindFfmpegPath(targetPath);
            if (foundPath != null)
            {
                string source = string.Equals(foundPath, targetPath, StringComparison.OrdinalIgnoreCase)
                    ? "位於程式所在資料夾"
                    : "透過系統 PATH 環境變數找到";
                Log($"已偵測到 ffmpeg（{source}: {foundPath}），影片分段合併功能可正常使用。");
                _ffmpegReady = true;
                btnStartRecord.Enabled = true;
                return;
            }

            Log("找不到 ffmpeg.exe，開始自動下載中，請稍候（下載完成前無法開始錄影）...");
            bool success = await DownloadAndExtractFfmpegAsync(targetPath);

            if (success && File.Exists(targetPath))
            {
                Log("ffmpeg.exe 下載並安裝成功，影片分段合併功能可正常使用。");
                _ffmpegReady = true;
                btnStartRecord.Enabled = true;
            }
            else
            {
                Log("[錯誤] 自動下載 ffmpeg 失敗，無法開始錄影。請檢查網路連線，或手動將 ffmpeg.exe 放到程式所在資料夾後重新啟動程式。");
                MessageBox.Show(
                    "自動下載 ffmpeg 失敗。\n\n請檢查網路連線後重新啟動程式重試，或手動將 ffmpeg.exe 放到本程式所在的資料夾中。\n\n" +
                    "在此之前將無法開始錄影。",
                    "ffmpeg 下載失敗",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        // Returns the full path to ffmpeg.exe if found (either next to the exe or via a directory
        // listed in the PATH environment variable), so the caller can log exactly where it came
        // from. Returns null if not found anywhere.
        private string? FindFfmpegPath(string exeDirFfmpegPath)
        {
            if (File.Exists(exeDirFfmpegPath)) return exeDirFfmpegPath;

            string? pathEnv = Environment.GetEnvironmentVariable("PATH");
            if (!string.IsNullOrEmpty(pathEnv))
            {
                foreach (string dir in pathEnv.Split(Path.PathSeparator))
                {
                    try
                    {
                        if (string.IsNullOrWhiteSpace(dir)) continue;
                        string candidate = Path.Combine(dir.Trim(), "ffmpeg.exe");
                        if (File.Exists(candidate)) return candidate;
                    }
                    catch
                    {
                        // Ignore malformed PATH entries
                    }
                }
            }
            return null;
        }

        private async Task<bool> DownloadAndExtractFfmpegAsync(string targetPath)
        {
            string tempZipPath = Path.Combine(Path.GetTempPath(), $"ffmpeg_download_{Guid.NewGuid():N}.zip");
            try
            {
                using (var httpClient = new HttpClient())
                {
                    httpClient.Timeout = TimeSpan.FromMinutes(15);
                    using var response = await httpClient.GetAsync(FfmpegDownloadUrl, HttpCompletionOption.ResponseHeadersRead);
                    response.EnsureSuccessStatusCode();

                    using var httpStream = await response.Content.ReadAsStreamAsync();
                    using var fileStream = new FileStream(tempZipPath, FileMode.Create, FileAccess.Write, FileShare.None);
                    await httpStream.CopyToAsync(fileStream);
                }

                Log("ffmpeg 下載完成，正在解壓縮...");

                using (var archive = ZipFile.OpenRead(tempZipPath))
                {
                    var entry = archive.Entries.FirstOrDefault(en =>
                            en.FullName.Replace('\\', '/').EndsWith("/bin/ffmpeg.exe", StringComparison.OrdinalIgnoreCase))
                        ?? archive.Entries.FirstOrDefault(en =>
                            en.Name.Equals("ffmpeg.exe", StringComparison.OrdinalIgnoreCase));

                    if (entry == null)
                    {
                        Log("[錯誤] 下載的壓縮檔內找不到 ffmpeg.exe。");
                        return false;
                    }

                    entry.ExtractToFile(targetPath, overwrite: true);
                }

                return File.Exists(targetPath);
            }
            catch (Exception ex)
            {
                Log($"[錯誤] 自動下載/解壓縮 ffmpeg 失敗: {ex.Message}");
                return false;
            }
            finally
            {
                try { if (File.Exists(tempZipPath)) File.Delete(tempZipPath); } catch { }
            }
        }

        private async void InitializeWebView()
        {
            try
            {
                Log("正在初始化 WebView2 瀏覽器...");
                
                // Set persistent user data folder in LocalAppData to preserve cookies and login sessions
                string userDataFolder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "GoogleMeetRecorder",
                    "WebView2_UserData"
                );

                var env = await CoreWebView2Environment.CreateAsync(null, userDataFolder);
                await webView.EnsureCoreWebView2Async(env);

                // Inject Javascript to automatically enter name 'PERRON' and click join buttons
                string autoJoinScript = @"
                    if (!window._perronAutoJoinInterval) {
                        window._perronAutoJoinInterval = setInterval(function() {
                            if (window._perronJoined) {
                                clearInterval(window._perronAutoJoinInterval);
                                return;
                            }
                            
                            // Find guest name input field
                            const inputs = document.querySelectorAll('input');
                            let nameInput = null;
                            for (const input of inputs) {
                                const ariaLabel = (input.getAttribute('aria-label') || '').toLowerCase();
                                const placeholder = (input.getAttribute('placeholder') || '').toLowerCase();
                                if (ariaLabel.includes('name') || ariaLabel.includes('姓名') || 
                                    placeholder.includes('name') || placeholder.includes('姓名')) {
                                    nameInput = input;
                                    break;
                                }
                            }
                            
                            if (nameInput) {
                                if (nameInput.value !== 'PERRON') {
                                    nameInput.value = 'PERRON';
                                    nameInput.dispatchEvent(new Event('input', { bubbles: true }));
                                    nameInput.dispatchEvent(new Event('change', { bubbles: true }));
                                    console.log('Automated: Filled name PERRON');
                                }
                            }
                            
                            // Find join buttons (Ask to join / Join now / 要求加入 / 立即加入)
                            const buttons = document.querySelectorAll('button, [role=""button""]');
                            let joinButton = null;
                            const targetTexts = ['ask to join', 'join now', '要求加入', '立即加入'];
                            for (const btn of buttons) {
                                const text = (btn.textContent || '').trim().toLowerCase();
                                if (targetTexts.some(t => text.includes(t))) {
                                    joinButton = btn;
                                    break;
                                }
                            }
                            
                            if (joinButton) {
                                if (!nameInput || nameInput.value === 'PERRON') {
                                    // Check if button is disabled or aria-disabled
                                    const isDisabled = joinButton.disabled || joinButton.getAttribute('aria-disabled') === 'true';
                                    if (!isDisabled) {
                                        joinButton.click();
                                        window._perronJoined = true;
                                        clearInterval(window._perronAutoJoinInterval);
                                        console.log('Automated: Clicked join button');
                                    }
                                }
                            }
                        }, 100);
                    }
                ";

                await webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(autoJoinScript);
                
                Log("WebView2 瀏覽器初始化成功。已啟用 PERRON 自動加入功能。請在上方輸入會議 ID 並按「載入」開始。");
            }
            catch (Exception ex)
            {
                Log($"WebView2 初始化失敗: {ex.Message}");
                MessageBox.Show(
                    $"無法載入內嵌瀏覽器。\n請確認您的系統中已安裝 Microsoft Edge WebView2 Runtime。\n\n詳細錯誤: {ex.Message}",
                    "瀏覽器初始化錯誤",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void Log(string message)
        {
            if (txtLog.InvokeRequired)
            {
                txtLog.Invoke(new Action<string>(Log), message);
                return;
            }

            string logLine = $"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}";
            txtLog.AppendText(logLine);

            // Keep log size managed
            if (txtLog.TextLength > 50000)
            {
                txtLog.Text = txtLog.Text.Substring(20000);
            }

            // Scroll to bottom
            txtLog.SelectionStart = txtLog.TextLength;
            txtLog.ScrollToCaret();
        }

        // Web navigation
        private void BtnGo_Click(object sender, EventArgs e)
        {
            string input = txtUrl.Text.Trim();
            if (string.IsNullOrWhiteSpace(input)) return;

            // Save last used meeting ID/URL
            try
            {
                string lastMeetPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "last_meet_id.txt");
                File.WriteAllText(lastMeetPath, input);
            }
            catch (Exception ex)
            {
                Log($"儲存會議 ID 失敗: {ex.Message}");
            }

            string? meetId = ExtractMeetId(input);
            string url;

            if (meetId != null)
            {
                url = $"https://meet.google.com/{meetId}";
                txtUrl.Text = meetId; // Display clean meet ID in textbox
                Log($"偵測到會議 ID: {meetId}，正在導向: {url}");
            }
            else
            {
                // Fallback: If they provided a URL starting with http, use it directly
                if (input.StartsWith("http://") || input.StartsWith("https://"))
                {
                    url = input;
                }
                else
                {
                    url = "https://" + input;
                }
                Log($"無法識別標準會議 ID 格式，將嘗試導向原輸入網址: {url}");
            }

            try
            {
                webView.Source = new Uri(url);
            }
            catch (Exception ex)
            {
                Log($"網址格式錯誤: {ex.Message}");
            }
        }

        private string? ExtractMeetId(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return null;

            input = input.Trim();

            // Check if it matches exactly xxx-yyyy-zzz
            var idRegex = new System.Text.RegularExpressions.Regex(@"^[a-zA-Z]{3}-[a-zA-Z]{4}-[a-zA-Z]{3}$");
            if (idRegex.IsMatch(input))
            {
                return input.ToLower();
            }

            // Try to extract from URL. It could be:
            // https://meet.google.com/xxx-yyyy-zzz
            // meet.google.com/xxx-yyyy-zzz
            // https://meet.google.com/xxx-yyyy-zzz?authuser=0
            var urlRegex = new System.Text.RegularExpressions.Regex(@"meet\.google\.com/([a-zA-Z]{3}-[a-zA-Z]{4}-[a-zA-Z]{3})");
            var match = urlRegex.Match(input);
            if (match.Success && match.Groups.Count > 1)
            {
                return match.Groups[1].Value.ToLower();
            }

            // Fallback: search for any 3-4-3 sequence in the text
            var generalRegex = new System.Text.RegularExpressions.Regex(@"([a-zA-Z]{3}-[a-zA-Z]{4}-[a-zA-Z]{3})");
            var generalMatch = generalRegex.Match(input);
            if (generalMatch.Success)
            {
                return generalMatch.Groups[1].Value.ToLower();
            }

            return null;
        }

        // Save Path Selection
        private void BtnBrowse_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                fbd.Description = "請選擇錄影存檔資料夾";
                fbd.SelectedPath = txtSavePath.Text;
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    txtSavePath.Text = fbd.SelectedPath;
                    Log($"儲存路徑已變更為: {fbd.SelectedPath}");
                }
            }
        }

        // Manual Record Controls
        private void BtnStartRecord_Click(object sender, EventArgs e)
        {
            StartRecordingProcess();
        }

        private void BtnStopRecord_Click(object sender, EventArgs e)
        {
            StopRecordingProcess();
        }

        private void StartRecordingProcess()
        {
            if (_recordingManager.IsRecording) return;

            // Also guards the scheduled auto-start path (SchedulerTimer_Tick calls this directly,
            // bypassing the button's Enabled state).
            if (!_ffmpegReady)
            {
                Log("ffmpeg 尚未就緒（可能仍在下載中），暫時無法開始錄影。");
                return;
            }

            // Gather options from UI
            _recordingManager.TargetFolder = txtSavePath.Text.Trim();

            // Format filename with recording start time if not custom named
            string fileName = txtFileName.Text.Trim();
            if (string.IsNullOrWhiteSpace(fileName))
            {
                fileName = DateTime.Now.ToString("yyyyMMddHHmmss") + ".mp4";
            }
            if (!fileName.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase))
            {
                fileName += ".mp4";
            }
            _recordingManager.FinalFileName = fileName;

            _recordingManager.CaptureMicrophone = chkCaptureMic.Checked;
            _recordingManager.CaptureSystemAudio = chkCaptureSysAudio.Checked;

            // Window to capture: Record this Form (so the recording keeps following Meet's
            // content even if another window covers it on screen).
            _recordingManager.WindowHandleToRecord = this.Handle;

            // Crop to the whole webView area (the complete right-side Google Meet panel).
            RecalculateCropRects();

            // Start recording in segment mode
            _recordingManager.Start();
        }

        private void StopRecordingProcess()
        {
            if (!_recordingManager.IsRecording) return;
            _recordingManager.Stop();
        }

        // Toggle layouts (Disabled - Left panel stays visible at all times!)
        private void SetUICompactMode(bool compact)
        {
            // Do nothing
        }

        private void BtnShowControls_Click(object sender, EventArgs e)
        {
            SetUICompactMode(false);
        }

        // Hotkey trigger
        private void Form1_KeyDown(object? sender, KeyEventArgs e)
        {
            // Toggle panel with Ctrl + Alt + C
            if (e.Control && e.Alt && e.KeyCode == Keys.C)
            {
                SetUICompactMode(!panelControls.Visible);
                e.Handled = true;
            }
        }

        // Scheduler Timer Tick
        private void SchedulerTimer_Tick(object? sender, EventArgs e)
        {
            DateTime now = DateTime.Now;

            if (_recordingManager.IsRecording)
            {
                TimeSpan elapsed = now - _recordingStartTime;
                string elapsedStr = string.Format("{0:D2}:{1:D2}:{2:D2}", elapsed.Hours, elapsed.Minutes, elapsed.Seconds);

                if (chkEnableSchedule.Checked)
                {
                    DateTime endTime = dtpEnd.Value;
                    if (now >= endTime)
                    {
                        Log("排程結束時間已到，自動停止錄影...");
                        StopRecordingProcess();
                    }
                    else
                    {
                        TimeSpan timeLeft = endTime - now;
                        lblStatus.Text = $"🔴 錄影中 (排程) | 已錄: {elapsedStr} | 剩餘: {FormatTimeSpan(timeLeft)}";
                        lblStatus.ForeColor = Color.FromArgb(219, 68, 85);
                    }
                }
                else
                {
                    lblStatus.Text = $"🔴 錄影中 (手動) | 已錄: {elapsedStr}";
                    lblStatus.ForeColor = Color.FromArgb(219, 68, 85);
                }
                return;
            }

            // Not recording below
            if (!chkEnableSchedule.Checked)
            {
                lblStatus.Text = "狀態: 閒置";
                lblStatus.ForeColor = Color.FromArgb(170, 180, 190);
                return;
            }

            DateTime startTime = dtpStart.Value;
            DateTime endTimeSchedule = dtpEnd.Value;

            if (endTimeSchedule <= startTime)
            {
                lblStatus.Text = "排程錯誤: 結束時間必須晚於開始時間";
                lblStatus.ForeColor = Color.FromArgb(219, 68, 85);
                return;
            }

            if (now < startTime)
            {
                TimeSpan waitTime = startTime - now;
                lblStatus.Text = $"等待排程錄影中... 剩餘: {FormatTimeSpan(waitTime)}";
                lblStatus.ForeColor = Color.FromArgb(138, 180, 248);
            }
            else if (now >= startTime && now < endTimeSchedule)
            {
                Log("排程啟動時間已到，自動啟動錄影...");
                StartRecordingProcess();
            }
            else
            {
                lblStatus.Text = "排程已過期";
                lblStatus.ForeColor = Color.FromArgb(170, 180, 190);
            }
        }

        private string FormatTimeSpan(TimeSpan ts)
        {
            if (ts.TotalDays >= 1)
            {
                return $"{(int)ts.TotalDays}天 {ts.Hours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}";
            }
            return $"{ts.Hours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}";
        }

        private void ChkEnableSchedule_CheckedChanged(object sender, EventArgs e)
        {
            bool isChecked = chkEnableSchedule.Checked;
            dtpStart.Enabled = isChecked;
            dtpEnd.Enabled = isChecked;
            Log(isChecked ? "已啟用排程錄影。" : "已停用排程錄影，切換回手動控制。");
        }

        // Recording Manager Event Handlers
        private void RecordingManager_OnStatusMessage(object? sender, string message)
        {
            Log(message);
        }

        private void RecordingManager_OnRecordingStarted(object? sender, EventArgs e)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new EventHandler(RecordingManager_OnRecordingStarted), sender, e);
                return;
            }

            _recordingStartTime = DateTime.Now; // Record the starting time

            btnStartRecord.Enabled = false;
            btnStopRecord.Enabled = true;
            btnRecover.Enabled = false;
            chkEnableSchedule.AutoCheck = false;

            if (!chkEnableSchedule.Checked)
            {
                lblStatus.Text = "🔴 錄影中 (手動控制)";
                lblStatus.ForeColor = Color.FromArgb(219, 68, 85);
            }
        }

        private void RecordingManager_OnRecordingStopped(object? sender, EventArgs e)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new EventHandler(RecordingManager_OnRecordingStopped), sender, e);
                return;
            }

            btnStartRecord.Enabled = true;
            btnStopRecord.Enabled = false;
            btnRecover.Enabled = true;
            chkEnableSchedule.AutoCheck = true;

            lblStatus.Text = "狀態: 閒置";
            lblStatus.ForeColor = Color.FromArgb(170, 180, 190);

            // Restore UI panels
            SetUICompactMode(false);
            
            MessageBox.Show("錄影已結束且檔案處理完成！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void RecordingManager_OnRecordingError(object? sender, string error)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new EventHandler<string>(RecordingManager_OnRecordingError), sender, error);
                return;
            }

            Log($"[錯誤] {error}");
            lblStatus.Text = "錄影發生異常！";
            lblStatus.ForeColor = Color.FromArgb(219, 68, 85);
        }

        // Crash Recovery Button Click
        private void BtnRecover_Click(object sender, EventArgs e)
        {
            string searchFolder = txtSavePath.Text.Trim();
            if (!Directory.Exists(searchFolder))
            {
                MessageBox.Show("儲存路徑不存在，無法掃描暫存檔。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Find directories starting with ".tmp_rec_" in the save path
            var tmpDirs = Directory.GetDirectories(searchFolder, ".tmp_rec_*");
            if (tmpDirs.Length == 0)
            {
                // No temp folders found automatically, let the user browse manually
                DialogResult dr = MessageBox.Show(
                    "在此儲存路徑下沒有發現自動產生的錄影暫存資料夾。\n是否要手動瀏覽並選取其他暫存資料夾進行修復？",
                    "未發現暫存檔",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

                if (dr == DialogResult.Yes)
                {
                    using (var fbd = new FolderBrowserDialog())
                    {
                        fbd.Description = "請選取 .tmp_rec_ 開頭的錄影暫存資料夾";
                        if (fbd.ShowDialog() == DialogResult.OK)
                        {
                            RecoverDirectory(fbd.SelectedPath);
                        }
                    }
                }
                return;
            }

            // Present list of temp folders to the user
            string message = "發現以下異常中斷的錄影暫存資料夾，是否要進行合併修復？\n\n";
            foreach (var dir in tmpDirs)
            {
                message += $"- {Path.GetFileName(dir)}\n";
            }

            DialogResult mergeDr = MessageBox.Show(message, "發現可修復的錄影檔", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (mergeDr == DialogResult.Yes)
            {
                foreach (var dir in tmpDirs)
                {
                    RecoverDirectory(dir);
                }
            }
        }

        // Close Program Button Click
        private void BtnClose_Click(object? sender, EventArgs e)
        {
            this.Close();
        }

        // Gracefully clean up and force terminate on close to prevent hangs
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (_recordingManager != null && _recordingManager.IsRecording)
            {
                // Stop() only kicks off an async merge (ffmpeg runs on a background task) - if we
                // let the form close immediately, Environment.Exit(0) below would kill the process
                // before the merge finishes, leaving raw segments behind and no final file. So the
                // first close request is cancelled; we actually close once merging is confirmed done.
                if (_closingAfterRecordingStop)
                {
                    e.Cancel = true;
                    return;
                }

                DialogResult dr = MessageBox.Show("目前正在錄影中，確定要結束錄影並關閉程式嗎？", "確認關閉", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (dr == DialogResult.No)
                {
                    e.Cancel = true;
                    return;
                }

                Log("正在停止錄影並合併影片，請稍候...");
                _closingAfterRecordingStop = true;
                _recordingManager.OnRecordingStopped += RecordingManager_OnStoppedDuringClose;
                _recordingManager.Stop();

                e.Cancel = true;
                return;
            }

            // Clean up timers
            _schedulerTimer?.Stop();
            _schedulerTimer?.Dispose();
            _resizeDebounceTimer?.Stop();
            _resizeDebounceTimer?.Dispose();

            base.OnFormClosing(e);

            // Force exit to ensure all child processes and threads are terminated
            Environment.Exit(0);
        }

        private void RecordingManager_OnStoppedDuringClose(object? sender, EventArgs e)
        {
            _recordingManager.OnRecordingStopped -= RecordingManager_OnStoppedDuringClose;

            if (this.InvokeRequired)
            {
                this.Invoke(new Action(this.Close));
                return;
            }

            this.Close();
        }

        private void RecoverDirectory(string tempDir)
        {
            string dirName = Path.GetFileName(tempDir);
            string recoveredFileName = $"recovered_{dirName.Replace(".tmp_rec_", "")}.mp4";

            Log($"啟動暫存檔手動修復: {dirName}...");
            bool success = RecordingManager.RecoverCrashedSession(
                tempDir,
                txtSavePath.Text.Trim(),
                recoveredFileName,
                msg => Log(msg)
            );

            if (success)
            {
                MessageBox.Show(
                    $"資料夾 {dirName} 已成功復原並合併！\n存檔檔名：{recoveredFileName}",
                    "復原成功",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
            else
            {
                MessageBox.Show(
                    $"資料夾 {dirName} 復原失敗，請檢查執行日誌以獲取更多錯誤細節。",
                    "復原失敗",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void Form1_Resize(object? sender, EventArgs e)
        {
            HandleWindowPositionChange();
        }

        private void Form1_LocationChanged(object? sender, EventArgs e)
        {
            HandleWindowPositionChange();
        }

        private void HandleWindowPositionChange()
        {
            if (_recordingManager != null && _recordingManager.IsRecording)
            {
                _resizeDebounceTimer.Stop();
                _resizeDebounceTimer.Start();
            }
        }

        private void ResizeDebounceTimer_Tick(object? sender, EventArgs e)
        {
            _resizeDebounceTimer.Stop();

            if (_recordingManager != null && _recordingManager.IsRecording)
            {
                RecalculateCropRects();

                // Push the updated crop into the live recorder without stopping/restarting
                // the current segment, so repeated maximize/restore never interrupts recording.
                _recordingManager.UpdateCropRectLive(_recordingManager.CropRectWindow, _recordingManager.CropRectScreen);
            }
        }

        // Computes the crop rectangles covering the whole webView area - i.e. the complete
        // right-side Google Meet panel - both relative to the captured window (for window-source
        // cropping) and in primary-screen coordinates (for the full-screen fallback). Uses
        // WinForms' own RectangleToScreen rather than hand-rolled DPI math: that math was already
        // proven unreliable once before (it mis-positioned the region selector overlay, and later
        // made the display-capture crop grab desktop area outside the actual app window).
        private void RecalculateCropRects()
        {
            Rectangle webViewScreenRect = webView.RectangleToScreen(webView.ClientRectangle);

            // For a top-level Form, .Bounds is already expressed in screen coordinates, so it can
            // be directly subtracted from webViewScreenRect (also screen coordinates) to get the
            // webView's position relative to the captured window's own top-left - no manual
            // border/title-bar/DPI math needed.
            Rectangle formScreenRect = this.Bounds;
            _recordingManager.CropRectWindow = new Rectangle(
                Math.Max(0, webViewScreenRect.X - formScreenRect.X),
                Math.Max(0, webViewScreenRect.Y - formScreenRect.Y),
                webViewScreenRect.Width,
                webViewScreenRect.Height);

            // Clamp to the primary display's bounds so the crop stays valid even if the window is
            // partially off-screen or spans onto another monitor.
            Rectangle screenBounds = Screen.PrimaryScreen!.Bounds;
            int x = Math.Max(webViewScreenRect.X, screenBounds.X);
            int y = Math.Max(webViewScreenRect.Y, screenBounds.Y);
            int right = Math.Min(webViewScreenRect.Right, screenBounds.Right);
            int bottom = Math.Min(webViewScreenRect.Bottom, screenBounds.Bottom);

            _recordingManager.CropRectScreen = new Rectangle(x, y, Math.Max(1, right - x), Math.Max(1, bottom - y));
        }
    }
}
