using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using HtmlAgilityPack;
using Thoth.Models;
using Xamarin.Forms;
using Xamarin.Essentials;
using System.Diagnostics;
using System.Net;

namespace Thoth.Services
{
    public class RssFeedHelper
    {
        public async static Task<XDocument> GetXDocFromLinkAsync(string uri, bool ignoreCertErrors = false)
        {
            string feed = null;

            //get result from uri
            XDocument doc = null;
            try
            {
                if (ignoreCertErrors)
                    ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true; //for SSL cert errors
                using (var client = new HttpClient())
                {
                    feed = await client.GetStringAsync(uri);
                }
                if (feed == null) return null;
                doc = XDocument.Load(uri);
            }
            catch(Exception ex)
            {
                Debug.WriteLine("RssFeedHelper.GetXDocFromLinkAsync Error getting XDocument from Uri " + ex.Message);
                if (!ignoreCertErrors)
                    doc = await GetXDocFromLinkAsync(uri, true);
            }
            return doc;
        }



        /*public async static Task<FeedItem> GetNewFeedItemFromLink(string uri)
        {   //not used
            var doc = await GetXDocFromLinkAsync(uri);
            var feedItem = GetFeedItemFromXDoc(doc, uri);

            var episodes = new List<RssEpisode>();
            foreach (var channel in doc.Descendants("channel"))
                foreach (var item in channel.Descendants("item"))
                {
                    var episode = new RssEpisode
                    {
                        Author = ConvertXElementToString(item.Element("author")),
                        Description = ConvertXElementToString(item.Element("description")), // GetInnerTextValue(ConvertXElementToString(item.Element("description"))),
                        PubDate = ConvertXElementToDateTime(item.Element("pubDate")),
                        Title = ConvertXElementToString(item.Element("title")),
                        LinkUrl = ConvertXElementToString(item.Element("link")),
                        ImageLink = GetImageUriFromXElement(item),
                        EnclosureLink = GetEnclosureUriFromXElement(item.Element("enclosure")),
                        IsDownloaded = false,
                        FeedItemId = feedItem.Id.Value
                    };
                    episodes.Add(episode);
            }

            return feedItem;
        }*/

        /*public async static Task<List<RssEpisode>> GetNewRssEpisodesFromFeedItem(FeedItem feedItem)
        {   //not used
            var episodes = new List<RssEpisode>();
            try
            {
                var uri = feedItem.Link;
                var doc = await GetXDocFromLinkAsync(uri);

                foreach (var channel in doc.Descendants("channel"))
                    foreach (var item in channel.Descendants("item"))
                    {
                        var episode = new RssEpisode
                        {
                            Author = ConvertXElementToString(item.Element("author")),
                            Description = ConvertXElementToString(item.Element("description")), // GetInnerTextValue(ConvertXElementToString(item.Element("description"))),
                            PubDate = ConvertXElementToDateTime(item.Element("pubDate")),
                            Title = ConvertXElementToString(item.Element("title")),
                            LinkUrl = ConvertXElementToString(item.Element("link")),
                            ImageLink = GetImageUriFromXElement(item),
                            EnclosureLink = GetEnclosureUriFromXElement(item.Element("enclosure")),
                            IsDownloaded = false,
                            FeedItemId = feedItem.Id.Value
                        };
                        episodes.Add(episode);
                    }
            }
            catch(Exception ex)
            {
                Debug.WriteLine("GetNewRssEpisodesFromFeedItem Error " + ex.Message);
            }

            return episodes;
        }*/


        /*public static async Task<bool> GetStreamAsync(string uri, string filePath)
        {
            //get result from uri
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    using (HttpResponseMessage response = await client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead))
                    using (Stream streamToReadFrom = await response.Content.ReadAsStreamAsync())
                    {
                        string fileToWriteTo = filePath;
                        using (Stream streamToWriteTo = File.Open(fileToWriteTo, FileMode.Create))
                        {
                            await streamToReadFrom.CopyToAsync(streamToWriteTo);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(string.Format("GetStreamAsync Failed : {0}", ex.Message));
                return false;
            }
            return true;
        }*/

        /*public static async Task<bool> GetPodcastFile(RssEpisode rssEpisode)
        {

            if (!string.IsNullOrEmpty(rssEpisode.EnclosureLink))
            {
                //get file path
                var fileName = Path.GetFileName(rssEpisode.EnclosureLink);
                var filePath = Path.Combine(Constants.BaseFilePath, fileName);

                //save mp3 file
                var result = await GetStreamAsync(rssEpisode.EnclosureLink, filePath);

                var success = File.Exists(filePath);

                //update RssEpisode
                rssEpisode.IsDownloaded = success;
            }
            return true;
        }*/

        /*public static async Task<bool> DownloadImageFile(string url)
        {
            var result = false;
            var filePath = GetImagePath(url);
            if (!File.Exists(filePath)) //don't redownload the same file
            {
                result = await GetStreamAsync(url, filePath);
            }
            return result;
        }

        public static string GetPodcastPath(string file)
        {
            var fileName = Path.GetFileName(file);
            var filePath = Path.Combine(Constants.BaseFilePath, fileName);
            return filePath;
        }

        public static string GetImagePath(string file)
        {
            var fileName = Path.GetFileName(file);
            var filePath = Path.Combine(Constants.BaseFilePath, fileName);
            return filePath;
        }

        public static bool DeletePodcastFile(string filePath)
        {
            var result = false;
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    result = true;
                }
            }
            catch( Exception ex)
            {
                //could not delete file
                Debug.WriteLine("Error could not delete Delete Podcast file" + ex.Message);
            }
            return result;
        }

        public static bool DeleteImageFile(string filePath)
        {
            var result = false;
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    result = true;
                }
            }
            catch (Exception ex)
            {
                //could not delete file
                Debug.WriteLine("Error could not delete Delete Image file" + ex.Message);
            }
            return result;
        }*/

        //*************************** XElement Functions ******************************************
        public static string ConvertXElementToString(XElement from)
        {
            string result = "";
            if (from != null)
                result = from.Value.ToString();
            return result;
        }
        public static string GetEnclosureUriFromXElement(XElement from)
        {
            string result = "";

            if (from != null)
            {
                var attributeList = from.Attributes();
                foreach(var attribute in attributeList)
                {
                    if (attribute.Name == "url")
                    {
                        result = attribute.Value;
                        break;
                    }
                }
            }
            return result;
        }
        public static string GetImageUriFromXElement(XElement from)
        {
            string result = "";

            if (from != null)
            {
                var elementList = from.Elements();
                foreach (var element in elementList)
                {
                    if (element.Name == "url")
                    {
                        result = element.Value;
                        break;
                    }
                }
            }
            return result;
        }
        public static string GetITunesImageUriFromXElement(XElement from)
        {
            string result = "";

            if (from != null)
            {
                XNamespace itunesNamespace = "http://www.itunes.com/dtds/podcast-1.0.dtd";
                var imageElement = from.Element(itunesNamespace + "image");
                if (imageElement != null)
                {
                    var attributeList = imageElement.Attributes();
                    foreach (var attribute in attributeList)
                    {
                        if (attribute.Name == "href")
                        {
                            result = attribute.Value;
                            break;
                        }
                    }
                }

            }
            return result;
        }
        public static DateTime ConvertXElementToDateTime(XElement from)
        {
            DateTime result = DateTime.MinValue;
            if (from != null)
                result = Convert.ToDateTime(from.Value);
            return result;
        }

        /*private static string GetInnerTextValue(string html)
        {
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(html);
            var innerText = document.DocumentNode.InnerText;
            return innerText;
        }*/
    }
}
