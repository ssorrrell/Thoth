using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
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
