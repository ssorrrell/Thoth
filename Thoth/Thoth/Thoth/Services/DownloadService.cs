using System;

using Thoth.Models;
using Thoth.Messages;
using Thoth.Helpers;
using Thoth.Common;

using Xamarin.Forms;
using System.Diagnostics;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Toolkit.Parsers.Markdown.Blocks;

namespace Thoth.Services
{
    enum DownloadStatus
    {
        NotStarted,
        IsDownloading,
        DownloadFinished,
    }

    public class DownloadService
    {
        private static readonly Lazy<DownloadService> //lazy implementor
            lazy =
            new Lazy<DownloadService>
                (() => new DownloadService());

        public static DownloadService Instance { get { return lazy.Value; } }

        private IDataStore DataStore => DependencyService.Get<IDataStore>();

        private Queue<QueueItem> _downloadQueue = new Queue<QueueItem>();
        private DownloadStatus _downloadStatus = DownloadStatus.NotStarted;

        public DownloadService()
        {
            SubscribeToMessages();
        }

        private void SubscribeToMessages()
        {
            MessagingCenter.Subscribe<DownloadFinishedMessage>(this, "DownloadFinishedMessage", message =>
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await DownloadFinishedAsync(message);
                });
            });
        }

        //************************************** Download Functions ******************************************
        public async Task<bool> DownloadPodcast(RssEpisode episode)
        {   //no need to be async, check some things then fire MessageCenter to background task
            var result = false;
            try
            {
                if (CanDownloadPodcast(episode))
                {
                    var foundItem = FetchQueueItem(episode, QueueType.PodcastFile);
                    if (foundItem == null)
                    {
                        episode.PlayPauseDownloadIcon = IconFont.CircleArrowHistory;
                        //save download status to the database
                        var resultSave = await DataStore.SaveEpisodeItemAsync(episode); //returns the number of items changed
                        if (resultSave == 1)
                        {
                            var finishedMessage2 = new UpdateEpisodeMessage
                            {
                                RssEpisode = episode
                            };
                            MessagingCenter.Send(finishedMessage2, "UpdateEpisodeMessage"); //goes to listening ViewModels that can download
                        }
                        else
                        {
                            Debug.WriteLine("DownloadService.DownloadPodcastAsync Could not Update episode");
                        }
                        //queue download
                        var queueItem = new QueueItem();
                        queueItem.ID = _downloadQueue.Count;
                        queueItem.ItemType = QueueType.PodcastFile;
                        queueItem.QueueDataObject = episode;
                        _downloadQueue.Enqueue(queueItem);
                        //start downloader
                        await StartDownloadAsync();
                        result = true;
                    }
                    else
                    {
                        Debug.WriteLine("DownloadService.DownloadPodcast episode was found in the _downloadQueue already");
                    }
                }
                else
                {
                    Debug.WriteLine("DownloadService.DownloadPodcast CanDownload(episode) was false");
                }
            }
            catch (Exception ex)
            {
                //could not delete item
                Debug.WriteLine("DownloadService.DownloadPodcast Could not Download Podcast File " + ex.Message);
            }
            return result;
        }

        public async Task<bool> DownloadImageAsync(RssEpisode episode)
        {   //no need to be async, check some things then fire MessageCenter to background task
            var result = false;
            try
            {
                if (CanDownloadImage(episode))
                {
                    if (!string.IsNullOrEmpty(episode.ImageFileName))
                    {
                        //queue download
                        var queueItem = new QueueItem();
                        queueItem.ID = _downloadQueue.Count;
                        queueItem.ItemType = QueueType.ImageFile;
                        queueItem.QueueDataObject = episode;
                        _downloadQueue.Enqueue(queueItem);
                        //start downloader
                        await StartDownloadAsync();
                        result = true;
                    }
                    else
                    {
                        Debug.WriteLine("DownloadService.DownloadImage episode == Downloaded");
                    }
                }
                else
                {
                    Debug.WriteLine("DownloadService.DownloadImage CanDownload(episode) was false");
                }
            }
            catch (Exception ex)
            {
                //could not delete item
                Debug.WriteLine("DownloadService.DownloadImage Could not Download Podcast File " + ex.Message);
            }
            return result;
        }

        public async Task<bool> DownloadFeed(RssEpisode episode)
        {   //no need to be async, check some things then fire MessageCenter to background task
            var result = false;
            try
            {
                if (CanDownloadFeed(episode))
                {
                    if (!string.IsNullOrEmpty(episode.ImageFileName))
                    {
                        //queue download
                        var queueItem = new QueueItem();
                        queueItem.ID = _downloadQueue.Count;
                        queueItem.ItemType = QueueType.RssFeed;
                        queueItem.QueueDataObject = episode;
                        _downloadQueue.Enqueue(queueItem);
                        //start downloader
                        await StartDownloadAsync();
                        result = true;
                    }
                    else
                    {
                        Debug.WriteLine("DownloadService.DownloadFeed episode == Downloaded");
                    }
                }
                else
                {
                    Debug.WriteLine("DownloadService.DownloadFeed CanDownload(episode) was false");
                }
            }
            catch (Exception ex)
            {
                //could not download item
                Debug.WriteLine("DownloadService.DownloadFeed Could not Download Podcast File " + ex.Message);
            }
            return result;
        }

        private QueueItem FetchQueueItem(RssEpisode episode, QueueType queueType)
        {
            //var foundItem = _downloadQueue.Select(x => (x.QueueDataObject == episode) && (x.ItemType == queueType)).First();
            QueueItem foundItem = null;
            if (_downloadQueue.Count > 0)
            {
                var foundItems = _downloadQueue.Where(x => (((RssEpisode)x.QueueDataObject).Id == episode.Id) && (x.ItemType == queueType)).ToList();
                if (foundItems.Count() > 0)
                    foundItem = foundItems[0];
            }
            return foundItem;
        }

        private async Task DownloadFinishedAsync(DownloadFinishedMessage message)
        {
            _downloadStatus = DownloadStatus.DownloadFinished;
            var stopMessage = new StopLongRunningTaskMessage();
            MessagingCenter.Send(stopMessage, "StopLongRunningTaskMessage"); //stop downloads

            var isSuccessful = System.IO.File.Exists(message.FilePath);
            if (message.Id > 0 && isSuccessful)
            {
                switch (message.QueueType)
                {
                    case QueueType.PodcastFile:
                        await PodcastFileCompletedAsync(message);
                        break;
                    case QueueType.ImageFile:
                        await ImageFileCompletedAsync(message);
                        break;
                    case QueueType.RssFeed:
                        await RssFeedFileCompletedAsync(message);
                        break;
                }
            }
            else
            {
                Debug.WriteLine("DownloadService.DownloadFinishedAsync message.Id <= 0 or !isSuccessful");
            }
        }

        private async Task StartDownloadAsync()
        {
            if (_downloadStatus == DownloadStatus.NotStarted && _downloadQueue.Count > 0)
            {
                _downloadStatus = DownloadStatus.IsDownloading;
                try
                {
                    var queueItem = _downloadQueue.Dequeue();
                    //download the file to storage
                    var startMessage = new StartLongRunningTaskMessage(); //goes to Android project Service, returns thru DownloadFinishedMessage
                    MessagingCenter.Send(startMessage, "StartLongRunningTaskMessage"); //start background downloads
                    switch (queueItem.ItemType)
                    {
                        case QueueType.PodcastFile:
                            var rssEpisode = (RssEpisode)queueItem.QueueDataObject;
                            await DownloadPodcastFileAsync(rssEpisode);
                            break;
                        case QueueType.ImageFile:
                            var fileEpisode = (RssEpisode)queueItem.QueueDataObject;
                            DownloadImageFile(fileEpisode);
                            break;
                        case QueueType.RssFeed:
                            var feedEpisode = (RssEpisode)queueItem.QueueDataObject;
                            DownloadFeedFile(feedEpisode);
                            break;
                    }
                }
                catch(Exception ex)
                {
                    Debug.WriteLine("DownloadService.StartDownload Could not Start Downloader " + ex.Message);
                }
            }
        }

        private async Task DownloadPodcastFileAsync(RssEpisode episode)
        {
            try
            {
                episode.PlayPauseDownloadIcon = IconFont.Clock;
                //save download status to the database
                var resultSave = await DataStore.SaveEpisodeItemAsync(episode); //returns the number of items changed
                if (resultSave == 1)
                {
                    var finishedMessage2 = new UpdateEpisodeMessage
                    {
                        RssEpisode = episode
                    };
                    MessagingCenter.Send(finishedMessage2, "UpdateEpisodeMessage"); //goes to listening ViewModels that can download
                }
                else
                {
                    Debug.WriteLine("DownloadService.DownloadPodcastFile Could not Update episode");
                }
                //download the file to storage
                var filePath = FileHelper.GetPodcastPath(episode.PodcastFileName);
                var message = new DownloadMessage
                {
                    Id = episode.Id.Value, //needed to update RssEpisode when done
                    Url = episode.EnclosureLink,
                    FilePath = filePath,
                    QueueType = QueueType.PodcastFile
                };
                MessagingCenter.Send(message, "Download"); //goes to Android project Service, returns thru DownloadFinishedMessage
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DownloadService.DownloadPodcastFile Error " + ex.Message);
            }
        }

        private void DownloadImageFile(RssEpisode episode)
        {
            try
            {
                //download the file to storage
                var filePath = FileHelper.GetImagePath(episode.ImageLink);
                var message = new DownloadMessage
                {
                    Id = episode.Id.Value, //needed to update RssEpisode when done
                    Url = episode.ImageLink,
                    FilePath = filePath,
                    QueueType = QueueType.ImageFile
                };
                MessagingCenter.Send(message, "Download"); //goes to Android project Service, returns thru DownloadFinishedMessage
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DownloadService.DownloadImageFile Error " + ex.Message);
            }
        }

        //this is not setup correctly yet
        private void DownloadFeedFile(RssEpisode episode)
        {
            try
            {
                //download the file to storage
                var filePath = FileHelper.GetImagePath(episode.ImageLink);
                var message = new DownloadMessage
                {
                    Id = episode.Id.Value, //needed to update RssEpisode when done
                    Url = episode.ImageLink,
                    FilePath = filePath,
                    QueueType = QueueType.RssFeed
                };
                MessagingCenter.Send(message, "Download"); //goes to Android project Service, returns thru DownloadFinishedMessage
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DownloadService.DownloadFeedFile Error " + ex.Message);
            }
        }

        private async Task PodcastFileCompletedAsync(DownloadFinishedMessage message)
        {
            try
            { 
                //refetch rssEpisode in case it's different from the currently viewed one
                var episode = await DataStore.GetEpisodeItemByIdAsync(message.Id);
                if (episode != null)
                {
                    episode.IsPlaying = IsPlayingEnum.NotStarted;
                    episode.IsDownloaded = IsDownloadedEnum.Downloaded;
                    episode.PlayPauseDownloadIcon = IconFont.PlayArrow;
                    RssEpisodeManager.UpdateRssEpisodeWithFileInfo(ref episode);
                    //save download status to the database
                    var resultSave = await DataStore.SaveEpisodeItemAsync(episode); //returns the number of items changed
                    if (resultSave == 1)
                    {
                        var finishedMessage2 = new UpdateEpisodeMessage
                        {
                            RssEpisode = episode
                        };
                        MessagingCenter.Send(finishedMessage2, "UpdateEpisodeMessage"); //goes to listening ViewModels that can download
                    }
                    else
                    {
                        Debug.WriteLine("DownloadService.PodcastFileCompletedAsync Could not Update episode");
                    }
                }
                else
                {
                    Debug.WriteLine("DownloadService.PodcastFileCompletedAsync episode was null from GetEpisodeItemByIdAsync");
                }
                _downloadStatus = DownloadStatus.NotStarted;
                await StartDownloadAsync(); //start next download
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DownloadService.PodcastFileCompletedAsync Error " + ex.Message);
            }
        }

        private async Task ImageFileCompletedAsync(DownloadFinishedMessage message)
        {
            try
            {
                //nothing in particular to do
                _downloadStatus = DownloadStatus.NotStarted;
                await StartDownloadAsync(); //start next download
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DownloadService.ImageFileCompletedAsync Error " + ex.Message);
            }
        }

        private async Task RssFeedFileCompletedAsync(DownloadFinishedMessage message)
        {
            try
            {
                //nothing in particular to do
                _downloadStatus = DownloadStatus.NotStarted;
                await StartDownloadAsync(); //start next download
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DownloadService.RssFeedFileCompletedAsync Error " + ex.Message);
            }
        }

        //************************************** Static Utility Functions ********************************************
        public static bool CanDownloadPodcast(RssEpisode episode)
        {
            var result = false;
            try
            {
                if (episode.Id != null)
                {
                    if (episode.IsDownloaded != IsDownloadedEnum.Downloaded)
                    {
                        var filePath = FileHelper.GetPodcastPath(episode.PodcastFileName);
                        var fileExists = File.Exists(filePath);
                        if (!fileExists)
                            result = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DownloadService.CanDownloadPodcast failed " + ex.Message);
            }
            return result;
        }

        public static bool CanDownloadImage(RssEpisode episode)
        {
            var result = false;
            try
            {
                var filePath = FileHelper.GetImagePath(episode.ImageFileName);
                var fileExists = File.Exists(filePath);
                if (!fileExists)
                    result = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DownloadService.CanDownloadImage failed " + ex.Message);
            }
            return result;
        }

        public static bool CanDownloadFeed(RssEpisode episode)
        {
            var result = false;
            /*try
            {
                if (episode.Id != null)
                {
                    var filePath = FileHelper.GetImagePath(episode.ImageFileName);
                    var fileExists = File.Exists(filePath);
                    if (!fileExists)
                        result = true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DownloadService.CanDownloadFeed failed " + ex.Message);
            }*/
            result = true;
            return result;
        }
    }
}
