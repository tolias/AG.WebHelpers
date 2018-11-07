using System.Net;
using AG.WebHelpers.Requesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AG.WebHelpers.Json.Requesting
{
    public class JsonRequestProcessor : RequestProcessor<JToken>
    {
        protected override HttpWebRequest CreateRequest(string requestUri)
        {
            var request = base.CreateRequest(requestUri);
            request.Accept = "application/json";
            request.ContentType = "application/json";
            return request;
        }

        protected override JToken DeserializeContent(string content)
        {
            return JToken.Parse(content);
        }

        protected override string SerializeContent<T>(T obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        protected override TConcreteType ParseContentToConcreteType<TConcreteType>(JToken typedContent)
        {
            TConcreteType responseObj = typedContent.ToObject<TConcreteType>();
            return responseObj;
        }
    }
}
