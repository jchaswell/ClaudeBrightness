using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Management;
using System.Runtime.InteropServices;
using System.IO;

namespace ClaudeBrightness
{
    public partial class Form1 : Form
    {
        private List<PInvokeHelper.PHYSICAL_MONITOR> physicalMonitors = new List<PInvokeHelper.PHYSICAL_MONITOR>();
        private System.Windows.Forms.Timer autoAdjustTimer;
        private BrightnessSchedule schedule = new BrightnessSchedule();
        private FileSystemWatcher fileWatcher;
        private static Mutex mutex = new Mutex(true, "{ClaudeBrightness}");
        public Form1()
        {
            InitializeComponent();
            InitializeTrayIcon();
            InitializeFileWatcher();
            autoAdjustTimer = new System.Windows.Forms.Timer();
            autoAdjustTimer.Interval = 5000; // Check every Interval/1000 seconds
            autoAdjustTimer.Tick += AutoAdjustTimer_Tick;
        }
        private void InitializeFileWatcher()
        {
            string configPath = Path.Combine(Application.StartupPath, "brightness_schedule.txt");
            fileWatcher = new FileSystemWatcher()
            {
                Path = Path.GetDirectoryName(configPath),
                Filter = Path.GetFileName(configPath),
                NotifyFilter = NotifyFilters.LastWrite
            };
            fileWatcher.Changed += FileWatcher_Changed;
            fileWatcher.EnableRaisingEvents = true;
        }
        private void FileWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            // Invoke because this event is raised on a different thread
            this.BeginInvoke((MethodInvoker)delegate
            {
                LoadSchedule();
                // Optionally notify the user
                MessageBox.Show("Schedule file updated and reloaded.", "Update", MessageBoxButtons.OK, MessageBoxIcon.Information);
            });
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            EnumerateMonitors();
            LoadSchedule();
            int currentBrightness = GetCurrentBrightness();
            trackBarBrightness.Value = currentBrightness;
            UpdateBrightnessLabel(currentBrightness);
            autoAdjustTimer.Start();
        }

