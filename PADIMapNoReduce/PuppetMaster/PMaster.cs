using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using PADIMapNoReduce;

using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

using System.IO;

namespace PuppetMaster
{
    class PMaster : MarshalByRefObject, PADIMapNoReduce.IPuppetMaster
    {

        private string puppetMasterURL;

        public PMaster(string puppetMasterURL)
        {
            this.puppetMasterURL = puppetMasterURL;
        }


        public bool createWorker(int id, string serviceURL, string entryURL)
        {
            Process worker = new Process();
            worker.StartInfo.FileName = "..\\..\\..\\WorkerJobTracker\\bin\\Debug\\WorkerJobTracker.exe";
            worker.StartInfo.Arguments = id+" "+serviceURL+" "+entryURL;
            worker.Start();
            return true;
        }


        public bool createWorker(int id, string serviceURL)
        {
            Process worker = new Process();
            worker.StartInfo.FileName = "..\\..\\..\\WorkerJobTracker\\bin\\Debug\\WorkerJobTracker.exe";
            worker.StartInfo.Arguments = id +" "+ serviceURL;
            worker.Start();
            return true;
        }
    }
}
