using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PADIMapNoReduce
{
    [Serializable]
    public class Work
    {
        public int splitNumber = -1;

        // Status - 0 not done, 1 int progress, 2 done
        public int status = NOTDONE;
        public int workerId = -1;
        public long startTime = -1;

        public const int NOTDONE = 0;
        public const int INPROGRESS = 1;
        public const int DONE = 2;
    }
}
