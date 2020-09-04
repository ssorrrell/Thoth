using System;
using System.Threading.Tasks;
using System.Diagnostics;

using Xamarin.Forms;

using Thoth.Models;
using Thoth.Services;
using Thoth.Messages;
using Thoth.Common;

namespace Thoth.ViewModels
{
    public class RssEpisodeDetailViewModel : BaseViewModel
    {
        FeedItem _feedItem = null;
        public FeedItem FeedItem
        {
            get { return _feedItem; }
            set { SetProperty(ref _feedItem, value); }
        }

        RssEpisode _episodeItem = null;
        public RssEpisode EpisodeItem
        {
            get { return _episodeItem; }
            set
            {
                SetProperty(ref _episodeItem, value);
                if (_episodeItem.IsDownloaded == IsDownloadedEnum.Downloaded)
                    DownloadIcon = Constants.DeleteIcon;
                else
                    DownloadIcon = Constants.DownloadIcon;
            }
        }

        string _downloadIcon = Constants.DownloadIcon;
        public string DownloadIcon
        {
            get { return _downloadIcon; }
            set { SetProperty(ref _downloadIcon, value); }
        }

        bool _downloadButtonEnabled = true;
        public bool DownloadButtonEnabled
        {
            get { return _downloadButtonEnabled; }
            set { SetProperty(ref _downloadButtonEnabled, value); }
        }

        string _playPauseIcon = IconFont.PlayArrow;
        public string PlayPauseIcon
        {
            get { return _playPauseIcon; }
            set { SetProperty(ref _playPauseIcon, value); }
        }

        bool _playPauseState = false;
        public bool PlayPauseState
        {
            get { return _playPauseState; }
            set { SetProperty(ref _playPauseState, value); }
        }

        string _positionText = "";
        public string PositionText
        {
            get { return _positionText; }
            set { SetProperty(ref _positionText, value); }
        }

        int _sliderPosition = 0;
        public int SliderPosition
        {
            get { return _sliderPosition; }
            set { SetProperty(ref _sliderPosition, value); }
        }
        
        int _sliderMaximum = 1;
        public int SliderMaximum
        {
            get { return _sliderMaximum; }
            set { SetProperty(ref _sliderMaximum, value); }
        }

        RssEpisodeManager _rssEpisodeManager = null;
        public RssEpisodeManager RssEpisodeManager
        {
            get { if (_rssEpisodeManager == null) { _rssEpisodeManager = new RssEpisodeManager(); } return _rssEpisodeManager; }
            set { _rssEpisodeManager = value; }
        }

        public Command DownloadCommand { get; set; }
        public Command DeleteCommand { get; set; }

        public RssEpisodeDetailViewModel(FeedItem feedItem = null, RssEpisode episodeItem = null)
        {
            Title = feedItem?.Text;
            FeedItem = feedItem;
            EpisodeItem = episodeItem;
            DownloadCommand = new Command(async () => await ExecuteDeleteCommand()); //Page actually uses the _Clicks
            DeleteCommand = new Command(async () => await ExecuteDeleteCommand());

            //if player is already playing
            if (PodcastPlayer.Instance.IsPlaying)
            {
                PlayPauseIcon = IconFont.Pause;
                if ((int)EpisodeItem.Duration > 0)
                    SliderMaximum = (int)PodcastPlayer.Instance.Duration;
                Device.StartTimer(TimeSpan.FromSeconds(0.7), UpdatePosition);
            }
            else
            {
                if (EpisodeItem.IsDownloaded == IsDownloadedEnum.Downloaded && !string.IsNullOrEmpty(EpisodeItem.EnclosureLink))
                {
                    PodcastPlayer.Instance.Episode = EpisodeItem;
                    if ((int)EpisodeItem.Duration > 0)
                        SliderMaximum = (int)EpisodeItem.Duration;
                }
            }

            MessagingCenter.Subscribe<UpdateEpisodeMessage>(this, "UpdateEpisodeMessage", message => {
                Device.BeginInvokeOnMainThread(() =>
                {   //receive the result from DownloadService
                    if (EpisodeItem.Id == message.RssEpisode.Id)
                    {   //check that this ViewModel is the correct one for the downloaded podcast
                        DownloadButtonEnabled = true;
                        EpisodeItem = message.RssEpisode;
                    }

                });
            });
            MessagingCenter.Subscribe<UpdateEpisodeMessage>(this, "StartEpisodePlaying", message => {
                Device.BeginInvokeOnMainThread(() =>
                {   //receive the result from DownloadService
                    if (EpisodeItem.Id == message.RssEpisode.Id)
                    {   //check that this ViewModel is the correct one for the downloaded podcast
                        PlayPauseState = true;
                        PlayPauseIcon = IconFont.Pause;
                        EpisodeItem = message.RssEpisode;
                    }

                });
            });
            MessagingCenter.Subscribe<UpdateEpisodeMessage>(this, "StopEpisodePlaying", message => {
                Device.BeginInvokeOnMainThread(() =>
                {   //receive the result from DownloadService
                    if (EpisodeItem.Id == message.RssEpisode.Id)
                    {   //check that this ViewModel is the correct one for the downloaded podcast
                        PlayPauseState = false;
                        PlayPauseIcon = IconFont.PlayArrow;
                        EpisodeItem = message.RssEpisode;
                    }

                });
            });
            MessagingCenter.Subscribe<UpdateEpisodeMessage>(this, "PlaybackEnded", message => {
                Device.BeginInvokeOnMainThread(async () =>
                {   //receive the result from DownloadService
                    if (EpisodeItem.Id == message.RssEpisode.Id)
                    {   //check that this ViewModel is the correct one for the downloaded podcast
                        await RssEpisodeManager.FinishedEpisodeAsync(message.RssEpisode);
                        PlayPauseState = false;
                        PlayPauseIcon = IconFont.PlayArrow;
                        EpisodeItem = message.RssEpisode;
                    }

                });
            });
        }

