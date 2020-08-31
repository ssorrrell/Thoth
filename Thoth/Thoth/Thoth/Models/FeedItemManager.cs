using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;

using Thoth.Services;
using Thoth.Helpers;
using Xamarin.Forms;

namespace Thoth.Models
{
    public class FeedItemManager
    {
        private IDataStore DataStore => DependencyService.Get<IDataStore>();

        RssEpisodeManager _rssEpisodeManager = null;
        public RssEpisodeManager RssEpisodeManager
        {
            get { if (_rssEpisodeManager == null) { _rssEpisodeManager = new RssEpisodeManager(); } return _rssEpisodeManager; }
            set { _rssEpisodeManager = value; }
        }

        public static FeedItem GetFeedItemFromXDoc(XDocument doc, string uri)
        {
            var feedItem = new FeedItem();

            //populate feedItem
            foreach (var channel in doc.Descendants("channel"))
            {
                if (channel != null)
                {
                    var imageLink = RssFeedHelper.GetImageUriFromXElement(channel.Element("image"));
                    var imageFileName = Path.GetFileName(imageLink);
                    feedItem.Text = RssFeedHelper.ConvertXElementToString(channel.Element("title"));
                    feedItem.Description = RssFeedHelper.ConvertXElementToString(channel.Element("description"));
                    feedItem.Link = uri;
                    feedItem.ImageLink = imageLink;
                    feedItem.ImageFileName = imageFileName;
                    feedItem.LastCheck = DateTime.Now; //save LastCheck
                }
            }
            return feedItem;
        }

        public async Task<FeedItem> DeleteFeedItem(FeedItem feedItem)
        {
            FeedItem returnFeedItem = null;
            try
            {
                //delete episodes
                var result = await RssEpisodeManager.DeleteAllEpisodes(feedItem);
                if (!result)
                    Debug.WriteLine("FeedItemManager.DeleteFeedItem DeleteAllEpisodes return false ");
                //delete feed item
                var deleteFeedResult = await DataStore.DeleteFeedItemByIdAsync(feedItem.Id); //returns the number of items deleted
                if (deleteFeedResult == 1)
                    MessagingCenter.Send(feedItem, "DeleteItem"); //update UI
                returnFeedItem = feedItem;
            }
            catch (Exception ex)
            {
                //could not delete item
                Debug.WriteLine("FeedItemManager.DeleteFeedItem Could not Delete Feed Item " + ex.Message);
            }
            return returnFeedItem;
        }

        public async Task<FeedItem> RefreshFeedItem(FeedItem feedItem)
        {
            try
            {
                if (feedItem != null)
                {
                    //get new episodes from saved URI in feedItem
                    var doc = await RssFeedHelper.GetXDocFromLinkAsync(feedItem.Link);

                    //update ImageLink and ImageFileName for FeedItem
                    var feedItemFromXDoc = FeedItemManager.GetFeedItemFromXDoc(doc, feedItem.Link);
                    feedItem.ImageLink = feedItemFromXDoc.ImageLink; //new image on FeedItem for new episode
                    feedItem.ImageFileName = feedItemFromXDoc.ImageFileName;
                    feedItem.LastCheck = DateTime.Now; //save LastCheck
                    var result = await DataStore.SaveFeedItemAsync(feedItem);
                    if (result == 0)
                        Debug.WriteLine("Could Not update FeedItem with new LastCheck date");

                    //get new episodes from saved URI in feedItem
                    var episodes = RssEpisodeManager.GetRssEpisodesFromXDoc(doc, feedItem.Id.Value);

                    //save new episodes to database
                    result = await DataStore.AddEpisodesToFeed(episodes); //returns the number of items deleted                                                                         
                    if (result == 0)
                        Debug.WriteLine("FeedItemManager.RefreshFeedItem Result from AddEpisodesToFeed was 0 ");
                }
            }
            catch (Exception ex)
            {
                //could not refresh item
                Debug.WriteLine("FeedItemManager.RefreshFeedItem Error " + ex.Message);
            }
            return feedItem;
        }
    }
}
