using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PADIMapNoReduce
{
    [Serializable]
    public class ReplicatedInfo2
    {
        public int totalBytes;
        public string mapperClassName;
        public string clientURL;
        public byte[] dll;
        public int totalSplit;
        public int totalSplitsDone = 0;

        // <int splitNumber, Work work> 
        public Dictionary<int, Work> workList;
        public int nextWork;
        // <int id, WorkerJobTracker worker>
        public Dictionary<int, string> workers;
    }
}
