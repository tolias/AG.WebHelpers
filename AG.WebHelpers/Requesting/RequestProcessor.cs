using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using AG.WebHelpers;

namespace AG.WebHelpers.Requesting
{
    public abstract class RequestProcessor<TContentFormat> : RawRequestProcessor, IRequestProcessor
    {
        public TResponse Get<TResponse>(string requestUri, bool throwOnHttpErrorCode = true)
        {
            return Get<TResponse, object>(requestUri, throwOnHttpErrorCode: throwOnHttpErrorCode);
        }

        public TResponse Get<TResponse>(UriQueryBuilder requestUri, bool throwOnHttpErrorCode = true)
        {
            return Get<TResponse>(requestUri.ToString(), throwOnHttpErrorCode);
        }

        public TResponse Get<TResponse, TRequest>(string requestUrl, TRequest objectBody = default(TRequest), string method = "GET", bool throwOnHttpErrorCode = true, bool rawObjectBody = false)
        {
            string serializedRequestContent;
            if (rawObjectBody)
            {
                serializedRequestContent = objectBody?.ToString();
            }
            else if (objectBody != null && !objectBody.Equals(default(TRequest)))
            {
                serializedRequestContent = SerializeContent(objectBody);
            }
            else
            {
                serializedRequestContent = null;
            }
            var responseContent = Get(requestUrl, serializedRequestContent, method, throwOnHttpErrorCode);

            TContentFormat typedContent = DeserializeContent(responseContent);
            HandleError(typedContent, requestUrl);
            TResponse responseObj = ParseContentToConcreteType<TResponse>(typedContent);
            return responseObj;
        }

        public TResponse Post<TResponse, TRequest>(string requestUri, TRequest objectBody, bool throwOnHttpErrorCode = true)
        {
            return Get<TResponse, TRequest>(requestUri, objectBody, "POST", throwOnHttpErrorCode: throwOnHttpErrorCode);
        }

        protected abstract string SerializeContent<T>(T obj);

        protected abstract TContentFormat DeserializeContent(string content);

        protected abstract TConcreteType ParseContentToConcreteType<TConcreteType>(TContentFormat typedContent);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="jToken"></param>
        /// <returns>Error code. If there is no error - returns 0</returns>
        protected virtual void HandleError(TContentFormat typedContent, string requestUrl)
        {
            return;
        }
    }
}
