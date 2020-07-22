namespace Thoth.Messages
{
    public class DownloadFinishedMessage
    {
        public int Id { get; set; }

        public string Url { get; set; }

        public string FilePath { get; set; }
    }
}