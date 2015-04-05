using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using PADIMapNoReduce;

using System.Reflection;
using System.Diagnostics;

namespace WorkerJobTracker
{
    class WorkerJobTracker : MarshalByRefObject, PADIMapNoReduce.WorkerJobTracker
    {

        private string serviceURL;
        private string JTURL;
        private int id;
        private bool isJT;

        private bool working = false;

        public WorkerJobTracker(int id, string serviceURL)
        {
            this.id = id;
            this.serviceURL = serviceURL;
            this.isJT = true;

            workers = new Dictionary<int, WorkerJobTracker>();
            // needs to selfAdd if he is gonna work
        }

        public WorkerJobTracker(int id, string serviceURL, string JTURL)
        {
            this.id = id;
            this.serviceURL = serviceURL;
            this.JTURL = JTURL;
            this.isJT = false;

            jobTracker = (WorkerJobTracker)Activator.GetObject(
                typeof(WorkerJobTracker), JTURL);
            jobTracker.addWorker(id, serviceURL);
        }

        //
        // JOBTRACKER
        //

        private class Work{
            public int splitNumber = -1;

            // Status - 0 not done, 1 int progress, 2 done
            public int status = NOTDONE;
            public int workerId = -1;
            public long startTime = -1;

            public const int NOTDONE = 0;
            public const int INPROGRESS = 1;
            public const int DONE = 2;
        }

        private int totalBytes;
        private string mapperClassName;
        private byte[] dll;
        private int totalSplit;
        private int totalSplitsDone = 0;

        // <int splitNumber, Work work> 
        private Dictionary<int,Work> workList;
        private int nextWork;
        // <int id, WorkerJobTracker worker>
        private Dictionary<int, WorkerJobTracker> workers;


        // Worker calls to tell the JobTracker he was created
        public void addWorker(int id, string workerURL)
        {
            if (!isJT)
                return;
            Console.WriteLine("ADDED worker:" + id);
            workers.Add(id, (WorkerJobTracker)Activator.GetObject(typeof(WorkerJobTracker), workerURL) );
        }

        // Client calls to give a job to the jobTracker
        public void startJob(int totalSplit, string mapperClassName, byte[] dll, string clientURL, int totalBytes)
        {
            if (!isJT)
                return;
            this.totalSplit = totalSplit;
            this.clientURL = clientURL;
            this.totalBytes = totalBytes;
            this.mapperClassName = mapperClassName;
            this.dll = dll;
            workList = new Dictionary<int, Work>();
            client = (Client)Activator.GetObject(typeof(Client), clientURL);
            for (int i = 0; i < totalSplit; i++)
            {
                Work work = new Work();
                work.splitNumber = i;
                workList.Add(i, work);
            }
            nextWork = 0;
            foreach (WorkerJobTracker w in workers.Values)
            {
                w.workAvailable(clientURL, mapperClassName, dll);
            }

            Console.WriteLine("starting Job");
            

        }

        // Worker calls to get work from the JobTracker
        // return is int[3] where 0 - start, 1 - end 2 - splitnumber
        public int[] getWork(int id)
        {
            if (!isJT)
                return null;

            int[] workInfo = new int[3];

            int splitNumber = workList[nextWork].splitNumber;
            workInfo[2] = splitNumber;
            workInfo[0] = (totalBytes / totalSplit) * splitNumber;
            workInfo[1] = (totalBytes / totalSplit) * (splitNumber + 1);

            workList[nextWork].status = Work.INPROGRESS;
            workList[nextWork].workerId = id;
            workList[nextWork].startTime = Stopwatch.GetTimestamp();
            
            // FIRST DO-WHILE EVER
            do
            {
                if (++nextWork >= totalSplit)
                    nextWork = 0;

            } while (workList[nextWork].status == Work.DONE);

            return workInfo;
        }

        // Worker calls to tell he finished the job he had requested
        public void workDone(int id, int splitnumber)
        {
            if (!isJT)
                return;

            workList[splitnumber].status = Work.DONE;
            totalSplitsDone++;
            if (totalSplitsDone == totalSplit)
            {
                client.jobDone();

                foreach (WorkerJobTracker w in workers.Values)
                {
                    w.workNotAvailable();
                }
            }
        }

