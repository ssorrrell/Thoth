using System.Threading.Tasks;

using Android.App;
using Android.Content;
using Android.OS;

using Xamarin.Forms;

using Thoth.Messages;
using Thoth.Models;

namespace Thoth.Droid.Services
{
    [Service]
	public class DownloaderService : Service
	{
		public override IBinder OnBind(Intent intent)
		{
			return null;
		}

		public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
		{
			var id = intent.GetIntExtra("id", 0);
			var url = intent.GetStringExtra("url");
			var filePath = intent.GetStringExtra("filePath");
			var iQueueType = intent.GetIntExtra("queueType", 0);

			QueueType queueType = ConvertIntToQueueType(iQueueType);

			Task.Run(() => {
				var imageHelper = new DownloadHelper();
				imageHelper.DownloadFileAsync(url, filePath)
						.ContinueWith(fp => {
							var message = new DownloadFinishedMessage
							{
								Id = id,
								Url = url,
								FilePath = fp.Result,
								QueueType = queueType
							};
							MessagingCenter.Send(message, "DownloadFinishedMessage");
						});
			});

			return StartCommandResult.Sticky;
		}

		private QueueType ConvertIntToQueueType(int i)
        {
			QueueType queueType;
			switch (i)
			{
				default:
					queueType = QueueType.PodcastFile;
					break;
				case 1:
					queueType = QueueType.ImageFile;
					break;
				case 2:
					queueType = QueueType.RssFeed;
					break;
			}
			return queueType;
		}
	}
}