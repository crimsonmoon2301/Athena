using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Text.Json;
using System.IO;
using static System.Net.WebRequestMethods;
using System.Diagnostics;
using Microsoft.VisualBasic;
using Galadarbs_IT23033.MVVM.ViewModel;

namespace Galadarbs_IT23033.MVVM.View
{
    /// <summary>
    /// Interaction logic for ISOBuilderView.xaml
    /// </summary>
    public partial class ISOBuilderView : UserControl
    {
        // Ground work for API. I have no idea if it even works.
        private static readonly HttpClient client = new HttpClient(); // Used to call the API

        public ISOBuilderView()
        {
            InitializeComponent();
        }
        
        // Incomplete. I will have a migraine with this.
        public async Task TestAPI()
        {
            string url = "https://api.uupdump.net/";
            try
            {
                string response = await client.GetStringAsync(url);

                if (!string.IsNullOrEmpty(response))
                {
                    // Parse the JSON response
                    using (JsonDocument doc = JsonDocument.Parse(response))
                    {
                        if (doc.RootElement.TryGetProperty("response", out JsonElement responseElement) &&
                            responseElement.TryGetProperty("apiVersion", out JsonElement apiVersion))
                        {
                            MessageBox.Show($"API Version: {apiVersion.GetString()}\n" +
                                $"You can try fetching builds now.", "Response Received");
                        }
                        else
                        {
                            MessageBox.Show("API version not found in response. The API format may be changed. Fix TBA");
                        }
                    }
                }
                else
                {
                    MessageBox.Show("No data received. Are you connected to the internet?");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error fetching data: " + ex.Message);
            }
        }

        private async void TestAPI_Click(object sender, RoutedEventArgs e)
        {
            await TestAPI();
        }

        // this thing is a absolute fucking nightmare istg
        public List<BuildData> Builds { get; set; }
        public Dictionary<string, List<BuildData>> BuildsByVersion { get; set; }

        private Dictionary<string, string> languages = BuilderConstants.Languages;
        private Dictionary<string, string> edition = BuilderConstants.Editions;
        private Dictionary<string, string> serverEdition = BuilderConstants.ServerEditions;
        public async Task GetBuildVersions()
        {
            string url = "https://api.uupdump.net/listid.php";
            try
            {
                string response = await client.GetStringAsync(url);
                var jsonResponse = JsonSerializer.Deserialize<JsonElement>(response);

                if (jsonResponse.TryGetProperty("response", out var responseData) &&
                    responseData.TryGetProperty("builds", out var builds))
                {
                    // When I thought that arrays would do the job, turns out Lists do it billion times better than arrays at the expense of a bit of performance degradation.
                    // I mean.. Dynamic arrays are amazing. Not my fault that I worsen global warming with this code, we're all screwed anyway.
                    Builds = new List<BuildData>();
                    BuildsByVersion = new Dictionary<string, List<BuildData>>();
                    HashSet<string> mainVersions = new HashSet<string>();

                    foreach (var build in builds.EnumerateArray())
                    {
                        string? title = build.GetProperty("title").GetString();
                        string? buildNumber = build.GetProperty("build").GetString();
                        string? architecture = build.GetProperty("arch").GetString();
                        string? uuid = build.GetProperty("uuid").GetString();


                        string mainVersion;
                        if (title.Contains("Windows 11"))
                        {
                            mainVersion = "Windows 11";
                        }
                        else if (title.Contains("Windows 10"))
                        {
                            mainVersion = "Windows 10";
                        }
                        else if (title.Contains("Windows Server"))
                        {
                            mainVersion = "Windows Server";
                        }
                        else
                        {
                            mainVersion = "Other";
                        }

                        var buildData = new BuildData
                        {
                            Title = title,
                            BuildNumber = buildNumber,
                            Architecture = architecture,
                            Uuid = uuid,
                            Edition = string.Join(", ", edition.Select(pvk => $"{pvk.Key}")),
                            Lang = string.Join(", ", languages.Select(kvp => $"{kvp.Key}"))

                            //Lang = string.Join(", ", languages.Select(kvp => $"{kvp.Key}: {kvp.Value}")) // Add all languages
                        };
                        Builds.Add(buildData);

                        if (!BuildsByVersion.ContainsKey(mainVersion))
                        {
                            BuildsByVersion[mainVersion] = new List<BuildData>();
                        }
                        BuildsByVersion[mainVersion].Add(buildData);
                        mainVersions.Add(mainVersion);
                    }

                    Dispatcher.Invoke(() =>
                    {
                        OperatingSystemComboBox.ItemsSource = mainVersions.ToList();
                        LangCombo.ItemsSource = languages.Values.ToList();
                        EditionCombo.ItemsSource = edition.Values.ToList();
                    });

                    MessageBox.Show("Builds loaded successfully. Use the Combobox to access them.");
                }
                else
                {
                    MessageBox.Show("No builds found.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error fetching builds: " + ex.Message);
            }
        }
        private bool isFiltering = false; // Flag to prevent multiple triggers

        private CheckBox x86CheckBox; // Declare a variable for the dynamically added checkbox. This will be managed in codeline to avoid cluttering the xaml file.

        private void OperatingSystemComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isFiltering) return; // Prevent re-entry during filtering
            isFiltering = true;

            try
            {
                // Dynamically add x32 checkbox for Windows 10 and for updates in "Others" since some have the x86 tag. 
                if (OperatingSystemComboBox.SelectedItem?.ToString() == "Windows 10" || OperatingSystemComboBox.SelectedItem?.ToString() == "Other")
                {
                    if (x86CheckBox == null) // Only add the checkbox if it doesn't already exist
                    {
                        x86CheckBox = new CheckBox
                        {
                            Name = "x86_chkbx",
                            Content = "x86",
                            Foreground = new SolidColorBrush(Colors.White),
                            FontSize = 14,
                        };

                        //// Apply the style from XAML. 
                        if (Application.Current.Resources.Contains("RoundedCheckboxStyle")) 
                        {
                            x86CheckBox.Style = (Style)Application.Current.Resources["RoundedCheckboxStyle"];
                        }

                        // Attach event handlers for the dynamically added checkbox
                        x86CheckBox.Checked += FilterCheckbox_Changed;
                        x86CheckBox.Unchecked += FilterCheckbox_Changed;

                        // Add the checkbox to the StackPanel
                        ArchitecturePanel.Children.Add(x86CheckBox);
                    }
                }
                else if (x86CheckBox != null) // Remove the x32 checkbox if a different OS is selected
                {
                    ArchitecturePanel.Children.Remove(x86CheckBox);
                    x86CheckBox = null; // Reset the variable
                }

                // Check if a version is selected before proceeding
                if (OperatingSystemComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Filter will not be applied due to the list being empty.\nPopulate the list first and then select your desired operating system.",
                                    "List empty", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Reset the checkboxes
                    AMD64_chkbx.IsChecked = false;
                    ARM64_chkbx.IsChecked = false;
                    if (x86CheckBox != null) x86CheckBox.IsChecked = false;

                    // Clear the ListBox
                    AvailableListBox.ItemsSource = null;
                    return; // Exit the method
                }

                string selectedVersion = OperatingSystemComboBox.SelectedItem.ToString();

                if (selectedVersion == "Windows Server")
                {
                    EditionCombo.ItemsSource = serverEdition.Values.ToList(); // Use Server Editions
                }
                else
                {
                    EditionCombo.ItemsSource = edition.Values.ToList(); // Use Default Editions
                }

                Dispatcher.Invoke(() =>
                {
                    if (BuildsByVersion.TryGetValue(selectedVersion, out var filteredBuilds))
                    {
                        var finalFilteredBuilds = new List<BuildData>();

                        foreach (var build in filteredBuilds)
                        {
                            bool addBuild = false;

                            if (AMD64_chkbx.IsChecked == true && build.Architecture == "amd64")
                            {
                                addBuild = true;
                            }
                            else if (ARM64_chkbx.IsChecked == true && build.Architecture == "arm64")
                            {
                                addBuild = true;
                            }
                            else if (x86CheckBox?.IsChecked == true && build.Architecture == "x86")
                            {
                                addBuild = true;
                            }
                            else if (AMD64_chkbx.IsChecked != true && ARM64_chkbx.IsChecked != true && (x86CheckBox?.IsChecked != true))
                            {
                                addBuild = true; // Add all builds if no checkbox is selected
                            }

                            // Check if both AMD64 and ARM64 are selected, but only when x86 checkbox is NOT visible
                            if (AMD64_chkbx.IsChecked == true && ARM64_chkbx.IsChecked == true && (x86CheckBox == null || !x86CheckBox.IsVisible))
                            {
                                var result = MessageBox.Show("You cannot select both AMD64 and ARM64 at the same time. Showing all builds instead.",
                                                             "Invalid Selection", MessageBoxButton.OK, MessageBoxImage.Warning);

                                if (result == MessageBoxResult.OK)
                                {
                                    AMD64_chkbx.IsChecked = false;
                                    ARM64_chkbx.IsChecked = false;
                                    if (x86CheckBox != null) x86CheckBox.IsChecked = false;

                                    AvailableListBox.ItemsSource = filteredBuilds; // Show all builds
                                }
                                return;
                            }

                            // Check if AMD64, ARM64, and x86 are all selected
                            if (AMD64_chkbx.IsChecked == true && ARM64_chkbx.IsChecked == true && x86CheckBox?.IsChecked == true)
                            {
                                var result = MessageBox.Show("You cannot select AMD64, ARM64, and x86 at the same time. Showing all builds instead.",
                                                             "Invalid Selection", MessageBoxButton.OK, MessageBoxImage.Warning);

                                if (result == MessageBoxResult.OK)
                                {
                                    AMD64_chkbx.IsChecked = false;
                                    ARM64_chkbx.IsChecked = false;
                                    if (x86CheckBox != null) x86CheckBox.IsChecked = false;

                                    AvailableListBox.ItemsSource = filteredBuilds; // Show all builds
                                }
                                return;
                            }

                            if (addBuild)
                            {
                                finalFilteredBuilds.Add(build);
                            }
                        }

                        AvailableListBox.ItemsSource = finalFilteredBuilds;
                    }
                    else
                    {
                        AvailableListBox.ItemsSource = null;
                    }
                });
            }
            finally
            {
                isFiltering = false; // Reset the flag after execution
            }
        }

