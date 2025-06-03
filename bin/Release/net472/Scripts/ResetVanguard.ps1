$Reset = [char]27 + "[0m"
$Red = [char]27 + "[31m"
$Green = [char]27 + "[32m"
$Yellow = [char]27 + "[33m"

Add-Type -AssemblyName System.Windows.Forms

Write-Host "$Green=>$Reset Vanguard reset script has started."

Try {
    Write-Host "$Yellow=>$Reset Attempting to stop Vanguard service (vgc)..."
    Stop-Service -Name "vgc" -Force -ErrorAction Stop
    Write-Host "$Green=>$Reset Vanguard service stopped successfully."

    Write-Host "$Yellow=>$Reset Waiting 5 seconds before restarting vgc..."
    Start-Sleep -Seconds 5

    Write-Host "$Yellow=>$Reset Attempting to start Vanguard service (vgc)..."
    Start-Service -Name "vgc" -ErrorAction Stop
    Write-Host "$Green=>$Reset Vanguard service started successfully."
}
Catch {
    Write-Host "$Red=>$Reset Failed to reset Vanguard (vgc). Error: $_"
}

Write-Host "$Green=>$Reset Vanguard reset script ended."