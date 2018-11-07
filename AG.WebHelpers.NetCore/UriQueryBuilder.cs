using System;
using System.Collections.Generic;
using System.Text;
using AG.WebHelpers.InternalNetClasses;
using Microsoft.Extensions.Primitives;

namespace AG.WebHelpers
{
    public class UriQueryBuilder
    {
        public string ProtocolAndHost;
        public string Path;
        public Dictionary<string, StringValues> Query;

        public UriQueryBuilder()
        {
            Query = new Dictionary<string, StringValues>();
        }

        public override string ToString()
        {
            return ToString(HttpEncoderUtility.IsUrlSafeChar);
        }

        public string ToString(Func<char, bool> isUrlSafeCharChecker)
        {
            StringBuilder builtUrlSb = new StringBuilder();
            builtUrlSb.AppendFormat("{0}{1}", ProtocolAndHost, Path);
            if (Query.Count != 0)
            {
                builtUrlSb.Append('?');
                foreach (var pair in Query)
                {
                    foreach(var param in pair.Value)
                    {
                        builtUrlSb.AppendFormat("{0}={1}&", CustomHttpUtility.UrlEncode(pair.Key, isUrlSafeCharChecker),
                            CustomHttpUtility.UrlEncode(param, isUrlSafeCharChecker));
                    }
                }
                builtUrlSb.Remove(builtUrlSb.Length - 1, 1); //remove last symbol &
            }

            return builtUrlSb.ToString();
        }

        public static UriQueryBuilder Parse(string url)
        {
            return Parse(new Uri(url));
        }

        public static UriQueryBuilder Parse(Uri uri)
        {
            UriQueryBuilder uriQueryBuilder = new UriQueryBuilder();
            uriQueryBuilder.ProtocolAndHost = uri.Scheme + "://" + uri.Host;
            uriQueryBuilder.Path = uri.AbsolutePath;
            uriQueryBuilder.Query = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query);
            return uriQueryBuilder;
        }
    }
}
