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
        public async Task<string?> FetchUUPData(string url) // Checks if the API responds. If it does, it returns a string. If not, it returns null.
        {
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }
                else
                {
                    return null;
                }
            }
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
                            MessageBox.Show("API version not found in response. The API may be possibly down.");
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

        private void ShowAdvancedOptionsCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            AdvancedOptionsPanel.Visibility = Visibility.Visible;
        }

        private void ShowAdvancedOptionsCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            AdvancedOptionsPanel.Visibility = Visibility.Collapsed;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void TestAPI_Click(object sender, RoutedEventArgs e)
        {
            await TestAPI();
        }

        // this thing is a absolute fucking nightmare istg
        public List<BuildData> Builds { get; set; }
        public Dictionary<string, List<BuildData>> BuildsByVersion { get; set; }

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

                        string mainVersion;
                        if (title.Contains("Windows 10"))
                        {
                            mainVersion = "Windows 10";
                        }
                        else if (title.Contains("Windows 11"))
                        {
                            mainVersion = "Windows 11";
                        }
                        else if (title.Contains("Windows Server"))
                        {
                            mainVersion = "Windows Server";
                        }
                        else
                        {
                            mainVersion = "Other";
                        }

                        var buildData = new BuildData { Title = title, BuildNumber = buildNumber, Architecture = architecture };
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

        private void OperatingSystemComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BuildsByVersion == null || OperatingSystemComboBox.SelectedItem == null)
            {
                return; // Exit if no builds or no item is selected
            }

            string? selectedVersion = OperatingSystemComboBox.SelectedItem.ToString();

            // Filter ListBox based on selected version. Dispatcher basically makes sure that the UI updates as it should, without throwing an exception. Win win!
            Dispatcher.Invoke(() =>
            {
                if (BuildsByVersion.TryGetValue(selectedVersion, out var filteredBuilds))
                {
                    AvailableListBox.ItemsSource = filteredBuilds; // Show builds for selected version
                }
                else
                {
                    AvailableListBox.ItemsSource = null; // Clear ListBox if no builds found
                }
            });
        }

        // Define the BuildData class
        public class BuildData
        {
            public string? Title { get; set; }
            public string? BuildNumber { get; set; }
            public string? Architecture { get; set; }

            public override string ToString()
            {
                return $"{Title} - {BuildNumber} ({Architecture})";
            }
        }

        private async void GetBuilds_Click(object sender, RoutedEventArgs e)
        {
            await GetBuildVersions();
        }

        private void AMDCheckBox_Toggled(object sender, RoutedEventArgs e)
        {

        }

        private void ARMCheckBox_Toggled(object sender, RoutedEventArgs e)
        {

        }
        private void AMDCheckBox_Untoggled(object sender, RoutedEventArgs e)
        {

        }
        private void ARMCheckBox_Untoggled(object sender, RoutedEventArgs e)
        {

        }
    }
}
