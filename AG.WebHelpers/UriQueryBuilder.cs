using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using AG.WebHelpers.InternalNetClasses;

namespace AG.WebHelpers
{
    public class UriQueryBuilder
    {
        public string ProtocolAndHost;
        public string Path;
        public Dictionary<string, string> Query;

        public UriQueryBuilder(Dictionary<string, string> query = null)
        {
            Query = query ?? new Dictionary<string, string>();
        }

        public string QueryString
        {
            get { return GetQueryString(); }
            set { SetQueryString(value); }
        }

        public string AllPath => ProtocolAndHost + Path;

        public string GetQueryString()
        {
            return GetQueryString(HttpEncoderUtility.IsUrlSafeChar);
        }

        public string GetQueryString(Func<char, bool> isUrlSafeCharChecker)
        {
            if (Query == null || Query.Count == 0)
            {
                return null;
            }
            var querySb = new StringBuilder();
            using (var enumerator = Query.GetEnumerator())
            {
                var isNotFirst = false;
                while(enumerator.MoveNext())
                {
                    if (isNotFirst)
                    {
                        querySb.Append('&');
                    }
                    isNotFirst = true;

                    var pair = enumerator.Current;
                    querySb.Append(CustomHttpUtility.UrlEncode(pair.Key, isUrlSafeCharChecker));
                    querySb.Append('=');
                    querySb.Append(CustomHttpUtility.UrlEncode(pair.Value, isUrlSafeCharChecker));
                }
            }
            return querySb.ToString();
        }

        public Dictionary<string, string> SetQueryString(string queryString)
        {
            Query = ParseQueryString(queryString);
            return Query;
        }

        public static Dictionary<string, string> ParseQueryString(string queryString)
        {
            var queryNameValueCollection = HttpUtility.ParseQueryString(queryString);
            return queryNameValueCollection.Keys.Cast<string>().ToDictionary(k => k, v => queryNameValueCollection[v]);
        }

        public override string ToString()
        {
            return ToString(HttpEncoderUtility.IsUrlSafeChar);
        }

        public string ToString(Func<char, bool> isUrlSafeCharChecker)
        {
            var queryString = GetQueryString(isUrlSafeCharChecker);
            var allPath = AllPath;
            if (!string.IsNullOrEmpty(queryString))
            {
                allPath += '?' + queryString;
            }
            return allPath;
        }

        public static UriQueryBuilder Parse(string url)
        {
            return Parse(new Uri(url));
        }

        public static UriQueryBuilder Parse(Uri uri)
        {
            var query = ParseQueryString(uri.Query);

            UriQueryBuilder uriQueryBuilder = new UriQueryBuilder(query)
            {
                ProtocolAndHost = uri.Scheme + "://" + uri.Host,
                Path = uri.AbsolutePath
            };
            return uriQueryBuilder;
        }
    }
}
