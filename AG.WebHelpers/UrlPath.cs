using System;
using System.Text;
using AG.PathStringOperations;

namespace AG.WebHelpers
{
    public static class UrlPath
    {
        public const string FtpProtocolPrefix = "ftp://";
        public const string HttpProtocolPrefix = "http://";
        public const string HttpsProtocolPrefix = "https://";
        public const char DirectorySeparatorChar = '/';

        public static string GetDirectoryName(string urlPath)
        {
            int lastSlashIndex = urlPath.LastIndexOf('/', urlPath.Length - 2);
            if (lastSlashIndex == -1 || lastSlashIndex == 0)
                return "";

            if (urlPath[lastSlashIndex - 1] != '/')
                return urlPath.Substring(0, lastSlashIndex);
            else
                return "";
        }

        public static string GetShortPath(string urlPath)
        {
            int lastIndex = urlPath.Length - 1;
            if (urlPath[lastIndex] == '/' && urlPath[lastIndex - 1] != '/')
                urlPath = urlPath.Substring(0, lastIndex);
            int lastSlashIndex = urlPath.LastIndexOf('/');
            if (lastSlashIndex != -1 && (lastSlashIndex == 0 || urlPath[lastSlashIndex - 1] != '/'))
            {
                return urlPath.Substring(lastSlashIndex + 1);
            }
            else
            {
                return urlPath;
            }
        }

        public static bool IsDirectory(string urlPath)
        {
            int slashIndex = urlPath.LastIndexOf('/', urlPath.Length - 2);
            if (slashIndex == -1)
                return true;
            return urlPath[slashIndex - 1] != '/';
        }

        public static string Combine(string urlPath1, string urlPath2)
        {
            if (urlPath1 == null || urlPath2 == null)
            {
                throw new ArgumentNullException((urlPath1 == null) ? "urlPath1" : "urlPath2");
            }
            if (urlPath2.Length == 0)
            {
                return urlPath1;
            }
            int urlPath1Length = urlPath1.Length;
            if (urlPath1Length == 0)
            {
                return urlPath2;
            }
            if (UrlPath.HasProtocolPrefix(urlPath2))
            {
                return urlPath2;
            }
            if (urlPath2[0] == UrlPath.DirectorySeparatorChar)
            {
                if (urlPath1[urlPath1Length - 1] == UrlPath.DirectorySeparatorChar)
                {
                    return urlPath1.Substring(0, urlPath1Length - 1) + urlPath2;
                }
                else
                {
                    return urlPath1 + urlPath2;
                }
            }
            else if (urlPath1[urlPath1Length - 1] == UrlPath.DirectorySeparatorChar)
            {
                return urlPath1 + urlPath2;
            }
            else
            {
                return urlPath1 + UrlPath.DirectorySeparatorChar + urlPath2;
            }
        }

        public static bool HasProtocolPrefix(string urlPath)
        {
            if (urlPath != null)
            {
                if (urlPath.StartsWith(FtpProtocolPrefix) || urlPath.StartsWith(HttpProtocolPrefix)
                    || urlPath.StartsWith(HttpsProtocolPrefix))
                {
                    return true;
                }
            }
            return false;
        }

        public static string AddParametersToUrl(string url, params string[] parameters)
        {
            int parametersLength = parameters.Length;
            if (parameters == null || parametersLength == 0)
                return url;

            string curParam = parameters[0];
            curParam = curParam.Trim('&', '?');
            StringBuilder sbParams = new StringBuilder(curParam);

            for (int i = 1; i < parametersLength; i++)
            {
                curParam = parameters[i];
                curParam = curParam.Trim('&', '?');
                if (curParam.Length > 0)
                {
                    sbParams.Append('&' + parameters[i]);
                }
            }

            char charToAdd;
            int questionPosition = url.LastIndexOf('?');
            if (questionPosition == -1)
            {
                charToAdd = '?';
            }
            else
            {
                charToAdd = '&';
            }
            return url + charToAdd + sbParams;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="paramName"></param>
        /// <returns>Parameter value. If parameter wasn't found - returns null</returns>
        public static string FindParameterInUrl(this string url, string paramName)
        {
            string toFind = paramName.EndsWith("=") ? paramName : paramName + '=';
            int pos = url.IndexOf(toFind);
            string paramValue;
            if (pos == -1)
            {
                return null;
            }
            else
            {
                pos += toFind.Length;
                int endPos = url.IndexOf('&', pos);
                if (endPos == -1)
                {
                    endPos = url.Length;
                }
                int paramValueLength = endPos - pos;
                if (paramValueLength == 0)
                {
                    return "";
                }
                else if(paramValueLength < 0)
                {
                    return null;
                }
                paramValue = url.Substring(pos, paramValueLength);
                return paramValue;
            }
        }

        public static string GetExtension(string url)
        {
            int lastDotIndex = url.LastIndexOf('.');
            if (lastDotIndex == -1)
                return null;

            var extension = url.Substring(lastDotIndex);
            if (ExtendedPath.ContainsInvalidFileNameChars(extension))
                return null;
            return extension;
        }
        
        public static string GetExternalLinkFromLocalPath(string fullLocalPath, string baseLocalPath, string baseUrl)
        {
            var relativePath = ExtendedPath.GetRelativePath(baseLocalPath, fullLocalPath);
            var relativeUrl = relativePath.Replace("\\", "/");
            var fullUrl = UrlPath.Combine(baseUrl, relativeUrl);
            return fullUrl;
        }
    }
}