        private void FilterCheckbox_Changed(object sender, RoutedEventArgs e)
        {
            OperatingSystemComboBox_SelectionChanged(null, null); // Reapply filtering logic
        }
        // Define the BuildData class
        public class BuildData
        {
            public string? Title { get; set; }
            public string? BuildNumber { get; set; }
            public string? Architecture { get; set; }

            public string? Uuid { get; set; }
            public string? Lang { get; set; }
            public string? Edition { get; set; }

            public override string ToString()
            {

                return $"{Title} - {BuildNumber} ({Architecture})";
            }
        }

        private async void GetBuilds_Click(object sender, RoutedEventArgs e)
        {
            await GetBuildVersions();
        }

        
        private async void Debug_Click(object sender, RoutedEventArgs e)
        {
            // Get selected language and edition
            string? selectedLanguageDisplay = "Not Selected";
            if (LangCombo.SelectedItem != null)
            {
                selectedLanguageDisplay = LangCombo.SelectedItem as string;
            }

            string? selectedEditionDisplay = "Not Selected";
            if (EditionCombo.SelectedItem != null)
            {
                selectedEditionDisplay = EditionCombo.SelectedItem as string;
            }

            // Determine the download type
            string downloadType = "Browser / File";
            if (CopyToClipboardCheckbox.IsChecked == true)
            {
                downloadType = "URL to Clipboard";
            }

            // Get the selected build
            if (AvailableListBox.SelectedItem is BuildData selectedBuild)
            {
                // Display build details including the download type
                string buildInfo = $"Build Information:\n" +
                                   $"- Name: {selectedBuild.Title}\n" +
                                   $"- Architecture: {selectedBuild.Architecture}\n" +
                                   $"- Edition: {selectedEditionDisplay}\n" +
                                   $"- Language: {selectedLanguageDisplay}\n" +
                                   $"- Download Type: {downloadType}";

                MessageBox.Show(buildInfo, "Build Details", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("No build is selected. Please select a build from the list.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
      
        private async void Downloadbtn_Click(object sender, RoutedEventArgs e)
        {
            // Call the function to handle all operations
            HandleDownload(LangCombo, EditionCombo, AvailableListBox, CopyToClipboardCheckbox, OperatingSystemComboBox);
        }

        private void HandleDownload(ComboBox languageCombo, ComboBox editionCombo, ListBox buildListBox, CheckBox clipboardCheckbox, ComboBox operatingSystemComboBox)
        {
            // Validate selections
            var selectedLanguageDisplay = languageCombo.SelectedItem as string;
            var selectedEditionDisplay = editionCombo.SelectedItem as string;
            var selectedOperatingSystem = operatingSystemComboBox.SelectedItem as string;

            if (string.IsNullOrEmpty(selectedLanguageDisplay) || string.IsNullOrEmpty(selectedEditionDisplay) || string.IsNullOrEmpty(selectedOperatingSystem))
            {
                MessageBox.Show("Please select a language, an edition, and an operating system before proceeding.",
                                "Selection Required",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                return; // Exit if validation fails
            }

            // Retrieve the correct dictionary based on the operating system
            Dictionary<string, string> selectedEditionDictionary = selectedOperatingSystem == "Windows Server" ? serverEdition : edition;

            // Retrieve codes based on the selected display names
            string selectedLanguageCode = languages.FirstOrDefault(kvp => kvp.Value == selectedLanguageDisplay).Key;
            string selectedEditionCode = selectedEditionDictionary.FirstOrDefault(kvp => kvp.Value == selectedEditionDisplay).Key;

            if (string.IsNullOrEmpty(selectedLanguageCode) || string.IsNullOrEmpty(selectedEditionCode))
            {
                MessageBox.Show("Invalid language or edition selection. Please select valid options.",
                                "Invalid Selection",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                return; // Exit if codes are invalid
            }

            if (buildListBox.SelectedItem is BuildData selectedBuild)
            {
                if (string.IsNullOrEmpty(selectedBuild.Uuid))
                {
                    MessageBox.Show("The selected build does not have a UUID.", "Invalid Build", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Construct the URL
                string baseUrl = "https://uupdump.net/download.php";
                string url = $"{baseUrl}?id={selectedBuild.Uuid}&pack={selectedLanguageCode}&edition={selectedEditionCode}";

                if (clipboardCheckbox.IsChecked == true)
                {
                    Clipboard.SetText(url);
                    MessageBox.Show($"Constructed URL copied to clipboard:\n{url}", "URL Copied", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    // Prompt the user for actions
                    var result = MessageBox.Show(
                        "Would you like to open the download link in your browser?",
                        "Open or Save URL",
                        MessageBoxButton.YesNoCancel,
                        MessageBoxImage.Question
                    );

                    if (result == MessageBoxResult.Yes)
                    {
                        // Open the URL in the default browser
                        try
                        {
                            Process.Start(new ProcessStartInfo
                            {
                                FileName = url,
                                UseShellExecute = true
                            });
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Failed to open the URL: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    else if (result == MessageBoxResult.No)
                    {
                        // Save the URL to a file
                        var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                        {
                            Title = "Save Download Link",
                            Filter = "Text File (*.txt)|*.txt",
                            FileName = "DownloadLink.txt"
                        };

                        if (saveFileDialog.ShowDialog() == true)
                        {
                            try
                            {
                                System.IO.File.WriteAllText(saveFileDialog.FileName, url);
                                MessageBox.Show("URL saved successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Failed to save the file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a build from the list.", "No Build Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

    }
}
