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
        public string SelectedDownloadPath { get; set; } = "C:\\Downloads";

        // For making commands on download buttons. Establishing properties.
        public ICommand SelectDownloadPathCommand { get; set; }
        public ICommand DownloadSelectedCommand { get; set; }

        // A class for XML objects that will be parsed into C#
        public class DownloadItem
        {
            public string? Officename { get; set; }
            public string? Edition { get; set; }
            public string? Info { get; set; }
            public string? Mirror1 { get; set; }
            public string? Mirror2 { get; set; }
            public int Progress { get; set; } // Progress property for the DataGrid column
        }

        // Load Data the moment the view is opened.
        public DownloadViewModel()
        {
            _ = LoadData();
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
    }
}
