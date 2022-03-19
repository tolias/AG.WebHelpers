using AG.Loggers;
using AG.WebHelpers.TCP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebHelpersTestConsoleApp
{
    class Program2
    {
        static void Main2(string[] args)
        {
            var sender = new TcpMessagesSender(4198);
            sender.Logger = new ConsoleLogger(LogLevel.Debug);
            sender.Start();
            Console.WriteLine("started.");
            Console.ReadKey();
            sender.SendMessage("Test1").Wait();
            Console.WriteLine("Sent 1");
            Console.ReadKey();
        }
    }
}
