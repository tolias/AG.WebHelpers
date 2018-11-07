using System.IO;
using System.Net;
using System.Text;

namespace AG.WebHelpers.Requesting
{
    public class RawRequestProcessor
    {
        public CookieCollection Cookies;
        public bool UseCookies = true;

        public string Get(string requestUrl, string requestBody = null, string method = "GET", bool throwOnHttpErrorCode = true)
        {
            HttpWebRequest request = CreateRequest(requestUrl);
            request.Method = method;

            if (!string.IsNullOrEmpty(requestBody))
            {
                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    streamWriter.Write(requestBody);
                }
            }
            return GetResponse(request, requestUrl, throwOnHttpErrorCode);
        }
        
        protected virtual HttpWebRequest CreateRequest(string requestUri)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUri);
            if (UseCookies && Cookies != null && Cookies.Count > 0)
            {
                var cookieContainer = new CookieContainer();
                cookieContainer.Add(Cookies);
                request.CookieContainer = cookieContainer;
            }
            //request.UserAgent = UserAgent;
            //request.Connection = Headers.Connection;
            return request;
        }

        private string GetResponse(HttpWebRequest request, string requestUrl, bool throwOnHttpErrorCode)
        {
            string content;

            HttpWebResponse response = null;
            try
            {
                if (throwOnHttpErrorCode)
                {
                    response = (HttpWebResponse)request.GetResponse();
                }
                else
                {
                    try
                    {
                        response = (HttpWebResponse)request.GetResponse();
                    }
                    catch (WebException ex)
                    {
                        response = (HttpWebResponse)ex.Response;
                    }
                }

                if (UseCookies)
                {
                    request.FixCookies(response);
                    if (Cookies == null)
                        Cookies = response.Cookies;
                    else
                        Cookies.Add(response.Cookies);
                }

                using (Stream receiveStream = response.GetResponseStream())
                {
                    //receiveStream.ReadTimeout = Timeout;
                    //receiveStream.WriteTimeout = Timeout;
                    using (StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8))
                    {
                        content = readStream.ReadToEnd();
                    }
                }
            }
            finally
            {
                if (response != null)
                {
                    response.Close();
                }
            }

            return content;
        }
    }
}
