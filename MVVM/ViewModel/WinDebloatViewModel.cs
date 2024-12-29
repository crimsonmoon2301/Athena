using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Galadarbs_IT23033.Core;

namespace Galadarbs_IT23033.MVVM.ViewModel
{
    internal class WinDebloatViewModel : ObservableObject
    {
        // Commands

        // Log storage for uninstalled packages
        private readonly ObservableCollection<string> _uninstallLogs = new ObservableCollection<string>();

        //public ICommand ReinstallCommand { get; }
        public ICommand ShowOptionsCommand { get; }
        public ICommand ViewLogsCommand { get; }
        public ICommand UninstallCommand { get; }


        // Properties for UI Binding
        private List<PackageInfo> _safeToRemovePackages;
        public List<PackageInfo> SafeToRemovePackages
        {
            get => _safeToRemovePackages;
            set
            {
                _safeToRemovePackages = value;
                OnPropertyChanged();
            }
        }

        private List<PackageInfo> _mayCauseBreakagesPackages;
        public List<PackageInfo> MayCauseBreakagesPackages
        {
            get => _mayCauseBreakagesPackages;
            set
            {
                _mayCauseBreakagesPackages = value;
                OnPropertyChanged();
            }
        }

        private PackageInfo _selectedPackage;
        public PackageInfo SelectedPackage
        {
            get => _selectedPackage;
            set
            {
                _selectedPackage = value;
                OnPropertyChanged(); // Notify the UI of the change
            }
        }


        public WinDebloatViewModel()
        {
            // Initialize commands with their respective logic
            //ReinstallCommand = new RelayCommand(ExecuteReinstall);
            ViewLogsCommand = new RelayCommand(ExecuteViewLogs);
            UninstallCommand = new RelayCommand(ExecuteUninstall);

            // Load package data on startup
            LoadPackageDataAsync();
        }

        // Command implementations with object parameter
       
