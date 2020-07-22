using System;
using System.Runtime.Serialization;
using SQLite;
using Microsoft.Toolkit.Parsers.Rss;
using System.Linq;

namespace Thoth.Models
{
    public class RssEpisode
    {
        //***************************************** Relational Elements **************************************
        /// <summary>
        /// Gets the id of the episode.
        /// </summary>
        [PrimaryKey]
        public int? Id { get { return _id; } set { _id = value; } }
        private int? _id;

        /// <summary>
        /// Gets the id of the feed.
        /// </summary>
        public int FeedItemId { get { return _feedItemId; } set { _feedItemId = value; } }
        private int _feedItemId;

        //***************************************** RSS Elements **************************************
        /// <summary>
        /// Gets the id of the feed.
        /// </summary>
        public string Author { get { return _author; } set { _author = value; } }
        private string _author;

        /// <summary>
        /// Gets the id of the feed.
        /// </summary>
        public string Title { get { return _title; } set { _title = value; } }
        private string _title;

        /// <summary>
        /// Gets the id of the feed.
        /// </summary>
        public string Description { get { return _description; } set { _description = value; } }
        private string _description;

        /// <summary>
        /// Gets the id of the feed.
        /// </summary>
        public string LinkUrl { get { return _linkUrl; } set { _linkUrl = value; } }
        private string _linkUrl;

        /// <summary>
        /// Gets the id of the feed.
        /// </summary>
        public string ImageLink { get { return _imageLink; } set { _imageLink = value; } }
        private string _imageLink;

        /// <summary>
        /// Gets the id of the feed.
        /// </summary>
        public string EnclosureLink { get { return _enclosureLink; } set { _enclosureLink = value; } }
        private string _enclosureLink;

        /// <summary>
        /// Gets the id of the feed.
        /// </summary>
        public DateTime PubDate { get { return _pubDate; } set { _pubDate = value; } }
        private DateTime _pubDate;

        //***************************** Operational Items for Binding to the UI *********************************
        /// <summary>
        /// Gets if the episode is downloaded
        /// </summary>
        /// 0 - not downloaded
        /// 1 - downloaded
        /// 2 - downloading
        /// 3 - queued
        public IsDownloadedEnum IsDownloaded { get { return _isdownloaded; } set { _isdownloaded = value; } }
        private IsDownloadedEnum _isdownloaded;

        /// <summary>
        /// Play/Pause/Download Icon for View
        /// </summary>
        public string PlayPauseDownloadIcon { get { return _playPauseDownloadIcon; } set { _playPauseDownloadIcon = value; } }
        private string _playPauseDownloadIcon;

        /// <summary>
        /// Current Position, for resuming
        /// </summary>
        public double CurrentPosition { get { return _currentPosition; } set { _currentPosition = value; } }
        private double _currentPosition;

        /// <summary>
        /// Duration
        /// </summary>
        public double Duration { get { return _duration; } set { _duration = value; } }
        private double _duration;

        /// <summary>
        /// There should only be one IsPlaying of all Episodes of all Feeds
        /// </summary>
        /// 0 - not playing, not started
        /// 1 - is playing
        /// 2 - not playing, started
        /// 3 - not playing, finished
        public IsPlayingEnum IsPlaying { get { return _isPlaying; } set { _isPlaying = value; } }
        private IsPlayingEnum _isPlaying;

        /// <summary>
        /// Podcast File Size
        /// </summary>
        public double PodcastFileSize { get { return _podcastFileSize; } set { _podcastFileSize = value; } }
        private double _podcastFileSize;

        /// <summary>
        /// Podcast FileName, used to put on other paths
        /// </summary>
        public string PodcastFileName { get { return _podcastFileName; } set { _podcastFileName = value; } }
        private string _podcastFileName;

        /// <summary>
        /// Cover Art for the Episode
        /// </summary>
        public string ImageFileName { get { return _imageFileName; } set { _imageFileName = value; } }
        private string _imageFileName;
    }
}