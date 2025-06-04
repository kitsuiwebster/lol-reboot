# LOL-REBOOT

Is your **League of Legends client** or **Vanguard anti-cheat** constantly crashing, freezing, or refusing to start? These PowerShell scripts help you reset them cleanly and quickly — no more hunting down rogue processes or restarting your PC. 💥

## 📁 Contents

### `ResetLeague.ps1`

Stops all League of Legends-related processes:

* `LeagueClientUx`
* `RiotClientServices`
* `LeagueClient`

Then restarts the Riot and League clients.

### `ResetVGC.ps1`

Stops and restarts the **Vanguard anti-cheat service** (`vgc`).
Perfect if you're having trouble launching **Valorant** due to Vanguard issues.

### `ResetLeague+VGC.ps1`

A combo of both: resets **League** and **Vanguard** at once.

---

## ⚡ How to Use

### 1. Open PowerShell as Administrator

* Press **Windows Key**
* Type `powershell`
* Right-click **Windows PowerShell** → **Run as Administrator**

### 2. Run a Script with This Command:

```powershell
powershell.exe -ExecutionPolicy Bypass -File "C:\Users\YourName\Script.ps1"
```

Replace:

* `Script.ps1` with one of:

  * `ResetLeague.ps1` (resets League of Legends)
  * `ResetVGC.ps1` (resets Vanguard anti-cheat)
  * `ResetLeague+VGC.ps1` (resets both League and Vanguard)
* `YourName` with your actual **Windows username**

> 💡 Example:
>
> ```powershell
> powershell.exe -ExecutionPolicy Bypass -File "C:\Users\raphm\Downloads\ResetVGC.ps1"
> ```

---

## ⚠️ Requirements

* Run scripts as **Administrator** (needed to control system services like Vanguard)
* Works on **Windows** with **PowerShell 5.1 or higher**

