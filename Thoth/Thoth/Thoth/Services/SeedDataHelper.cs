
using System.Collections.Generic;
using Thoth.Models;

namespace Thoth.Services
{
    public class SeedDataHelper
    {
        public static FeedItem CreateFeedItem(int index)
        {
            var feedItem = new FeedItem();
            feedItem.Id = null;
            feedItem.Link = "http://www.google.com";
            feedItem.Text = "Test " + index.ToString();
            feedItem.Description = "Description " + index.ToString();
            return feedItem;
        }

        public static List<RssEpisode> CreateRssEpisodeForFeedItem(int feedItemId)
        {
            var episodeList = new List<RssEpisode>();

            for (var i = 0; i < 10; i++)
            {
                var episode = new RssEpisode();
                episode.Title = "Episode " + i.ToString();
                episode.IsDownloaded = IsDownloadedEnum.NotDownloaded;
                episode.Author = "Isaac Asimov";
                episode.Description = "Description " + i.ToString();
                episode.FeedItemId = feedItemId;
                episodeList.Add(episode);
            }
            return episodeList;
        }
    }
}
