using System.Web;
using AG.PathStringOperations;

namespace AG.WebHelpers
{
    public static class ExtensionMethods
    {
        public static string MapServerPath(this HttpServerUtilityBase httpServerUtility, string localPath)
        {
            string domainRootLocalPath = httpServerUtility.MapPath("/");
            string relativeLocalPath = ExtendedPath.GetRelativePath(domainRootLocalPath, localPath);
            string serverPath = relativeLocalPath.Replace('\\', '/');
            return serverPath;
        }
    }
}
