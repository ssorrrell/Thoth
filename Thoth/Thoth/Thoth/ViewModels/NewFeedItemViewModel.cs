using System;
using System.Diagnostics;
using System.Threading.Tasks;

using Xamarin.Forms;

using Thoth.Models;
using Thoth.Helpers;
using Thoth.Common;

namespace Thoth.ViewModels
{
    public class NewFeedItemViewModel : BaseViewModel
    {
        public string Link { get; set; }
        public string SaveIcon { get { return Constants.SaveIcon; } }
        public string CancelIcon { get { return Constants.CancelIcon; } }
        public Command NewFeedItemCommand { get; set; }

        public NewFeedItemViewModel()
        {
            NewFeedItemCommand = new Command(async () => await ExecuteNewFeedItemCommand());
        }

        public async Task ExecuteNewFeedItemCommand()
        {
            IsBusy = true;

            try
            {
                var doc = await RssFeedHelper.GetXDocFromLinkAsync(Link);
                if (doc != null)
                {
                    var newFeedItem = FeedItemManager.GetFeedItemFromXDoc(doc, Link);
                    if (newFeedItem != null)
                    {
                        var successFeedImage = await FileHelper.DownloadImageFile(newFeedItem.ImageLink);
                        if (!successFeedImage)
                            Debug.WriteLine("Could not download feed image file " + newFeedItem.ImageLink);

                        var result = await DataStore.SaveFeedItemAsync(newFeedItem);
                        if (newFeedItem.Id != null)
                        {
                            //defensive programming, delete any existing records for this new FeedItem.Id
                            //this is because Ids get reused and not all the episodes in and Id might get deleted
                            try
                            {
                                result = await DataStore.DeleteAllEpisodesByFeedIdAsync(newFeedItem.Id); //don't care about the result
                            }
                            catch(Exception ex)
                            {
                                Debug.WriteLine("Info Error Deleting Episodes before adding new Feed Item " + ex.Message);
                            }
                            var episodeList = RssEpisodeManager.GetRssEpisodesFromXDoc(doc, newFeedItem.Id.Value); //get the episodes
                            result = await DataStore.SaveEpisodeListAsync(episodeList); //save the episodes
                            //get the images
                            var count = 0;
                            var lastImageLink = "";
                            foreach(var episode in episodeList)
                            {
                                if (lastImageLink != episode.ImageLink)
                                {   //don't try to download when the image is the same from episode to episode
                                    lastImageLink = episode.ImageLink;
                                    var success = await FileHelper.DownloadImageFile(episode.ImageLink);
                                    if (!success)
                                        Debug.WriteLine("Could not download image file " + episode.ImageLink);
                                }
                                count++;
                                if (count > 20) break; //only get 20, not like 1000
                            }
                            MessagingCenter.Send(newFeedItem, "AddFeedItem");
                        }
                        else
                        {
                            //newFeedItem.Id was null
                            Debug.WriteLine("newFeedItem.Id was null");
                        }
                    }
                    else
                    {
                        //newFeedItem was null
                        Debug.WriteLine("newFeedItem was null");
                    }
                }
                else
                {
                    //problem getting feed
                    await Application.Current.MainPage.DisplayAlert("Alert", "Could not get RSS Feed from Link", "OK");
                }
            }
            catch (Exception ex)
            {
                //could not delete item
                Debug.WriteLine("Could not Add Podcast Feed Item " + ex.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

    }
}
