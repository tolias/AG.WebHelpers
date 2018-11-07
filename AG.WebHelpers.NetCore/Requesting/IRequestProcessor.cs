using System.Threading.Tasks;

namespace AG.WebHelpers.Requesting
{
    public interface IRequestProcessor
    {
        Task<T> GetAsync<T>(string requestUri, bool throwOnHttpErrorCode = true);

        Task<T> GetAsync<T>(UriQueryBuilder requestUri, bool throwOnHttpErrorCode = true);

        Task<TResponse> GetAsync<TResponse, TRequest>(string requestUri, TRequest objectBody = default(TRequest), string method = "GET", bool throwOnHttpErrorCode = true);

        Task<TResponse> PostAsync<TResponse, TRequest>(string requestUri, TRequest objectBody, bool throwOnHttpErrorCode = true);
    }
}
