using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PADIMapNoReduce
{
    public interface Client
    {
        // Worker calls to get a piece of the file
        // WARNING: cliente precisa de passar linhas inteiras
        byte[] getFileSplit(int startFile, int endFile);  

        // Worker calls to give the cliente the processed split
        bool receiveProcessedSplit( int split, byte[] file);

        // JobTracker calls to tell the cliente the job is done
        void jobDone();
    }
}
