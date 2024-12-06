using Galadarbs_IT23033.Core;

namespace Galadarbs_IT23033.MVVM.ViewModel
{
    class MainViewModel : ObservableObject
    {
        // main screen
        public RelayCommand HomeViewCommand { get; set; }

        // Config window
        public RelayCommand MaintainViewCommand { get; set; }

        // registry view properties
        public RelayCommand RegViewCommand { get; set; }

        // os mod view properties
        public RelayCommand OSModCommand { get; set; }

        // ISO builder view properties
        public RelayCommand ISOBuildViewCommand { get; set; }

        // About this program view property
        public RelayCommand AboutViewCommand { get; set; }

        // Computer Diagnostic property
        public RelayCommand CompDiagnosticViewCommand { get; set; }

        // System Tuner property
        public RelayCommand OSTunerViewCommand { get; set; }

        // Downloader property
        public RelayCommand DownloaderViewCommand {  get; set; }

        // defining the views to their appropiate view models, get set helps assign properties
        public HomeViewModel HomeVm { get; set; }
        public MaintainViewModel MaintainVm { get; set; }

        public RegViewModel RegVm { get; set; }

        public OSModViewModel OSModVm { get; set; }

        public ISOBuilderViewModel ISOBuildVm { get; set; }

        public DownloadViewModel DownloadVm { get; set; }

        public AboutViewModel AboutVm { get; set; }

        public CompDiagnosticViewModel  CompDiagVm {  get; set; }

        public OSTunerViewModel OSTunerVm {  get; set; }

        private object _currentView;

        public object CurrentView // uses construction function to assign the values 
        {
            get { return _currentView; }
            set
            {
                _currentView = value;
                OnPropertyChanged(nameof(CurrentView));
            }
        }

        public MainViewModel() // Making new view model objects and they get "assigned" to the currentview function
        {
            HomeVm = new HomeViewModel();
            MaintainVm = new MaintainViewModel();
            RegVm = new RegViewModel();
            OSModVm = new OSModViewModel();
            ISOBuildVm = new ISOBuilderViewModel();
            AboutVm = new AboutViewModel();
            CompDiagVm = new CompDiagnosticViewModel();
            OSTunerVm = new OSTunerViewModel();
            DownloadVm = new DownloadViewModel();
            CurrentView = HomeVm;

            HomeViewCommand = new RelayCommand(o =>
            {
                CurrentView = HomeVm;
            });

            MaintainViewCommand = new RelayCommand(o =>
            {
                CurrentView = MaintainVm;
            });

            RegViewCommand = new RelayCommand(o =>
            {
                CurrentView = RegVm;
            });

            OSModCommand = new RelayCommand(o =>
            {
                CurrentView = OSModVm;
            });

            ISOBuildViewCommand = new RelayCommand(o =>
            {
                CurrentView = ISOBuildVm;
            });

            AboutViewCommand = new RelayCommand(o =>
            {
                CurrentView = AboutVm;
            });

            CompDiagnosticViewCommand = new RelayCommand(o =>
            {
                CurrentView = CompDiagVm;
            });

            OSTunerViewCommand = new RelayCommand(o =>
            {
                CurrentView = OSTunerVm;
            });

            DownloaderViewCommand = new RelayCommand(o =>
            {
                CurrentView = DownloadVm;
            });
        }
    }
}
