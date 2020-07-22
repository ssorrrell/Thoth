using System;
using System.Collections.Generic;
using System.Text;

namespace Thoth.Models
{
    public enum IsDownloadedEnum
    {
        NotDownloaded, Downloaded, Downloading, Queued
    }

    public enum IsPlayingEnum
    {
        NotStarted, IsPlaying, Started, Finished
    }
}
