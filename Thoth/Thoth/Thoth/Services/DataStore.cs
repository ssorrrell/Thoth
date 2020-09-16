using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using SQLite;

using Thoth.Models;
using Thoth.Helpers;
using Thoth.Common;
using Thoth.Messages;
using Xamarin.Forms;

namespace Thoth.Services
{
    public class DataStore : IDataStore
    {
        static readonly Lazy<SQLiteAsyncConnection> lazyInitializer = new Lazy<SQLiteAsyncConnection>(() =>
        {
            return new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
        });

        static SQLiteAsyncConnection Database => lazyInitializer.Value;
        static bool initialized = false;

        List<FeedItem> items;
        List<RssEpisode> episodes;

        public DataStore()
        {
            InitializeAsync().SafeFireAndForget(false);
        }

        async Task InitializeAsync()
        {
            if (!initialized)
            {
                if (!Database.TableMappings.Any(m => m.MappedType.Name == typeof(FeedItem).Name))
                {
                    await Database.CreateTablesAsync(CreateFlags.None, typeof(FeedItem)).ConfigureAwait(false);
                    await Database.CreateTablesAsync(CreateFlags.None, typeof(RssEpisode)).ConfigureAwait(false);
                    initialized = true;
                }
            }
        }

         //********************** FeedItem Functions ********************************
        public async Task<List<FeedItem>> GetAllFeedItemsAsync()
        {
            items = await Database.Table<FeedItem>().ToListAsync();
            return items;
        }

        public async Task<FeedItem> GetFeedItemByTextAsync(string text)
        {
            return await Database.Table<FeedItem>().Where(i => i.Text == text).FirstOrDefaultAsync();
        }

        public async Task<int> GetMaxFeedItemId()
        {
            return await Database.ExecuteScalarAsync<int>("SELECT Max(Id) FROM [FeedItem]").ConfigureAwait(false);
        }

        public async Task<int> SaveFeedItemAsync(FeedItem item)
        {
            if (item.Id != 0 && item.Id != null)
            {
                var oldItem = items.Where((FeedItem arg) => arg.Text == item.Text).FirstOrDefault();
                items.Remove(oldItem);
                items.Add(item);
                return await Database.UpdateAsync(item);
            }
            else
            {
                var maxId = await GetMaxFeedItemId();
                item.Id = maxId + 1;
                items.Add(item);
                return await Database.InsertAsync(item);
            }
        }

        public async Task<int> DeleteFeedItemByIdAsync(int? id)
        {
            items = await Database.Table<FeedItem>().ToListAsync();
            var oldItem = items.Where((FeedItem arg) => arg.Id == id).FirstOrDefault();
            if (oldItem == null)
                throw new Exception("Can not find FeedItem " + id);
            items.Remove(oldItem);
            return await Database.DeleteAsync<FeedItem>(id);
            //return await Database.DeleteAsync(item);
            //return await Database.DeleteAllAsync<FeedItem>();
        }


        //********************** RssEpisode Functions ********************************
        public async Task<List<RssEpisode>> GetAllEpisodeItemsIdAsync()
        {
            episodes = await Database.Table<RssEpisode>()
                .OrderBy(x => x.FeedItemId)
                .ToListAsync();
            return episodes;
        }

        public async Task<List<RssEpisode>> GetAllEpisodeItemsByFeedIdAsync(int feedItemId)
        {
            var feedEpisodes = await Database.Table<RssEpisode>()
                .Where(i => i.FeedItemId == feedItemId)
                .OrderByDescending(d => d.PubDate)
                .ToListAsync();
            return feedEpisodes;
        }

        public async Task<RssEpisode> GetEpisodeItemByIdAsync(int episodeId)
        {
            var episodeItem = await Database.Table<RssEpisode>()
                .Where(i => i.Id == episodeId)
                .FirstOrDefaultAsync();
            return episodeItem;
        }

        public async Task<int> GetMaxRssEpisodeId()
        {
            return await Database.ExecuteScalarAsync<int>("SELECT Max(Id) FROM [RssEpisode]").ConfigureAwait(false);
        }

