using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PADIMapNoReduce
{
    public interface JobTracker
    {
        // Client calls to give a job to the jobTracker
        void startJob( int totalSplit, IMapper mapper, string clientURL, int totalBytes);

        // Worker calls to get work from the JobTracker
        // return is int[3] where 0 - start, 1 - end 2 - splitnumber
        int[] getWork(int id); 

        // Worker calls to tell he finished the job he had requested
        void workDone(int id, int splitnumber);

        // Prints the status to the console
        void status();
    }
}
