using Galadarbs_IT23033.MVVM.ViewModel;
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

        private void DataGrid_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            // Get the clicked DataGrid row
            if (sender is DataGrid dataGrid && dataGrid.SelectedItem is DownloadViewModel.DownloadItem clickedRow)
            {
                // Check if the Info property exists and display it
                string infoMessage;
                if (clickedRow.Info == null)
                {
                    infoMessage = "No information available.";
                }
                else
                {
                    infoMessage = clickedRow.Info;
                }
                MessageBox.Show(infoMessage, "Product Info", MessageBoxButton.OK, MessageBoxImage.Information);

                // Check if the Mirror1 property exists and display it
                string mirrorMessage;
                if (clickedRow.Mirror1 == null)
                {
                    mirrorMessage = "No Download link available.";
                }
                else
                {
                    mirrorMessage = clickedRow.Mirror1;
                }
                MessageBox.Show(mirrorMessage, "Download Link (Online)", MessageBoxButton.OK, MessageBoxImage.Information);

                // Check if the Mirror2 property exists and display it
                string mirrorMessage1;
                if (clickedRow.Mirror2 == null)
                {
                    mirrorMessage1 = "No Download link available.";
                }
                else
                {
                    mirrorMessage1 = clickedRow.Mirror2;
                }
                MessageBox.Show(mirrorMessage1, "Download Link (Offline)", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                // Optional: Message if no row was clicked
                MessageBox.Show("Please select a valid item to view information.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
