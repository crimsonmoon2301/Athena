using System.Windows;
using System.Windows.Controls;

namespace Galadarbs_IT23033.MVVM.View
{
    /// <summary>
    /// Interaction logic for MaintainView.xaml
    /// </summary>
    public partial class MaintainView : UserControl
    {
        private static bool IsDisclaimerShown = false;

        public MaintainView()
        {
            InitializeComponent();
            Loaded += MaintainView_Loaded;
        }

        private void MaintainView_Loaded(object sender, RoutedEventArgs e)
        {
            ShowDisclaimerIfNeeded();
        }

        private void ShowDisclaimerIfNeeded()
        {
            if (!IsDisclaimerShown)
            {
                MessageBoxResult warningMessage = MessageBox.Show(
                    "The tools in this view can disable critical system services or uninstall apps, potentially causing system damage. Proceed with caution!",
                    "Disclaimer",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                IsDisclaimerShown = true; // Mark disclaimer as shown
            }
        }
    }
}
