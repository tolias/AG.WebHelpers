using System.IO;
using System.Net;
using System.Text;

namespace AG.WebHelpers.Requesting
{
    public class HttpRequests
    {
        public static string GetResponseFromUrl(string url, int timeout = 30)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            //request.UserAgent = Headers.UserAgent;
            //request.Connection = Headers.Connection;
            //request.Accept = Headers.Accept;
            //request.Referer = referer;
            //request.CookieContainer = cookie;

            string content;

            using (WebResponse response = request.GetResponse())
            {
                using (Stream receiveStream = response.GetResponseStream())
                {
                    receiveStream.ReadTimeout = timeout * 1000;
                    receiveStream.WriteTimeout = timeout * 1000;
                    using (StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8))
                    {
                        content = readStream.ReadToEnd();
                        readStream.Close();
                    }
                }
                response.Close();
            }
            return content;
        }
    }
}
