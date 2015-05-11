using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PADIMapNoReduce;

using System.Reflection;
using System.Diagnostics;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Threading;
using System.Threading.Tasks;

namespace WorkerJobTracker
{

    public class WorkerJobTracker : MarshalByRefObject, PADIMapNoReduce.IWorkerJobTracker
    {
        private TcpChannel tcpChannel;

        private string serviceURL;
        private string JTURL;
        private int id;
        private bool isJT;
        private bool isJTSecondary;

        private bool working = false;

        private const int TIMEOUT = 2;

        public WorkerJobTracker(int id, string serviceURL, TcpChannel clientChannel)
        {
            this.id = id;
            this.serviceURL = serviceURL;
            this.JTURL = serviceURL;
            this.isJT = true;
            this.tcpChannel = clientChannel;

            replicatedInfo = new ReplicatedInfo();
            replicatedInfo2 = new ReplicatedInfo2();

            replicatedInfo.workers = new Dictionary<int, string>();

            jobTracker = (IWorkerJobTracker)Activator.GetObject(
                typeof(IWorkerJobTracker), serviceURL);

            
           addWorker(id, serviceURL);

           heartbeatDelegate del = new heartbeatDelegate(heartbeat);
           del.BeginInvoke(null, null);
        }

        public WorkerJobTracker(int id, string serviceURL, string JTURL, TcpChannel clientChannel)
        {
            this.id = id;
            this.serviceURL = serviceURL;
            this.JTURL = JTURL;
            this.isJT = false;
            this.tcpChannel = clientChannel;

            jobTracker = (IWorkerJobTracker)Activator.GetObject(
                typeof(IWorkerJobTracker), JTURL);

            jobTracker.addWorker(id, serviceURL);
        }

        //
        // JOBTRACKER
        //

        private IWorkerJobTracker secondary = null;

        private ReplicatedInfo replicatedInfo;
        private ReplicatedInfo2 replicatedInfo2;
        /*
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
        */
        private Semaphore JTsemaphore = new Semaphore(1,1);

        // Worker calls to tell the JobTracker he was created
        public void addWorker(int id, string workerURL)
        {
            if (!isJT)
                return;

            JTsemaphore.WaitOne();
            JTsemaphore.Release();
            //IWorkerJobTracker worker = (IWorkerJobTracker)Activator.GetObject(typeof(IWorkerJobTracker), workerURL);

            Console.WriteLine("ADDED worker:" + id);
            replicatedInfo.workers.Add(id, workerURL);
            
        }

        // Client calls to give a job to the jobTracker
        public void startJob(int totalSplit, string mapperClassName, byte[] dll, string clientURL, int totalBytes)
        {
            if (!isJT)
                return;

            JTsemaphore.WaitOne();
            JTsemaphore.Release();

            replicatedInfo.totalSplit = totalSplit;
            replicatedInfo.clientURL = clientURL;
            replicatedInfo.totalBytes = totalBytes;
            replicatedInfo.mapperClassName = mapperClassName;
            replicatedInfo.dll = dll;
            replicatedInfo.workList = new Dictionary<int, Work>();
            client = (Client)Activator.GetObject(typeof(Client), clientURL);
            for (int i = 0; i < totalSplit; i++)
            {
                Work work = new Work();
                work.splitNumber = i;
                replicatedInfo.workList.Add(i, work);
            }
            replicatedInfo.nextWork = 0;

            foreach (string w in replicatedInfo.workers.Values)
            {
                ((IWorkerJobTracker)Activator.GetObject(typeof(IWorkerJobTracker), w)).workAvailable(clientURL, mapperClassName, dll);
            }

            Console.WriteLine("starting Job");

        }

        // Worker calls to get work from the JobTracker
        // return is int[3] where 0 - start, 1 - end 2 - splitnumber
        public delegate int[] getWorkDelegate(int id);
        public int[] getWork(int id)
        {
            if (!isJT)
                return null;

            JTsemaphore.WaitOne();
            JTsemaphore.Release();

            int[] workInfo = new int[3];

            if (replicatedInfo.workList[replicatedInfo.nextWork].status == Work.DONE)
            {
                do
                {
                    if (++replicatedInfo.nextWork >= replicatedInfo.totalSplit)
                        replicatedInfo.nextWork = 0;

                } while (replicatedInfo.workList[replicatedInfo.nextWork].status == Work.DONE);
            }

            int splitNumber = replicatedInfo.workList[replicatedInfo.nextWork].splitNumber;
            workInfo[2] = splitNumber;
            workInfo[0] = (replicatedInfo.totalBytes / replicatedInfo.totalSplit) * splitNumber;
            workInfo[1] = (replicatedInfo.totalBytes / replicatedInfo.totalSplit) * (splitNumber + 1) - 1;

            replicatedInfo.workList[replicatedInfo.nextWork].status = Work.INPROGRESS;
            replicatedInfo.workList[replicatedInfo.nextWork].workerId = id;
            replicatedInfo.workList[replicatedInfo.nextWork].startTime = Stopwatch.GetTimestamp();
            
            // FIRST DO-WHILE EVER
            do
            {
                if (++replicatedInfo.nextWork >= replicatedInfo.totalSplit)
                    replicatedInfo.nextWork = 0;

            } while (replicatedInfo.workList[replicatedInfo.nextWork].status == Work.DONE);

            if(secondary != null)
                secondary.getWork(id);

            return workInfo;
        }

