using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using AG.WebHelpers;

namespace AG.WebHelpers.Requesting
{
    public abstract class RequestProcessor<TContentFormat> : IRequestProcessor
    {
        public CookieCollection Cookies;

        public async Task<TResponse> GetAsync<TResponse>(string requestUri, bool throwOnHttpErrorCode = true)
        {
            return await GetAsync<TResponse, object>(requestUri, throwOnHttpErrorCode: throwOnHttpErrorCode);
        }

        public Task<T> GetAsync<T>(UriQueryBuilder requestUri, bool throwOnHttpErrorCode = true)
        {
            return GetAsync<T>(requestUri.ToString(), throwOnHttpErrorCode);
        }

        public async Task<TResponse> GetAsync<TResponse, TRequest>(string requestUrl, TRequest objectBody = default(TRequest), string method = "GET", bool throwOnHttpErrorCode = true)
        {
            HttpWebRequest request = CreateRequest(requestUrl);
            request.Method = method;

            if (objectBody != null && !objectBody.Equals(default(TRequest)))
            {
                var requestStream = await request.GetRequestStreamAsync();
                using (var streamWriter = new StreamWriter(requestStream))
                {
                    string serializedContent = SerializeContent(objectBody);
                    streamWriter.Write(serializedContent);
                }
            }
            return await GetResponseAsync<TResponse>(request, requestUrl, throwOnHttpErrorCode);
        }

        public async Task<TResponse> PostAsync<TResponse, TRequest>(string requestUri, TRequest objectBody, bool throwOnHttpErrorCode = true)
        {
            return await GetAsync<TResponse, TRequest>(requestUri, objectBody, "POST", throwOnHttpErrorCode: throwOnHttpErrorCode);
        }

        private async Task<TResponse> GetResponseAsync<TResponse>(HttpWebRequest request, string requestUrl, bool throwOnHttpErrorCode)
        {
            string content;

            HttpWebResponse response = null;
            try
            {
                if (throwOnHttpErrorCode)
                {
                    var webResponse = await request.GetResponseAsync();
                    response = (HttpWebResponse)webResponse;
                }
                else
                {
                    try
                    {
                        var webResponse = await request.GetResponseAsync();
                        response = (HttpWebResponse)webResponse;
                    }
                    catch (WebException ex)
                    {
                        response = (HttpWebResponse)ex.Response;
                    }
                }

                //request.FixCookies(response);
                if (Cookies == null)
                    Cookies = response.Cookies;
                else
                    Cookies.Add(response.Cookies);

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
                if(response != null)
                {
                    response.Dispose();
                }
            }

            TContentFormat typedContent = DeserializeContent(content);
            HandleError(typedContent, requestUrl);

            TResponse responseObj = ParseContentToConcreteType<TResponse>(typedContent);
            return responseObj;
        }

        protected virtual HttpWebRequest CreateRequest(string requestUri)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUri);
            if (Cookies != null && Cookies.Count > 0)
            {
                var cookieContainer = new CookieContainer();
                cookieContainer.Add(new Uri(requestUri), Cookies);
                request.CookieContainer = cookieContainer;
            }
            //request.UserAgent = UserAgent;
            //request.Connection = Headers.Connection;
            return request;
        }

        protected abstract string SerializeContent<T>(T obj);

        protected abstract TContentFormat DeserializeContent(string content);

        protected abstract TConcreteType ParseContentToConcreteType<TConcreteType>(TContentFormat typedContent);
        
        protected virtual void HandleError(TContentFormat typedContent, string requestUrl)
        {
            return;
        }
    }
}
