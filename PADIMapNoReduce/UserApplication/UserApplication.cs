using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PADIMapNoReduce;

using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

using System.IO;

namespace UserApplication
{
    class UserApplication
    {
        private string inputPath;
        private string outputPath;

        private int numberSplits;
        private string className;
        private string dllPath;

        private Client client;

        private TcpChannel channel;

        private bool doneJob = false;

        public UserApplication(string inputPath, string outputPath, int numberSplits, string className, string dllPath)
        {
            this.inputPath = inputPath;
            this.outputPath = outputPath;
            this.numberSplits = numberSplits;
            this.className = className;
            this.dllPath = dllPath;
        }

        public void init(string WorkerEntryURL)
        {
            channel = new TcpChannel(10001);
            ChannelServices.RegisterChannel(channel, false);

            client = new Client(WorkerEntryURL, "tcp://"+Program.getIP()+":10001/C",this);

            RemotingServices.Marshal(client, "C");
        }

        public void submitJob()
        {
            Byte[] dll = File.ReadAllBytes(dllPath);
            client.submit(inputPath, outputPath, numberSplits, className, dll);

            Console.WriteLine("Waiting for job to be done (activly bleh)");
            while (!doneJob) ;
        }

        public void jobDone()
        {
            doneJob = true;
        }
    }
}
