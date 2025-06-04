
Add-Type -AssemblyName System.Windows.Forms

Write-Host "=> Vanguard reset script has started."

Try {
    Write-Host "=> Attempting to stop Vanguard service (vgc)..."
    Stop-Service -Name "vgc" -Force -ErrorAction Stop
    Write-Host "=> Vanguard service stopped successfully."

    Write-Host "=> Waiting 5 seconds before restarting vgc..."
    Start-Sleep -Seconds 5

    Write-Host "=> Attempting to start Vanguard service (vgc)..."
    Start-Service -Name "vgc" -ErrorAction Stop
    Write-Host "=> Vanguard service started successfully."
}
Catch {
    Write-Host "=> Failed to reset Vanguard (vgc). Error: $_"
}

Write-Host "=> Vanguard reset script ended."