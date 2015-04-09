using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.Sockets;

namespace UserApplication
{
    class Program
    {
        // Returns IP ( not the street IP)
        public static string getIP()
        {
            IPHostEntry host;
            string localIP = "?";
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                    break;
                }
            }
            return localIP;
        }

        // args <ENTRY-URL> <FILE> <OUTPUT> <S> <MAP> <DLL>
        static void Main(string[] args)
        {
            if (args == null || args.Length < 6 )
            {
                Console.WriteLine("Error: Not Enough Args");
                Console.ReadLine();
                return;
            }

            string workerEntryUrl = args[0];
            string filePath = args[1];
            string outputPath = args[2];
            int numberSplits = Int32.Parse(args[3]);
            string mapClassName = args[4];
            string dllPath = args[5];

            UserApplication userApplication = new UserApplication(filePath, outputPath, numberSplits, mapClassName, dllPath);

            userApplication.init(workerEntryUrl);

            userApplication.submitJob();

            Console.WriteLine("UserApplication: Job Done");
            Console.ReadLine();

        }


    }
}
