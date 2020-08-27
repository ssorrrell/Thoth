using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Thoth.Services;

using Xamarin.Forms;
using Thoth.Messages;

namespace Thoth.Models
{
    public class RssEpisodeManager
    {
        private IDataStore DataStore => DependencyService.Get<IDataStore>();

        //******************************** Static Functions **************************************
        public static List<RssEpisode> GetRssEpisodesFromXDoc(XDocument doc, int feedItemId)
        {   //used in AddFeedItem and RefreshEpisodes
            var episodes = new List<RssEpisode>();
            try
            {
                foreach (var channel in doc.Descendants("channel"))
                    foreach (var item in channel.Descendants("item"))
                    {
                        var imageLink = RssFeedHelper.GetITunesImageUriFromXElement(item);
                        var imageFileName = Path.GetFileName(imageLink);
                        var podcastLink = RssFeedHelper.GetEnclosureUriFromXElement(item.Element("enclosure"));
                        var podcastFileName = Path.GetFileName(podcastLink);
                        var episode = new RssEpisode
                        {
                            FeedItemId = feedItemId,
                            Author = RssFeedHelper.ConvertXElementToString(item.Element("author")),
                            Description = RssFeedHelper.ConvertXElementToString(item.Element("description")),
                            PubDate = RssFeedHelper.ConvertXElementToDateTime(item.Element("pubDate")),
                            Title = RssFeedHelper.ConvertXElementToString(item.Element("title")),
                            LinkUrl = RssFeedHelper.ConvertXElementToString(item.Element("link")),
                            ImageLink = imageLink,
                            ImageFileName = imageFileName,
                            EnclosureLink = podcastLink,
                            PodcastFileName = podcastFileName,
                            IsDownloaded = IsDownloadedEnum.NotDownloaded,
                            CurrentPosition = 0,
                            Duration = 0, //reset to correct size when downloaded
                            PodcastFileSize = 0, //reset to correct size when downloaded
                            IsPlaying = IsPlayingEnum.NotStarted,
                            PlayPauseDownloadIcon = IconFont.FileDownload
                        };
                        episodes.Add(episode);
                    }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("GetRssEpisodesFromXDoc Error " + ex.Message);
            }
            return episodes;
        }

        public static DateTime GetMostRecentPubDate(List<RssEpisode> episodes)
        {
            var mostRecentEpisode = episodes.OrderByDescending(e => e.PubDate).FirstOrDefault();
            var pubDate = mostRecentEpisode.PubDate;
            return pubDate;
        }

        public static void UpdateRssEpisodeWithFileInfo(ref RssEpisode episode)
        {
            episode.PodcastFileName = Path.GetFileName(episode.EnclosureLink);
            var filePath = FileHelper.GetPodcastPath(episode.PodcastFileName);
            System.IO.FileInfo fileInfo = new System.IO.FileInfo(filePath);
            if (fileInfo != null)
                episode.PodcastFileSize = fileInfo.Length;
        }

        public static void SetEpisodeToPlaying(ref RssEpisode episode)
        {
            episode.PlayPauseDownloadIcon = IconFont.Pause;
            episode.IsPlaying = IsPlayingEnum.IsPlaying;
        }

        public static void SetEpisodeToPause(ref RssEpisode episode)
        {
            episode.PlayPauseDownloadIcon = IconFont.PlayArrow;
            episode.IsPlaying = IsPlayingEnum.Started;
        }

        public static void SetEpisodeToFinished(ref RssEpisode episode)
        {
            episode.PlayPauseDownloadIcon = IconFont.Checked;
            episode.IsPlaying = IsPlayingEnum.Finished;
        }

