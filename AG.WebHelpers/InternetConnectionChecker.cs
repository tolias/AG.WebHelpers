using System;
using System.Diagnostics;
using System.Net;
using Serilog.Core;

namespace AG.WebHelpers
{
    public class InternetConnectionChecker : AvailabilityCheckerBase
    {
        public const string URL_TO_CHECK = "http://clients3.google.com/generate_204";

        public InternetConnectionLogger InternetConnectionLogger;
        public int Timeout = 50000;

        public InternetConnectionChecker(Logger logger) : base(logger)
        {
            MaxUnavailabilityIntervalToIgnore = TimeSpan.FromMinutes(5);
        }

        protected override bool CheckForAvailabilityInternal(out string message)
        {
            int attempts = 0;
            Stopwatch stopWatch;
            DateTime time;
            retry:
            bool useLogging = InternetConnectionLogger != null;
            if (useLogging)
            {
                stopWatch = new Stopwatch();
                time = DateTime.Now;
                stopWatch.Start();
            }
            else
            {
                time = DateTime.MinValue;
                stopWatch = null;
            }
            try
            {
                attempts++;
                var webRequest = (HttpWebRequest)WebRequest.Create(URL_TO_CHECK);
                webRequest.Timeout = Timeout;
                webRequest.ContinueTimeout = Timeout;
                webRequest.ReadWriteTimeout = Timeout;

                using (var webResponse = webRequest.GetResponse())
                {
                    using (var responseStream = webResponse.GetResponseStream())
                    {
                        if (useLogging)
                        {
                            stopWatch.Stop();
                            InternetConnectionLogger?.Log(time, true, stopWatch.ElapsedMilliseconds, $"#{attempts}");
                        }
                        message = null;
                        return true;
                    }
                }

                //using (var client = new WebClient())
                //{
                //    using (client.OpenRead(URL_TO_CHECK))
                //    {
                //        message = null;
                //        return true;
                //    }
                //}
            }
            catch (Exception ex)
            {
                if (useLogging)
                {
                    stopWatch.Stop();
                    var errMsg = ExceptionInfoProvider.GetExceptionInfo(ex, false, false, false, false);
                    message = errMsg;
                    InternetConnectionLogger?.Log(time, false, stopWatch.ElapsedMilliseconds, $"#{attempts} " + errMsg);
                }
                else
                {
                    message = ExceptionInfoProvider.GetExceptionInfo(ex, false, false, false, false);
                }
                if (attempts < 2)
                {
                    goto retry;
                }

                return false;
            }
        }
    }
}
