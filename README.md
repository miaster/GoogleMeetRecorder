# Google Meet 備份錄影助手 (Google Meet Backup Recorder)

本專案是一個基於 C# .NET 8.0 WinForms 語系開發的 **Google Meet 備份錄影工具**。
主要目的是在進行 Google Meet 會議時，做為防範原生雲端錄影失敗或中斷的**在地備份方案**。

---

## 🌟 核心特色

1. **固定左右雙面板佈局 & 自動畫面裁剪 (Crop)**
   * **左側控制面板**：永久顯示，包含會議網址載入、錄影存檔路徑、錄影排程、音訊來源設定，以及即時執行日誌。
   * **右側瀏覽器區域**：載入 Google Meet 畫面。
   * **精準錄製**：程式在啟動錄影時，會自動計算右側瀏覽器在螢幕上的實際位置，並以「螢幕擷取 + 裁剪範圍」的方式錄製，**最終錄製出來的 MP4 影片只會包含 Google Meet 畫面，左側控制面板與視窗邊框都不會被錄入**。
   * **即時裁切更新**：錄影期間若視窗被移動、放大、還原，程式會自動重新計算裁切範圍並即時套用到錄影中的片段，**不需要中斷或重啟錄影**。

2. **防毀損的分段錄影與無損合併 (續存能力)**
   * 錄影期間，程式會將影片拆分為每 **10 分鐘** 一個 MP4 分段暫存檔，儲存於 `_Record\.tmp_rec_...` 臨時目錄中。
   * **預設檔名為開始時間**：若未特別指定自訂名稱（存檔名稱欄位保留空白），錄影啟動時會自動以開始時間命名，格式為 `YYYYMMDDhhmmss.mp4`，避免覆蓋舊檔案。
   * **異常復原機制**：當錄影過程中因系統休眠、顯示卡驅動異常等導致錄影中斷，錄影管理器會在 1 秒內自動於新分段中重啟錄製。
   * 錄影結束時，程式會呼叫內建的 `FFmpeg` 快速無損合併 (`-f concat`) 所有分段，生成最終的 MP4 影片，避免因單次當機導致整場會議錄影毀損。

3. **以螢幕擷取為基礎的穩定錄製方式**
   * 錄影引擎統一採用「螢幕畫面擷取 (Display Capture) + 裁剪範圍」的方式進行錄製，而非直接擷取視窗本身，避免了不同 Windows 版本對「視窗擷取 API (Windows Graphics Capture)」支援度不一致，導致裁切範圍失效、錄到整個視窗甚至整個螢幕的問題。此方式在舊版 Windows 10、Windows Server 或虛擬機環境下也能穩定運作。
   * **重要限制**：由於是擷取螢幕上實際顯示的畫面區域，**錄影期間請勿將程式視窗縮到最小化，也不要用其他視窗完全遮擋住程式畫面**，否則錄到的內容會是遮擋物或桌面，而不是 Google Meet 畫面。

4. **PERRON 自動加入會議**
   * 內建 WebView2 瀏覽器。
   * 當導向至 Google Meet 會議室後，程式會以每 0.1 秒的極速頻率自動偵測姓名輸入框與加入按鈕，**自動填入訪客姓名「PERRON」並點選「要求加入 / 立即加入」**。

5. **Cookie 與登入狀態保持**
   * 瀏覽器設定檔獨立儲存於系統的 `LocalApplicationData` 目錄下。
   * 一旦您在內建瀏覽器登入過 Google 帳號，後續啟動程式皆會保持登入狀態，無需重複驗證。

6. **異常中斷修復工具**
   * 若程式執行中遭遇強制關閉，導致分段暫存檔未能成功合併，再次啟動程式時，可點選左側的 **「復原異常中斷的錄影暫存檔...」** 按鈕，程式將自動掃描並將遺留的分段重新合併為完整的影片。

---

## 🛠️ 系統需求