        //******************************** Regular Functions **************************************
        public async Task<RssEpisode> DeletePodcastFile(RssEpisode episode)
        {   //Takes in and episode, modifies it, saves it, and returns the episode
            var returnEpisode = episode;
            try
            {
                if (episode.IsDownloaded == IsDownloadedEnum.Downloaded)
                {
                    //delete podcast file
                    var podcastFilePath = FileHelper.GetPodcastPath(episode.PodcastFileName);
                    var result = FileHelper.DeletePodcastFile(podcastFilePath);
                    if (result)
                    {
                        episode.IsDownloaded = IsDownloadedEnum.NotDownloaded;
                        episode.PlayPauseDownloadIcon = IconFont.FileDownload;
                        episode.PodcastFileSize = 0;
                        episode.Duration = 0;
                        episode.CurrentPosition = 0;
                        //save download status to the database
                        var resultSave = await DataStore.SaveEpisodeItemAsync(episode); //returns the number of items changed
                        if (resultSave == 0 )
                        {
                            Debug.WriteLine("RssEpisodeManager.DeletePodcastFile Could not Update episode");
                        }
                    }
                    else
                    {
                        Debug.WriteLine("RssEpisodeManager.DeletePodcastFile Could not Delete Podcast File");
                    }
                    returnEpisode = episode;
                }
            }
            catch (Exception ex)
            {
                //could not delete item
                Debug.WriteLine("RssEpisodeManager.DeletePodcastFile Could not Delete Podcast File " + ex.Message);
            }
            return returnEpisode;
        }

        public async Task<bool> DeleteAllEpisodes(FeedItem feedItem)
        {
            var returnResult = false;
            try
            {
                //delete Files for Episodes
                var episodeList = await DataStore.GetAllEpisodeItemsByFeedIdAsync(feedItem.Id.Value);
                foreach (var episode in episodeList)
                {
                    //delete podcast file
                    var podcastFilePath = FileHelper.GetPodcastPath(episode.PodcastFileName);
                    if (episode.IsDownloaded == IsDownloadedEnum.Downloaded)
                    {
                        var podcastDeleteResult = FileHelper.DeletePodcastFile(podcastFilePath);
                        if (!podcastDeleteResult)
                            Debug.WriteLine("RssEpisodeManager.DeleteAllEpisodes Could not Delete Podcast File for " + episode.Id);
                    }
                    //delete image file
                    var imageFilePath = FileHelper.GetImagePath(episode.ImageFileName);
                    var fileDeleteResult = FileHelper.DeleteImageFile(imageFilePath);
                    if (!fileDeleteResult)
                        Debug.WriteLine("RssEpisodeManager.DeleteAllEpisodes Could not Delete Image File for " + episode.Id);
                }
                //delete episodes in FeedItem
                var deleteEpisodeResult = await DataStore.DeleteAllEpisodesByFeedIdAsync(feedItem.Id);
                if (deleteEpisodeResult > 0)
                    Debug.WriteLine("RssEpisodeManager.DeleteAllEpisodes Could not Delete Episodes by Feed Id " + feedItem.Id);
                returnResult = true;
            }
            catch (Exception ex)
            {
                //could not delete item
                Debug.WriteLine("RssEpisodeManager.DeleteAllEpisodes Could not Delete Feed Item " + ex.Message);
            }
            return returnResult;
        }

        public async Task<RssEpisode> PlayEpisodeAsync(RssEpisode episode)
        {
            try
            {
                if (episode != null && episode.Id != null)
                {
                    if (PodcastPlayer.Instance.IsPlaying)
                    {   //save the current play position of the other episode then start playing this episode
                        //pause other episode
                        var playerEpisode = PodcastPlayer.Instance.Episode; //if player is playing this should never be null
                        await PauseEpisodeAsync(playerEpisode); //pause the other episode
                        //play this episode
                        await PlayEpisodeAsync(episode);
                    }
                    else
                    {
                        //update icon on item - updates screen
                        SetEpisodeToPlaying(ref episode);
                        //broadcast to other viewmodels
                        var startMessage = new DownloadFinishedMessage2 { RssEpisode = episode };
                        MessagingCenter.Send(startMessage, "StartEpisodePlaying");
                        //save episode - updates db
                        var result = await DataStore.SaveEpisodeItemAsync(episode);
                        if (result == 0)
                            Debug.WriteLine("RssEpisodeListViewModel.ExecutePlayCommandAsync Could Not update RssEpisode with start playing");
                        //player load episode
                        PodcastPlayer.Instance.Episode = episode;
                        //update current position
                        if (episode.CurrentPosition > 0)
                            PodcastPlayer.Instance.CurrentPosition = episode.CurrentPosition;
                        //player play
                        PodcastPlayer.Instance.PlayPause();
                    }
                }
                else
                {
                    Debug.WriteLine("RssEpisodeListViewModel.ExecutePlayCommandAsync episode or episode id was null ");
                }
            }
            catch (Exception ex)
            {
                //could not delete item
                Debug.WriteLine("RssEpisodeListViewModel.ExecutePlayCommandAsync Could not Play Podcast File " + ex.Message);
            }
            return episode;
        }

