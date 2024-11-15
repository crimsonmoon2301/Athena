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
        public async Task FetchBuildsAsync()
        {
            string url = "https://api.uupdump.net/";
            var response = await client.GetStringAsync(url);
           /// var buildData = JsonConvert.DeserializeObject<BuildResponse>(response);

           
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
    }
}