        private void EnumerateMonitors()
        {
            PInvokeHelper.MonitorEnumDelegate monitorEnumProc = (IntPtr hMonitor, IntPtr hdcMonitor, ref PInvokeHelper.RECT lprcMonitor, IntPtr dwData) =>
            {
                if (PInvokeHelper.GetNumberOfPhysicalMonitorsFromHMONITOR(hMonitor, out uint monitorCount))
                {
                    PInvokeHelper.PHYSICAL_MONITOR[] monitors = new PInvokeHelper.PHYSICAL_MONITOR[monitorCount];
                    if (PInvokeHelper.GetPhysicalMonitorsFromHMONITOR(hMonitor, monitorCount, monitors))
                    {
                        physicalMonitors.AddRange(monitors);
                    }
                }
                return true;
            };

            PInvokeHelper.EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, monitorEnumProc, IntPtr.Zero);
        }

        private void LoadSchedule()
        {
            string configPath = Path.Combine(Application.StartupPath, "brightness_schedule.txt");
            File.WriteAllText("debug_log.txt", $"Attempting to load schedule from: {configPath}\n");

            if (File.Exists(configPath))
            {
                try
                {
                    schedule.LoadFromFile(configPath);
                    if (schedule.GetEntryCount() == 0)
                    {
                        File.AppendAllText("debug_log.txt", "Schedule is empty after loading.\n");
                        MessageBox.Show("Brightness schedule file is empty or contains invalid entries. Using default settings.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else
                    {
                        File.AppendAllText("debug_log.txt", $"Loaded {schedule.GetEntryCount()} entries.\n");
                    }
                }
                catch (Exception ex)
                {
                    File.AppendAllText("debug_log.txt", $"Error loading brightness schedule: {ex.Message}\n");
                    MessageBox.Show($"Error loading brightness schedule: {ex.Message}\nUsing default settings.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                File.AppendAllText("debug_log.txt", "Brightness schedule file not found.\n");
                MessageBox.Show("Brightness schedule file not found. Using default settings.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void AutoAdjustTimer_Tick(object sender, EventArgs e)
        {
            if (!checkBoxOverrideSchedule.Checked)
            {
                int newBrightness = schedule.GetBrightnessForCurrentTime();
                trackBarBrightness.Value = newBrightness;
                SetBrightness(newBrightness);
                UpdateBrightnessLabel(newBrightness);
            }
        }

        private int GetCurrentBrightness()
        {
            if (physicalMonitors.Count == 0) return 50;

            IntPtr currentValue = Marshal.AllocCoTaskMem(sizeof(uint));
            IntPtr maxValue = Marshal.AllocCoTaskMem(sizeof(uint));

            try
            {
                if (PInvokeHelper.GetVCPFeatureAndVCPFeatureReply(physicalMonitors[0].hPhysicalMonitor, PInvokeHelper.VCP_CODE_BRIGHTNESS, IntPtr.Zero, currentValue, maxValue))
                {
                    uint currentBrightness = (uint)Marshal.ReadInt32(currentValue);
                    uint maxBrightness = (uint)Marshal.ReadInt32(maxValue);
                    return (int)((currentBrightness * 100) / maxBrightness);
                }
            }
            finally
            {
                Marshal.FreeCoTaskMem(currentValue);
                Marshal.FreeCoTaskMem(maxValue);
            }

            return 50;
        }

        private void SetBrightness(int brightness)
        {
            foreach (var monitor in physicalMonitors)
            {
                if (!PInvokeHelper.SetVCPFeature(monitor.hPhysicalMonitor, PInvokeHelper.VCP_CODE_BRIGHTNESS, (uint)brightness))
                {
                    int errorCode = Marshal.GetLastWin32Error();
                    MessageBox.Show($"Failed to set brightness. Error code: {errorCode}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void checkBoxOverrideSchedule_CheckedChanged(object sender, EventArgs e)
        {
            trackBarBrightness.Enabled = checkBoxOverrideSchedule.Checked;
            if (!checkBoxOverrideSchedule.Checked)
            {
                int newBrightness = schedule.GetBrightnessForCurrentTime();
                trackBarBrightness.Value = newBrightness;
                SetBrightness(newBrightness);
                UpdateBrightnessLabel(newBrightness);
            }
        }



        private void UpdateBrightnessLabel(int brightness)
        {
            labelBrightness.Text = $"Brightness: {brightness}%";
        }

        private void trackBarBrightness_Scroll(object sender, EventArgs e)
        {
            int brightness = trackBarBrightness.Value;
            UpdateBrightnessLabel(brightness);
            SetBrightness(brightness);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
            }
            else
            {
                trayIcon.Visible = false;
            }
            base.OnFormClosing(e);
        }

        private void OpenScheduleFile_Click(object sender, EventArgs e)
        {
            string configPath = Path.Combine(Application.StartupPath, "brightness_schedule.txt");
            if (File.Exists(configPath))
            {
                System.Diagnostics.Process.Start("notepad.exe", configPath);
            }
            else
            {
                MessageBox.Show("Schedule file not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        //system tray
        private NotifyIcon trayIcon;
        private ContextMenuStrip trayMenu;

        private void InitializeTrayIcon()
        {
            trayIcon = new NotifyIcon();
            trayIcon.Text = "Brightness Control";
            trayIcon.Icon = this.Icon; // Set an appropriate icon

            trayMenu = new ContextMenuStrip();
            trayMenu.Items.Add("Show", null, ShowForm);
            trayMenu.Items.Add("Exit", null, ExitApplication);

            trayIcon.ContextMenuStrip = trayMenu;
            trayIcon.Visible = true;

            this.FormClosing += Form1_FormClosing;
        }

        private void ShowForm(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }

        private void ExitApplication(object sender, EventArgs e)
        {
            trayIcon.Visible = false;
            Application.Exit();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
            }
        }

    }

    public class BrightnessScheduleEntry
    {
        public TimeSpan Time { get; set; }
        public int Brightness { get; set; }
    }

    public class BrightnessSchedule
    {
        private List<BrightnessScheduleEntry> _entries = new List<BrightnessScheduleEntry>();

        public void LoadFromFile(string filePath)
        {
            _entries.Clear();
            try
            {
                using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var reader = new StreamReader(fileStream))
                {
                    string[] lines = reader.ReadToEnd().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    File.AppendAllText("debug_log.txt", $"Read {lines.Length} lines from file.\n");

                    foreach (string line in lines)
                    {
                        File.AppendAllText("debug_log.txt", $"Processing line: '{line}'\n");
                        string[] parts = line.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        File.AppendAllText("debug_log.txt", $"Split into {parts.Length} parts.\n");

                        if (parts.Length == 2)
                        {
                            if (TryParseTimeFormat(parts[0], out TimeSpan time) &&
                                int.TryParse(parts[1], out int brightness) &&
                                brightness >= 0 && brightness <= 100)
                            {
                                _entries.Add(new BrightnessScheduleEntry { Time = time, Brightness = brightness });
                                File.AppendAllText("debug_log.txt", $"Added entry: Time={time}, Brightness={brightness}\n");
                            }
                            else
                            {
                                File.AppendAllText("debug_log.txt", $"Failed to parse line: Invalid time format or brightness value.\n");
                            }
                        }
                        else
                        {
                            File.AppendAllText("debug_log.txt", $"Invalid line format: Expected 2 parts, got {parts.Length}.\n");
                        }
                    }
                }
            }
            catch (IOException ex)
            {
                File.AppendAllText("debug_log.txt", $"IOException in LoadFromFile: {ex.Message}\n");
                throw new ApplicationException("Error reading schedule file. It may be in use by another process.", ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                File.AppendAllText("debug_log.txt", $"UnauthorizedAccessException in LoadFromFile: {ex.Message}\n");
                throw new ApplicationException("Access to the schedule file was denied.", ex);
            }
            catch (Exception ex)
            {
                File.AppendAllText("debug_log.txt", $"Unexpected exception in LoadFromFile: {ex.Message}\n");
                throw new ApplicationException("An unexpected error occurred while loading the schedule.", ex);
            }

            _entries = _entries.OrderBy(e => e.Time).ToList();
            File.AppendAllText("debug_log.txt", $"Final entry count: {_entries.Count}\n");
        }

        private bool TryParseTimeFormat(string timeString, out TimeSpan result)
        {
            result = TimeSpan.Zero;
            if (timeString.Length != 4 || !int.TryParse(timeString, out int timeInt))
            {
                return false;
            }

            int hours = timeInt / 100;
            int minutes = timeInt % 100;
            if (hours < 0 || hours >= 24 || minutes < 0 || minutes >= 60)
            {
                return false;
            }

            result = new TimeSpan(hours, minutes, 0);
            return true;
        }

        public int GetBrightnessForCurrentTime()
        {
            if (_entries.Count == 0)
            {
                return 100; // Default brightness if no entries
            }

            var now = DateTime.Now.TimeOfDay;
            var lastEntry = _entries.LastOrDefault(e => e.Time <= now);

            if (lastEntry == null)
            {
                // If current time is before the first scheduled time, use the last entry of the day
                return _entries.Last().Brightness;
            }

            return lastEntry.Brightness;
        }

        public int GetEntryCount()
        {
            return _entries.Count;
        }

    }
}