        // Prints the status to the console
        public void status()
        {
            if (!isJT)
                return;
            Console.WriteLine("Job Tracker Status");
        }

        //
        // Worker
        //

        private const string VACATION = "Vacation";
        private const string GETTINGWORK = "Getting Work";
        private const string GETTINGFILE = "Getting File";
        private const string PROCESSINGFILE = "Processing File";
        private const string SENDINGFILE = "Sending File";

        private Type type;
        private object mapper;
        private string clientURL;
        private bool isWorkAvailable = false;
        private string workStatus = VACATION;
        private int linesTotal = 0, linesDone = 0;

        private WorkerJobTracker jobTracker;
        private Client client;


        // not very elegant
        public void start()
        {
            working = true;
            while (working)
            {
                // DO STUFF 
                if (isWorkAvailable)
                {
                    workStatus = GETTINGWORK;
                    int[] workInfo = jobTracker.getWork(id);
                    workStatus = GETTINGFILE;
                    byte[] fileSplit = client.getFileSplit(workInfo[0],workInfo[1]);

                    workStatus = PROCESSINGFILE;
                    Console.WriteLine("Processing:" + workInfo[2]);
                    byte[] result = processSplit(fileSplit);

                    workStatus = SENDINGFILE;
                    client.receiveProcessedSplit(workInfo[2],result);
                    jobTracker.workDone(id,workInfo[2]);
                }
            }
        }

        private byte[] processSplit(byte[] fileSplit)
        {
            //System.Threading.Thread.Sleep(5000);
            linesDone = 0;
            List<IList<KeyValuePair<string, string>>> resultLines = new List<IList<KeyValuePair<string, string>>>();
            string[] lines = bytesToLines(fileSplit);
            foreach (string line in lines)
            {
                Console.WriteLine(line);
                object[] args = new object[] { line };
                object resultObject = type.InvokeMember("Map",
                  BindingFlags.Default | BindingFlags.InvokeMethod,
                       null,
                       mapper,
                       args);
                resultLines.Add((IList<KeyValuePair<string, string>>)resultObject);
                linesDone++;
            }

            // Falta decidir como passar os resultados para byte[]

            string str = "result";
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        private string[] bytesToLines(byte[] fileSplit )
        {
            List<string> lines = new List<string>();
            int numberOfLines = 0;
            int start = 0;
            int end = 0;
            while( end < fileSplit.Length ){
                if (fileSplit[end] == '\n')
                {
                    lines.Add(Encoding.UTF8.GetString(fileSplit, start, end-start));
                    numberOfLines++;
                    start = ++end;
                }
                end++;
            }
            linesTotal = numberOfLines;
            return lines.ToArray();
        }

        // JobTracker calls to inform the worker he can ask for work
        public void workAvailable(string clientURL, string mapperClassName, byte[] dll)
        {
            this.clientURL = clientURL;
            this.mapper = createMapper(mapperClassName, dll);
            client = (Client)Activator.GetObject(typeof(Client), clientURL);
            this.isWorkAvailable = true;

            Console.WriteLine("Work");
        }

        //  JobTracker calls to inform the worker that there is no more work
        public void workNotAvailable()
        {
            isWorkAvailable = false;
            Console.WriteLine("rip Work");
        }

        // prints to the console the status
        public void workerStatus()
        {
            if (workStatus == PROCESSINGFILE)
                Console.WriteLine(workStatus + " " + linesDone + "/" + linesTotal);
            else
                Console.WriteLine(workStatus);
        }

        private object createMapper(string className, byte[] code)
        {
            Assembly assembly = Assembly.Load(code);
            object mapper = null;
            // Walk through each type in the assembly looking for our class
            foreach (Type type in assembly.GetTypes())
            {
                if (type.IsClass == true)
                {
                    if (type.FullName.EndsWith("." + className))
                    {
                        // create an instance of the object
                        mapper =  Activator.CreateInstance(type);
                        this.type = type;
                    }
                }
            }
            return mapper;
        }

    }
}
