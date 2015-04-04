using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PADIMapNoReduce
{
    public interface Worker
    {
        // JobTracker calls to inform the worker he can ask for work
        void workAvailable(string clientURL, string mapperClassName, byte[] dll);

        //  JobTracker calls to inform the worker that there is no more work
        void workNotAvailable();

        // prints to the console the status
        void workerStatus();
    }
}