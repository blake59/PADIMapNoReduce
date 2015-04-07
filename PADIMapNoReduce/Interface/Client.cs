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

        // Worker calls to give the client the processed split
        bool receiveProcessedSplit( int split, List<IList<KeyValuePair<string, string>>> result);

        // JobTracker calls to tell the client the job is done
        void jobDone();
    }
}