        public async Task<int> SaveEpisodeItemAsync(RssEpisode item, bool doPublish = false)
        {
            if (episodes == null)
                episodes = new List<RssEpisode>();
            var result = 0;
            if (item.Id != null && item.Id != 0)
            {
                var oldItem = episodes.Where((RssEpisode arg) => arg.Id == item.Id).FirstOrDefault();
                episodes.Remove(oldItem);
                episodes.Add(item);
                result = await Database.UpdateAsync(item);
            }
            else
            {
                var maxId = await GetMaxRssEpisodeId();
                item.Id = maxId + 1;
                episodes.Add(item);
                result = await Database.InsertAsync(item);
            }
            if (result > 0 && doPublish)
            {
                var updateEpisodeMessage = new UpdateEpisodeMessage
                {
                    RssEpisode = item
                };
                MessagingCenter.Send(updateEpisodeMessage, "UpdateEpisodeMessage"); //goes to listening ViewModels that can download
            }
            return result;
        }

        public async Task<int> SaveEpisodeListAsync(List<RssEpisode> itemList)
        {
            int result = 0;
            foreach(var item in itemList)
            {
                result = await SaveEpisodeItemAsync(item);
            }
            return result;
        }

        public async Task<int> AddEpisodesToFeed(List<RssEpisode> itemList)
        {
            var item = itemList.FirstOrDefault<RssEpisode>();
            var feedId = item.FeedItemId;
            var additionalItems = new List<RssEpisode>();
            var existingList = await GetAllEpisodeItemsByFeedIdAsync(feedId);
            foreach (var newItem in itemList)
            {
                var found = false;
                foreach (var existingItem in existingList)
                {
                    if (existingItem.Title == newItem.Title)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                    additionalItems.Add(newItem);
            }
            int result = 0;
            if (additionalItems.Count > 0)
                result = await SaveEpisodeListAsync(additionalItems);
            return result;
        }

        public async Task<int> DeleteEpisodeItemByIdAsync(int? id)
        {
            var feedEpisodes = await Database.Table<RssEpisode>().Where(i => i.Id == id).ToListAsync();
            var oldItem = feedEpisodes.Where((RssEpisode arg) => arg.Id == id).FirstOrDefault();
            if (oldItem == null)
                throw new Exception("Can not find RssEpisode " + id);
            feedEpisodes.Remove(oldItem);
            return await Database.DeleteAsync<RssEpisode>(id);
        }

        public async Task<int> DeleteAllEpisodesByFeedIdAsync(int? feedId)
        {
            var deletedItemCount = 0;
            var feedEpisodes = await Database.Table<RssEpisode>().Where(i => i.FeedItemId == feedId).ToListAsync();
            if (feedEpisodes != null)
            {
                foreach (var oldItem in feedEpisodes)
                {
                    //delete downloaded files
                    var podcastFilePath = FileHelper.GetPodcastPath(oldItem.EnclosureLink);
                    var success = FileHelper.DeletePodcastFile(podcastFilePath);
                    var imageFilePath = FileHelper.GetImagePath(oldItem.ImageLink);
                    success = FileHelper.DeleteImageFile(imageFilePath);
                    //delete database entry for episode
                    var result = await Database.DeleteAsync<RssEpisode>(oldItem.Id);
                    deletedItemCount += result;
                }
            }
            return deletedItemCount;
        }

        //********************** RssEpisode Functions ********************************
        public async Task<int> SeedData()
        {
            var result3 = await Database.DeleteAllAsync<FeedItem>();
            var result4 = await Database.DeleteAllAsync<RssEpisode>();
            var feedItem = SeedDataHelper.CreateFeedItem(1);
            var result = await SaveFeedItemAsync(feedItem);
            if (feedItem.Id != null)
            {
                var episodeList = SeedDataHelper.CreateRssEpisodeForFeedItem(feedItem.Id.Value);
                var result2 = await SaveEpisodeListAsync(episodeList);
            }
            return 0;
        }
    }
}