using System;
using SQLite;

namespace Thoth.Models
{
    public class FeedItem
    {
        //***************************************** Relational Elements *******************************
        /// <summary>
        /// Gets the id of the feed.
        /// </summary>
        [PrimaryKey]
        public int? Id { get { return _id; } set { _id = value; } }
        private int? _id;

        //***************************************** RSS Elements **************************************
        /// <summary>
        /// Gets the name of the feed.
        /// </summary>
        public string Text { get { return _text; } set { _text = value; } }
        private string _text;

        /// <summary>
        /// Gets the description of the feed.
        /// </summary>
        public string Description { get { return _description; } set { _description = value; } }
        private string _description;

        /// <summary>
        /// Gets the URI of the feed.
        /// </summary>
        public string Link { get { return _link; } set { _link = value; } }
        private string _link;

        /// <summary>
        /// Gets the imgae URI of the episode.
        /// </summary>
        public string ImageLink { get { return _imageLink; } set { _imageLink = value; } }
        private string _imageLink;

        //***************************** Operational Items for Binding to the UI *********************************
        /// <summary>
        /// Cover Art from most recent Podcast
        /// </summary>
        public string ImageFileName { get { return _imageFileName; } set { _imageFileName = value; } }
        private string _imageFileName;

        /// <summary>
        /// Last time the feed was checked
        /// </summary>
        public DateTime LastCheck { get { return _lastCheck; } set { _lastCheck = value; } }
        private DateTime _lastCheck;
    }
}