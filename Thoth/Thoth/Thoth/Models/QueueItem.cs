using System;
using System.Collections.Generic;
using System.Text;

namespace Thoth.Models
{
    public class QueueItem
    {
        public int ID;
        public QueueType ItemType;
        public object QueueDataObject;
    }

    public enum QueueType
    {
        PodcastFile,
        ImageFile,
        RssFeed
    }
}
