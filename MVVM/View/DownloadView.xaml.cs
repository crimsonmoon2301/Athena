using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace Galadarbs_IT23033.MVVM.View
{
    /// <summary>
    /// Interaction logic for DownloadView.xaml
    /// </summary>
    public partial class DownloadView : UserControl
    {
        public DownloadView()
        {
            InitializeComponent();
        }

        private async void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            if (AvailableListBox.SelectedItem == null)
            {
                MessageBox.Show("Please select a build from the list.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string selectedBuild = AvailableListBox.SelectedItem.ToString();
            string downloadUrl = $"https://msdl.gravesoft.dev/"; // Adjust URL structure as needed

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                FileName = $"{selectedBuild}.iso",
                Filter = "ISO Files (*.iso)|*.iso|All Files (*.*)|*.*"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    await DownloadFileAsync(downloadUrl, saveFileDialog.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Download failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async Task DownloadFileAsync(string url, string destinationPath)
        {
            using (HttpClient client = new HttpClient())
            {
                var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();

                using (var fs = new FileStream(destinationPath, FileMode.Create))
                {
                    await response.Content.CopyToAsync(fs);
                }
            }

            MessageBox.Show("Download completed successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
