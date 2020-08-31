using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Linq;

using Xamarin.Forms;

using Thoth.Models;
using Thoth.Services;
using Thoth.Messages;
using Thoth.Common;


namespace Thoth.ViewModels
{
    public class RssEpisodeListViewModel : BaseViewModel
    {
        FeedItem _feedItem = null;
        public FeedItem FeedItem
        {
            get { return _feedItem; }
            set { SetProperty(ref _feedItem, value); }
        }

        string _deleteIcon = Constants.DeleteIcon;
        public string DeleteIcon
        {
            get { return _deleteIcon; }
            set { SetProperty(ref _deleteIcon, value); }
        }

        string _refreshIcon = Constants.RefreshIcon;
        public string RefreshIcon
        {
            get { return _refreshIcon; }
            set { SetProperty(ref _refreshIcon, value); }
        }

        bool _showEpisodeImages = true;
        public bool ShowEpisodeImages
        {
            get { return _showEpisodeImages; }
            set { SetProperty(ref _showEpisodeImages, value); }
        }

        FeedItemManager _feedItemManager = null;
        public FeedItemManager FeedItemManager
        {
            get { if (_feedItemManager == null) { _feedItemManager = new FeedItemManager(); } return _feedItemManager; }
            set { _feedItemManager = value; }
        }

        RssEpisodeManager _rssEpisodeManager = null;
        public RssEpisodeManager RssEpisodeManager
        {
            get { if (_rssEpisodeManager == null) { _rssEpisodeManager = new RssEpisodeManager(); } return _rssEpisodeManager; }
            set { _rssEpisodeManager = value; }
        }

        public ObservableCollection<RssEpisode> Items { get; set; }
        public Command LoadItemsCommand { get; set; }
        public Command RefreshCommand { get; set; }
        public Command DeleteCommand { get; set; }

        public RssEpisodeListViewModel(FeedItem item = null)
        {
            Title = item?.Text;
            FeedItem = item;
            Items = new ObservableCollection<RssEpisode>();
            LoadItemsCommand = new Command(async () => await ExecuteLoadItemsCommand());
            RefreshCommand = new Command(async () => await ExecuteRefreshCommand());
            DeleteCommand = new Command(async () => await ExecuteDeleteCommand());

            MessagingCenter.Subscribe<UpdateEpisodeMessage>(this, "UpdateEpisodeMessage", message => {
                Device.BeginInvokeOnMainThread(() =>
                {   //receive the result from DownloadService
                    //check that this ViewModel is the correct one for the downloaded podcast
                    if (message.RssEpisode.FeedItemId == FeedItem.Id)
                    {   //check the Feed Id first
                        UpdateEpisodeInItemsList(message.RssEpisode);
                    }
                });
            });
            MessagingCenter.Subscribe<UpdateEpisodeMessage>(this, "PlaybackEnded", message => {
                Device.BeginInvokeOnMainThread(async () =>
                {   //receive the result from DownloadService
                    if (message.RssEpisode.FeedItemId == FeedItem.Id)
                    {   //check that this ViewModel is the correct one for the downloaded podcast
                        await RssEpisodeManager.FinishedEpisodeAsync(message.RssEpisode);
                        UpdateEpisodeInItemsList(message.RssEpisode);
                    }

                });
            });
        }

        async Task ExecuteLoadItemsCommand()
        {
            IsBusy = true;

            try
            {
                Items.Clear();
                ShowEpisodeImages = false;
                if (FeedItem != null && FeedItem.Id != null)
                {
                    var count = 0;
                    var items = await DataStore.GetAllEpisodeItemsByFeedIdAsync(FeedItem.Id.Value);
                    var oldImageFileName = items.First<RssEpisode>().ImageFileName;
                    foreach (var item in items)
                    {
                        if (!ShowEpisodeImages && item.ImageFileName != oldImageFileName)
                            ShowEpisodeImages = true;
                        Items.Add(item);
                        count++;
                        //if (count >= 20)
                        //    break;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        async Task ExecuteRefreshCommand()
        {
            try
            {
                if (FeedItem != null)
                {
                    FeedItem = await FeedItemManager.RefreshFeedItem(FeedItem);
                    await ExecuteLoadItemsCommand();
                }
            }
            catch(Exception ex)
            {
                //could not refresh item
                Debug.WriteLine("RssEpisodeListViewModel.ExecuteRefreshCommand Error " + ex.Message);
            }
        }

        public void ExecuteDownloadCommand(RssEpisode episode)
        {
            IsBusy = true;

            try
            {
                if (episode != null && episode.Id != null)
                {
                    if (DownloadService.CanDownloadPodcast(episode))
                    {
                        DownloadService.Instance.DownloadPodcast(episode); //fires message center messages
                    }
                    else
                    {
                        Debug.WriteLine("RssEpisodeListViewModel.ExecuteDownloadCommand CanDownload was false ");
                    }
                }
                else
                {
                    Debug.WriteLine("RssEpisodeListViewModel.ExecuteDownloadCommand episode or episode id was null ");
                }
            }
            catch (Exception ex)
            {
                //could not delete item
                Debug.WriteLine("RssEpisodeListViewModel.ExecuteDownloadCommand Could not Download Podcast File " + ex.Message);
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
                FeedItem = await FeedItemManager.DeleteFeedItem(FeedItem);
            }
            catch (Exception ex)
            {
                //could not delete item
                Debug.WriteLine("RssEpisodeListViewModel.ExecuteDeleteCommand Could not Delete Feed Item " + ex.Message);
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
                UpdateEpisodeInItemsList(updatedEpisode); //do this at the end to avoid CollectionView refreshing
            }
            catch (Exception ex)
            {
                //could not delete item
                Debug.WriteLine("RssEpisodeListViewModel.ExecutePauseCommandAsync Could not Pause Podcast File " + ex.Message);
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
                UpdateEpisodeInItemsList(updatedEpisode); //do this at the end to avoid CollectionView refreshing
            }
            catch (Exception ex)
            {
                //could not delete item
                Debug.WriteLine("RssEpisodeListViewModel.ExecutePlayCommandAsync Could not Play Podcast File " + ex.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void UpdateEpisodeInItemsList(RssEpisode newEpisode)
        {
            RssEpisode foundEpisode = null;
            int index = 0;
            foreach (var e in Items)
            {   //find the episode that finished
                if (e.Id == newEpisode.Id)
                {
                    foundEpisode = e;
                    break;
                }
                index++;
            }
            if (foundEpisode != null) //update the one episode
                Items[index] = newEpisode;
        }
    }
}
