using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        // Injects the specified delay in the worker processes with the <ID> identifier.        void sloww(int delay);

        //Disables the communication of a worker and pauses its map computation in order to simulate the worker’s failure.        void freezeW();

        //Undoes the effects of a previous FREEZEW command
        void unfreezeW();
    }

    public interface WorkerJobTracker : Worker, JobTracker
    {

    }
}