* **作業系統**：Windows 10 / Windows 11 / Windows Server 2016 或以上
* **執行階段**：
  * [.NET 8.0 Runtime (x64)](https://dotnet.microsoft.com/download/dotnet/8.0)
  * [Microsoft Edge WebView2 Runtime](https://developer.microsoft.com/microsoft-edge/webview2/)
* **編譯工具**（若需自行修改程式）：Visual Studio 2022 或 .NET 8.0 SDK

---

## 🚀 執行與編譯

由於專案內嵌了 `ScreenRecorderLib` 錄影核心的 x64 原生 C++ DLL，本專案**不支援 AnyCPU 編譯**，必須指定為 `x64` 平台。

### 1. 使用 .NET CLI 命令列執行
在專案根目錄開啟 PowerShell 或終端機，執行以下命令：

```powershell
# 編譯專案
dotnet build GoogleMeetRecorder.csproj -p:Platform=x64

# 執行專案
dotnet run --project GoogleMeetRecorder.csproj -p:Platform=x64
```

### 2. 使用 Visual Studio 2022 編輯與執行
1. 按兩下開啟根目錄下的方案檔 `GoogleMeetRecorder.sln`。
2. 在上方工具列的 **解決方案平台 (Solution Platforms)** 下拉選單中，將 `Any CPU` 切換為 **`x64`**。
3. 按下 **F5** 或點選啟動按鈕即可進行偵錯與執行。

---

## 📦 建置輸出位置（重要，避免跑錯執行檔）

* **實際會被執行、也是唯一應該使用的執行檔路徑**：
  ```
  bin\x64\Release\net8.0-windows\GoogleMeetRecorder.exe
  ```
  這是 `dotnet build -c Release -p:Platform=x64`（或 Visual Studio 以 Release + x64 建置）產生的一般「框架相依」輸出，執行時需要這台機器已安裝 .NET 8.0 Runtime。每次改完程式碼、重新編譯，更新的都是這一份。

* **`bin\x64\Release\net8.0-windows\win-x64\` 是應清除的空殼資料夾，請勿使用**：
  這個子資料夾原本是某次手動執行 `dotnet publish -r win-x64` 產生的「獨立部署（self-contained）」版本殘留物（內含完整 .NET 執行環境、多國語言資源資料夾），**該份執行檔已確認無法正常執行**，且不會被一般 `dotnet build` 更新，內容早已過期。裡面的檔案已於 2026-08-01 清空，只剩一個空資料夾本身因雲端硬碟（G: 這個 CloudDrive）同步狀態卡住、暫時無法透過命令列刪除。**若要清除，請直接用檔案總管手動刪除**：
  ```
  bin\x64\Release\net8.0-windows\win-x64\
  ```
  如果之後需要真正可攜式的獨立部署版本，應該重新執行 `dotnet publish -r win-x64 --self-contained`，而不是沿用這個殘留資料夾。

* **`bin\x64\Debug\`、`bin\Debug\` 不會固定存在**：只有在用 Debug 組態建置時才會產生，日常錄影測試請一律使用上面的 Release x64 路徑。

* **`ffmpeg.exe` 不是編譯產物，會在程式啟動時自動下載**：因為檔案約 90MB，超過 GitHub 網頁上傳 25MB 的限制，所以沒有一併存入原始碼庫。程式啟動時（`Form1_Load` → `EnsureFfmpegAvailableAsync`）會檢查執行檔同層資料夾（以及系統 PATH）是否已存在 `ffmpeg.exe`；找不到的話會自動從 `https://www.gyan.dev/ffmpeg/builds/ffmpeg-release-essentials.zip` 下載並解壓縮到 `bin\x64\Release\net8.0-windows\ffmpeg.exe`。**在確認 ffmpeg.exe 存在之前，「開始錄影」按鈕會保持停用**（排程自動錄影也會被同一個檢查擋下），避免使用者在合併工具就緒前開始錄影、導致超過 10 分鐘的錄影在停止時合併失敗。若自動下載失敗（例如沒有網路），程式會跳出訊息框提示，可重新啟動程式重試，或手動把 `ffmpeg.exe` 放進執行檔同一層資料夾。

* **`recorder_debug.log`**：與執行檔同層，記錄每次啟動/分段/錯誤的完整時間戳記日誌，即使程式當機或被強制關閉也會保留，用於事後診斷分段錄影問題。

---

## 📂 檔案結構說明

* `Form1.cs`：主畫面的控制邏輯、排程計時器、WebView2 初始化與自動加入指令碼注入。
* `Form1.Designer.cs`：WinForms UI 佈局設定（深色主題、左右雙面板等）。
* `RecordingManager.cs`：錄影核心管理器。包含錄影選項配置（螢幕擷取＋裁剪範圍）、即時裁切更新、10分鐘分段計時器、FFmpeg 合併與修復邏輯。
* `Program.cs`：應用程式進入點。
* `_Record/`：預設的錄影輸出目錄（位於執行檔同層）。
* `ffmpeg.exe`：無損合併影片使用的二進位工具。**不在原始碼庫中**，程式啟動時若偵測不到會自動下載到執行檔同層目錄（見上方「建置輸出位置」章節）。
