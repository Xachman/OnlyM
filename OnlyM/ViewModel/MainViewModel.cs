namespace OnlyM.ViewModel
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Threading.Tasks;
    using System.Windows;
    using AutoUpdates;
    using Core.Services.Options;
    using GalaSoft.MvvmLight;
    using GalaSoft.MvvmLight.Command;
    using GalaSoft.MvvmLight.Messaging;
    using MaterialDesignThemes.Wpf;
    using PubSubMessages;
    using Services.DragAndDrop;
    using Services.HiddenMediaItems;
    using Services.MediaChanging;
    using Services.Pages;
    using Services.Snackbar;

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class MainViewModel : ViewModelBase
    {
        private readonly IPageService _pageService;
        private readonly IOptionsService _optionsService;
        private readonly ISnackbarService _snackbarService;
        private readonly IMediaStatusChangingService _mediaStatusChangingService;
        private readonly IHiddenMediaItemsService _hiddenMediaItemsService;

        public MainViewModel(
            IPageService pageService,
            IOptionsService optionsService,
            ISnackbarService snackbarService,
            IMediaStatusChangingService mediaStatusChangingService,
            IHiddenMediaItemsService hiddenMediaItemsService,
            IDragAndDropService dragAndDropService)
        {
            Messenger.Default.Register<MediaListUpdatingMessage>(this, OnMediaListUpdating);
            Messenger.Default.Register<MediaListUpdatedMessage>(this, OnMediaListUpdated);

            _mediaStatusChangingService = mediaStatusChangingService;
            _hiddenMediaItemsService = hiddenMediaItemsService;

            _hiddenMediaItemsService.HiddenItemsChangedEvent += HandleHiddenItemsChangedEvent;

            _pageService = pageService;
            _pageService.NavigationEvent += HandlePageNavigationEvent;
            _pageService.MediaMonitorChangedEvent += HandleMediaMonitorChangedEvent;
            _pageService.MediaWindowOpenedEvent += HandleMediaWindowOpenedEvent;
            _pageService.MediaWindowClosedEvent += HandleMediaWindowClosedEvent;

            _snackbarService = snackbarService;
            
            _optionsService = optionsService;
            _optionsService.AlwaysOnTopChangedEvent += HandleAlwaysOnTopChangedEvent;

            _pageService.GotoOperatorPage();

            dragAndDropService.CopyingFilesProgressEvent += HandleCopyingFilesProgressEvent;

            InitCommands();

            if (!IsInDesignMode && _optionsService.Options.PermanentBackdrop)
            {
                _pageService.OpenMediaWindow();
            }

            GetVersionData();
        }

        private void HandleCopyingFilesProgressEvent(object sender, Models.FilesCopyProgressEventArgs e)
        {
            ProgressPercentage = e.PercentageComplete;
        }

        private double _progressPercentage;

        public double ProgressPercentage
        {
            get => _progressPercentage;
            set
            {
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (_progressPercentage != value)
                {
                    _progressPercentage = value;
                    RaisePropertyChanged();
                    ShowProgressBar = value > 0.0;
                }
            }
        }

        private void HandleHiddenItemsChangedEvent(object sender, EventArgs e)
        {
            RaisePropertyChanged(nameof(IsUnhideButtonVisible));
        }

        private bool _isMediaListLoading;

        public bool IsMediaListLoading
        {
            get => _isMediaListLoading;
            set
            {
                if (_isMediaListLoading != value)
                {
                    _isMediaListLoading = value;
                    RaisePropertyChanged();
                }
            }
        }

        private void OnMediaListUpdating(MediaListUpdatingMessage message)
        {
            IsMediaListLoading = true;
        }

        private void OnMediaListUpdated(MediaListUpdatedMessage message)
        {
            IsMediaListLoading = false;
            IsMediaListEmpty = message.Count == 0;
        }

        private void HandleMediaWindowOpenedEvent(object sender, EventArgs e)
        {
            Application.Current.MainWindow?.Activate();
            RaisePropertyChanged(nameof(AlwaysOnTop));
        }

        private void HandleMediaWindowClosedEvent(object sender, EventArgs e)
        {
            RaisePropertyChanged(nameof(AlwaysOnTop));
        }

        private void HandleAlwaysOnTopChangedEvent(object sender, EventArgs e)
        {
            RaisePropertyChanged(nameof(AlwaysOnTop));
        }

        private void HandlePageNavigationEvent(object sender, NavigationEventArgs e)
        {
            _currentPageName = e.PageName;
            CurrentPage = _pageService.GetPage(e.PageName);

            RaisePropertyChanged(nameof(IsUnhideButtonVisible));
        }

        private string _currentPageName;
        private FrameworkElement _currentPage;

        public FrameworkElement CurrentPage
        {
            get => _currentPage;
            set
            {
                if (_currentPage == null || !_currentPage.Equals(value))
                {
                    _currentPage = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(IsSettingsPageActive));
                    RaisePropertyChanged(nameof(IsOperatorPageActive));
                    RaisePropertyChanged(nameof(ShowNewVersionButton));
                    RaisePropertyChanged(nameof(ShowDragAndDropHint));
                }
            }
        }

        private void HandleMediaMonitorChangedEvent(object sender, EventArgs e)
        {
            RaisePropertyChanged(nameof(AlwaysOnTop));
        }

        private void GetVersionData()
        {
            Task.Delay(2000).ContinueWith(_ =>
            {
                var latestVersion = VersionDetection.GetLatestReleaseVersion();
                if (latestVersion != null)
                {
                    if (latestVersion != VersionDetection.GetCurrentVersion())
                    {
                        // there is a new version....
                        _newVersionAvailable = true;
                        RaisePropertyChanged(nameof(ShowNewVersionButton));

                        _snackbarService.Enqueue(
                            Properties.Resources.NEW_UPDATE_AVAILABLE, 
                            Properties.Resources.VIEW, 
                            LaunchReleasePage);
                    }
                }
            });
        }

        public ISnackbarMessageQueue TheSnackbarMessageQueue => _snackbarService.TheSnackbarMessageQueue;
    
        private bool _newVersionAvailable;

        public bool ShowNewVersionButton => _newVersionAvailable && IsOperatorPageActive;

        public bool AlwaysOnTop => _optionsService.Options.AlwaysOnTop || _pageService.IsMediaWindowVisible;

        public bool IsSettingsPageActive => _currentPageName.Equals(_pageService.SettingsPageName);

        public bool IsOperatorPageActive => _currentPageName.Equals(_pageService.OperatorPageName);

        public bool ShowDragAndDropHint => IsMediaListEmpty && IsOperatorPageActive;

        private bool _isMediaListEmpty = true;

        public bool IsMediaListEmpty
        {
            get => _isMediaListEmpty;
            set
            {
                if (_isMediaListEmpty != value)
                {
                    _isMediaListEmpty = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(ShowDragAndDropHint));
                }
            }
        }

        public bool IsUnhideButtonVisible => 
            IsInDesignMode || (IsOperatorPageActive && !ShowProgressBar && _hiddenMediaItemsService.SomeHiddenMediaItems());


        private bool _showProgressBar;

        public bool ShowProgressBar
        {
            get => _showProgressBar;
            set
            {
                if (_showProgressBar != value)
                {
                    _showProgressBar = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(IsUnhideButtonVisible));
                }
            }
        }

        // commands...
        public RelayCommand GotoSettingsCommand { get; set; }

        public RelayCommand GotoOperatorCommand { get; set; }

        public RelayCommand LaunchMediaFolderCommand { get; set; }

        public RelayCommand LaunchHelpPageCommand { get; set; }

        public RelayCommand LaunchReleasePageCommand { get; set; }

        public RelayCommand UnhideCommand { get; set; }

        private void InitCommands()
        {
            GotoSettingsCommand = new RelayCommand(NavigateSettings);
            GotoOperatorCommand = new RelayCommand(NavigateOperator);
            LaunchMediaFolderCommand = new RelayCommand(LaunchMediaFolder);
            LaunchHelpPageCommand = new RelayCommand(LaunchHelpPage);
            LaunchReleasePageCommand = new RelayCommand(LaunchReleasePage);
            UnhideCommand = new RelayCommand(UnhideAll);
        }

        private void UnhideAll()
        {
            _hiddenMediaItemsService.UnhideAllMediaItems();
        }

        private void LaunchReleasePage()
        {
            Process.Start(VersionDetection.LatestReleaseUrl);
        }

        private void LaunchHelpPage()
        {
            Process.Start(@"https://github.com/AntonyCorbett/OnlyM/wiki");
        }

        private void LaunchMediaFolder()
        {
            if (Directory.Exists(_optionsService.Options.MediaFolder))
            {
                Process.Start(_optionsService.Options.MediaFolder);
            }
        }

        private void NavigateOperator()
        {
            _optionsService.Save();
            _pageService.GotoOperatorPage();
        }

        private void NavigateSettings()
        {
            // prevent navigation to the settings page when media is in flux...
            if (!_mediaStatusChangingService.IsMediaStatusChanging())
            {
                _pageService.GotoSettingsPage();
            }
        }
    }
}