        private void ExecuteViewLogs(object parameter)
        {
            if (!_uninstallLogs.Any())
            {
                MessageBox.Show("No uninstallation logs available.", "Logs", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var logsMessage = string.Join("\n", _uninstallLogs);
            MessageBox.Show(logsMessage, "Uninstallation Logs", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // Load JSON data from the GitHub URL
        private async void LoadPackageDataAsync()
        {
            string url = "https://raw.githubusercontent.com/crimsonmoon2301/AthenaJSON/main/getpackages.json";

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // Disable caching. Committing too fast apparently caches the result. B O L L O C K S
                    client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue
                    {
                        NoCache = true,
                        NoStore = true
                    };

                    string response = await client.GetStringAsync(url);

                    if (!string.IsNullOrEmpty(response))
                    {
                        // Parse the JSON response
                        using (JsonDocument doc = JsonDocument.Parse(response))
                        {
                            if (doc.RootElement.TryGetProperty("packages", out JsonElement packagesElement))
                            {
                                // Initialize lists
                                _safeToRemovePackages = new List<PackageInfo>();
                                _mayCauseBreakagesPackages = new List<PackageInfo>();

                                // Extract and populate packages that are safe to remove
                                if (packagesElement.TryGetProperty("safe_to_remove", out JsonElement safeToRemove))
                                {
                                    foreach (JsonElement package in safeToRemove.EnumerateArray())
                                    {
                                        PackageInfo packageInfo = new PackageInfo();
                                        if (package.TryGetProperty("name", out JsonElement name))
                                            packageInfo.Name = name.GetString();

                                        if (package.TryGetProperty("packName", out JsonElement packName))
                                            packageInfo.packName = packName.GetString();

                                        if (package.TryGetProperty("description", out JsonElement description))
                                            packageInfo.Description = description.GetString();

                                        if (package.TryGetProperty("tip", out JsonElement tip))
                                            packageInfo.Tip = tip.GetString();

                                        if (package.TryGetProperty("comm", out JsonElement comm))
                                            packageInfo.Command = comm.GetString();

                                        _safeToRemovePackages.Add(packageInfo);
                                    }
                                }

                                // Extract and populate packages that cause breakages. 
                                if (packagesElement.TryGetProperty("may_cause_breakages", out JsonElement mayCauseBreakages))
                                {
                                    foreach (JsonElement package in mayCauseBreakages.EnumerateArray())
                                    {
                                        PackageInfo packageInfo = new PackageInfo();
                                        if (package.TryGetProperty("name", out JsonElement name))
                                            packageInfo.Name = name.GetString();

                                        if (package.TryGetProperty("packName", out JsonElement packName))
                                            packageInfo.packName = packName.GetString();

                                        if (package.TryGetProperty("description", out JsonElement description))
                                            packageInfo.Description = description.GetString();

                                        if (package.TryGetProperty("tip", out JsonElement tip))
                                            packageInfo.Tip = tip.GetString();

                                        if (package.TryGetProperty("comm", out JsonElement comm))
                                            packageInfo.Command = comm.GetString();

                                        _mayCauseBreakagesPackages.Add(packageInfo);
                                    }
                                }

                                // Update UI bindings
                                SafeToRemovePackages = _safeToRemovePackages;
                                MayCauseBreakagesPackages = _mayCauseBreakagesPackages;
                            }
                            else
                            {
                                MessageBox.Show("The 'packages' property was not found in the JSON.");
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("No data received. Please check the URL or your internet connection.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error fetching data: {ex.Message}");
            }
        }

        private void ExecuteUninstall(object parameter)
        {
            if (parameter is PackageInfo package && !string.IsNullOrEmpty(package.Command))
            {
                try
                {
                    // Define log file path
                    string logDirectory = Path.Combine(Path.GetTempPath(), "AthenaPCUtility");
                    Directory.CreateDirectory(logDirectory); // Ensure the directory exists
                    string logFilePath = Path.Combine(logDirectory, "uninstall.log");

                    // Create a PowerShell script for the uninstall operation
                    string tempScriptPath = Path.Combine(logDirectory, "uninstall_package.ps1");
                    string scriptContent = $@"
                    $logFile = '{logFilePath}'
                    $package = Get-AppxPackage -AllUsers -Name '{package.packName}' -ErrorAction SilentlyContinue
                    if ($null -ne $package) {{
                        {package.Command}
                        Add-Content -Path $logFile -Value '[{DateTime.Now:dd-MM-yyyy HH:mm:ss}] Uninstallation successful: {package.Name}'
                    }} else {{
                        Add-Content -Path $logFile -Value '[{DateTime.Now:dd-MM-yyyy HH:mm:ss}] Package not found: {package.Name}'
                    }}";
                    File.WriteAllText(tempScriptPath, scriptContent);

                    // Configure and execute the PowerShell process
                    var process = new System.Diagnostics.Process
                    {
                        StartInfo = new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = "powershell.exe",
                            Arguments = $"-NoProfile -ExecutionPolicy Bypass -File \"{tempScriptPath}\"",
                            Verb = "runas", // Elevate to admin
                            UseShellExecute = true,
                            CreateNoWindow = true
                        }
                    };
                    process.Start();
                    process.WaitForExit();

                    // Reload log file contents after script execution
                    List<string> existingLogs = new List<string>();
                    if (File.Exists(logFilePath))
                    {
                        existingLogs = File.ReadAllLines(logFilePath).ToList();
                    }

                    // Notify the user and log the result
                    if (existingLogs.Count > 0)
                    {
                        string lastLogEntry = existingLogs[^1]; // Get the last log entry
                        _uninstallLogs.Add(lastLogEntry); // Add to in-memory log storage

                        if (lastLogEntry.Contains("Uninstallation successful"))
                        {
                            MessageBox.Show($"The package '{package.Name}' has been successfully uninstalled.");
                        }
                        else if (lastLogEntry.Contains("Package not found"))
                        {
                            MessageBox.Show($"The package '{package.Name}' was not found.");
                        }
                        else
                        {
                            MessageBox.Show($"Unexpected log entry: {lastLogEntry}");
                        }
                    }
                    else
                    {
                        string errorMessage = "No log entry found. Something went wrong.";
                        _uninstallLogs.Add(errorMessage); // Log the error
                        MessageBox.Show(errorMessage);
                    }

                    // Clean up the temporary script
                    File.Delete(tempScriptPath);
                }
                catch (Exception ex)
                {
                    string exceptionMessage = $"An error occurred: {ex.Message}";
                    _uninstallLogs.Add(exceptionMessage); // Log the exception
                    MessageBox.Show(exceptionMessage);
                }
            }
            else
            {
                string invalidPackageMessage = "Invalid package or missing uninstall command.";
                _uninstallLogs.Add(invalidPackageMessage); // Log the invalid package message
                MessageBox.Show(invalidPackageMessage);
            }
        }


        // JSON Classes
        public class PackageInfo
        {
            public string Name { get; set; }
            public string packName { get; set; }
            public string Description { get; set; }
            public string Tip { get; set; }
            public string Command { get; set; }
        }

        private class PackagesWrapper
        {
            public Packages Packages { get; set; }
        }

        public class Packages
        {
            public List<PackageInfo> SafeToRemove { get; set; }
            public List<PackageInfo> MayCauseBreakages { get; set; }
        }
    }
}

