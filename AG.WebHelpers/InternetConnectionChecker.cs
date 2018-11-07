using AG.Loggers;
using AG.Utilities.Binding;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Timers;

namespace AG.WebHelpers
{
    public class InternetConnectionChecker : AvailabilityCheckerBase
    {
        public const string URL_TO_CHECK = "http://clients3.google.com/generate_204";

        public InternetConnectionLogger InternetConnectionLogger;
        public int Timeout = 10000;

        public override bool CheckForAvailability(out Func<string> messageFunc)
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
                var webRequest = WebRequest.Create(URL_TO_CHECK);
                webRequest.Timeout = Timeout;
                using (var webResponse = webRequest.GetResponse())
                {
                    using (var responseStream = webResponse.GetResponseStream())
                    {
                        if (useLogging)
                        {
                            stopWatch.Stop();
                            InternetConnectionLogger?.Log(time, true, stopWatch.ElapsedMilliseconds, $"#{attempts}");
                        }
                        messageFunc = null;
                        return true;
                    }
                }

                //using (var client = new WebClient())
                //{
                //    using (client.OpenRead(URL_TO_CHECK))
                //    {
                //        messageFunc = null;
                //        return true;
                //    }
                //}
            }
            catch (Exception ex)
            {
                if(useLogging)
                {
                    stopWatch.Stop();
                    var errMsg = ExceptionInfoProvider.GetExceptionInfo(ex, false, false, false, false);
                    messageFunc = () => errMsg;
                    InternetConnectionLogger?.Log(time, false, stopWatch.ElapsedMilliseconds, $"#{attempts} " + errMsg);
                }
                else
                {
                    messageFunc = () => ExceptionInfoProvider.GetExceptionInfo(ex, false, false, false, false);
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