        public async Task ExecuteDownloadCommand()
        {
            IsBusy = true;

            try
            {
                if (DownloadService.CanDownloadPodcast(EpisodeItem))
                {
                    DownloadButtonEnabled = false;
                    DownloadService.Instance.DownloadPodcast(EpisodeItem); //fires message center messages
                }
            }
            catch (Exception ex)
            {
                //could not delete item
                Debug.WriteLine("Could not Download Podcast File " + ex.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task ExecuteDeleteCommand()
        {
            IsBusy = true;

            try
            {
                EpisodeItem = await RssEpisodeManager.DeletePodcastFile(EpisodeItem);
                var finishedMessage2 = new UpdateEpisodeMessage
                {
                    RssEpisode = EpisodeItem
                };
                MessagingCenter.Send(finishedMessage2, "UpdateEpisodeMessage"); //goes to listening ViewModels that can download
            }
            catch (Exception ex)
            {
                //could not delete item
                Debug.WriteLine("Could not Delete Podcast File " + ex.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task ExecutePauseCommandAsync(RssEpisode episode)
        {
            IsBusy = true;

            try
            {
                var updatedEpisode = await RssEpisodeManager.PauseEpisodeAsync(episode);
                UpdateEpisodeItem(updatedEpisode); //do this at the end to avoid CollectionView refreshing
                SliderMaximum = (int)PodcastPlayer.Instance.Duration;
            }
            catch (Exception ex)
            {
                //could not delete item
                Debug.WriteLine("RssEpisodeDetailViewModel.ExecutePauseCommandAsync Could not Pause Podcast File " + ex.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task ExecutePlayCommandAsync(RssEpisode episode)
        {
            IsBusy = true;

            try
            {
                var updatedEpisode = await RssEpisodeManager.PlayEpisodeAsync(episode);
                UpdateEpisodeItem(updatedEpisode); //do this at the end to avoid CollectionView refreshing
                SliderMaximum = (int)PodcastPlayer.Instance.Duration;
                Device.StartTimer(TimeSpan.FromSeconds(0.7), UpdatePosition);
            }
            catch (Exception ex)
            {
                //could not delete item
                Debug.WriteLine("RssEpisodeDetailViewModel.ExecutePlayCommandAsync Could not Play Podcast File " + ex.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        public void ExecuteRewindCommand()
        {
            IsBusy = true;

            try
            {
                PodcastPlayer.Instance.Rewind();
                UpdatePosition();
            }
            catch (Exception ex)
            {
                //could not delete item
                Debug.WriteLine("RssEpisodeDetailViewModel.ExecuteRewindCommand Could not Rewind Podcast File " + ex.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        public void ExecuteFastForwardCommand()
        {
            IsBusy = true;

            try
            {
                PodcastPlayer.Instance.FastForward();
                UpdatePosition();
            }
            catch (Exception ex)
            {
                //could not delete item
                Debug.WriteLine("RssEpisodeDetailViewModel.ExecuteFastForwardCommand Could not FastForward Podcast File " + ex.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        public void ExecuteDragCompletedCommand(double newPosition)
        {
            IsBusy = true;

            try
            {
                PodcastPlayer.Instance.CurrentPosition = newPosition;
                UpdatePosition();
            }
            catch (Exception ex)
            {
                //could not delete item
                Debug.WriteLine("RssEpisodeDetailViewModel.ExecuteDragCompletedCommand Could not Move Slider " + ex.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void UpdateEpisodeItem(RssEpisode newEpisode)
        {
            EpisodeItem = newEpisode;
        }

        private bool UpdatePosition()
        {
            string title = PodcastPlayer.Instance.Episode.Title;
            int pos = (int)PodcastPlayer.Instance.CurrentPosition; //asking for CurrentPosition causes CurrentPositionString to be refreshed
            var position = PodcastPlayer.Instance.CurrentPositionString;
            var duration = PodcastPlayer.Instance.DurationString;
            PositionText = $"{title} Position: {position} / {duration}";
            SliderPosition = pos;
            return PodcastPlayer.Instance.IsPlaying;
        }
    }
}
