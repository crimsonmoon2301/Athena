using Galadarbs_IT23033.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Net.Http.Headers;



namespace Galadarbs_IT23033.MVVM.ViewModel
{
    internal class DownloadViewModel : ObservableObject
    {
        // Establish a list where all the parsed objects will reside in. Obseravble updates the list and the UI.
        public ObservableCollection<DownloadItem> Downloads { get; set; } = new ObservableCollection<DownloadItem>();

        // Default path.
        public string SelectedDownloadPath { get; set; } = "";

        // For making commands on download buttons. Establishing properties.
        public ICommand SelectDownloadPathCommand { get; set; }
        public ICommand DownloadSelectedCommand { get; set; }

        // A class for XML objects that will be parsed into C#
        public class DownloadItem : ObservableObject
        {
            public string? Officename { get; set; }
            public string? Edition { get; set; }
            public string? Info { get; set; }
            public string? Mirror1 { get; set; }
            public string? Mirror2 { get; set; }
            private double progress;
            public double Progress
            {
                get => progress;
                set
                {
                    if (progress != value)
                    {
                        progress = value;
                        OnPropertyChanged(nameof(Progress)); // Notify the UI about the property change
                    }
                }
            }
        }

        // for the progress bar

        private DownloadItem _selectedDownload;
        public DownloadItem SelectedDownload
        {
            get => _selectedDownload;
            set
            {
                _selectedDownload = value;
                OnPropertyChanged();
            }
        }


        // Load Data the moment the view is opened.
        public DownloadViewModel()
        {
            SelectDownloadPathCommand = new RelayCommand(param => SelectDownloadPath());
            DownloadSelectedCommand = new RelayCommand(param => DownloadFile(param?.ToString()));
            _ = LoadData();

            // Reset the progress bar if it was 100

            foreach (var item in Downloads)
            {
                if (item.Progress == 100)
                {
                    item.Progress = 0;
                }
            }
            OnPropertyChanged(nameof(Downloads));
        }

        private async Task LoadData()
        {
            string url = "https://raw.githubusercontent.com/crimsonmoon2301/AthenaEXT/main/downloadurl.xml";

            try
            {
                using (var client = new HttpClient())
                {
                    // Disables caching. The XML sometimes refuses to load due caching.
                    client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue
                    {
                        NoCache = true,
                        NoStore = true
                    };

                    string xmlcontent = await client.GetStringAsync(url);
                    ParseXml (xmlcontent);
                }
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show("Couldn't parse the XML. Structure was most likely changed. Fix TBA");
            }
        }

        // Parse and add the objects in.
        private async void ParseXml(string xmlcontent)
        {
            XDocument doc = XDocument.Parse(xmlcontent);

            var products = doc.Descendants("product")
            .Select(p => new DownloadItem
            {
                Officename = p.Element("officename")?.Value,
                Edition = p.Element("edition")?.Value,
                Info = p.Element("info")?.Value,
                Mirror1 = p.Element("mirrors")?.Element("downloadmirror")?.Value,
                Mirror2 = p.Element("mirrors")?.Element("downloadmirror1")?.Value,
                Progress = 0 // Initialize with a default value
            });

            foreach (var product in products)
            {
                Downloads.Add(product);
            }
        }
        private void SelectDownloadPath()
        {
            var dialog = new Microsoft.Win32.OpenFolderDialog
            {
                Title = "Select Download Path"
            };

            if (dialog.ShowDialog() == true) // ShowDialog() returns a nullable bool
            {
                SelectedDownloadPath = dialog.FolderName;
                OnPropertyChanged(nameof(SelectedDownloadPath));
            }
        }
        private async void DownloadFile(string mirror)
        {
            if (string.IsNullOrWhiteSpace(SelectedDownloadPath))
            {
                MessageBox.Show("Please select a valid download path.");
                return;
            }

            if (SelectedDownload == null)
            {
                MessageBox.Show("Please select an item to download.");
                return;
            }

            var itemToDownload = SelectedDownload; // Store the current item being downloaded

            string? url = mirror == "Mirror1" ? itemToDownload.Mirror1 : itemToDownload.Mirror2;

            if (url == null || url.Trim() == string.Empty || url == "N/A")
            {
                string mirrorType;
                if (mirror == "Mirror1")
                {
                    mirrorType = "online installer";
                }
                else
                {
                    mirrorType = "offline installer";
                }

                MessageBox.Show($"This build does not have a {mirrorType} available.", "Mirror Unavailable", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var client = new HttpClient())
                {
                    var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                    response.EnsureSuccessStatusCode();

                    // Extract the default filename from the URL
                    string fileName = Path.GetFileName(new Uri(url).LocalPath);
                    // Attempt to extract the filename from the Content-Disposition header
                    if (response.Content.Headers.ContentDisposition != null && !string.IsNullOrWhiteSpace(response.Content.Headers.ContentDisposition.FileName))
                    {
                        fileName = response.Content.Headers.ContentDisposition.FileName.Trim('"'); // Remove quotes if present
                    }
                    else
                    {
                        // Fall back to extracting the filename from the URL
                        fileName = Path.GetFileName(new Uri(url).LocalPath);
                    }
                    // Ensure the filename has the correct extension
                    if (mirror == "Mirror1" && !fileName.EndsWith(".exe"))
                    {
                        fileName = Path.ChangeExtension(fileName, ".exe");
                    }
                    else if (mirror == "Mirror2" && !fileName.EndsWith(".iso"))
                    {
                        fileName = Path.ChangeExtension(fileName, ".iso");
                    }

                    string downloadPath = Path.Combine(SelectedDownloadPath, fileName);

                    using (var stream = await response.Content.ReadAsStreamAsync())
                    using (var fileStream = new FileStream(downloadPath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        var buffer = new byte[81920];
                        int bytesRead;
                        long totalBytesRead = 0;
                        long totalBytes = response.Content.Headers.ContentLength ?? -1;

                        while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            await fileStream.WriteAsync(buffer, 0, bytesRead);
                            totalBytesRead += bytesRead;

                            if (totalBytes > 0)
                            {
                                itemToDownload.Progress = (totalBytesRead / (double)totalBytes) * 100;
                                OnPropertyChanged(nameof(Downloads)); // Notify UI
                            }
                        }
                    }
                    MessageBox.Show($"Download completed: {downloadPath}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Download failed: {ex.Message}");
            }
        }
    }
}