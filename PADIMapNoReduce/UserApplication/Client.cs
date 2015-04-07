using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PADIMapNoReduce;

using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

using System.IO;

namespace UserApplication
{
    class Client : MarshalByRefObject, PADIMapNoReduce.Client
    {

        private string WorkerEntryURL, selfURL;

        private Byte[] file;
        private string outputFile;
        private UserApplication userApplication;

        private bool[] splitsDone;
        private int totalSplits;

        public Client(string WorkerEntryURL, string selfURL, UserApplication userApplication)
        {
            this.WorkerEntryURL = WorkerEntryURL;
            this.selfURL = selfURL;
            this.userApplication = userApplication;
        }

        public bool submit(string inputFile, string outputFile, int numberSplits, string className, byte[] dll)
        {
            this.outputFile = outputFile;
            file = File.ReadAllBytes(inputFile);
            this.totalSplits = numberSplits;
            splitsDone = new bool[numberSplits];
            
            WorkerJobTracker jobTracker = (WorkerJobTracker)Activator.GetObject(
                typeof(WorkerJobTracker), WorkerEntryURL);

            jobTracker.startJob(numberSplits, className, dll, selfURL, file.Length);

            return true;
        }

        //
        // REMOTING
        //

        // Worker calls to get a piece of the file
        // WARNING: cliente precisa de passar linhas inteiras
        public byte[] getFileSplit(int startFile, int endFile)
        {
            Console.WriteLine("Client: GetFileSplit");
            Byte[] fileSplit;

            while ( startFile != 0 && Convert.ToChar(file[startFile-1]) != '\n')
            {
                startFile++;
            }

            if (startFile > endFile)
                endFile = startFile;

            while (Convert.ToChar(file[endFile]) != '\n')
            {
                endFile++;
            }

            // ACHO QUE NUNCA ACONTECE
            if (startFile == endFile)
            {
                Console.WriteLine("same s:" + startFile + "e:" +endFile);
                Console.ReadLine();
            }

            fileSplit = new Byte[endFile - startFile + 1];

            for (int i = 0; startFile + i < endFile + 1; i++)
            {
                fileSplit[i] = file[startFile + i];
            }

            //Console.WriteLine("s:" + startFile + "e:" + endFile);
            return fileSplit;
        }

        // Worker calls to give the client the processed split
        public bool receiveProcessedSplit(int split, List<IList<KeyValuePair<string, string>>> file)
        {
            
            if (!splitsDone[split])
            {
                splitsDone[split] = true;
                System.IO.StreamWriter filewriter = new System.IO.StreamWriter(outputFile + split + ".out");
                foreach (IList<KeyValuePair<string, string>> list in file)
                {
                    foreach (KeyValuePair<string, string> pair in list)
                    {
                        filewriter.WriteLine(pair.Key + " " + pair.Value);
                    }
                }
                filewriter.Close();
                //File.WriteAllBytes(outputFile + split + ".out", file);
                Console.WriteLine("Client: ReceiveProcessedSplit:" + split);
            }

            return true;
        }

        // JobTracker calls to tell the client the job is done
        public void jobDone()
        {
            Console.WriteLine("Client: Job Done");
            userApplication.jobDone();
        }
    }
}
