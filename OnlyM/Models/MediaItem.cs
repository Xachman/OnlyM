﻿namespace OnlyM.Models
{
    using System;
    using System.Windows.Media;
    using Core.Extensions;
    using Core.Models;
    using GalaSoft.MvvmLight;

    public sealed class MediaItem : ObservableObject
    {
        private static readonly SolidColorBrush ImageIconBrush = new SolidColorBrush(Colors.DarkGreen);
        private static readonly SolidColorBrush AudioIconBrush = new SolidColorBrush(Colors.CornflowerBlue);
        private static readonly SolidColorBrush VideoIconBrush = new SolidColorBrush(Colors.Chocolate);
        private static readonly SolidColorBrush UnknownIconBrush = new SolidColorBrush(Colors.Crimson);
        private static readonly SolidColorBrush GreenBrush = new SolidColorBrush(Colors.DarkGreen);
        private static readonly SolidColorBrush BlackBrush = new SolidColorBrush(Colors.Black);
        private static readonly SolidColorBrush GrayBrush = new SolidColorBrush(Colors.DarkGray);

        public event EventHandler PlaybackPositionChangedEvent;

        public MediaItem()
        {
            IsWaitingAnimationVisible = true;
        }

        public Guid Id { get; set; }

        public bool IsVideo => MediaType.Classification == MediaClassification.Video; 

        public bool IsBlankScreen { get; set; }

        private bool _commandPanelVisible;

        public bool CommandPanelVisible
        {
            get => _commandPanelVisible;
            set
            {
                if (_commandPanelVisible != value)
                {
                    _commandPanelVisible = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(CommandPanelBtnColWidth));

                    if (!_commandPanelVisible && IsCommandPanelOpen)
                    {
                        IsCommandPanelOpen = false;
                    }
                }
            }
        }

        private bool _pauseOnLastFrame;

        public bool PauseOnLastFrame
        {
            get => _pauseOnLastFrame;
            set
            {
                if (_pauseOnLastFrame != value)
                {
                    _pauseOnLastFrame = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _allowFreezeCommand;

        public bool AllowFreezeCommand
        {
            get => _allowFreezeCommand;
            set
            {
                if (_allowFreezeCommand != value)
                {
                    _allowFreezeCommand = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(ShouldDisplayFreezeCommand));
                }
            }
        }

        public bool ShouldDisplayFreezeCommand => IsVideo && AllowFreezeCommand;

        private bool _isCommandPanelOpen;

        public bool IsCommandPanelOpen
        {
            get => _isCommandPanelOpen;
            set
            {
                if (_isCommandPanelOpen != value)
                {
                    _isCommandPanelOpen = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _isVisible;

        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                if (_isVisible != value)
                {
                    _isVisible = value;
                    RaisePropertyChanged();
                }
            }
        }

        public bool CommandPanelEnabled => !IsBlankScreen && !IsMediaActive;

        private string _name;

        public string Name
        {
            get => _name;
            set
            {
                if (_name == null || !_name.Equals(value))
                {
                    _name = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(AlphaNumericName));
                }
            }
        }

        public string AlphaNumericName
        {
            get
            {
                var prefix = Name.GetNumericPrefix();
                if (string.IsNullOrEmpty(prefix))
                {
                    return Name;
                }

                if (!int.TryParse(prefix, out var prefixNum))
                {
                    return Name;
                }

                var remainder = Name.Replace(prefix, string.Empty).Trim();
                return $"{prefixNum:D6} {remainder}";
            }
        }

        public string FilePath { get; set; }

        public long LastChanged { get; set; }

        private bool _isPaused;

        public bool IsPaused
        {
            get => _isPaused;

            set
            {
                if (_isPaused != value)
                {
                    _isPaused = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(PauseIconKind));
                    RaisePropertyChanged(nameof(HasDurationAndIsPlaying));
                    RaisePropertyChanged(nameof(IsSliderVisible));
                }
            }
        }

        public string PauseIconKind =>
            IsPaused
                ? "Play"
                : "Pause";
        
        public SupportedMediaType MediaType { get; set; }

        private ImageSource _thumbnailImageSource;

        public ImageSource ThumbnailImageSource
        {
            get => _thumbnailImageSource;
            set
            {
                if (_thumbnailImageSource == null || !_thumbnailImageSource.Equals(value))
                {
                    _thumbnailImageSource = value;
                    IsWaitingAnimationVisible = value == null;
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(IsPlayButtonVisible));
                    RaisePropertyChanged(nameof(IsStopButtonVisible));
                }
            }
        }

        private bool _isWaitingAnimationVisible;

        public bool IsWaitingAnimationVisible
        {
            get => _isWaitingAnimationVisible;
            set
            {
                if (_isWaitingAnimationVisible != value)
                {
                    _isWaitingAnimationVisible = value;
                    RaisePropertyChanged();
                }
            }
        }

        public bool IsPlayButtonVisible => !IsMediaActive && !IsWaitingAnimationVisible;

        public bool IsStopButtonVisible => IsMediaActive && !IsWaitingAnimationVisible;

        private bool _isMediaActive;

        public bool IsMediaActive
        {
            get => _isMediaActive;
            set
            {
                if (_isMediaActive != value)
                {
                    _isMediaActive = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(HasDurationAndIsPlaying));
                    RaisePropertyChanged(nameof(IsPauseButtonVisible));
                    RaisePropertyChanged(nameof(IsPlayButtonVisible));
                    RaisePropertyChanged(nameof(IsStopButtonVisible));
                    RaisePropertyChanged(nameof(IsSliderVisible));
                    RaisePropertyChanged(nameof(PlaybackTimeColorBrush));
                    RaisePropertyChanged(nameof(DurationColorBrush));
                    RaisePropertyChanged(nameof(CommandPanelEnabled));
                }
            }
        }

        private static string GenerateTimeString(long milliseconds)
        {
            return TimeSpan.FromMilliseconds(milliseconds).ToString(@"hh\:mm\:ss");
        }

        private bool _isMediaChanging;

        public bool IsMediaChanging
        {
            get => _isMediaChanging;
            set
            {
                if (_isMediaChanging != value)
                {
                    _isMediaChanging = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _isPlayButtonEnabled;

        public bool IsPlayButtonEnabled
        {
            get => _isPlayButtonEnabled;
            set
            {
                if (_isPlayButtonEnabled != value)
                {
                    _isPlayButtonEnabled = value;
                    RaisePropertyChanged();
                }
            }
        }

        public bool HasDuration =>
            MediaType.Classification == MediaClassification.Audio ||
            MediaType.Classification == MediaClassification.Video;

        public bool HasDurationAndIsPlaying => HasDuration && IsMediaActive && !IsPaused;

        private bool _allowPositionSeeking;

        public bool AllowPositionSeeking
        {
            get => _allowPositionSeeking;
            set
            {
                if (_allowPositionSeeking != value)
                {
                    _allowPositionSeeking = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(IsSliderVisible));
                }
            }
        }

        private bool _allowPause;

        public bool AllowPause
        {
            get => _allowPause;
            set
            {
                if (_allowPause != value)
                {
                    _allowPause = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(IsPauseButtonVisible));
                }
            }
        }

        public bool IsPauseButtonVisible => HasDuration && IsMediaActive && AllowPause;

        public bool IsSliderVisible => HasDuration && AllowPositionSeeking && (!IsMediaActive || IsPaused);

        private int _playbackPositionDeciseconds;

        public int PlaybackPositionDeciseconds
        {
            get => _playbackPositionDeciseconds;
            set
            {
                if (_playbackPositionDeciseconds != value)
                {
                    _playbackPositionDeciseconds = value;

                    PlaybackTimeString = GenerateTimeString(_playbackPositionDeciseconds * 10);

                    RaisePropertyChanged();
                    OnPlaybackPositionChangedEvent();
                }
            }
        }

        private string _playbackTimeString = GenerateTimeString(0);

        public string PlaybackTimeString
        {
            get => _playbackTimeString;
            set
            {
                if (!_playbackTimeString.Equals(value))
                {
                    _playbackTimeString = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string DurationString => GenerateTimeString(DurationDeciseconds * 10);

        private int _durationDeciseconds;

        public int DurationDeciseconds
        {
            get => _durationDeciseconds;
            set
            {
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (_durationDeciseconds != value)
                {
                    _durationDeciseconds = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(DurationString));
                }
            }
        }

        public Brush PlaybackTimeColorBrush =>
            IsMediaActive
                ? GreenBrush
                : GrayBrush;

        public Brush DurationColorBrush =>
            IsMediaActive
                ? BlackBrush
                : GrayBrush;

        public Brush IconBrush
        {
            get
            {
                switch (MediaType.Classification)
                {
                    case MediaClassification.Image:
                        return ImageIconBrush;

                    case MediaClassification.Video:
                        return VideoIconBrush;

                    case MediaClassification.Audio:
                        return AudioIconBrush;

                    default:
                        return UnknownIconBrush;
                }
            }
        }

        public string IconName
        {
            get
            {
                switch (MediaType.Classification)
                {
                    case MediaClassification.Image:
                        return "ImageFilterHdr";
                        
                    case MediaClassification.Video:
                        return "Video";

                    case MediaClassification.Audio:
                        return "VolumeHigh";

                    default:
                        return "Question";
                }
            }
        }

        public int CommandPanelBtnColWidth => CommandPanelVisible ? 12 : 0;

        private void OnPlaybackPositionChangedEvent()
        {
            PlaybackPositionChangedEvent?.Invoke(this, EventArgs.Empty);
        }
    }
}
