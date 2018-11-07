using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AG.WebHelpers;

namespace AG.WebHelpers.Requesting
{
    public interface IRequestProcessor
    {
        TResponse Get<TResponse>(string requestUri, bool throwOnHttpErrorCode = true);

        TResponse Get<TResponse>(UriQueryBuilder requestUri, bool throwOnHttpErrorCode = true);

        TResponse Get<TResponse, TRequest>(string requestUri, TRequest objectBody = default(TRequest),
            string method = "GET", bool throwOnHttpErrorCode = true, bool rawObjectBody = false);

        TResponse Post<TResponse, TRequest>(string requestUri, TRequest objectBody, bool throwOnHttpErrorCode = true);
    }
}
