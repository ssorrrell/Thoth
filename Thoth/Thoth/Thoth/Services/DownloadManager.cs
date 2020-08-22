using System;

using Thoth.Models;
using Thoth.Messages;

using Xamarin.Forms;
using System.Diagnostics;
using System.Threading.Tasks;
using System.IO;

namespace Thoth.Services
{
    public class DownloadManager
    {
        private static readonly Lazy<DownloadManager> //lazy implementor
            lazy =
            new Lazy<DownloadManager>
                (() => new DownloadManager());

        public static DownloadManager Instance { get { return lazy.Value; } }

        private IDataStore DataStore => DependencyService.Get<IDataStore>();
        private RssEpisodeManager RssEpisodeManager { get; set; } = new RssEpisodeManager();

        public DownloadManager()
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
        public bool Download(RssEpisode episode)
        {   //no need to be async, check some things then fire MessageCenter to background task
            var result = false;
            try
            {
                if (CanDownload(episode))
                {
                    if (episode.IsDownloaded != IsDownloadedEnum.Downloaded)
                    {
                        //download the file to storage
                        var startMessage = new StartLongRunningTaskMessage(); //goes to Android project Service, returns thru DownloadFinishedMessage
                        MessagingCenter.Send(startMessage, "StartLongRunningTaskMessage"); //start background downloads

                        var filePath = FileHelper.GetPodcastPath(episode.PodcastFileName);
                        var message = new DownloadMessage
                        {
                            Id = episode.Id.Value, //needed to update RssEpisode when done
                            Url = episode.EnclosureLink,
                            FilePath = filePath
                        };
                        MessagingCenter.Send(message, "Download"); //goes to Android project Service, returns thru DownloadFinishedMessage
                        result = true;
                    }
                    else
                    {
                        Debug.WriteLine("DownloadManager.Download episode == Downloaded");
                    }
                }
                else
                {
                    Debug.WriteLine("DownloadManager.Download CanDownload(episode) was false");
                }
            }
            catch (Exception ex)
            {
                //could not delete item
                Debug.WriteLine("DownloadManager.Download Could not Download Podcast File " + ex.Message);
            }
            return result;
        }

        private async Task DownloadFinishedAsync(DownloadFinishedMessage message)
        {
            var stopMessage = new StopLongRunningTaskMessage();
            MessagingCenter.Send(stopMessage, "StopLongRunningTaskMessage"); //stop downloads

            var isSuccessful = System.IO.File.Exists(message.FilePath);
            if (message.Id > 0 && isSuccessful)
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
                        var finishedMessage2 = new DownloadFinishedMessage2
                        {
                            RssEpisode = episode
                        };
                        MessagingCenter.Send(finishedMessage2, "DownloadFinishedMessage2"); //goes to listening ViewModels that can download
                    }
                    else
                    {
                        Debug.WriteLine("DownloadManager.DownloadFinishedAsync Could not Update episode");
                    }
                }
                else
                {
                    Debug.WriteLine("DownloadManager.DownloadFinishedAsync episode was null from GetEpisodeItemByIdAsync");
                }
            }
            else
            {
                Debug.WriteLine("DownloadManager.DownloadFinishedAsync message.Id <= 0 or !isSuccessful");
            }
        }

        //************************************** Static Utility Functions ********************************************
        public static bool CanDownload(RssEpisode episode)
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
                Debug.WriteLine("DownloadManager.CanDownload failed " + ex.Message);
            }
            return result;
        }


    }
}
