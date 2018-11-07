using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AG.WebHelpers.Requesting
{
    public class AsyncRawRequestProcessor : RawRequestProcessor
    {
        public async Task<string> GetAsync(string requestUrl, string requestBody = null, string method = "GET", bool throwOnHttpErrorCode = true)
        {
            HttpWebRequest request = CreateRequest(requestUrl);
            request.Method = method;
            if (!string.IsNullOrEmpty(requestBody))
            {
                using (var requestStream = await request.GetRequestStreamAsync())
                {
                    using (var streamWriter = new StreamWriter(requestStream))
                    {
                        streamWriter.Write(requestBody);
                    }
                }
            }
            return await GetResponseAsync(request, requestUrl, throwOnHttpErrorCode);
        }
        
        private async Task<string> GetResponseAsync(HttpWebRequest request, string requestUrl, bool throwOnHttpErrorCode)
        {
            string content;

            HttpWebResponse response = null;
            try
            {
                if (throwOnHttpErrorCode)
                {
                    response = (HttpWebResponse) (await request.GetResponseAsync());
                }
                else
                {
                    try
                    {
                        response = (HttpWebResponse)(await request.GetResponseAsync());
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
                content = response.GetContent();
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
