using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using Plugin.SimpleAudioPlayer;
using Thoth.Messages;
using Thoth.Models;
using Xamarin.Forms;

namespace Thoth.Services
{
    public sealed class PodcastPlayer : INotifyPropertyChanged, IDisposable
    {
        ISimpleAudioPlayer _player;

        private static readonly Lazy<PodcastPlayer> //lazy implementor
            lazy =
            new Lazy<PodcastPlayer>
                (() => new PodcastPlayer());

        public static PodcastPlayer Instance { get { return lazy.Value; } }

        private PodcastPlayer()
        {
            _player = CrossSimpleAudioPlayer.CreateSimpleAudioPlayer();
            _player.PlaybackEnded += OnPlaybackEnded;
        }

        public void Dispose()
        {
            _player.Stop();
            _player.PlaybackEnded -= OnPlaybackEnded;
            _player.Dispose();
        }

        ///<Summary>
        /// Raised when audio playback completes successfully 
        ///</Summary>
        public event EventHandler PlaybackEnded;

        // State Properties
        public bool _playPauseState = false; //Play = true, Pause = false
        public bool PlayPauseState
        {
            get { return _playPauseState; }
            set { SetProperty(ref _playPauseState, value); }
        }

        string _playPauseIcon = IconFont.PlayArrow;
        public string PlayPauseIcon
        {
            get { return _playPauseIcon; }
            set { SetProperty(ref _playPauseIcon, value); }
        }

        string _fastForwardIcon = IconFont.FastForward;
        public string FastForwardIcon
        {
            get { return _fastForwardIcon; }
            set { SetProperty(ref _fastForwardIcon, value); }
        }

        string _fastRewindIcon = IconFont.FastRewind;
        public string FastRewindIcon
        {
            get { return _fastRewindIcon; }
            set { SetProperty(ref _fastRewindIcon, value); }
        }

        string filePath = "";
        RssEpisode _episode = null;
        public RssEpisode Episode
        {
            get { return _episode; }
            set
            {
                if (_episode == null || ( _episode != null && _episode.Id != value.Id))
                {
                    filePath = FileHelper.GetPodcastPath(value.EnclosureLink);
                    var fileExists = File.Exists(filePath);
                    if (fileExists)
                    {
                        var fileStream = GetStreamFromFile(filePath);
                        if (fileStream != null && fileExists && !_player.IsPlaying)
                        {
                            _player.Load(fileStream);
                            Duration = _player.Duration;
                            SetProperty(ref _episode, value);
                        }
                    }
                }
            }
        }

        double _currentPosition = 0;
        public double CurrentPosition
        {
            get
            {
                return _currentPosition;
            }
            set
            {
                if (value >= 0 && value <= Duration && _player != null && _player.CanSeek)
                    _player.Seek(value);
                SetProperty(ref _currentPosition, value); 
            }
        }

        private double GetPrivateSetCurrentPosition()
        {
            return _currentPosition;
        }
        private void SetPrivateSetCurrentPosition(double value)
        {   //need to use SetProperty to trigger INotify, but not the .Seek
            SetProperty(ref _currentPosition, value);
        }

        double _duration = 0;
        public double Duration
        {
            get { return _duration; }
            set { SetProperty(ref _duration, value); }
        }

        public bool CanSeek
        {
            get { return _player.CanSeek; }
        }

        public bool IsPlaying
        {
            get { return _player.IsPlaying; }
        }

        // Public Player Functions
        public void PlayPause()
        {
            if (PlayPauseState)
            {
                PlayPauseIcon = IconFont.PlayArrow;
                PlayPauseState = false;
                _player.Pause();
                UpdatePosition();
            }
            else
            {
                PlayPauseIcon = IconFont.Pause;
                PlayPauseState = true;
                _player.Play();
                Device.StartTimer(TimeSpan.FromSeconds(0.7), UpdatePosition);
            }
        }

        public void Stop()
        {
            PlayPauseIcon = IconFont.PlayArrow;
            PlayPauseState = false;
            if (_player.IsPlaying)
                _player.Stop();
            UpdatePosition();
        }

        public void FastForward()
        {   //30 seconds
            if (CurrentPosition < (Duration - 30))
                CurrentPosition += 30; //30 seconds
            else
                CurrentPosition = Duration;
        }

        public void Rewind()
        {   //30 seconds
            if (CurrentPosition > 30)
                CurrentPosition -= 30; //30 seconds
            else
                CurrentPosition = 0;
        }

        private bool UpdatePosition()
        {   //from the player to the class variable
            if (_player != null)
            {
                SetPrivateSetCurrentPosition(_player.CurrentPosition);

                return _player.IsPlaying;
            }
            else
            {
                return false;
            }
        }

        void OnPlaybackEnded(object sender, EventArgs e)
        {
            Stop();
            PlaybackEnded?.Invoke(sender, e);
            var startMessage = new DownloadFinishedMessage2 { RssEpisode = Episode };
            MessagingCenter.Send(startMessage, "PlaybackEnded");
        }

        private Stream GetStreamFromFile(string filename)
        {
            Stream fileStream = null;
            var fileExists = File.Exists(filename);
            if (fileExists)
                fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
            return fileStream;
        }

        // Notify Changed Functions
        private bool SetProperty<T>(ref T backingStore, T value,
            [CallerMemberName] string propertyName = "",
            Action onChanged = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            onChanged?.Invoke();
            OnPropertyChanged(propertyName);
            return true;
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            var changed = PropertyChanged;
            if (changed == null)
                return;

            changed.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
