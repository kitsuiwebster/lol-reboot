using System;
using System.Diagnostics;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Runtime.InteropServices;

namespace LolReset
{
    public class Program
    {
        [STAThread]
        static void Main()
        {
            // Initialize the Windows Forms application
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }

    public class MainForm : Form
    {
        // UI Elements
        private LeagueButton btnResetLeague;
        private LeagueButton btnResetVanguard;
        private LeagueButton btnResetBoth;
        private RichTextBox outputBox;
        private Panel contentPanel;
        private Panel buttonPanel;
        private LinkLabel footerLink;
        
        // Constants for button positioning
        private const int BUTTON_WIDTH = 230;
        private const int BUTTON_HEIGHT = 50;
        private const int BUTTON_SPACING = 20;
        private const int BUTTON_TOP_MARGIN = 15;
        
        // Operation state
        private bool isRunning = false;
        
        // Custom colors used throughout the UI
        private readonly Color leagueGold = Color.FromArgb(200, 170, 60);
        private readonly Color leagueBlue = Color.FromArgb(0, 136, 169);
        private readonly Color leagueDarkBlue = Color.FromArgb(1, 10, 19);
        private readonly Color leagueMediumBlue = Color.FromArgb(4, 30, 47);
        private readonly Color leagueAccentBlue = Color.FromArgb(7, 48, 73);
        private readonly Color commandBlack = Color.FromArgb(10, 15, 20);

        // Custom fonts
        private Font titleFont;
        private Font buttonFont;

        // Animation state for fade-in effect
        private Timer fadeTimer;
        private float opacity = 0.0f;

        // Import Windows GDI+ function to load custom fonts
        [DllImport("gdi32.dll")]
        private static extern IntPtr AddFontMemResourceEx(IntPtr pbFont, uint cbFont, IntPtr pdv, [In] ref uint pcFonts);

        private PrivateFontCollection fonts = new PrivateFontCollection();

        public MainForm()
        {
            // Load custom fonts
            LoadCustomFonts();

            // Setup form with standard Windows style
            this.Text = "League of Legends Reboot Tool";
            this.Size = new Size(800, 600);
            this.MinimumSize = new Size(750, 500); 
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MinimizeBox = true;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = SystemColors.Control;
            this.Opacity = 0;
            this.ShowInTaskbar = true;

            // Fix for app.ico loading - Use assembly path to ensure correct loading
            try
            {
                // First try to use the icon from the application's executable
                this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);

                // If that fails, look for app.ico in the directory
                if (this.Icon == null)
                {
                    string iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app.ico");
                    if (File.Exists(iconPath))
                    {
                        this.Icon = new Icon(iconPath);
                    }
                }
            }
            catch
            {
                // Fallback to system icon if we can't load the icon
                this.Icon = SystemIcons.Application;
            }

            // Create content panel
            contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = leagueDarkBlue,
                Padding = new Padding(20)
            };

            // Create button panel with fixed height
            buttonPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = BUTTON_HEIGHT + BUTTON_TOP_MARGIN * 2,
                BackColor = leagueMediumBlue,
                Padding = new Padding(15)
            };

            // Create buttons with fixed size
            btnResetLeague = new LeagueButton
            {
                Text = "REBOOT LEAGUE",
                Size = new Size(BUTTON_WIDTH, BUTTON_HEIGHT),
                Font = buttonFont,
                BackColor = leagueAccentBlue,
                ForeColor = leagueGold,
                FlatStyle = FlatStyle.Flat,
                ImageAlign = ContentAlignment.MiddleLeft,
                TextAlign = ContentAlignment.MiddleCenter,
                Cursor = Cursors.Hand
            };
            btnResetLeague.Click += async (sender, e) => await RunScript(ScriptType.LeagueOnly);

            btnResetVanguard = new LeagueButton
            {
                Text = "REBOOT VANGUARD",
                Size = new Size(BUTTON_WIDTH, BUTTON_HEIGHT),
                Font = buttonFont,
                BackColor = leagueAccentBlue,
                ForeColor = leagueGold,
                FlatStyle = FlatStyle.Flat,
                ImageAlign = ContentAlignment.MiddleLeft,
                TextAlign = ContentAlignment.MiddleCenter,
                Cursor = Cursors.Hand
            };
            btnResetVanguard.Click += async (sender, e) => await RunScript(ScriptType.VanguardOnly);

