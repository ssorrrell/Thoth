using System;
using System.Collections.ObjectModel;

using Thoth.Models;
using Thoth.ViewModels;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Thoth.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RssEpisodeListPage : ContentPage
    {
        RssEpisodeListViewModel ViewModel;
        public ObservableCollection<string> Items { get; set; }

        public RssEpisodeListPage(RssEpisodeListViewModel viewModel)
        {
            InitializeComponent();

            if (viewModel == null)
            {
                this.ViewModel = new RssEpisodeListViewModel();
            }
            else
            {
                this.ViewModel = viewModel;
            }
            BindingContext = this.ViewModel;
        }

        async void DeleteItem_Clicked(object sender, EventArgs e)
        {
            var answer = await DisplayAlert("Delete Feed ", "Are you sure you want to delete this," + ViewModel.FeedItem.Text + ", Podcast subscription and downloaded files.", "Yes", "No");
            if (answer)
            {
                await ViewModel.ExecuteDeleteCommand();
                await Navigation.PopAsync();
            }
        }

        private object syncLock = new object();
        bool isInCall = false;

        async void Refresh_Clicked(object sender, EventArgs e)
        {
            lock (syncLock)
            {
                if (isInCall)
                    return;
                isInCall = true;
            }

            try
            {
                MessagingCenter.Send(this, "RefreshEpisodes", ViewModel.FeedItem);
            }
            finally
            {
                lock (syncLock)
                {
                    isInCall = false;
                 }
            }
        }

        async void Play_Clicked(object sender, EventArgs e)
        {
            var layout = (BindableObject)sender;
            var episode = (RssEpisode)layout.BindingContext;

            if (episode.IsDownloaded == IsDownloadedEnum.NotDownloaded)
            {
                //download
                ViewModel.ExecuteDownloadCommand(episode);
            }
            else if (episode.IsDownloaded == IsDownloadedEnum.Downloaded)
            {
                //play or pause
                if (episode.IsPlaying == IsPlayingEnum.IsPlaying)
                {
                    //pause
                    await ViewModel.ExecutePauseCommandAsync(episode);
                }
                else
                {
                    //play
                    await ViewModel.ExecutePlayCommandAsync(episode);
                }
            }

            /*if (episode.IsDownloaded == IsDownloadedEnum.Downloaded && !string.IsNullOrEmpty(episode.EnclosureLink)
                && !PodcastPlayer.Instance.IsPlaying)
            {   //might need to reload the Episode here if a downlooad completed 
                //or another podcast was playing when the page loaded
                PodcastPlayer.Instance.Episode = episode;
            }

            if (!PodcastPlayer.Instance.IsPlaying)
            {
                ViewModel.PlayPauseIcon = IconFont.PlayArrow;
                PlayPauseState = false;
                PodcastPlayer.Instance.PlayPause();
                sliderPosition.Maximum = PodcastPlayer.Instance.Duration;
                sliderPosition.IsEnabled = PodcastPlayer.Instance.CanSeek;
            }
            else
            {
                ViewModel.PlayPauseIcon = IconFont.Pause;
                PlayPauseState = true;
                PodcastPlayer.Instance.PlayPause();
                sliderPosition.Maximum = PodcastPlayer.Instance.Duration;
                sliderPosition.IsEnabled = PodcastPlayer.Instance.CanSeek;
                Device.StartTimer(TimeSpan.FromSeconds(0.7), UpdatePosition);
            }*/
        }

        /*private bool UpdatePosition()
        {
            int pos = (int)PodcastPlayer.Instance.CurrentPosition;
            lblPosition.Text = $"Position: {pos} / {PodcastPlayer.Instance.Duration}";

            sliderPosition.ValueChanged -= SliderPositionValueChanged;
            sliderPosition.Value = pos;
            sliderPosition.ValueChanged += SliderPositionValueChanged;

            return PodcastPlayer.Instance.IsPlaying;
        }*/

        async void OnItemSelected(object sender, EventArgs args)
        {
            var layout = (BindableObject)sender;
            var item = (RssEpisode)layout.BindingContext;

            await Navigation.PushAsync(new RssEpisodeDetailPage(new RssEpisodeDetailViewModel(ViewModel.FeedItem, item)));
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (ViewModel.Items.Count == 0)
                ViewModel.IsBusy = true;
        }
    }
}
