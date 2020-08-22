using System;
using System.Threading.Tasks;
using Android.Util;
using Java.IO;
using Java.Net;

namespace Thoth.Droid.Services
{
    public class DownloadHelper
	{
		private bool NeedsDownload (string filePath)
		{
			return !System.IO.File.Exists(filePath);
		}

		public Task<string> DownloadFileAsync (string url, string filePath)
		{
			if (NeedsDownload(filePath)) {
				return Task.Run (() => DownloadFile (url, filePath));
			} else {
				return Task.FromResult(filePath);
			}
		}

		private string DownloadFile (string sUrl, string filePath)
		{
            try
            {
				//get result from uri
				URL url = new Java.Net.URL(sUrl);
				URLConnection connection = url.OpenConnection();
				connection.Connect();

				// this will be useful so that you can show a tipical 0-100%
				// progress bar
				int lengthOfFile = connection.ContentLength;

				// download the file
				InputStream input = new BufferedInputStream(url.OpenStream(), 8192);

				// Output stream
				Log.WriteLine(LogPriority.Info, "DownloadFile FilePath ", filePath);
				OutputStream output = new FileOutputStream(filePath);

				byte[] data = new byte[1024];

				long total = 0;

				int count;
				while ((count = input.Read(data)) != -1)
				{
					total += count;
					// publishing the progress....
					// After this onProgressUpdate will be called
					//publishProgress("" + (int)((total * 100) / lengthOfFile));
					if (total % 10 == 10) //log for every 10th increment
						Log.WriteLine(LogPriority.Info, "DownloadFile Progress ", "" + (int)((total * 100) / lengthOfFile));

					// writing data to file
					output.Write(data, 0, count);
				}

				// flushing output
				output.Flush();

				// closing streams
				output.Close();
				input.Close();
			}
			catch (Exception ex)
			{
				Log.WriteLine(LogPriority.Error, "DownloadFile Error", ex.Message);
			}
			return filePath;
		}
	}
}