        public async Task<RssEpisode> PauseEpisodeAsync(RssEpisode episode)
        {
            RssEpisode returnEpisode = null;
            try
            {
                if (PodcastPlayer.Instance.IsPlaying)
                {
                    //player pause
                    PodcastPlayer.Instance.PlayPause();
                    //get episode to update
                    episode = PodcastPlayer.Instance.Episode;
                    //update icon on item - updates screen
                    SetEpisodeToPause(ref episode);
                    //broadcast to other viewmodels
                    var pauseMessage = new DownloadFinishedMessage2 { RssEpisode = episode };
                    MessagingCenter.Send(pauseMessage, "StopEpisodePlaying");
                    //update current position
                    episode.CurrentPosition = PodcastPlayer.Instance.CurrentPosition;
                    //save episode - updates db
                    var result = await DataStore.SaveEpisodeItemAsync(episode);
                    if (result == 0)
                        Debug.WriteLine("RssEpisodeListViewModel.ExecutePauseCommandAsync Could Not update RssEpisode with start playing");
                    returnEpisode = episode;
                }
                else
                {
                    Debug.WriteLine("RssEpisodeListViewModel.ExecutePauseCommandAsync player is not playing ");
                    //get episode to update
                    var playerEpisode = PodcastPlayer.Instance.Episode;
                    if (playerEpisode == null)
                        playerEpisode = episode;
                    //update icon on item - updates screen
                    SetEpisodeToPause(ref playerEpisode);
                    //save episode - updates db
                    var result = await DataStore.SaveEpisodeItemAsync(playerEpisode);
                    if (result == 0)
                        Debug.WriteLine("RssEpisodeListViewModel.ExecutePauseCommandAsync Could Not update RssEpisode with start playing");
                    returnEpisode = playerEpisode;
                }
            }
            catch (Exception ex)
            {
                //could not delete item
                Debug.WriteLine("RssEpisodeListViewModel.ExecutePauseCommandAsync Could not Pause Podcast File " + ex.Message);
            }
            return returnEpisode;
        }

        public async Task<RssEpisode> FinishedEpisodeAsync(RssEpisode episode)
        {
            RssEpisode returnEpisode = null;
            try
            {
                //get episode to update
                var playerEpisode = PodcastPlayer.Instance.Episode;
                if (playerEpisode == null)
                    playerEpisode = episode;
                //update icon on item - updates screen
                if (playerEpisode.IsPlaying != IsPlayingEnum.Finished) //episode is already updated, skip this step
                {   
                    SetEpisodeToFinished(ref playerEpisode);
                    //save episode - updates db
                    var result = await DataStore.SaveEpisodeItemAsync(playerEpisode);
                    if (result == 0)
                        Debug.WriteLine("RssEpisodeListViewModel.FinishedEpisodeAsync Could Not update RssEpisode with Finished");
                }
                returnEpisode = playerEpisode;
            }
            catch (Exception ex)
            {
                //could not delete item
                Debug.WriteLine("RssEpisodeListViewModel.FinishedEpisodeAsync Could not Finish Podcast File " + ex.Message);
            }
            return returnEpisode;
        }
    }
}
