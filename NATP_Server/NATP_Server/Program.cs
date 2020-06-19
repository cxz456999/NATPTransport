using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using NetCoreServer;


namespace NATP.Server
{
    class Program
    {
        private static int port = 1122;
        private static string External_IP_Address = "127.0.0.1";
        private static int rcvBufferSize = 8192;
        private static int sndBufferSize = 8192;
        public static void ReadConfiguration()
        {
            var ip = ConfigurationManager.AppSettings["External_IP_Address"];
            if (ip.Length > 0) External_IP_Address = ip;
            var p = ConfigurationManager.AppSettings["Port"];
            uint uport;
            if (uint.TryParse(p, out uport))
                port = (int)uport;
            var sndStr = ConfigurationManager.AppSettings["SendBufferSize"];
            var rcvStr = ConfigurationManager.AppSettings["ReceiveBufferSize"]; 
            int sndInt;
            int rcvInt;
            if (int.TryParse(sndStr, out sndInt) && sndInt > 0) sndBufferSize = sndInt;
            if (int.TryParse(rcvStr, out rcvInt) && rcvInt > 0) rcvBufferSize = rcvInt;
            var Users = ConfigurationManager.AppSettings["Users"].Split(",");
            int countUser = 0;
            foreach (var user in Users)
            {
                var passwd = ConfigurationManager.AppSettings[user];
                if (user.Length>0)
                {
                    NATP_ServerCore.users.Add(user, passwd);
                    countUser++;
                }
                    
            }
            Console.WriteLine("================= Configuration =================");
            Console.WriteLine("External IP Address: {0}", External_IP_Address);
            Console.WriteLine("Port: {0}", port);
            Console.WriteLine("Receive Buffer Size: {0}", rcvBufferSize);
            Console.WriteLine("Send Buffer Size: {0}", sndBufferSize);
            Console.WriteLine("Users: {0}", countUser);
            Console.WriteLine("=================================================\n");
        }

        static void Main(string[] args)
        {
            // SSL server port
            
            ReadConfiguration();

            if (args.Length > 0)
                port = int.Parse(args[0]);

            Console.WriteLine($"server port: {port}");

            Console.WriteLine();

            // Create and prepare a new SSL server context
            //var context = new SslContext(SslProtocols.Tls12, new X509Certificate2("./natp.pfx", "natp"));

            // Create a new SSL server
            //var server = new NATP_SSL_SignalingServer(context, IPAddress.Any, port);
            // Create a new TCP server
            var server = new NATP_TCP_Server(IPAddress.Any, port);
            server.OptionSendBufferSize = sndBufferSize;
            server.OptionReceiveBufferSize = rcvBufferSize;
            NATP_ServerCore.ExternalIPAddress = External_IP_Address;
            // Start the server
            Console.Write("Server starting...");
            server.Start();
            Console.WriteLine("Done!");
            // Perform text input
            for (; ; )
            {
                string line = Console.ReadLine();
                if (string.IsNullOrEmpty(line))
                    break;

                /*// Restart the server
                if (line == "!")
                {
                    Console.Write("Server restarting...");
                    server.Restart();
                    Console.WriteLine("Done!");
                    continue;
                }*/
            }

            // Stop the server
            Console.Write("Server stopping...");
            server.Stop();
            Console.WriteLine("Done!");
        }

    }
}