        // Worker calls to tell he finished the job he had requested
        public delegate void workDoneDelegate(int id, int splitnumber);
        public void workDone(int id, int splitnumber)   // FIX ME
        {
            if (!isJT)
                return;

            if (replicatedInfo.workList[splitnumber].status != Work.DONE)
            {
                replicatedInfo.workList[splitnumber].status = Work.DONE;
                replicatedInfo.totalSplitsDone++;
            }

            if (replicatedInfo.totalSplitsDone == replicatedInfo.totalSplit)
            {
                client.jobDone();

                foreach (string w in replicatedInfo.workers.Values)
                {
                    ((IWorkerJobTracker)Activator.GetObject(typeof(IWorkerJobTracker), w)).workNotAvailable();
                }
            }

            if (secondary != null)
                secondary.workDone(id, splitnumber);
        }

        // Prints the status to the console
        public void status()
        {
            if (!isJT)
                return;

            JTsemaphore.WaitOne();
            JTsemaphore.Release();

            Console.WriteLine("Job Tracker Status");
            Console.WriteLine("Workers Active:" + replicatedInfo.workers.Count);

            foreach (string w in replicatedInfo.workers.Values)
            {
                ((IWorkerJobTracker)Activator.GetObject(typeof(IWorkerJobTracker), w)).workerStatus();
            }
        }

        //Disables the communication of the job tracker aspect of a worker node in order to simulate its failures.
        public void freezeC()
        {
            if (isJT)
            {
                Console.WriteLine("Freezing");
                JTsemaphore.WaitOne();
            }
        }

