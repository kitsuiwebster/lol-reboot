$Reset = [char]27 + "[0m"
$Red = [char]27 + "[31m"
$Green = [char]27 + "[32m"
$Yellow = [char]27 + "[33m"

Add-Type -AssemblyName System.Windows.Forms

Write-Host "$Green=>$Reset Script has started."
$stopTasks = "LeagueClientUx", "RiotClientServices", "LeagueClient"

foreach ($stopTask in $stopTasks) {
    Try {
        Write-Host "$Yellow=>$Reset Attempting to stop $stopTask gracefully..."
        $processes = Get-Process -Name $stopTask -ErrorAction SilentlyContinue
        if ($null -ne $processes) {
            foreach ($process in $processes) {
                Write-Host "$Yellow=>$Reset Found running instance of $stopTask with PID $($process.Id)"
                $process.CloseMainWindow() | Out-Null
                Write-Host "$Yellow=>$Reset Sent close request to $stopTask with PID $($process.Id)"
                Write-Host "$Yellow=>$Reset ."
                Start-Sleep -Seconds 1
                Write-Host "$Yellow=>$Reset .."
                Start-Sleep -Seconds 1
                Write-Host "$Yellow=>$Reset ..."
                Start-Sleep -Seconds 1
                Write-Host "$Yellow=>$Reset ...."
                Start-Sleep -Seconds 1
                Write-Host "$Yellow=>$Reset ....."
                Start-Sleep -Seconds 1
                if (!$process.HasExited) {
                    Write-Host "$Red=>$Reset Process $stopTask with PID $($process.Id) did not close gracefully, forcing termination..."
                    $process | Stop-Process -Force
                } else {
                    Write-Host "$Green=>$Reset Process $stopTask with PID $($process.Id) closed gracefully."
                }
            }
        } else {
            Write-Host "$Red=>$Reset No running instances of $stopTask found."
        }
        Write-Host "$Yellow=>$Reset Stopped $stopTask"
    }
    Catch {
        Write-Host "$Red=>$Reset Failed to stop $stopTask. Error: $_"
    }
}

Write-Host "$Yellow=>$Reset Be patient... restarting League of legends (don't forget to report your jungler)"
Start-Sleep -Seconds 15

$startTasks = @{
    "LeagueClientUx" = "C:\Riot Games\League of Legends\LeagueClientUx.exe"
    "RiotClientServices" = "C:\Riot Games\Riot Client\RiotClientServices.exe"
}

foreach ($startTask in $startTasks.Keys) {
    Try {
        Write-Host "$Yellow=>$Reset Attempting to start $startTask..."
        Start-Process $startTasks[$startTask]
        Write-Host "$Yellow=>$Reset Started $startTask"
    }
    Catch {
        Write-Host "$Red=>$Reset Failed to start $startTask. Error: $_"
    }
}

Write-Host "$Green=>$Reset Script ended"