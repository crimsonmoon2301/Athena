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
        public async Task<string> FetchUUPData(string url) // Checks if the API responds. If it does, it returns a string. If not, it returns null.
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
                    foreach (var build in builds.EnumerateArray())
                    {
                        string title = build.GetProperty("title").GetString();
                        string buildNumber = build.GetProperty("build").GetString();
                        string architecture = build.GetProperty("arch").GetString();

                        // Add to UI
                        Dispatcher.Invoke(() =>
                        {
                            AvailableListBox.Items.Add($"{title} - {buildNumber} ({architecture})");
                        });
                    }

                    MessageBox.Show("Builds loaded successfully.");
                }
                else
                {
                    MessageBox.Show("No builds found in the response.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error fetching builds: " + ex.Message);
            }
        }

        private async void GetBuilds_Click(object sender, RoutedEventArgs e)
        {
            await GetBuildVersions();
        }
    }
}
