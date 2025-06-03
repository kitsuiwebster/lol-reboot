# LOL-RESET

PowerShell scripts to manage Riot-related processes for League of Legends and Vanguard (VGC).

## 📁 Contents

### `ResetLeague.ps1`

Stops all League of Legends processes:

* `LeagueClientUx`
* `RiotClientServices`
* `LeagueClient`

Then restarts the Riot and League clients.

### `ResetVGC.ps1`

Stops and restarts the Vanguard anti-cheat service (`vgc`).
Useful for fixing issues with Valorant's anti-cheat or restarting it cleanly.

### `ResetLeague+VGC.ps1`

Combines both scripts: resets the League client **and** the Vanguard service.

---

## ▶️ How to Run

Run any script using PowerShell with execution policy bypass (for local scripts):

```powershell
powershell.exe -ExecutionPolicy Bypass -File "C:\Users\Username\Script.ps1"
```

> 💡 Replace `Script.ps1` with one of:
>
> * `ResetLeague.ps1`
> * `ResetVGC.ps1`
> * `ResetLeague+VGC.ps1`

> 💡 Replace `Username` by your Windows username.

---

## ⚠️ Requirements

* Run as **Administrator** (required to restart services like `vgc`)
* PowerShell 5.1 or higher
