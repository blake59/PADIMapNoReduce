using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PADIMapNoReduce
{
    public interface IPuppetMaster
    {
        // Called by a GUI interface to create a worker in the same machine
        // as the PuppetMaster
        bool createWorker(int id, string serviceURL, string entryURL);

        bool createWorker(int id, string serviceURL);
    }
}
