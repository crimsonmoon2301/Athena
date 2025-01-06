using System.IO;
using System.Windows;
using System.Windows.Controls;
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
        private DispatcherTimer idleTimer;
        private readonly TimeSpan idleDuration = TimeSpan.FromMinutes(5); // Idle time before action
        private DispatcherTimer countdownTimer;
        private int countdownTime = 30;

        private int _currentWallpaperIndex = 1; // Start with the first wallpaper
        private readonly int _wallpaperCount = 5; // Total number of wallpapers

        public MainWindow()
        {
            InitializeComponent();
            LoadThemes();
            InitializeIdleTimer();
            AttachActivityHandlers();
            Loaded += MainView_Loaded;
            SetInitialWallpaper();
        }

        private async void MainView_Loaded(object sender, RoutedEventArgs e)
        {
            await Initialize();
        }

        public async Task Initialize()
        {
            await Task.Delay(500); // Wait for half a second
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

            if (warningMessage == MessageBoxResult.No)
            {
                MessageBox.Show("You cannot use this program. Closing now.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
                Application.Current.Shutdown();
                return false;
            }

            if (warningMessage == MessageBoxResult.Yes)
            {
                MessageBox.Show(
                    "It is recommended to make a system restore point from here on out.",
                    "Proceed at your own risk",
                    MessageBoxButton.OK,
                    MessageBoxImage.Question);
            }
            return false;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ResetIdleTimer();
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

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

        private bool _isFirstLoad = true; // Tracks if it's the first load

        private void ThemeSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ThemeSelector.SelectedItem is ComboBoxItem selectedItem)
            {
                string selectedTheme = selectedItem.Content.ToString();
                ResourceDictionary themeDictionary = new ResourceDictionary();

                switch (selectedTheme)
                {
                    case "Abstract":
                        themeDictionary.Source = new Uri("Theme/AbstractTheme.xaml", UriKind.Relative);
                        break;
                    case "Phantom":
                        themeDictionary.Source = new Uri("Theme/PhantomTheme.xaml", UriKind.Relative);
                        break;
                    case "Cyber":
                        themeDictionary.Source = new Uri("Theme/CyberTheme.xaml", UriKind.Relative);
                        break;
                    case "Binary":
                        themeDictionary.Source = new Uri("Theme/BinaryTheme.xaml", UriKind.Relative);
                        break;
                    default:
                        MessageBox.Show("Theme not found!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                }

                // Reset wallpaper index to 1 when theme changes
                _currentWallpaperIndex = 1;

                if (_isFirstLoad)
                {
                    // If it's the first load, just apply the theme and wallpaper without fade animations
                    _isFirstLoad = false;

                    Application.Current.Resources.MergedDictionaries.Clear();
                    Application.Current.Resources.MergedDictionaries.Add(themeDictionary);
                    SetInitialWallpaper();
                }
                else
                {
                    // Use fade-out and fade-in animations for theme changes after the first load
                    Storyboard fadeOutStoryboard = (Storyboard)FindResource("FadeOutStoryboard");
                    fadeOutStoryboard.Completed += (s, args) =>
                    {
                        // Apply the new theme
                        Application.Current.Resources.MergedDictionaries.Clear();
                        Application.Current.Resources.MergedDictionaries.Add(themeDictionary);

                        // Set the initial wallpaper for the new theme
                        SetInitialWallpaper();

                        // Start fade-in animation
                        Storyboard fadeInStoryboard = (Storyboard)FindResource("FadeInStoryboard");
                        fadeInStoryboard.Begin();
                    };

                    fadeOutStoryboard.Begin();
                }
            }
        }

        private void LoadThemes()
        {
            var themes = new[] { "Abstract", "Phantom", "Cyber", "Binary" };

            foreach (var theme in themes)
            {
                ComboBoxItem item = new ComboBoxItem { Content = theme };
                ThemeSelector.Items.Add(item);
            }

            ThemeSelector.SelectedIndex = 0;
        }

        private void SetInitialWallpaper()
        {
            string resourceKey = $"Wallpaper{_currentWallpaperIndex}";

            if (Application.Current.Resources.Contains(resourceKey))
            {
                ImageBrush backgroundImageBrush = (ImageBrush)Application.Current.Resources[resourceKey];
                MainGrid.Background = backgroundImageBrush;
            }
            else
            {
                MessageBox.Show($"Initial wallpaper resource '{resourceKey}' not found!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ChangeWall_btn_Click(object sender, RoutedEventArgs e)
        {
            // Increment wallpaper index
            _currentWallpaperIndex = _currentWallpaperIndex % _wallpaperCount + 1; // Loop back to 1 after reaching _wallpaperCount
            string resourceKey = $"Wallpaper{_currentWallpaperIndex}";

            if (Application.Current.Resources.Contains(resourceKey))
            {
                // Start fade-out animation
                Storyboard fadeOutStoryboard = (Storyboard)FindResource("FadeOutStoryboard");
                fadeOutStoryboard.Completed += (s, args) =>
                {
                    // Change wallpaper after fade-out
                    ImageBrush backgroundImageBrush = (ImageBrush)Application.Current.Resources[resourceKey];
                    MainGrid.Background = backgroundImageBrush;

                    // Start fade-in animation
                    Storyboard fadeInStoryboard = (Storyboard)FindResource("FadeInStoryboard");
                    fadeInStoryboard.Begin();
                };

                fadeOutStoryboard.Begin();
            }
            else
            {
                MessageBox.Show($"Wallpaper resource '{resourceKey}' not found!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void Wallpaper_cycle(object sender, EventArgs e)
        {
            // Cycle the wallpaper index (loop back to 1 after reaching the maximum count)
            object currentIndexObject = Application.Current.Resources["CurrentWallpaperIndex"];
            int currentIndex = currentIndexObject != null ? (int)currentIndexObject : 0; // Default to 0 if not set
            int nextIndex = (currentIndex % _wallpaperCount) + 1;

            // Update the current index in resources
            Application.Current.Resources["CurrentWallpaperIndex"] = nextIndex;

            string resourceKey = $"Wallpaper{nextIndex}";

            if (Application.Current.Resources.Contains(resourceKey))
            {
                ImageBrush backgroundImageBrush = (ImageBrush)Application.Current.Resources[resourceKey];
                MainGrid.Background = backgroundImageBrush;
            }
            else
            {
                MessageBox.Show($"Wallpaper resource '{resourceKey}' not found!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            // Start fade-in animation
            Storyboard fadeInStoryboard = (Storyboard)FindResource("FadeInStoryboard");
            fadeInStoryboard.Begin();
        }
        private void InitializeIdleTimer()
        {
            idleTimer = new DispatcherTimer { Interval = idleDuration };
            idleTimer.Tick += OnIdleTimeout;
            idleTimer.Start();
        }

        private void OnIdleTimeout(object sender, EventArgs e)
        {
            idleTimer.Stop();

            countdownTime = 30;
            countdownTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            countdownTimer.Tick += CountdownTimer_Tick;
            countdownTimer.Start();

            MessageBoxResult result = MessageBox.Show(
                $"You've been idle for a while. Would you like the program to close?\n\nThe program will auto-close in {countdownTime} seconds if no response.",
                "Are you still there?",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.No)
            {
                countdownTimer.Stop();
                Application.Current.Shutdown();
            }
            else
            {
                countdownTimer.Stop();
                idleTimer.Start();
            }
        }

        private void CountdownTimer_Tick(object sender, EventArgs e)
        {
            countdownTime--;

            if (countdownTime <= 0)
            {
                countdownTimer.Stop();
                Application.Current.Shutdown();
            }
        }

        private void ResetIdleTimer()
        {
            idleTimer?.Stop();
            idleTimer?.Start();
        }

        private void AttachActivityHandlers()
        {
            this.MouseMove += (s, e) => ResetIdleTimer();
            this.KeyDown += (s, e) => ResetIdleTimer();
            this.MouseDown += (s, e) => ResetIdleTimer();
        }
    }
}