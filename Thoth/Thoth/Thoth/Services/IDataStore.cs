using System.Collections.Generic;
using System.Threading.Tasks;
using Thoth.Models;

namespace Thoth.Services
{
    public interface IDataStore
    {
        Task<List<FeedItem>> GetAllFeedItemsAsync();
        Task<FeedItem> GetFeedItemByTextAsync(string text);
        Task<int> GetMaxFeedItemId();
        Task<int> SaveFeedItemAsync(FeedItem item);
        Task<int> DeleteFeedItemByIdAsync(int? id);

        Task<List<RssEpisode>> GetAllEpisodeItemsIdAsync();
        Task<List<RssEpisode>> GetAllEpisodeItemsByFeedIdAsync(int id);
        Task<RssEpisode> GetEpisodeItemByIdAsync(int episodeId);
        Task<int> GetMaxRssEpisodeId();
        Task<int> SaveEpisodeItemAsync(RssEpisode item, bool doPublish = false);
        Task<int> SaveEpisodeListAsync(List<RssEpisode> itemList);
        Task<int> AddEpisodesToFeed(List<RssEpisode> itemList);
        Task<int> DeleteEpisodeItemByIdAsync(int? id);
        Task<int> DeleteAllEpisodesByFeedIdAsync(int? feedId);

        Task<int> SeedData();
    }
}
