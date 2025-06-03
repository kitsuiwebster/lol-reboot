using System;
using System.Diagnostics;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace LolReset
{
    public class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }

    public class MainForm : Form
    {
        private Button btnResetLeague;
        private Button btnResetVanguard;
        private Button btnResetBoth;
        private RichTextBox outputBox;
        private bool isRunning = false;

        public MainForm()
        {
            this.Text = "Game Reset Tool";
            this.Size = new Size(600, 500);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Icon = SystemIcons.Application;

            // Create buttons
            btnResetLeague = new Button
            {
                Text = "Reset League of Legends",
                Location = new Point(50, 20),
                Size = new Size(150, 40)
            };
            btnResetLeague.Click += async (sender, e) => await RunScript(ScriptType.LeagueOnly);

            btnResetVanguard = new Button
            {
                Text = "Reset Vanguard",
                Location = new Point(220, 20),
                Size = new Size(150, 40)
            };
            btnResetVanguard.Click += async (sender, e) => await RunScript(ScriptType.VanguardOnly);

            btnResetBoth = new Button
            {
                Text = "Reset Both",
                Location = new Point(390, 20),
                Size = new Size(150, 40)
            };
            btnResetBoth.Click += async (sender, e) => await RunScript(ScriptType.Both);

            // Create output box
            outputBox = new RichTextBox
            {
                Location = new Point(50, 80),
                Size = new Size(490, 350),
                ReadOnly = true,
                BackColor = Color.Black,
                ForeColor = Color.White,
                Font = new Font("Consolas", 10F),
                Multiline = true,
                ScrollBars = RichTextBoxScrollBars.Vertical
            };

            // Add controls to form
            this.Controls.Add(btnResetLeague);
            this.Controls.Add(btnResetVanguard);
            this.Controls.Add(btnResetBoth);
            this.Controls.Add(outputBox);

            // Create the scripts folder and save scripts when the application starts
            CreateScripts();
        }

        private enum ScriptType
        {
            LeagueOnly,
            VanguardOnly,
            Both
        }

        private async Task RunScript(ScriptType scriptType)
        {
            if (isRunning)
            {
                MessageBox.Show("A script is already running. Please wait for it to complete.", "Script Running", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            isRunning = true;
            SetButtonsEnabled(false);
            outputBox.Clear();

            try
            {
                string scriptPath = "";
                switch (scriptType)
                {
                    case ScriptType.LeagueOnly:
                        scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Scripts", "ResetLeague.ps1");
                        break;
                    case ScriptType.VanguardOnly:
                        scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Scripts", "ResetVanguard.ps1");
                        break;
                    case ScriptType.Both:
                        scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Scripts", "ResetBoth.ps1");
                        break;
                }

                if (!File.Exists(scriptPath))
                {
                    AppendColoredText("Script file not found: " + scriptPath, Color.Red);
                    return;
                }

                await Task.Run(() =>
                {
                    // Create PowerShell runspace
                    using (Runspace runspace = RunspaceFactory.CreateRunspace())
                    {
                        runspace.Open();

                        // Create PowerShell pipeline
                        using (PowerShell powershell = PowerShell.Create())
                        {
                            powershell.Runspace = runspace;

                            // Add script to pipeline
                            powershell.AddScript(File.ReadAllText(scriptPath));

                            // Set up output handlers
                            powershell.Streams.Error.DataAdded += (sender, e) =>
                            {
                                var error = powershell.Streams.Error[e.Index];
                                AppendColoredText(error.ToString(), Color.Red);
                            };

                            powershell.Streams.Warning.DataAdded += (sender, e) =>
                            {
                                var warning = powershell.Streams.Warning[e.Index];
                                AppendColoredText(warning.ToString(), Color.Yellow);
                            };

                            powershell.Streams.Information.DataAdded += (sender, e) =>
                            {
                                var info = powershell.Streams.Information[e.Index];
                                AppendColoredText(info.MessageData.ToString(), Color.White);
                            };

                            // Execute the script and capture output
                            var results = powershell.Invoke();

                            // Display results
                            foreach (var result in results)
                            {
                                string text = result.ToString();
                                if (text.Contains("=>"))
                                {
                                    // Parse colored text
                                    if (text.Contains("[31m"))
                                    {
                                        // Red text
                                        AppendColoredText(text.Replace("[31m", "").Replace("[0m", ""), Color.Red);
                                    }
                                    else if (text.Contains("[32m"))
                                    {
                                        // Green text
                                        AppendColoredText(text.Replace("[32m", "").Replace("[0m", ""), Color.Green);
                                    }
                                    else if (text.Contains("[33m"))
                                    {
                                        // Yellow text
                                        AppendColoredText(text.Replace("[33m", "").Replace("[0m", ""), Color.Yellow);
                                    }
                                    else
                                    {
                                        AppendColoredText(text, Color.White);
                                    }
                                }
                                else
                                {
                                    AppendColoredText(text, Color.White);
                                }
                            }
                        }
                    }
                });

                AppendColoredText("\r\nScript execution completed.", Color.Lime);
            }
            catch (Exception ex)
            {
                AppendColoredText($"Error executing script: {ex.Message}", Color.Red);
            }
            finally
            {
                isRunning = false;
                SetButtonsEnabled(true);
            }
        }

        private void AppendColoredText(string text, Color color)
        {
            if (outputBox.InvokeRequired)
            {
                outputBox.Invoke(new Action<string, Color>(AppendColoredText), new object[] { text, color });
                return;
            }

            outputBox.SelectionStart = outputBox.TextLength;
            outputBox.SelectionLength = 0;
            outputBox.SelectionColor = color;
            outputBox.AppendText(text + Environment.NewLine);
            outputBox.SelectionColor = outputBox.ForeColor;
            outputBox.ScrollToCaret();
        }

        private void SetButtonsEnabled(bool enabled)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<bool>(SetButtonsEnabled), new object[] { enabled });
                return;
            }

            btnResetLeague.Enabled = enabled;
            btnResetVanguard.Enabled = enabled;
            btnResetBoth.Enabled = enabled;
        }

        private void CreateScripts()
        {
            string scriptDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Scripts");
            
            if (!Directory.Exists(scriptDirectory))
            {
                Directory.CreateDirectory(scriptDirectory);
            }

            // League of Legends Reset Script
            string leagueScriptPath = Path.Combine(scriptDirectory, "ResetLeague.ps1");
            File.WriteAllText(leagueScriptPath, @"$Reset = [char]27 + ""[0m""
$Red = [char]27 + ""[31m""
$Green = [char]27 + ""[32m""
$Yellow = [char]27 + ""[33m""

Add-Type -AssemblyName System.Windows.Forms

Write-Host ""$Green=>$Reset League reset script has started.""
$stopTasks = ""LeagueClientUx"", ""RiotClientServices"", ""LeagueClient""

foreach ($stopTask in $stopTasks) {
    Try {
        Write-Host ""$Yellow=>$Reset Attempting to stop $stopTask gracefully...""
        $processes = Get-Process -Name $stopTask -ErrorAction SilentlyContinue
        if ($null -ne $processes) {
            foreach ($process in $processes) {
                Write-Host ""$Yellow=>$Reset Found running instance of $stopTask with PID $($process.Id)""
                $process.CloseMainWindow() | Out-Null
                Write-Host ""$Yellow=>$Reset Sent close request to $stopTask with PID $($process.Id)""
                Write-Host ""$Yellow=>$Reset .""
                Start-Sleep -Seconds 1
                Write-Host ""$Yellow=>$Reset ..""
                Start-Sleep -Seconds 1
                Write-Host ""$Yellow=>$Reset ...""
                Start-Sleep -Seconds 1
                Write-Host ""$Yellow=>$Reset ....""
                Start-Sleep -Seconds 1
                Write-Host ""$Yellow=>$Reset .....""
                Start-Sleep -Seconds 1
                if (!$process.HasExited) {
                    Write-Host ""$Red=>$Reset Process $stopTask with PID $($process.Id) did not close gracefully, forcing termination...""
                    $process | Stop-Process -Force
                } else {
                    Write-Host ""$Green=>$Reset Process $stopTask with PID $($process.Id) closed gracefully.""
                }
            }
        } else {
            Write-Host ""$Red=>$Reset No running instances of $stopTask found.""
        }
        Write-Host ""$Yellow=>$Reset Stopped $stopTask""
    }
    Catch {
        Write-Host ""$Red=>$Reset Failed to stop $stopTask. Error: $_""
    }
}

Write-Host ""$Yellow=>$Reset Be patient... restarting League of legends (don't forget to report your jungler)""
Start-Sleep -Seconds 15

$startTasks = @{
    ""LeagueClientUx"" = ""C:\Riot Games\League of Legends\LeagueClientUx.exe""
    ""RiotClientServices"" = ""C:\Riot Games\Riot Client\RiotClientServices.exe""
}

foreach ($startTask in $startTasks.Keys) {
    Try {
        Write-Host ""$Yellow=>$Reset Attempting to start $startTask...""
        Start-Process $startTasks[$startTask]
        Write-Host ""$Yellow=>$Reset Started $startTask""
    }
    Catch {
        Write-Host ""$Red=>$Reset Failed to start $startTask. Error: $_""
    }
}

Write-Host ""$Green=>$Reset League reset script ended""");

            // Vanguard Reset Script
            string vanguardScriptPath = Path.Combine(scriptDirectory, "ResetVanguard.ps1");
            File.WriteAllText(vanguardScriptPath, @"$Reset = [char]27 + ""[0m""
$Red = [char]27 + ""[31m""
$Green = [char]27 + ""[32m""
$Yellow = [char]27 + ""[33m""

Add-Type -AssemblyName System.Windows.Forms

Write-Host ""$Green=>$Reset Vanguard reset script has started.""

Try {
    Write-Host ""$Yellow=>$Reset Attempting to stop Vanguard service (vgc)...""
    Stop-Service -Name ""vgc"" -Force -ErrorAction Stop
    Write-Host ""$Green=>$Reset Vanguard service stopped successfully.""

    Write-Host ""$Yellow=>$Reset Waiting 5 seconds before restarting vgc...""
    Start-Sleep -Seconds 5

    Write-Host ""$Yellow=>$Reset Attempting to start Vanguard service (vgc)...""
    Start-Service -Name ""vgc"" -ErrorAction Stop
    Write-Host ""$Green=>$Reset Vanguard service started successfully.""
}
Catch {
    Write-Host ""$Red=>$Reset Failed to reset Vanguard (vgc). Error: $_""
}

Write-Host ""$Green=>$Reset Vanguard reset script ended.""");

            // Both Reset Script
            string bothScriptPath = Path.Combine(scriptDirectory, "ResetBoth.ps1");
            File.WriteAllText(bothScriptPath, @"$Reset = [char]27 + ""[0m""
$Red = [char]27 + ""[31m""
$Green = [char]27 + ""[32m""
$Yellow = [char]27 + ""[33m""

Add-Type -AssemblyName System.Windows.Forms

Write-Host ""$Green=>$Reset Script has started.""

# Reset Vanguard (vgc) Service
Try {
    Write-Host ""$Yellow=>$Reset Attempting to stop Vanguard service (vgc)...""
    Stop-Service -Name ""vgc"" -Force -ErrorAction Stop
    Write-Host ""$Green=>$Reset Vanguard service stopped successfully.""

    Write-Host ""$Yellow=>$Reset Waiting 5 seconds before restarting vgc...""
    Start-Sleep -Seconds 5

    Write-Host ""$Yellow=>$Reset Attempting to start Vanguard service (vgc)...""
    Start-Service -Name ""vgc"" -ErrorAction Stop
    Write-Host ""$Green=>$Reset Vanguard service started successfully.""
}
Catch {
    Write-Host ""$Red=>$Reset Failed to reset Vanguard (vgc). Error: $_""
}

# Stop League-related processes
$stopTasks = ""LeagueClientUx"", ""RiotClientServices"", ""LeagueClient""

foreach ($stopTask in $stopTasks) {
    Try {
        Write-Host ""$Yellow=>$Reset Attempting to stop $stopTask gracefully...""
        $processes = Get-Process -Name $stopTask -ErrorAction SilentlyContinue
        if ($null -ne $processes) {
            foreach ($process in $processes) {
                Write-Host ""$Yellow=>$Reset Found running instance of $stopTask with PID $($process.Id)""
                $process.CloseMainWindow() | Out-Null
                Write-Host ""$Yellow=>$Reset Sent close request to $stopTask with PID $($process.Id)""
                Start-Sleep -Seconds 5
                if (!$process.HasExited) {
                    Write-Host ""$Red=>$Reset Process $stopTask with PID $($process.Id) did not close gracefully, forcing termination...""
                    $process | Stop-Process -Force
                } else {
                    Write-Host ""$Green=>$Reset Process $stopTask with PID $($process.Id) closed gracefully.""
                }
            }
        } else {
            Write-Host ""$Red=>$Reset No running instances of $stopTask found.""
        }
        Write-Host ""$Yellow=>$Reset Stopped $stopTask""
    }
    Catch {
        Write-Host ""$Red=>$Reset Failed to stop $stopTask. Error: $_""
    }
}

Write-Host ""$Yellow=>$Reset Be patient... restarting League of Legends (don't forget to report your jungler)""
Start-Sleep -Seconds 15

$startTasks = @{
    ""LeagueClientUx"" = ""C:\Riot Games\League of Legends\LeagueClientUx.exe""
    ""RiotClientServices"" = ""C:\Riot Games\Riot Client\RiotClientServices.exe""
}

foreach ($startTask in $startTasks.Keys) {
    Try {
        Write-Host ""$Yellow=>$Reset Attempting to start $startTask...""
        Start-Process $startTasks[$startTask]
        Write-Host ""$Yellow=>$Reset Started $startTask""
    }
    Catch {
        Write-Host ""$Red=>$Reset Failed to start $startTask. Error: $_""
    }
}

Write-Host ""$Green=>$Reset Script ended""");
        }
    }
}