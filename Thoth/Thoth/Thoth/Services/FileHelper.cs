using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Net.Http;
using System.Diagnostics;

namespace Thoth.Services
{
    public class FileHelper
    {
        public static async Task<bool> DownloadImageFile(string url)
        {
            var result = false;
            var filePath = GetImagePath(url);
            if (!File.Exists(filePath)) //don't redownload the same file
            {
                result = await GetStreamAsync(url, filePath);
            }
            return result;
        }

        public static async Task<bool> GetStreamAsync(string uri, string filePath)
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
        }

        public static string GetPodcastPath(string file)
        {
            string filePath = null;
            if (!string.IsNullOrEmpty(file))
            {
                var fileName = Path.GetFileName(file);
                filePath = Path.Combine(Constants.BaseFilePath, fileName);
            }
            return filePath;
        }

        public static string GetImagePath(string file)
        {
            string filePath = null;
            if (!string.IsNullOrEmpty(file))
            {
                var fileName = Path.GetFileName(file);
                filePath = Path.Combine(Constants.BaseFilePath, fileName);
            }
            return filePath;
        }

        public static bool DeletePodcastFile(string filePath)
        {
            var result = false;
            try
            {
                if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                {
                    File.Delete(filePath);
                    result = true;
                }
            }
            catch (Exception ex)
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
                if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
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
        }

    }
}
