﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PADIMapNoReduce
{
    interface PuppetMaster
    {
        // Called by a GUI interface to create a worker in the same machine
        // as the PuppetMaster
        bool createWorker(int id, string serviceURL, string entryURL);
    }
}