        //Undoes the effects of a previous FREEZEC command
        public void unfreezeC()
        {

            if (isJT)
            {
                Console.WriteLine("Unfreezing");
                JTsemaphore.Release();
            }
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

        private IWorkerJobTracker jobTracker;
        private Client client;

        private Semaphore workerSem = new Semaphore(1, 1);


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
                    int[] workInfo = getWorkAux(id);

                    workStatus = GETTINGFILE;
                    byte[] fileSplit = getFileSplitAux(workInfo[0],workInfo[1]);

                    if (!isWorkAvailable)
                    {
                        workStatus = VACATION;
                        continue;
                    }

                    workStatus = PROCESSINGFILE;
                    Console.WriteLine("Processing:" + workInfo[2]);
                    List<IList<KeyValuePair<string, string>>> result = processSplit(fileSplit);

                    if (!isWorkAvailable)
                    {
                        workStatus = VACATION;
                        continue;
                    }

                    workStatus = SENDINGFILE;
                    receiveProcessedSplitAux(workInfo[2],result);

                    workDoneAux(id,workInfo[2]);

                    workStatus = GETTINGWORK;
                }
            }
        }

        private int[] getWorkAux(int id)
        {
            // check if communication is blocked
            workerSem.WaitOne();
            workerSem.Release();    // dirty but meh

            getWorkDelegate del = new getWorkDelegate(jobTracker.getWork);

            IAsyncResult result = del.BeginInvoke(id, null, null);

            DateTime time = DateTime.Now;
            while (!result.IsCompleted)
            {
                if ((DateTime.Now - time).TotalMilliseconds > TIMEOUT * 1000)
                {
                    return getWorkAux(id);
                }
            }

            return del.EndInvoke(result);
        }

        private void workDoneAux(int id, int split)
        {
            
            // check if communication is blocked
            workerSem.WaitOne();
            workerSem.Release();    // dirty but meh
            workDoneDelegate del = new workDoneDelegate(jobTracker.workDone);

            IAsyncResult result = del.BeginInvoke(id,split, null, null);

            DateTime time = DateTime.Now;
            while (!result.IsCompleted)
            {
                if ((DateTime.Now - time).TotalMilliseconds > TIMEOUT * 1000)
                {
                    workDoneAux(id,split);
                }
            }
        }

        private byte[] getFileSplitAux(int start, int end)
        {
            // check if communication is blocked
            workerSem.WaitOne();
            workerSem.Release();    // dirty but meh
            try
            {
                return client.getFileSplit(start, end);
            }
            catch (Exception e)
            {
                Console.WriteLine("fail!");
                return null;
            }
        }

        private void receiveProcessedSplitAux(int split,  List<IList<KeyValuePair<string, string>>> result)
        {
            // check if communication is blocked
            workerSem.WaitOne();
            workerSem.Release();    // dirty but meh
            try
            {
                client.receiveProcessedSplit(split, result);
            }
            catch (Exception e)
            {
                Console.WriteLine("fail!");
            }
        }

        private List<IList<KeyValuePair<string, string>>> processSplit(byte[] fileSplit)
        {
            linesDone = 0;
            List<IList<KeyValuePair<string, string>>> resultLines = new List<IList<KeyValuePair<string, string>>>();
            string[] lines = bytesToLines(fileSplit);
            foreach (string line in lines)
            {
                // check if communication is blocked if so cant work
                workerSem.WaitOne();
                workerSem.Release();    // dirty but meh
                object[] args = new object[] { line };
                object resultObject = type.InvokeMember("Map",
                  BindingFlags.Default | BindingFlags.InvokeMethod,
                       null,
                       mapper,
                       args);
                resultLines.Add((IList<KeyValuePair<string, string>>)resultObject);
                linesDone++;
            }

            return resultLines;
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
            workStatus = VACATION;
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

        // Injects the specified delay in the worker processes with the <ID> identifier.
        public void slowW(int delay)
        {
            Console.WriteLine("sleeping for:" + delay + " " + DateTime.Now);
            workerSem.WaitOne();
            System.Threading.Thread.Sleep(1000 * delay);
            workerSem.Release();
            Console.WriteLine("not sleeping " + DateTime.Now);
        }

        //Disables the communication of a worker and pauses its map computation in order to simulate the worker’s failure.
        public void freezeW()
        {
            Console.WriteLine("Worker freeze");
            workerSem.WaitOne();
        }

        //Undoes the effects of a previous FREEZEW command
        public void unfreezeW()
        {
            Console.WriteLine("Worker unfreeze");
            workerSem.Release();
        }

        public void newJobTracker(string URL)
        {
            JTURL = URL;
            jobTracker = (IWorkerJobTracker)Activator.GetObject(
            typeof(IWorkerJobTracker), JTURL);
        }

        // JOBTRACKER REPLICATION

        private bool beating = false;
        public void fullReplication( ReplicatedInfo replicatedInfo ) 
        {
            Console.WriteLine("I AM NOW SECONDARY URAYYY");
            return;

            //this.replicatedInfo.workers = replicatedInfo;

            if (!beating)
            {
                lastPrimaryHeartBeat = DateTime.Now;
                // start heartbeats
                heartbeatDelegate del = new heartbeatDelegate(heartbeat);
                del.BeginInvoke(null, null);
                beating = true;
            }

        }

        private const int HEARTBEATTIMEOUT = 2;
        private int heartbeatDelay = 1;
        private DateTime lastPrimaryHeartBeat;
        public delegate void heartbeatDelegate();
        public void heartbeat()
        {
            Console.WriteLine("heartBeat");
            if ( isJT && replicatedInfo.workers.Count > 1 && secondary == null)
            {
                IWorkerJobTracker worker = (IWorkerJobTracker)Activator.GetObject(typeof(IWorkerJobTracker), replicatedInfo.workers[2]);
                worker.fullReplication(replicatedInfo);
                secondary = worker;
                Console.WriteLine("creating secondary");   
                
            } else if (isJT && secondary != null)
            {
                Console.WriteLine("sending heartbeat");
                sendHeartbeatDelegator del = new sendHeartbeatDelegator(secondary.sendHeartbeat);

                JTsemaphore.WaitOne();
                JTsemaphore.Release();
                IAsyncResult result = del.BeginInvoke(null, null);

                DateTime time = DateTime.Now;
                while (!result.IsCompleted)
                {
                    if ((DateTime.Now - time).TotalMilliseconds > HEARTBEATTIMEOUT * 1000)
                    {
                       //replace secondary
                    }
                }
            }
            else if(!isJT && secondary == null)
            {
                Console.WriteLine("checking on secundary");
                if((DateTime.Now - lastPrimaryHeartBeat).TotalMilliseconds > HEARTBEATTIMEOUT * 1000)
                {
                    // replace primary
                }
            }

            Task.Delay(TimeSpan.FromSeconds(heartbeatDelay)).Wait();
            heartbeat();
        }

        public delegate void sendHeartbeatDelegator();
        public void sendHeartbeat()
        {
            lastPrimaryHeartBeat = DateTime.Now;
            Console.WriteLine("bump bump");
        }

    }
}
