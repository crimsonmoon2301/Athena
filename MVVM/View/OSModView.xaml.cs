using System.Diagnostics; // For finer managment in metrics
using System.IO; // For reading the storage
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading; // For the timer



namespace Galadarbs_IT23033.MVVM.View
{
    /// <summary>
    /// Interaction logic for OSModView.xaml
    /// </summary>
    public partial class OSModView : UserControl
    {
        private DispatcherTimer timer;
        private PerformanceCounter cpuCounter;
        private PerformanceCounter ramCounter;
        private bool dialogShown = false; // Add a check so that the dialog doesn't pop up every second after being clicked.
        private object RegViewCommand;

        public OSModView()
        {
            InitializeComponent();
            cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            cpuCounter.NextValue(); // Initialize the counter

            ramCounter = new PerformanceCounter("Memory", "Available MBytes"); // Initialize RAM counter

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1); // Wait one second before opening the dialog box so it doesn't show 0.00
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            string cpuUsage = cpuCounter.NextValue().ToString("0.00");
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Get and display system information when the button is clicked
            string systemInfo = GetSystemInfo();
            MessageBox.Show(systemInfo, "System Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private string GetSystemInfo()
        {
            // Get basic environment information
            string osName = GetWindowsVersion();
            string machineName = Environment.MachineName;
            string userName = Environment.UserName;

            // Retrieve the current CPU usage
            float cpuUsage = cpuCounter.NextValue(); // Get the current CPU usage
            float availableRam = ramCounter.NextValue(); // Get available RAM in MB
                                                         // float freeStorageMB = GetFreeStorage();
            float totalFreeSpaceGB = GetTotalFreeSpace();


            return $"OS Name: {osName}\n" +
                   $"Machine Name: {machineName}\n" +
                   $"User Name: {userName}\n" +
                   $"CPU Usage: {cpuUsage.ToString("0.00")}%\n" +
                   $"Available RAM: {availableRam.ToString("0")} MB\n" +
                   $"Total Free Space: {totalFreeSpaceGB.ToString("0.0")} GB";
        }

        private static float GetTotalFreeSpace()
        {
            float totalFreeSpaceGB = 0;
            DriveInfo[] drives = DriveInfo.GetDrives(); // Defining a array for storing the values in for the foreach loop.

            foreach (DriveInfo drive in drives)
            {
                if (drive.IsReady) // Checks if the drive is formatted, functioning and ready for use
                {
                    totalFreeSpaceGB += drive.TotalFreeSpace / (1024 * 1024 * 1024); // Add free space in GB
                }
            }

            return totalFreeSpaceGB;                                   // Due to the way that explorer calculates the storage,
                                                                       // there's like 300-400 MB error. This uses directly from bytes converted to GB.
                                                                       // But since we also count in the factor that there may be system reserved partitions,
                                                                       // aswell as files that are not included in the calculations, aswell as limitations of this function
                                                                       // It's possible that this is the "absolute error" and we won't get any more accurate readings than this.

        }

        private string GetWindowsVersion() // A function that checks the host's version number using interopservices library.
        {
            var osVersion = Environment.OSVersion.Version;
            string versionName;

            switch (osVersion.Major)
            {
                case 10:
                    versionName = osVersion.Build switch
                    {
                        26100 => "Windows 11 24H2",
                        25398 => "Windows Server (Based on 11)",
                        22631 => "Windows 11 23H2",
                        22621 => "Windows 11 22H2",
                        22000 => "Windows 11",
                        19041 => "Windows 10 Build 2004",
                        19042 => "Windows 10 20H2",
                        19043 => "Windows 10 21H1",
                        19044 => "Windows 10 21H2",
                        19045 => "Windows 10 22H2",
                        20348 => "Windows Server 2022",
                        _ => "Unable to determine version"
                    };
                    break;

                default:
                    versionName = "Not running Windows";
                    break;
            }
            return versionName;
        }

        private void WinDeb_btn_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}

