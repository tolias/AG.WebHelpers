using AG.Loggers;
using AG.PathStringOperations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AG.WebHelpers
{
    public class InternetConnectionLogger
    {
        public string FileName;
        public LoggerBase Logger;

        public InternetConnectionLogger(string fileName)
        {
            FileName = fileName;
        }

        public string Log(DateTime time, bool success, long millisecondsSpent, string message)
        {
            message = message.Trim();
            var line = $"{time.ToString(@"yyMMdd HHmmss")},{(success?'1':'0')},{millisecondsSpent},{message}";
            Log(line);
            return line;
        }

        private void Log(string line)
        {
            try
            {
                FileDirectoryManager.CreateDirectoryIfItDoesntExistForFile(FileName, () =>
                {
                    using (var sw = new StreamWriter(FileName, true))
                    {
                        sw.WriteLine(line);
                        sw.Close();
                    }
                });
            }
            catch (Exception ex) when (Logger != null)
            {
                Logger.Log(LogLevel.Error, ex, $"Error writing internet log msg \"{line}\"");
            }
        }
    }
}
