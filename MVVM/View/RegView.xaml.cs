using Galadarbs_IT23033.Scripts;
using Microsoft.Win32; // For the save dialog
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace Galadarbs_IT23033.MVVM.View
{
    /// <summary>
    /// Interaction logic for RegView.xaml
    /// </summary>
    public partial class RegView : UserControl
    {
        public ObservableCollection<string> AvailableOptions { get; set; }
        public ObservableCollection<string> SelectedOptions { get; set; }
        public ObservableCollection<string> PresetOptions { get; set; }

        private string _selectedPreset;
        public string SelectedPreset // Constructor for the presets. Everything crumbles without it and it's making my hair go grey
        {
            get => _selectedPreset;
            set
            {
                _selectedPreset = value;
                LoadAvailableOptions(); // Update available options when the preset changes
            }
        }

        // For the experimental preset. It is disabled by default till checkbox is checked.
        private bool isExperimentalEnabled;
        public bool IsExperimentalEnabled
        {
            get => isExperimentalEnabled;
            set
            {
                if (isExperimentalEnabled != value)
                {
                    isExperimentalEnabled = value;
                    LoadAvailableOptions();
                }
            }
        }

        public RegView()
        {
            InitializeComponent();
            DataContext = this; // Set the DataContext to this instance

            AvailableOptions = new ObservableCollection<string>();
            SelectedOptions = new ObservableCollection<string>();
            PresetOptions = new ObservableCollection<string>
            {
                 "Custom",
                 "Privacy Only",
                 "Performance Only",
                 "Experimental"
            };

            SelectedPreset = "Custom";
            Recom_btn.IsChecked = false;
            Experim_btn.IsChecked = false;
        }

        // A function that gets called. It's basically a big switch which calls functions that actually contains the lists. Fascinating, ain't it?
        private void LoadAvailableOptions()
        {
            AvailableOptions.Clear(); // Clear the list at the beginning

            if (!string.IsNullOrEmpty(SelectedPreset))
            {
                switch (SelectedPreset)
                {
                    case "Custom":
                        LoadPrivacyOptions();
                        LoadPerformanceOptions();
                        break;
                    case "Privacy Only":
                        LoadPrivacyOptions();
                        break;
                    case "Performance Only":
                        LoadPerformanceOptions();
                        break;
                    case "Experimental":
                        if (IsExperimentalEnabled) // Check if the experimental preset is enabled
                        {
                            LoadExperimentalSettings();
                        }
                        else
                        {
                            MessageBoxResult warning = MessageBox.Show(
                             "This preset is disabled by default due to them being untested, therefore causing possible stability issues. If you want to use them, enable the preset first.",
                                "Preset disabled",
                                MessageBoxButton.OK,
                                 MessageBoxImage.Error);
                        }
                        break;
                }
            }

            // Remove any options that are already selected
            foreach (var selectedOption in SelectedOptions.ToList())
            {
                AvailableOptions.Remove(selectedOption);
            }
        }

        private void LoadPrivacyOptions()
        {
            string[] privacyOptions = new string[]
            {
                "Disable Customer Experience Program",
                "Disable MareBackup",
                "Disable Telemetry",
                "Disable Copilot",
                "Disable Cortana",
                "Disable Location Tracking",
                "Disable Recall",
                "Disable Ad ID for All Users"

            };

            foreach (var option in privacyOptions)
            {
                if (!AvailableOptions.Contains(option))
                {
                    AvailableOptions.Add(option);
                }
            }
        }
        private void LoadPerformanceOptions()
        {
            string[] performanceOptions = new string[]
            {
                "Enable Ultimate Performance Powerplan",
                "Empty Recycle Bin",
                "Give 3D apps higher priority",
                "Give GPU higher priority",
                "Give CPU higher priority",
                "Give Multimedia higher priority",

            };

            foreach (var option in performanceOptions)
            {
                if (!AvailableOptions.Contains(option))
                {
                    AvailableOptions.Add(option);
                }
            }
        }

        private void LoadExperimentalSettings()
        {
            
            string[] experimentalOptions = new string[]
            {
                "Disable Defender",
                "Disable Security",
                "Clear pagefile after shutdown"
            };

            foreach (var option in experimentalOptions)
            {
                if (!AvailableOptions.Contains(option))
                {
                    AvailableOptions.Add(option);
                }
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (AvailableListBox.SelectedItem is string selectedItem)
            {
                SelectedOptions.Add(selectedItem);
                AvailableOptions.Remove(selectedItem);
            }
        }

        private void ScriptSaveBtn_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                FileName = "output.bat", // Default file name
                Filter = "Batch files (*.bat)|*.bat", // Filter for .bat files
                DefaultExt = ".bat" // Default file extension
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                string filePath = saveFileDialog.FileName;
                string fileExtension = filePath.Substring(filePath.Length - 4).ToLower(); // Get last 4 characters

                // Call the appropriate method based on the file extension
                if (fileExtension == ".bat")
                {
                    BatFileGenerator.GenerateBatFile(SelectedOptions, filePath);
                    MessageBox.Show($"BAT file saved as {filePath}");
                }
                else
                {
                    MessageBox.Show("Please select a valid file type.");
                }
            }
        }
        
        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedListBox.SelectedItem is string selectedItem)
            {
                SelectedOptions.Remove(selectedItem);
                AvailableOptions.Add(selectedItem); // Return the removed item to AvailableOptions
            }
        }

        private void RemoveAllButton_Click(object sender, RoutedEventArgs e)
        {
            // Show confirmation dialog
            MessageBoxResult result = MessageBox.Show("Are you sure you want to remove all selected options? If you just need to remove one item, you can use " +
                                                        "Remove instead.",
                                                       "Confirm Removal",
                                                       MessageBoxButton.YesNo,
                                                       MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                if (SelectedOptions.Count == 0)
                {
                    MessageBox.Show("There are no items to remove.",
                                    "Error",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                    return; // Exit the method early
                }

                // Move all items back to AvailableOptions
                foreach (var item in SelectedOptions.ToList()) // Use ToList() to avoid modifying the collection while iterating
                {
                    SelectedOptions.Remove(item);
                    AvailableOptions.Add(item);
                }
                MessageBox.Show("All selected options have been removed successfully.",
                       "Success",
                       MessageBoxButton.OK,
                       MessageBoxImage.Information);
            }
        }

        private void Recom_btn_Checked(object sender, RoutedEventArgs e)
        {
            if (SelectedPreset == "Custom")
            {
                MessageBox.Show("Cannot use recommended settings with a custom preset.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Recom_btn.IsChecked = false; // Uncheck the checkbox if it's a custom preset
                return;
            }

            // Add recommended options if the checkbox is checked
            if (Recom_btn.IsChecked == true)
            {
                AddRecommendedOptions();
            }
        }

        private void Recom_btn_Unchecked(object sender, RoutedEventArgs e)
        {
            // Clear the selected options list and move them back to available options
            foreach (var item in SelectedOptions.ToList()) // Use ToList() to avoid modifying the collection while iterating
            {
                SelectedOptions.Remove(item);
                if (!AvailableOptions.Contains(item)) // Check if the item is already in AvailableOptions
                {
                    AvailableOptions.Add(item);
                }
            }
        }

        private void AddRecommendedOptions()
        {
            string[] recommendedOptions = SelectedPreset switch
            {
                "Performance Only" => new string[]
                {
                    "Enable Ultimate Performance Powerplan",
                    "Give GPU higher priority",
                    "Give CPU higher priority"
                },
                "Privacy Only" => new string[]
                {
                    "Disable Customer Experience Program",
                    "Disable Telemetry",
                    "Disable Location Tracking",
                    "Disable Recall",
                    "Disable Ad ID for All Users"
                },
                _ => Array.Empty<string>()
            };

            foreach (var option in recommendedOptions)
            {
                if (!SelectedOptions.Contains(option) && AvailableOptions.Contains(option))
                {
                    SelectedOptions.Add(option);
                }
            }
        }

        private void ScriptExecBtn_Click(object sender, RoutedEventArgs e)
        {
            // Check if there are any selected items in the SelectedListBox
            if (SelectedListBox.Items.Count > 0)
            {
                // Create a temporary file path for the batch file
                string tempFilePath = Path.Combine(Path.GetTempPath(), string.Format("{0}.bat", Guid.NewGuid()));

                // Create an array to hold all options from the SelectedListBox
                string[] selectedOptions = new string[SelectedListBox.Items.Count];

                // Collect all items into the array (instead of just selected)
                for (int i = 0; i < SelectedListBox.Items.Count; i++)
                {
                    selectedOptions[i] = (string)SelectedListBox.Items[i];
                }

                // Generate the batch file using the BatFileGenerator
                BatFileGenerator.GenerateBatFile(selectedOptions, tempFilePath);

                // Execute the generated batch file
                ExecuteScript(tempFilePath);
            }
            else
            {
                MessageBox.Show("No scripts available to execute.", "No Scripts", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ExecuteScript(string filePath)
        {
            try
            {
                // Execute the temporary batch file
                System.Diagnostics.ProcessStartInfo processStartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true // Use shell to execute
                };

                // Start the process
                System.Diagnostics.Process.Start(processStartInfo);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while executing the script: {ex.Message}",
                                "Execution Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }

        private void Experim_btn_Checked(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(
           "You're about to enable a highly experimental preset that are untested and can possibly cause stability issues. Are you sure you want to do this?",
           "Warning: Experimental Preset",
           MessageBoxButton.YesNo,
           MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                IsExperimentalEnabled = true; // Enable the experimental preset
                MessageBox.Show("Experimental preset enabled. Hic sunt dracones.");
            }
            else
            {
                (sender as CheckBox).IsChecked = false; // Uncheck the checkbox if the user chooses No
            }
        }
        public void Experim_btn_Unchecked(object sender, RoutedEventArgs e)
        {
            IsExperimentalEnabled = false; // Disable the experimental preset
        }
    }
}