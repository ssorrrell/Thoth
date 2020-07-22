using System;

using Xamarin.Forms;

using Thoth.Models;
using Thoth.Views;
using Thoth.Services;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Thoth.ViewModels
{
    public class FeedItemDetailViewModel : BaseViewModel
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

        public Command DeleteFeedItemCommand { get; set; }

        public FeedItemDetailViewModel(FeedItem item = null)
        {
            Title = item?.Text;
            FeedItem = item;
            DeleteFeedItemCommand = new Command(async () => await ExecuteDeleteFeedItemCommand());
        }

        async Task ExecuteDeleteFeedItemCommand()
        {
            IsBusy = true;

            try
            {
                //Delete downloaded Files

                var result = await DataStore.DeleteFeedItemByIdAsync(FeedItem.Id); //returns the number of items deleted
                if (result == 1)
                    MessagingCenter.Send(FeedItem, "DeleteItem");
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
