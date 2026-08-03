namespace GoogleMeetRecorder
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            
            // Panels
            this.panelControls = new System.Windows.Forms.Panel();
            this.panelBrowser = new System.Windows.Forms.Panel();
            this.panelTopBar = new System.Windows.Forms.Panel();
            
            // WebView
            this.webView = new Microsoft.Web.WebView2.WinForms.WebView2();
            
            // Title & Status
            this.lblAppTitle = new System.Windows.Forms.Label();
            this.lblStatus = new System.Windows.Forms.Label();
            
            // Controls inside panelControls
            this.lblUrl = new System.Windows.Forms.Label();
            this.txtUrl = new System.Windows.Forms.TextBox();
            this.btnGo = new System.Windows.Forms.Button();
            
            this.lblSavePath = new System.Windows.Forms.Label();
            this.txtSavePath = new System.Windows.Forms.TextBox();
            this.btnBrowse = new System.Windows.Forms.Button();
            
            this.lblFileName = new System.Windows.Forms.Label();
            this.txtFileName = new System.Windows.Forms.TextBox();
            
            this.lblStart = new System.Windows.Forms.Label();
            this.dtpStart = new System.Windows.Forms.DateTimePicker();
            this.lblEnd = new System.Windows.Forms.Label();
            this.dtpEnd = new System.Windows.Forms.DateTimePicker();
            this.chkEnableSchedule = new System.Windows.Forms.CheckBox();
            
            this.chkCaptureSysAudio = new System.Windows.Forms.CheckBox();
            this.chkCaptureMic = new System.Windows.Forms.CheckBox();
            this.chkAutoHideControls = new System.Windows.Forms.CheckBox();
            
            this.btnStartRecord = new System.Windows.Forms.Button();
            this.btnStopRecord = new System.Windows.Forms.Button();
            this.btnRecover = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            
            this.lblLogTitle = new System.Windows.Forms.Label();
            this.txtLog = new System.Windows.Forms.TextBox();
            
            // Controls inside panelTopBar
            this.lblRecordingAlert = new System.Windows.Forms.Label();
            this.btnShowControls = new System.Windows.Forms.Button();

            ((System.ComponentModel.ISupportInitialize)(this.webView)).BeginInit();
            this.panelControls.SuspendLayout();
            this.panelBrowser.SuspendLayout();
            this.panelTopBar.SuspendLayout();
            this.SuspendLayout();

            // 
            // panelControls
            // 
            this.panelControls.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(33)))), ((int)(((byte)(36)))));
            this.panelControls.Controls.Add(this.lblAppTitle);
            this.panelControls.Controls.Add(this.lblUrl);
            this.panelControls.Controls.Add(this.txtUrl);
            this.panelControls.Controls.Add(this.btnGo);
            this.panelControls.Controls.Add(this.lblSavePath);
            this.panelControls.Controls.Add(this.txtSavePath);
            this.panelControls.Controls.Add(this.btnBrowse);
            this.panelControls.Controls.Add(this.lblFileName);
            this.panelControls.Controls.Add(this.txtFileName);
            this.panelControls.Controls.Add(this.lblStart);
            this.panelControls.Controls.Add(this.dtpStart);
            this.panelControls.Controls.Add(this.lblEnd);
            this.panelControls.Controls.Add(this.dtpEnd);
            this.panelControls.Controls.Add(this.chkEnableSchedule);
            this.panelControls.Controls.Add(this.chkCaptureSysAudio);
            this.panelControls.Controls.Add(this.chkCaptureMic);
            this.panelControls.Controls.Add(this.chkAutoHideControls);
            this.panelControls.Controls.Add(this.btnStartRecord);
            this.panelControls.Controls.Add(this.btnStopRecord);
            this.panelControls.Controls.Add(this.lblStatus);
            this.panelControls.Controls.Add(this.btnRecover);
            this.panelControls.Controls.Add(this.btnClose);
            this.panelControls.Controls.Add(this.lblLogTitle);
            this.panelControls.Controls.Add(this.txtLog);
            this.panelControls.Dock = System.Windows.Forms.DockStyle.Left;
            this.panelControls.Location = new System.Drawing.Point(0, 0);
            this.panelControls.Name = "panelControls";
            this.panelControls.Size = new System.Drawing.Size(340, 761);
            this.panelControls.TabIndex = 0;
            // 
            // lblAppTitle
            // 
            this.lblAppTitle.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblAppTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(138)))), ((int)(((byte)(180)))), ((int)(((byte)(248)))));
            this.lblAppTitle.Location = new System.Drawing.Point(15, 15);
            this.lblAppTitle.Name = "lblAppTitle";
            this.lblAppTitle.Size = new System.Drawing.Size(310, 35);
            this.lblAppTitle.TabIndex = 0;
            this.lblAppTitle.Text = "MEET RECORDER";
            this.lblAppTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblUrl
            // 
            this.lblUrl.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(243)))), ((int)(((byte)(244)))));
            this.lblUrl.Location = new System.Drawing.Point(15, 60);
            this.lblUrl.Name = "lblUrl";
            this.lblUrl.Size = new System.Drawing.Size(310, 20);
            this.lblUrl.Text = "Google Meet 會議ID 或 完整網址:";
            // 
            // txtUrl
            // 
            this.txtUrl.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(51)))), ((int)(((byte)(54)))));
            this.txtUrl.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtUrl.ForeColor = System.Drawing.Color.White;
            this.txtUrl.Location = new System.Drawing.Point(15, 80);
            this.txtUrl.Name = "txtUrl";
            this.txtUrl.Size = new System.Drawing.Size(230, 25);
            this.txtUrl.Text = "";
            this.txtUrl.PlaceholderText = "例如: rwj-drvr-car 或網址";
            // 
            // btnGo
            // 
            this.btnGo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(115)))), ((int)(((byte)(232)))));
            this.btnGo.FlatAppearance.BorderSize = 0;
            this.btnGo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnGo.ForeColor = System.Drawing.Color.White;
            this.btnGo.Location = new System.Drawing.Point(250, 80);
            this.btnGo.Name = "btnGo";
            this.btnGo.Size = new System.Drawing.Size(75, 25);
            this.btnGo.Text = "載入";
            this.btnGo.UseVisualStyleBackColor = false;
            this.btnGo.Click += new System.EventHandler(this.BtnGo_Click);
            // 
            // lblSavePath
            // 
            this.lblSavePath.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(243)))), ((int)(((byte)(244)))));
            this.lblSavePath.Location = new System.Drawing.Point(15, 115);
            this.lblSavePath.Name = "lblSavePath";
            this.lblSavePath.Size = new System.Drawing.Size(310, 20);
            this.lblSavePath.Text = "儲存路徑:";
            // 
            // txtSavePath
            // 
            this.txtSavePath.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(51)))), ((int)(((byte)(54)))));
            this.txtSavePath.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtSavePath.ForeColor = System.Drawing.Color.White;
            this.txtSavePath.Location = new System.Drawing.Point(15, 135);
            this.txtSavePath.Name = "txtSavePath";
            this.txtSavePath.Size = new System.Drawing.Size(230, 25);
            // 
            // btnBrowse
            // 
            this.btnBrowse.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(64)))), ((int)(((byte)(67)))));
            this.btnBrowse.FlatAppearance.BorderSize = 0;
            this.btnBrowse.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBrowse.ForeColor = System.Drawing.Color.White;
            this.btnBrowse.Location = new System.Drawing.Point(250, 135);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(75, 25);
            this.btnBrowse.Text = "瀏覽...";
            this.btnBrowse.UseVisualStyleBackColor = false;
            this.btnBrowse.Click += new System.EventHandler(this.BtnBrowse_Click);
            // 
            // lblFileName
            // 
            this.lblFileName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(243)))), ((int)(((byte)(244)))));
            this.lblFileName.Location = new System.Drawing.Point(15, 170);
            this.lblFileName.Name = "lblFileName";
            this.lblFileName.Size = new System.Drawing.Size(310, 20);
            this.lblFileName.Text = "存檔名稱 (副檔名為 mp4):";
            // 
            // txtFileName
            // 
            this.txtFileName.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(51)))), ((int)(((byte)(54)))));
            this.txtFileName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtFileName.ForeColor = System.Drawing.Color.White;
            this.txtFileName.Location = new System.Drawing.Point(15, 190);
            this.txtFileName.Name = "txtFileName";
            this.txtFileName.Size = new System.Drawing.Size(310, 25);
            this.txtFileName.Text = "";
            this.txtFileName.PlaceholderText = "預設: YYYYMMDDhhmmss.mp4";
            // 
            // lblStart
            // 
            this.lblStart.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(243)))), ((int)(((byte)(244)))));
            this.lblStart.Location = new System.Drawing.Point(15, 230);
            this.lblStart.Name = "lblStart";
            this.lblStart.Size = new System.Drawing.Size(140, 20);
            this.lblStart.Text = "開始錄影時間:";
            // 
            // dtpStart
            // 
            this.dtpStart.CalendarMonthBackground = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(51)))), ((int)(((byte)(54)))));
            this.dtpStart.CustomFormat = "yyyy-MM-dd HH:mm";
            this.dtpStart.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpStart.Location = new System.Drawing.Point(15, 250);
            this.dtpStart.Name = "dtpStart";
            this.dtpStart.Size = new System.Drawing.Size(140, 25);
            // 
            // lblEnd
            // 
            this.lblEnd.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(243)))), ((int)(((byte)(244)))));
            this.lblEnd.Location = new System.Drawing.Point(185, 230);
            this.lblEnd.Name = "lblEnd";
            this.lblEnd.Size = new System.Drawing.Size(140, 20);
            this.lblEnd.Text = "結束錄影時間:";
            // 
            // dtpEnd
            // 
            this.dtpEnd.CalendarMonthBackground = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(51)))), ((int)(((byte)(54)))));
            this.dtpEnd.CustomFormat = "yyyy-MM-dd HH:mm";
            this.dtpEnd.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpEnd.Location = new System.Drawing.Point(185, 250);
            this.dtpEnd.Name = "dtpEnd";
            this.dtpEnd.Size = new System.Drawing.Size(140, 25);
            // 
            // chkEnableSchedule
            // 
            this.chkEnableSchedule.ForeColor = System.Drawing.Color.White;
            this.chkEnableSchedule.Location = new System.Drawing.Point(15, 285);
            this.chkEnableSchedule.Name = "chkEnableSchedule";
            this.chkEnableSchedule.Size = new System.Drawing.Size(310, 24);
            this.chkEnableSchedule.Text = "啟用排程自動錄影";
            this.chkEnableSchedule.UseVisualStyleBackColor = true;
            this.chkEnableSchedule.CheckedChanged += new System.EventHandler(this.ChkEnableSchedule_CheckedChanged);
            // 
            // chkCaptureSysAudio
            // 
            this.chkCaptureSysAudio.Checked = true;
            this.chkCaptureSysAudio.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkCaptureSysAudio.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(243)))), ((int)(((byte)(244)))));
            this.chkCaptureSysAudio.Location = new System.Drawing.Point(15, 320);
            this.chkCaptureSysAudio.Name = "chkCaptureSysAudio";
            this.chkCaptureSysAudio.Size = new System.Drawing.Size(140, 24);
            this.chkCaptureSysAudio.Text = "錄製喇叭聲音";
            this.chkCaptureSysAudio.UseVisualStyleBackColor = true;
            // 
            // chkCaptureMic
            // 
            this.chkCaptureMic.Checked = true;
            this.chkCaptureMic.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkCaptureMic.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(243)))), ((int)(((byte)(244)))));
            this.chkCaptureMic.Location = new System.Drawing.Point(185, 320);
            this.chkCaptureMic.Name = "chkCaptureMic";
            this.chkCaptureMic.Size = new System.Drawing.Size(140, 24);
            this.chkCaptureMic.Text = "錄製麥克風";
            this.chkCaptureMic.UseVisualStyleBackColor = true;
            // 
            // chkAutoHideControls
            // 
            this.chkAutoHideControls.Checked = true;
            this.chkAutoHideControls.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkAutoHideControls.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(243)))), ((int)(((byte)(244)))));
            this.chkAutoHideControls.Location = new System.Drawing.Point(15, 350);
            this.chkAutoHideControls.Name = "chkAutoHideControls";
            this.chkAutoHideControls.Size = new System.Drawing.Size(310, 24);
            this.chkAutoHideControls.Text = "錄影開始後自動隱藏控制面板";
            this.chkAutoHideControls.UseVisualStyleBackColor = true;
            this.chkAutoHideControls.Visible = false;
            // 
            // btnStartRecord
            // 
            this.btnStartRecord.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(157)))), ((int)(((byte)(88)))));
            this.btnStartRecord.FlatAppearance.BorderSize = 0;
            this.btnStartRecord.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStartRecord.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnStartRecord.ForeColor = System.Drawing.Color.White;
            this.btnStartRecord.Location = new System.Drawing.Point(15, 390);
            this.btnStartRecord.Name = "btnStartRecord";
            this.btnStartRecord.Size = new System.Drawing.Size(140, 40);
            this.btnStartRecord.Text = "🔴 開始錄影";
            this.btnStartRecord.UseVisualStyleBackColor = false;
            this.btnStartRecord.Click += new System.EventHandler(this.BtnStartRecord_Click);
            // 
            // btnStopRecord
            // 
            this.btnStopRecord.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(219)))), ((int)(((byte)(68)))), ((int)(((byte)(85)))));
            this.btnStopRecord.Enabled = false;
            this.btnStopRecord.FlatAppearance.BorderSize = 0;
            this.btnStopRecord.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStopRecord.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnStopRecord.ForeColor = System.Drawing.Color.White;
            this.btnStopRecord.Location = new System.Drawing.Point(185, 390);
            this.btnStopRecord.Name = "btnStopRecord";
            this.btnStopRecord.Size = new System.Drawing.Size(140, 40);
            this.btnStopRecord.Text = "⏹️ 停止錄影";
            this.btnStopRecord.UseVisualStyleBackColor = false;
            this.btnStopRecord.Click += new System.EventHandler(this.BtnStopRecord_Click);
            // 
            // lblStatus
            // 
            this.lblStatus.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(42)))), ((int)(((byte)(45)))));
            this.lblStatus.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblStatus.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(170)))), ((int)(((byte)(180)))), ((int)(((byte)(190)))));
            this.lblStatus.Location = new System.Drawing.Point(15, 445);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(310, 35);
            this.lblStatus.Text = "狀態: 閒置";
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnRecover
            // 
            this.btnRecover.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(102)))), ((int)(((byte)(60)))), ((int)(((byte)(180)))));
            this.btnRecover.FlatAppearance.BorderSize = 0;
            this.btnRecover.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRecover.ForeColor = System.Drawing.Color.White;
            this.btnRecover.Location = new System.Drawing.Point(15, 495);
            this.btnRecover.Name = "btnRecover";
            this.btnRecover.Size = new System.Drawing.Size(140, 30);
            this.btnRecover.Text = "🛠️ 復原暫存檔";
            this.btnRecover.UseVisualStyleBackColor = false;
            this.btnRecover.Click += new System.EventHandler(this.BtnRecover_Click);
            // 
            // btnClose
            // 
            this.btnClose.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.btnClose.FlatAppearance.BorderSize = 0;
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.ForeColor = System.Drawing.Color.White;
            this.btnClose.Location = new System.Drawing.Point(185, 495);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(140, 30);
            this.btnClose.Text = "❌ 關閉程式";
            this.btnClose.UseVisualStyleBackColor = false;
            this.btnClose.Click += new System.EventHandler(this.BtnClose_Click);
            // 
            // lblLogTitle
            // 
            this.lblLogTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(243)))), ((int)(((byte)(244)))));
            this.lblLogTitle.Location = new System.Drawing.Point(15, 540);
            this.lblLogTitle.Name = "lblLogTitle";
            this.lblLogTitle.Size = new System.Drawing.Size(310, 20);
            this.lblLogTitle.Text = "執行日誌:";
            // 
            // txtLog
            // 
            this.txtLog.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(24)))), ((int)(((byte)(24)))), ((int)(((byte)(28)))));
            this.txtLog.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtLog.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(170)))), ((int)(((byte)(180)))), ((int)(((byte)(190)))));
            this.txtLog.Location = new System.Drawing.Point(15, 560);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ReadOnly = true;
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLog.Size = new System.Drawing.Size(310, 185);
            // 
            // panelBrowser
            // 
            this.panelBrowser.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(24)))), ((int)(((byte)(24)))), ((int)(((byte)(28)))));
            this.panelBrowser.Controls.Add(this.webView);
            this.panelBrowser.Controls.Add(this.panelTopBar);
            this.panelBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelBrowser.Location = new System.Drawing.Point(340, 0);
            this.panelBrowser.Name = "panelBrowser";
            this.panelBrowser.Size = new System.Drawing.Size(844, 761);
            this.panelBrowser.TabIndex = 1;
            // 
            // panelTopBar
            // 
            this.panelTopBar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(42)))), ((int)(((byte)(45)))));
            this.panelTopBar.Controls.Add(this.lblRecordingAlert);
            this.panelTopBar.Controls.Add(this.btnShowControls);
            this.panelTopBar.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTopBar.Location = new System.Drawing.Point(0, 0);
            this.panelTopBar.Name = "panelTopBar";
            this.panelTopBar.Size = new System.Drawing.Size(844, 40);
            this.panelTopBar.TabIndex = 1;
            this.panelTopBar.Visible = false;
            // 
            // lblRecordingAlert
            // 
            this.lblRecordingAlert.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblRecordingAlert.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(219)))), ((int)(((byte)(68)))), ((int)(((byte)(85)))));
            this.lblRecordingAlert.Location = new System.Drawing.Point(15, 5);
            this.lblRecordingAlert.Name = "lblRecordingAlert";
            this.lblRecordingAlert.Size = new System.Drawing.Size(300, 30);
            this.lblRecordingAlert.Text = "🔴 背景錄影中... 控制面板已隱藏";
            this.lblRecordingAlert.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnShowControls
            // 
            this.btnShowControls.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnShowControls.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(115)))), ((int)(((byte)(232)))));
            this.btnShowControls.FlatAppearance.BorderSize = 0;
            this.btnShowControls.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnShowControls.ForeColor = System.Drawing.Color.White;
            this.btnShowControls.Location = new System.Drawing.Point(730, 7);
            this.btnShowControls.Name = "btnShowControls";
            this.btnShowControls.Size = new System.Drawing.Size(100, 26);
            this.btnShowControls.Text = "顯示控制台";
            this.btnShowControls.UseVisualStyleBackColor = false;
            this.btnShowControls.Click += new System.EventHandler(this.BtnShowControls_Click);
            // 
            // webView
            // 
            this.webView.AllowExternalDrop = true;
            this.webView.CreationProperties = null;
            this.webView.DefaultBackgroundColor = System.Drawing.Color.White;
            this.webView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webView.Location = new System.Drawing.Point(0, 0);
            this.webView.Name = "webView";
            this.webView.Size = new System.Drawing.Size(844, 761);
            this.webView.TabIndex = 0;
            this.webView.ZoomFactor = 1D;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(20)))), ((int)(((byte)(22)))));
            this.ClientSize = new System.Drawing.Size(1184, 761);
            this.Controls.Add(this.panelBrowser);
            this.Controls.Add(this.panelControls);
            this.MinimumSize = new System.Drawing.Size(800, 600);
            this.Name = "Form1";
            this.Text = "Google Meet Backup Recorder";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.webView)).EndInit();
            this.panelControls.ResumeLayout(false);
            this.panelControls.PerformLayout();
            this.panelBrowser.ResumeLayout(false);
            this.panelTopBar.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelControls;
        private System.Windows.Forms.Panel panelBrowser;
        private System.Windows.Forms.Panel panelTopBar;
        private Microsoft.Web.WebView2.WinForms.WebView2 webView;
        
        private System.Windows.Forms.Label lblAppTitle;
        private System.Windows.Forms.Label lblUrl;
        private System.Windows.Forms.TextBox txtUrl;
        private System.Windows.Forms.Button btnGo;
        
        private System.Windows.Forms.Label lblSavePath;
        private System.Windows.Forms.TextBox txtSavePath;
        private System.Windows.Forms.Button btnBrowse;
        
        private System.Windows.Forms.Label lblFileName;
        private System.Windows.Forms.TextBox txtFileName;
        
        private System.Windows.Forms.Label lblStart;
        private System.Windows.Forms.DateTimePicker dtpStart;
        private System.Windows.Forms.Label lblEnd;
        private System.Windows.Forms.DateTimePicker dtpEnd;
        private System.Windows.Forms.CheckBox chkEnableSchedule;
        
        private System.Windows.Forms.CheckBox chkCaptureSysAudio;
        private System.Windows.Forms.CheckBox chkCaptureMic;
        private System.Windows.Forms.CheckBox chkAutoHideControls;
        
        private System.Windows.Forms.Button btnStartRecord;
        private System.Windows.Forms.Button btnStopRecord;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Button btnRecover;
        private System.Windows.Forms.Button btnClose;
        
        private System.Windows.Forms.Label lblLogTitle;
        private System.Windows.Forms.TextBox txtLog;
        
        private System.Windows.Forms.Label lblRecordingAlert;
        private System.Windows.Forms.Button btnShowControls;
    }
}
