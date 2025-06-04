
Add-Type -AssemblyName System.Windows.Forms

Write-Host "=> League reset script has started."
$stopTasks = "LeagueClientUx", "RiotClientServices", "LeagueClient"

foreach ($stopTask in $stopTasks) {
    Try {
        Write-Host "=> Attempting to stop $stopTask gracefully..."
        $processes = Get-Process -Name $stopTask -ErrorAction SilentlyContinue
        if ($null -ne $processes) {
            foreach ($process in $processes) {
                Write-Host "=> Found running instance of $stopTask with PID $($process.Id)"
                $process.CloseMainWindow() | Out-Null
                Write-Host "=> Sent close request to $stopTask with PID $($process.Id)"
                Write-Host "=> ."
                Start-Sleep -Seconds 1
                Write-Host "=> .."
                Start-Sleep -Seconds 1
                Write-Host "=> ..."
                Start-Sleep -Seconds 1
                Write-Host "=> ...."
                Start-Sleep -Seconds 1
                Write-Host "=> ....."
                Start-Sleep -Seconds 1
                if (!$process.HasExited) {
                    Write-Host "=> Process $stopTask with PID $($process.Id) did not close gracefully, forcing termination..."
                    $process | Stop-Process -Force
                } else {
                    Write-Host "=> Process $stopTask with PID $($process.Id) closed gracefully."
                }
            }
        } else {
            Write-Host "=> No running instances of $stopTask found."
        }
        Write-Host "=> Stopped $stopTask"
    }
    Catch {
        Write-Host "=> Failed to stop $stopTask. Error: $_"
    }
}

Write-Host "=> Be patient... restarting League of legends (don't forget to report your jungler)"
Start-Sleep -Seconds 15

$startTasks = @{
    "LeagueClientUx" = "C:\Riot Games\League of Legends\LeagueClientUx.exe"
    "RiotClientServices" = "C:\Riot Games\Riot Client\RiotClientServices.exe"
}

foreach ($startTask in $startTasks.Keys) {
    Try {
        Write-Host "=> Attempting to start $startTask..."
        Start-Process $startTasks[$startTask]
        Write-Host "=> Started $startTask"
    }
    Catch {
        Write-Host "=> Failed to start $startTask. Error: $_"
    }
}

Write-Host "=> League reset script ended"