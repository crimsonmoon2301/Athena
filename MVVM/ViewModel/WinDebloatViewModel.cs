using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
        public ICommand DefaultDebloatCommand { get; }
        public ICommand CustomDebloatCommand { get; }
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
            DefaultDebloatCommand = new RelayCommand(ExecuteDefaultDebloat);
            CustomDebloatCommand = new RelayCommand(ExecuteCustomDebloat);
            ShowOptionsCommand = new RelayCommand(ExecuteShowDebloatOptions);
            ViewLogsCommand = new RelayCommand(ExecuteViewLogs);
            UninstallCommand = new RelayCommand(ExecuteUninstall);

            // Load package data on startup
            LoadPackageDataAsync();
        }

        // Command implementations with object parameter
        private void ExecuteDefaultDebloat(object parameter)
        {
            System.Windows.MessageBox.Show("Executing Default Debloat...");
        }

        private void ExecuteCustomDebloat(object parameter)
        {
            System.Windows.MessageBox.Show("Opening Custom Debloat Options...");
        }

        private void ExecuteShowDebloatOptions(object parameter)
        {
            System.Windows.MessageBox.Show("Showing Debloat Options...");
        }

        private void ExecuteViewLogs(object parameter)
        {
            System.Windows.MessageBox.Show("Displaying logs...");
        }

        // Load JSON data from the GitHub URL
        private async void LoadPackageDataAsync()
        {
            string url = "https://raw.githubusercontent.com/crimsonmoon2301/AthenaJSON/main/getpackages.json";

            try
            {
                using (HttpClient client = new HttpClient())
                {
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

                                // Extract and populate SafeToRemovePackages
                                if (packagesElement.TryGetProperty("safe_to_remove", out JsonElement safeToRemove))
                                {
                                    foreach (JsonElement package in safeToRemove.EnumerateArray())
                                    {
                                        PackageInfo packageInfo = new PackageInfo();
                                        if (package.TryGetProperty("name", out JsonElement name))
                                            packageInfo.Name = name.GetString();

                                        if (package.TryGetProperty("description", out JsonElement description))
                                            packageInfo.Description = description.GetString();

                                        if (package.TryGetProperty("tip", out JsonElement tip))
                                            packageInfo.Tip = tip.GetString();

                                        if (package.TryGetProperty("comm", out JsonElement comm))
                                            packageInfo.Command = comm.GetString();

                                        _safeToRemovePackages.Add(packageInfo);
                                    }
                                }

                                // Extract and populate MayCauseBreakagesPackages
                                if (packagesElement.TryGetProperty("may_cause_breakages", out JsonElement mayCauseBreakages))
                                {
                                    foreach (JsonElement package in mayCauseBreakages.EnumerateArray())
                                    {
                                        PackageInfo packageInfo = new PackageInfo();
                                        if (package.TryGetProperty("name", out JsonElement name))
                                            packageInfo.Name = name.GetString();

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
                    // Create the process to run PowerShell as admin
                    var process = new System.Diagnostics.Process();
                    process.StartInfo.FileName = "powershell.exe";
                    process.StartInfo.Arguments = $" {package.Command}";
                    process.StartInfo.Verb = "runas"; // Elevates process to admin
                    process.StartInfo.UseShellExecute = true; // Required for Verb = "runas"
                    process.StartInfo.CreateNoWindow = false;

                    process.Start();

                    // Since we're running as admin, output capture won't work unless redirected to a file
                    process.WaitForExit();
                }
                catch (Exception ex)
                {
                    // Log any exceptions for debugging
                    System.Windows.MessageBox.Show($"An error occurred: {ex.Message}");
                }
            }
            else
            {
                System.Windows.MessageBox.Show($"Something went horribly wrong. Process halted!");
            }
        }


        // JSON Classes
        public class PackageInfo
        {
            public string Name { get; set; }
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