            btnResetBoth = new LeagueButton
            {
                Text = "REBOOT BOTH",
                Size = new Size(BUTTON_WIDTH, BUTTON_HEIGHT),
                Font = buttonFont,
                BackColor = leagueAccentBlue,
                ForeColor = leagueGold,
                FlatStyle = FlatStyle.Flat,
                ImageAlign = ContentAlignment.MiddleLeft,
                TextAlign = ContentAlignment.MiddleCenter,
                Cursor = Cursors.Hand
            };
            btnResetBoth.Click += async (sender, e) => await RunScript(ScriptType.Both);

            // Add buttons directly to button panel
            buttonPanel.Controls.Add(btnResetLeague);
            buttonPanel.Controls.Add(btnResetVanguard);
            buttonPanel.Controls.Add(btnResetBoth);

            // Create output box (terminal style)
            outputBox = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                BackColor = commandBlack,
                ForeColor = Color.LightGray,
                Font = new Font("Consolas", 10F),
                BorderStyle = BorderStyle.None,
                Multiline = true,
                ScrollBars = RichTextBoxScrollBars.Vertical,
                Margin = new Padding(0, 10, 0, 0),
                HideSelection = true
            };

            // Prevent focus on outputBox
            outputBox.GotFocus += (s, e) => { this.Focus(); };

            // Create panel for output box with border effect
            Panel outputPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(2),
                BackColor = leagueBlue,
                Margin = new Padding(0, 10, 0, 0)
            };
            outputPanel.Controls.Add(outputBox);

            // Add controls to content panel
            contentPanel.Controls.Add(outputPanel);
            contentPanel.Controls.Add(buttonPanel);

            // Add content panel to form
            this.Controls.Add(contentPanel);

            // Create and configure the footer link
            footerLink = new LinkLabel
            {
                Text = "By @kitsuiwebster | Source: https://github.com/kitsuiwebster/lol-reboot.git",
                LinkColor = Color.FromArgb(120, 120, 180),
                ActiveLinkColor = leagueGold,
                LinkBehavior = LinkBehavior.HoverUnderline,
                BackColor = leagueDarkBlue,
                Font = new Font("Consolas", 8F),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Bottom,
                Height = 20,
                Padding = new Padding(5, 3, 3, 5)
            };

            // Handle click on the link
            footerLink.LinkClicked += (s, e) => 
            {
                try 
                {
                    Process.Start("https://github.com/kitsuiwebster/lol-reboot.git");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Could not open link: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            // Add the footer to the form
            this.Controls.Add(footerLink);

            // Ensure the footer is above other controls
            footerLink.BringToFront();

            // Create scripts folder and save PowerShell scripts
            CreateScripts();

            // Setup fade-in animation
            fadeTimer = new Timer();
            fadeTimer.Interval = 20;
            fadeTimer.Tick += FadeIn;
            fadeTimer.Start();

            // Display welcome message with ASCII art logo
            AppendColoredText(" ___      _______  ___        ______    _______  _______  _______  _______  _______ ", leagueGold);
            AppendColoredText("|   |    |       ||   |      |    _ |  |       ||  _    ||       ||       ||       |", leagueGold);
            AppendColoredText("|   |    |   _   ||   |      |   | ||  |    ___|| |_|   ||   _   ||   _   ||_     _|", leagueGold);
            AppendColoredText("|   |    |  | |  ||   |      |   |_||_ |   |___ |       ||  | |  ||  | |  |  |   |  ", leagueGold);
            AppendColoredText("|   |___ |  |_|  ||   |___   |    __  ||    ___||  _   | |  |_|  ||  |_|  |  |   |  ", leagueGold);
            AppendColoredText("|       ||       ||       |  |   |  | ||   |___ | |_|   ||       ||       |  |   |  ", leagueGold);
            AppendColoredText("|_______||_______||_______|  |___|  |_||_______||_______||_______||_______|  |___|  ", leagueGold);
            AppendColoredText("By @kitsuiwebster", leagueGold);
            AppendColoredText("\nWelcome Summoner! Choose an option to proceed.", Color.White);
            AppendColoredText("This tool will help you resolve connectivity issues by resetting game services.", Color.LightGray);
            AppendColoredText("\nREBOOT LEAGUE - Restarts League of Legends processes", leagueBlue);
            AppendColoredText("REBOOT VANGUARD - Restarts Riot Vanguard service", leagueBlue);
            AppendColoredText("REBOOT BOTH - Restarts both League and Vanguard", leagueBlue);
            AppendColoredText("\nStatus: Ready", Color.Green);

            // Position buttons after creating all controls
            this.Load += (sender, e) => RepositionButtons();
        }

        // Centralized method to calculate and position buttons
        private void RepositionButtons()
        {
            if (btnResetLeague == null || btnResetVanguard == null || btnResetBoth == null || buttonPanel == null)
                return;
                
            // Total width needed for all buttons with spacing
            int totalButtonWidth = (BUTTON_WIDTH * 3) + (BUTTON_SPACING * 2);
            
            // Calculate the starting point to center buttons in the panel
            int availableWidth = buttonPanel.ClientSize.Width;
            int startX = (availableWidth - totalButtonWidth) / 2;
            
            // Ensure startX is not negative
            startX = Math.Max(startX, BUTTON_SPACING);
            
            // Position each button
            btnResetLeague.Location = new Point(startX, BUTTON_TOP_MARGIN);
            btnResetVanguard.Location = new Point(startX + BUTTON_WIDTH + BUTTON_SPACING, BUTTON_TOP_MARGIN);
            btnResetBoth.Location = new Point(startX + (BUTTON_WIDTH * 2) + (BUTTON_SPACING * 2), BUTTON_TOP_MARGIN);
            
            // Check if buttons are partially off-screen and adjust if necessary
            if (startX + totalButtonWidth > availableWidth)
            {
                // If space is insufficient, reduce button size or reorganize
                int adjustedWidth = (availableWidth - (BUTTON_SPACING * 4)) / 3;
                adjustedWidth = Math.Max(adjustedWidth, 150); 
                
                btnResetLeague.Size = new Size(adjustedWidth, BUTTON_HEIGHT);
                btnResetVanguard.Size = new Size(adjustedWidth, BUTTON_HEIGHT);
                btnResetBoth.Size = new Size(adjustedWidth, BUTTON_HEIGHT);
                
                // Recalculate positions with new size
                int newTotalWidth = (adjustedWidth * 3) + (BUTTON_SPACING * 2);
                int newStartX = (availableWidth - newTotalWidth) / 2;
                newStartX = Math.Max(newStartX, BUTTON_SPACING);
                
                btnResetLeague.Location = new Point(newStartX, BUTTON_TOP_MARGIN);
                btnResetVanguard.Location = new Point(newStartX + adjustedWidth + BUTTON_SPACING, BUTTON_TOP_MARGIN);
                btnResetBoth.Location = new Point(newStartX + (adjustedWidth * 2) + (BUTTON_SPACING * 2), BUTTON_TOP_MARGIN);
            }
        }

        // Load custom fonts from resources
        private void LoadCustomFonts()
        {

            // Set default fonts
            titleFont = new Font("Courrier New", 16f, FontStyle.Bold);
            buttonFont = new Font("Courrier New", 11f, FontStyle.Bold);

        }

        // Handle resizing of the form
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            
            // Recalculate button positioning on each resize event
            RepositionButtons();
        }

        // Fade-in animation handler
        private void FadeIn(object sender, EventArgs e)
        {
            opacity += 0.1f;
            if (opacity >= 1.0f)
            {
                opacity = 1.0f;
                fadeTimer.Stop();
            }
            this.Opacity = opacity;
        }

        // Enum to specify which script to run
        private enum ScriptType
        {
            LeagueOnly,
            VanguardOnly,
            Both
        }
        
        // Execute the selected PowerShell script asynchronously
        private async Task RunScript(ScriptType scriptType)
        {
            if (isRunning)
            {
                MessageBox.Show("A script is already running. Please wait for it to complete.", "Process Running", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            isRunning = true;
            SetButtonsEnabled(false);
            outputBox.Clear();

            string scriptName = "";
            switch (scriptType)
            {
                case ScriptType.LeagueOnly:
                    scriptName = "LEAGUE OF LEGENDS";
                    break;
                case ScriptType.VanguardOnly:
                    scriptName = "VANGUARD";
                    break;
                case ScriptType.Both:
                    scriptName = "LEAGUE AND VANGUARD";
                    break;
            }

            AppendColoredText($"\nINITIATING {scriptName} REBOOT", leagueGold);
            AppendColoredText($"", Color.White);

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
                    AppendColoredText("⚠ Script file not found: " + scriptPath, Color.Red);
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
                                AppendColoredText("⚠ " + error.ToString(), Color.Red);
                            };

                            powershell.Streams.Warning.DataAdded += (sender, e) =>
                            {
                                var warning = powershell.Streams.Warning[e.Index];
                                AppendColoredText("⚠ " + warning.ToString(), Color.Yellow);
                            };

                            powershell.Streams.Information.DataAdded += (sender, e) =>
                            {
                                var info = powershell.Streams.Information[e.Index];
                                AppendColoredText(info.MessageData.ToString(), Color.White);
                            };

                            // Execute the script and capture output
                            var results = powershell.Invoke();

                            // Display results with intelligent coloring based on content
                            foreach (var result in results)
                            {
                                string text = result.ToString();
                                
                                // If the text contains "=>" (our standard marker)
                                if (text.Contains("=>"))
                                {
                                    string lowercaseText = text.ToLower();
                                    
                                    // Apply colors based on message content
                                    if (lowercaseText.Contains("error") || 
                                        lowercaseText.Contains("failed") || 
                                        lowercaseText.Contains("no running instances") ||
                                        lowercaseText.Contains("did not close gracefully") ||
                                        lowercaseText.Contains("forcing termination"))
                                    {
                                        // Error messages in red
                                        AppendColoredText(text, Color.FromArgb(255, 80, 80));
                                    }
                                    else if (lowercaseText.Contains("started") || 
                                            lowercaseText.Contains("complete") || 
                                            lowercaseText.Contains("successfully") ||
                                            lowercaseText.Contains("closed gracefully"))
                                    {
                                        // Success messages in green
                                        AppendColoredText(text, Color.FromArgb(80, 255, 80));
                                    }
                                    else
                                    {
                                        // Informational messages in yellow
                                        AppendColoredText(text, Color.FromArgb(255, 255, 80));
                                    }
                                }
                                else
                                {
                                    // Standard text in white
                                    AppendColoredText(text, Color.White);
                                }
                            }
                        }
                    }
                });

                AppendColoredText("\nREBOOT COMPLETE\n", Color.FromArgb(80, 255, 80));
            }
            catch (Exception ex)
            {
                AppendColoredText($"⚠ Error executing script: {ex.Message}", Color.Red);
            }
            finally
            {
                isRunning = false;
                SetButtonsEnabled(true);
            }
        }

        // Add colored text to the output box
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

        // Enable or disable buttons during script execution
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
            
            if (enabled)
            {
                btnResetLeague.BackColor = leagueAccentBlue;
                btnResetVanguard.BackColor = leagueAccentBlue;
                btnResetBoth.BackColor = leagueAccentBlue;
            }
            else
            {
                btnResetLeague.BackColor = Color.FromArgb(30, 40, 50);
                btnResetVanguard.BackColor = Color.FromArgb(30, 40, 50);
                btnResetBoth.BackColor = Color.FromArgb(30, 40, 50);
            }
        }

        // Create PowerShell scripts in the Scripts directory
        private void CreateScripts()
        {
            string scriptDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Scripts");
            
            if (!Directory.Exists(scriptDirectory))
            {
                Directory.CreateDirectory(scriptDirectory);
            }

            // League of Legends Reset Script
            string leagueScriptPath = Path.Combine(scriptDirectory, "ResetLeague.ps1");
            File.WriteAllText(leagueScriptPath, @"
Add-Type -AssemblyName System.Windows.Forms

Write-Host ""=> League reset script has started.""
$stopTasks = ""LeagueClientUx"", ""RiotClientServices"", ""LeagueClient""

foreach ($stopTask in $stopTasks) {
    Try {
        Write-Host ""=> Attempting to stop $stopTask gracefully...""
        $processes = Get-Process -Name $stopTask -ErrorAction SilentlyContinue
        if ($null -ne $processes) {
            foreach ($process in $processes) {
                Write-Host ""=> Found running instance of $stopTask with PID $($process.Id)""
                $process.CloseMainWindow() | Out-Null
                Write-Host ""=> Sent close request to $stopTask with PID $($process.Id)""
                Write-Host ""=> .""
                Start-Sleep -Seconds 1
                Write-Host ""=> ..""
                Start-Sleep -Seconds 1
                Write-Host ""=> ...""
                Start-Sleep -Seconds 1
                Write-Host ""=> ....""
                Start-Sleep -Seconds 1
                Write-Host ""=> .....""
                Start-Sleep -Seconds 1
                if (!$process.HasExited) {
                    Write-Host ""=> Process $stopTask with PID $($process.Id) did not close gracefully, forcing termination...""
                    $process | Stop-Process -Force
                } else {
                    Write-Host ""=> Process $stopTask with PID $($process.Id) closed gracefully.""
                }
            }
        } else {
            Write-Host ""=> No running instances of $stopTask found.""
        }
        Write-Host ""=> Stopped $stopTask""
    }
    Catch {
        Write-Host ""=> Failed to stop $stopTask. Error: $_""
    }
}

Write-Host ""=> Be patient... restarting League of legends (don't forget to report your jungler)""
Start-Sleep -Seconds 15

$startTasks = @{
    ""LeagueClientUx"" = ""C:\Riot Games\League of Legends\LeagueClientUx.exe""
    ""RiotClientServices"" = ""C:\Riot Games\Riot Client\RiotClientServices.exe""
}

foreach ($startTask in $startTasks.Keys) {
    Try {
        Write-Host ""=> Attempting to start $startTask...""
        Start-Process $startTasks[$startTask]
        Write-Host ""=> Started $startTask""
    }
    Catch {
        Write-Host ""=> Failed to start $startTask. Error: $_""
    }
}

Write-Host ""=> League reset script ended""");

            // Vanguard Reset Script
            string vanguardScriptPath = Path.Combine(scriptDirectory, "ResetVanguard.ps1");
            File.WriteAllText(vanguardScriptPath, @"
Add-Type -AssemblyName System.Windows.Forms

Write-Host ""=> Vanguard reset script has started.""

Try {
    Write-Host ""=> Attempting to stop Vanguard service (vgc)...""
    Stop-Service -Name ""vgc"" -Force -ErrorAction Stop
    Write-Host ""=> Vanguard service stopped successfully.""

    Write-Host ""=> Waiting 5 seconds before restarting vgc...""
    Start-Sleep -Seconds 5

    Write-Host ""=> Attempting to start Vanguard service (vgc)...""
    Start-Service -Name ""vgc"" -ErrorAction Stop
    Write-Host ""=> Vanguard service started successfully.""
}
Catch {
    Write-Host ""=> Failed to reset Vanguard (vgc). Error: $_""
}

Write-Host ""=> Vanguard reset script ended.""");

            // Both Reset Script
            string bothScriptPath = Path.Combine(scriptDirectory, "ResetBoth.ps1");
            File.WriteAllText(bothScriptPath, @"
Add-Type -AssemblyName System.Windows.Forms

Write-Host ""=> Script has started.""

# Reset Vanguard (vgc) Service
Try {
    Write-Host ""=> Attempting to stop Vanguard service (vgc)...""
    Stop-Service -Name ""vgc"" -Force -ErrorAction Stop
    Write-Host ""=> Vanguard service stopped successfully.""

    Write-Host ""=> Waiting 5 seconds before restarting vgc...""
    Start-Sleep -Seconds 5

    Write-Host ""=> Attempting to start Vanguard service (vgc)...""
    Start-Service -Name ""vgc"" -ErrorAction Stop
    Write-Host ""=> Vanguard service started successfully.""
}
Catch {
    Write-Host ""=> Failed to reset Vanguard (vgc). Error: $_""
}

# Stop League-related processes
$stopTasks = ""LeagueClientUx"", ""RiotClientServices"", ""LeagueClient""

foreach ($stopTask in $stopTasks) {
    Try {
        Write-Host ""=> Attempting to stop $stopTask gracefully...""
        $processes = Get-Process -Name $stopTask -ErrorAction SilentlyContinue
        if ($null -ne $processes) {
            foreach ($process in $processes) {
                Write-Host ""=> Found running instance of $stopTask with PID $($process.Id)""
                $process.CloseMainWindow() | Out-Null
                Write-Host ""=> Sent close request to $stopTask with PID $($process.Id)""
                Start-Sleep -Seconds 5
                if (!$process.HasExited) {
                    Write-Host ""=> Process $stopTask with PID $($process.Id) did not close gracefully, forcing termination...""
                    $process | Stop-Process -Force
                } else {
                    Write-Host ""=> Process $stopTask with PID $($process.Id) closed gracefully.""
                }
            }
        } else {
            Write-Host ""=> No running instances of $stopTask found.""
        }
        Write-Host ""=> Stopped $stopTask""
    }
    Catch {
        Write-Host ""=> Failed to stop $stopTask. Error: $_""
    }
}

Write-Host ""=> Be patient... restarting League of Legends (don't forget to report your jungler)""
Start-Sleep -Seconds 15

$startTasks = @{
    ""LeagueClientUx"" = ""C:\Riot Games\League of Legends\LeagueClientUx.exe""
    ""RiotClientServices"" = ""C:\Riot Games\Riot Client\RiotClientServices.exe""
}

foreach ($startTask in $startTasks.Keys) {
    Try {
        Write-Host ""=> Attempting to start $startTask...""
        Start-Process $startTasks[$startTask]
        Write-Host ""=> Started $startTask""
    }
    Catch {
        Write-Host ""=> Failed to start $startTask. Error: $_""
    }
}

Write-Host ""=> Script ended""");
        }
    }

    // Custom Button class with League of Legends style
    public class LeagueButton : Button
    {
        // Colors for different button states
        private Color hoverColor = Color.FromArgb(10, 60, 100);
        private Color pressColor = Color.FromArgb(15, 80, 120);
        private Color borderColor = Color.FromArgb(200, 170, 60);
        private bool isHovered = false;
        private bool isPressed = false;

        public LeagueButton()
        {
            // Initialize button with League of Legends styling
            this.FlatStyle = FlatStyle.Flat;
            this.FlatAppearance.BorderColor = borderColor;
            this.FlatAppearance.BorderSize = 1;
            this.FlatAppearance.MouseOverBackColor = hoverColor;
            this.FlatAppearance.MouseDownBackColor = pressColor;
            this.BackColor = Color.FromArgb(7, 48, 73);
            this.ForeColor = Color.FromArgb(200, 170, 60);
            this.Font = new Font("Arial", 10f, FontStyle.Bold);
            this.TextAlign = ContentAlignment.MiddleCenter;
            this.Cursor = Cursors.Hand;
            
            // Track mouse events for custom painting
            this.MouseEnter += (s, e) => { isHovered = true; this.Invalidate(); };
            this.MouseLeave += (s, e) => { isHovered = false; isPressed = false; this.Invalidate(); };
            this.MouseDown += (s, e) => { isPressed = true; this.Invalidate(); };
            this.MouseUp += (s, e) => { isPressed = false; this.Invalidate(); };
        }

        // Custom painting for League of Legends button style
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            
            // Draw custom glowing border effect when hovered
            if (isHovered)
            {
                using (Pen pen = new Pen(Color.FromArgb(200, borderColor), 2))
                {
                    g.DrawRectangle(pen, 1, 1, this.Width - 3, this.Height - 3);
                }
                
                // Add a subtle glow
                using (Pen pen = new Pen(Color.FromArgb(80, borderColor), 1))
                {
                    g.DrawRectangle(pen, 0, 0, this.Width - 1, this.Height - 1);
                    g.DrawRectangle(pen, 2, 2, this.Width - 5, this.Height - 5);
                }
            }
            
            // Add diagonal line accents in corners (League style)
            using (Pen accentPen = new Pen(Color.FromArgb(200, borderColor), 1))
            {
                // Top left corner
                g.DrawLine(accentPen, 0, 10, 10, 0);
                
                // Top right corner
                g.DrawLine(accentPen, this.Width - 11, 0, this.Width - 1, 10);
                
                // Bottom left corner
                g.DrawLine(accentPen, 0, this.Height - 11, 10, this.Height - 1);
                
                // Bottom right corner
                g.DrawLine(accentPen, this.Width - 11, this.Height - 1, this.Width - 1, this.Height - 11);
            }
        }
    }
}