using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Galadarbs_IT23033
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Random random = new Random();

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainView_Loaded;
        }

        private async void MainView_Loaded(object sender, RoutedEventArgs e)
        {
            await InitializeAsync(); // Call the async method to initialize
        }

        public async Task InitializeAsync()
        {
            // Wait for 1 second (1000 milliseconds)
            await Task.Delay(500);

            // Show the welcome dialog after the delay
            ShowWelcomeDialog();
        }
        private bool ShowWelcomeDialog()
        {
            MessageBoxResult warningMessage = MessageBox.Show(
                 "This tweaker can both improve or damage certain functionality depending on user preference. Please, be sure that you know what you're doing." +
                 "\nAre you sure you want to proceed?",
                 "Some Valuable Info",
                 MessageBoxButton.YesNo,
                 MessageBoxImage.Warning);

            // If user chooses no, exit
            if (warningMessage == MessageBoxResult.No)
            {
                MessageBox.Show("You cannot use this program. Closing now.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
                Application.Current.Shutdown();
                return false; // Exit early if the user does not want to proceed
            }

            // Second prompt for confirmation
            if (warningMessage == MessageBoxResult.Yes)
            {
                MessageBoxResult confirmMessage = MessageBox.Show(
                "It is recommended to make a system restore point from here on out.",
                "Proceed at your own risk",
                MessageBoxButton.OK,
                MessageBoxImage.Question);
            }
            return false;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove(); // This enables window dragging.
            }
        }

        // Exit button, aka our poweroff icon
        // finally... I was getting tired of alt+f4 all the time.
        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(
                "Are you sure you want to exit?",
                "Confirmation",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }

        private void ChangeWall_btn_Click(object sender, RoutedEventArgs e)
        {
            Storyboard fadeOutStoryboard = (Storyboard)FindResource("FadeOutStoryboard");

            // Register the event handler for Completed
            fadeOutStoryboard.Completed += Wallpaper_cycle;

            fadeOutStoryboard.Begin();
        }
        private void Wallpaper_cycle(object sender, EventArgs e)
        {
            // Select a random wallpaper
            int randomIndex = random.Next(1, 5); 
            string resourceKey = $"Wallpaper{randomIndex}";

            // Change the background image after fading out
            ImageBrush backgroundImageBrush = (ImageBrush)FindResource(resourceKey);
            MainGrid.Background = backgroundImageBrush; // Set the new background

            // Start fade-in animation
            Storyboard fadeInStoryboard = (Storyboard)FindResource("FadeInStoryboard");
            fadeInStoryboard.Begin();
        }
    }
}