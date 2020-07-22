using System;
using System.ComponentModel;

using Xamarin.Forms;

using Thoth.Models;
using Thoth.ViewModels;
using Thoth.Services;

[assembly: ExportFont("MaterialsIcon-Regular.ttf", Alias = "Material Design Icons")]
namespace Thoth.Views
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class RssEpisodeDetailPage : ContentPage
    {
        readonly RssEpisodeDetailViewModel ViewModel;

        public RssEpisodeDetailPage(RssEpisodeDetailViewModel viewModel)
        {
            InitializeComponent();

            if (viewModel == null)
            {
                this.ViewModel = new RssEpisodeDetailViewModel();
            }
            else
            {
                this.ViewModel = viewModel;
            }

            BindingContext = this.ViewModel;
        }

        async void DownloadEpisode_Clicked(object sender, EventArgs e)
        {
            if (ViewModel.DownloadIcon == Constants.DeleteIcon)
            {
                var answer = await DisplayAlert("Delete File ", "Are you sure you want to delete this," + ViewModel.EpisodeItem.Title + ", downloaded file.", "Yes", "No");
                if (answer)
                {
                    await ViewModel.ExecuteDeleteCommand();
                }
            }
            else
            {
                await ViewModel.ExecuteDownloadCommand();
            }
        }

        async void Play_Clicked(object sender, EventArgs e)
        {
            var episode = ViewModel.EpisodeItem;
            if (episode.IsDownloaded == IsDownloadedEnum.NotDownloaded)
            {
                await ViewModel.ExecuteDownloadCommand();
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

        async void OnCovertArtClicked(object sender, EventArgs args)
        {
            var imageSource = (Image)this.FindByName("CoverArt");
            if (imageSource != null)
            {
                var filePath = imageSource.Source.ToString();
                if (!string.IsNullOrEmpty(filePath))
                {
                    filePath = filePath.Replace("File: ", "");
                    await Navigation.PushAsync(new CoverArtPage(filePath));
                }
            }
        }

        private string ConvertPosition(double value)
        {   //convert seconds to hr:min:sec
            //5555 - sec
            //92.583 - min
            //1.543 - hr
            
            //12160 - sec
            //202.667 - min
            //3.3778 - hr


            var sec = value;
            var min = TimeSpan.FromSeconds(value).TotalMinutes;
            var hr = TimeSpan.FromSeconds(value).TotalHours;
            if (hr > 0)
            {

            }

            var day = Math.Truncate(TimeSpan.FromSeconds(value).TotalDays);

            return day + ":" + hr + ":" + min + ":" + sec;
        }

        private void Rewind_Clicked(object sender, EventArgs e)
        {
            //Rewind Clicked
            ViewModel.ExecuteRewindCommand();
        }

        private void FastForward_Clicked(object sender, EventArgs e)
        {
            //Fast Forward Clicked
            ViewModel.ExecuteFastForwardCommand();
        }

        private void Drag_Completed(object sender, EventArgs e)
        {
            //Slider Dragged by user
            var control = (Slider)sender;
            if (control != null)
                ViewModel.ExecuteDragCompletedCommand(control.Value);
        }
    }
}