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
        }

        async void OnItemSelected(object sender, EventArgs args)
        {   //tap item in list
            var layout = (BindableObject)sender;
            var item = (RssEpisode)layout.BindingContext;

            await Navigation.PushAsync(new RssEpisodeDetailPage(new RssEpisodeDetailViewModel(ViewModel.FeedItem, item)));
        }

        async void OnCollectionViewRemainingItemsThresholdReachedAsync(object sender, EventArgs args)
        {   //list scrolled below loaded records
            await ViewModel.ExecuteLoadNextItemsCommand();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (ViewModel.Items.Count == 0)
                ViewModel.IsBusy = true;
        }
    }
}
