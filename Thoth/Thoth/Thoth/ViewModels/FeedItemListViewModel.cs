using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;

using Xamarin.Forms;

using Thoth.Models;
using Thoth.Common;

namespace Thoth.ViewModels
{
    public class FeedItemListViewModel : BaseViewModel
    {
        FeedItem _feedItem = null;
        public FeedItem FeedItem
        {
            get { return _feedItem; }
            set { SetProperty(ref _feedItem, value); }
        }

        string _addIcon = Constants.AddIcon;
        public string AddIcon
        {
            get { return _addIcon; }
            set { SetProperty(ref _addIcon, value); }
        }

        public ObservableCollection<FeedItem> Items { get; set; }
        public Command LoadItemsCommand { get; set; }

        public FeedItemListViewModel(FeedItem feedItem = null)
        {
            FeedItem = feedItem;
            Title = "Podcasts";
            Items = new ObservableCollection<FeedItem>();
            LoadItemsCommand = new Command(async () => await ExecuteLoadItemsCommand());

            MessagingCenter.Subscribe<FeedItem>(this, "AddFeedItem", newFeedItem =>
            {
                Items.Add(newFeedItem); //add the new Feed Item to the list
            });

            MessagingCenter.Subscribe<FeedItem>(this, "DeleteItem", oldFeedItem =>
            {
                if (oldFeedItem != null && Items != null) //remove the FeedItem from the list from the RssEpisodeListViewModel
                    Items.Remove(oldFeedItem);
            });
        }

        async Task ExecuteLoadItemsCommand()
        {
            IsBusy = true;

            try
            {
                Items.Clear();
                var items = await DataStore.GetAllFeedItemsAsync();
                foreach (var item in items)
                {
                    Items.Add(item);
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
    }
}