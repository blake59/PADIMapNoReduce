using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            TcpChannel clientChannel = new TcpChannel(30001);

            ChannelServices.RegisterChannel(clientChannel,false);

            RemotingConfiguration.RegisterWellKnownServiceType(
            typeof(WorkerJobTracker), "W",
            WellKnownObjectMode.Singleton);

            Console.WriteLine("Worker started");
            Console.ReadLine();
        }

    }
}
