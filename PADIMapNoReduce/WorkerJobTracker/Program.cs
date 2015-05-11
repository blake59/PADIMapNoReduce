using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PADIMapNoReduce;

using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

namespace WorkerJobTracker
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args == null || args.Length < 2)
            {
                Console.WriteLine("Error: Not Enough Args");
                Console.ReadLine();
                return;
            }
            int id = Int32.Parse(args[0]);
            string serviceURL = args[1];
            bool isFirst = true;
            string JTURL = "";

            if (args.Length > 2)
            {
                isFirst = false;
                JTURL = args[2];
            }
            // Get Port
            char[] delimiters = { ':', '/' };
            string[] aux = serviceURL.Split(delimiters);
            int port = Int32.Parse(aux[4]);
            string serviceName = aux[5];

            TcpChannel clientChannel = new TcpChannel(port);
            //TcpChannel clientChannel2 = new TcpChannel();

            ChannelServices.RegisterChannel(clientChannel, false);

            WorkerJobTracker WJT;

            if (isFirst)
                WJT = new WorkerJobTracker(id, serviceURL, clientChannel);
            else
                WJT = new WorkerJobTracker(id, serviceURL, JTURL, clientChannel);


            RemotingServices.Marshal(WJT, serviceName);


            Console.WriteLine("Worker started");
            WJT.start();
            
            Console.ReadLine();
        }

    }
}
