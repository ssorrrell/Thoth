namespace Thoth.Messages
{
	public class DownloadMessage
	{
		public int Id { get; set; }
		public string Url { get; set; }
		public string FilePath { get; set; }
	}
}