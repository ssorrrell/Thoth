using System;
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
        Task<int> SaveEpisodeItemAsync(RssEpisode item);
        Task<int> SaveEpisodeListAsync(List<RssEpisode> itemList);
        Task<int> AddEpisodesToFeed(List<RssEpisode> itemList);
        Task<int> DeleteEpisodeItemByIdAsync(int? id);
        Task<int> DeleteAllEpisodesByFeedIdAsync(int? feedId);

        Task<int> SeedData();

        /*Task<bool> AddItemAsync(T item);
        Task<bool> UpdateItemAsync(T item);
        Task<bool> DeleteItemAsync(int id);
        Task<T> GetItemAsync(int id);
        Task<IEnumerable<T>> GetItemsAsync(bool forceRefresh = false);*/
    }
}
