using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Windows.Forms;

using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Net;
using System.Net.Sockets;
using PADIMapNoReduce;

/*
using System.Diagnostics;
 *           CAN USE TO CREATE A NEW WORKER PROCESS
 *          Process worker = new Process();
            worker.StartInfo.FileName = "..\\..\\..\\WorkerJobTracker\\bin\\Debug\\WorkerJobTracker.exe";
            worker.StartInfo.Arguments = "arg1 arg2 arg3";
            worker.Start();
 * 
*/
namespace PuppetMaster
{
   

    public partial class PuppetMasterForm : Form
    {
        private static int LOG_MAX_LINES = 25;
        private static string PUPPETMASTERURL;
        private static int PORT=20001;
        private TcpChannel channel;
        private PMaster puppetMaster;
        private string jobTrackerUrl;

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

        public PuppetMasterForm()
        {
            InitializeComponent();
            portTB.Text = Convert.ToString(PORT);
            AddToLog("Welcome to PuppetMaster");
            AddToLog("Please insert a valid Port Number to start");
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void loadScriptBtn_Click(object sender, EventArgs e)
        {
            OpenFileDialog theDialog = new OpenFileDialog();
            theDialog.Title = "Open Script File";
            theDialog.Filter = "TXT files|*.txt";
            theDialog.InitialDirectory = @"C:\";
            if (theDialog.ShowDialog() == DialogResult.OK)
            {
                string filename = theDialog.FileName;

                string[] filelines = File.ReadAllLines(filename);

                AddToLog("Run from script: " + filename);

                //parse line by line
                for (int a = 0; a < filelines.Length; a++)
                {
                    if (filelines[a].Length == 0) continue;
                    //Comment line
                    if (filelines[a][0] == '%') continue;
                    
                    parseLine(filelines[a].Trim());
                    
                }
            }
        }

        private void parseLine(string line)
        {
            string[] splitLines = line.Split(' ');
            switch (splitLines[0])
            {
                case "WORKER":
                    if ((splitLines.Length != 5) && (splitLines.Length != 4))
                        AddToLog("Invalid Command: " + line);
                    else
                        try
                        {
                          if(splitLines.Length == 5)
                              workerCmd(Convert.ToInt32(splitLines[1]), splitLines[2],
                                    splitLines[3], splitLines[4]);
                          else
                              workerCmd(Convert.ToInt32(splitLines[1]), splitLines[2],
                                    splitLines[3]);
                        }
                        catch (FormatException)
                        {
                            AddToLog("Invalid Command: " + line);
                        }
                    break;
                case "SUBMIT":
                    if (splitLines.Length != 7)
                        AddToLog("Invalid Command: " + line);
                    else
                        try
                        {
                        submitCmd(splitLines[1], splitLines[2],
                            splitLines[3], Convert.ToInt32(splitLines[4]), 
                            splitLines[5], splitLines[6]);
                        }
                        catch (FormatException)
                        {
                            AddToLog("Invalid Command: " + line);
                        }
                    break;
                case "WAIT":
                    if (splitLines.Length != 2)
                        AddToLog("Invalid Command: " + line);
                    else
                        try{
                        waitCmd(Convert.ToInt32(splitLines[1]));
                        }
                        catch (FormatException)
                        {
                            AddToLog("Invalid Command: " + line);
                        }
                    break;
                case "STATUS":
                    if (splitLines.Length != 1)
                        AddToLog("Invalid Command: " + line);
                    else
                        statusCmd();
                    break;
                case "SLOWW":
                    if (splitLines.Length != 3)
                        AddToLog("Invalid Command: " + line);
                    else
                        try { 
                            slowwCmd(Convert.ToInt32(splitLines[1]), Convert.ToInt32(splitLines[2]));
                        }
                        catch (FormatException)
                        {
                            AddToLog("Invalid Command: " + line);
                        }
                    break;
                case "FREEZEW":
                    if (splitLines.Length != 2)
                        AddToLog("Invalid Command: " + line);
                    else
                        try { 
                            freezewCmd(Convert.ToInt32(splitLines[1]));
                        }
                        catch (FormatException)
                        {
                            AddToLog("Invalid Command: " + line);
                        }
                    break;
                case "UNFREEZEW":
                    if (splitLines.Length != 2)
                        AddToLog("Invalid Command: " + line);
                    else
                        try { 
                            unfreezewCmd(Convert.ToInt32(splitLines[1]));
                        }
                        catch (FormatException)
                        {
                            AddToLog("Invalid Command: " + line);
                        }
                    break;
                case "FREEZEC":
                    if (splitLines.Length != 2)
                        AddToLog("Invalid Command: " + line);
                    else
                        try { 
                            freezecCmd(Convert.ToInt32(splitLines[1]));
                        }
                        catch (FormatException)
                        {
                            AddToLog("Invalid Command: " + line);
                        }
                    break;
                case "UNFREEZEC":
                    if (splitLines.Length != 2)
                        AddToLog("Invalid Command: " + line);
                    else
                        try { 
                            unfreezecCmd(Convert.ToInt32(splitLines[1]));
                        }
                        catch (FormatException)
                        {
                            AddToLog("Invalid Command: " + line);
                        }
                    break;
                default:
                    AddToLog("Invalid Command: "+line);
                    break;
            }
        }


        public void AddToLog(String message)    
        {
            // add this line at the top of the log
            infoLB.Items.Insert(0, message);

            // keep only a few lines in the log
            while (infoLB.Items.Count > LOG_MAX_LINES)
            {
              infoLB.Items.RemoveAt(infoLB.Items.Count - 1);
            }
        }

        private void submitBtn_Click(object sender, EventArgs e)
        {
            if (commandTB.Text != null)
            {
                AddToLog("Single command");
                parseLine(commandTB.Text.Trim());
                commandTB.Text = "";
            }
        }

        /* PuppetMaster Available Commands */
        private void workerCmd(int id, string puppetUrl, string serviceUrl)
        {
            String command = "WORKER " + id + " " + puppetUrl + " " + serviceUrl;

            jobTrackerUrl = serviceUrl;
            //TODO add code here
            IPuppetMaster pm = (IPuppetMaster)Activator.GetObject(typeof(IPuppetMaster), puppetUrl);
            if (pm.createWorker(id, serviceUrl)) AddToLog(command);
            else AddToLog("Invalid Command: " + command);

        }

        private void workerCmd(int id, string puppetUrl, string serviceUrl, string entryUrl)
        {
            String command = "WORKER " + id + " " + puppetUrl + " " + serviceUrl + " " + entryUrl;
         
            //TODO add code here
            IPuppetMaster pm = (IPuppetMaster)Activator.GetObject(typeof(IPuppetMaster), puppetUrl);
            if (pm.createWorker(id, serviceUrl, entryUrl)) AddToLog(command);
            else AddToLog("Invalid Command: " + command);
            
        }

        private void submitCmd(string entryUrl, string file, string output, int splitsNumber, 
                               string mapClass, string dllPath)
        {
            String command = "SUBMIT " + entryUrl + " " + file + " " + output + " " + 
                              splitsNumber + " " + mapClass + " " + dllPath;
            AddToLog(command);

            Process worker = new Process();
            worker.StartInfo.FileName = "..\\..\\..\\UserApplication\\bin\\Debug\\UserApplication.exe";
            worker.StartInfo.Arguments = entryUrl+" "+file+" "+output+" "+
                                         splitsNumber+" "+mapClass+" "+dllPath;
            worker.Start();
        }

        private void waitCmd(int secs)
        {
            String command = "WAIT " + secs;
            AddToLog(command);
            System.Threading.Thread.Sleep(1000 * secs);
        }

        private void statusCmd()
        {
            String command = "STATUS";
            AddToLog(command);
            ((WorkerJobTracker)Activator.GetObject(typeof(WorkerJobTracker), jobTrackerUrl)).status();
            //TODO add code here
        }

        private void slowwCmd(int id, int delay)
        {
            String command = "SLOWW " + id + " " + delay;
            AddToLog(command);
            
            //TODO add code here
        }

        private void freezewCmd(int id)
        {
            String command = "FREEZEW " + id;
            AddToLog(command);
            
            //TODO add code here
        }

        private void unfreezewCmd(int id)
        {
            String command = "UNFREEZEW " + id;
            AddToLog(command);
            
            //TODO add code here
        }

        private void freezecCmd(int id)
        {
            String command = "FREEZEC " + id;
            AddToLog(command);
            
            //TODO add code here
        }

        private void unfreezecCmd(int id)
        {
            String command = "UNFREEZEC " + id;
            AddToLog(command);
            
            //TODO add code here
        }

        private void puppetMasterTB_TextChanged(object sender, EventArgs e)
        {
        }

        private void puppetMasterUrlBtn_Click(object sender, EventArgs e)
        {
            if (portTB.Text != null && portTB.Text != "")
            {
                try
                {
                    PORT = Convert.ToInt32(portTB.Text);
                    puppetMasterUrlBtn.Enabled = false;
                    PUPPETMASTERURL = puppetMasterTB.Text;

                    channel = new TcpChannel(PORT);
                    ChannelServices.RegisterChannel(channel, false);
                    PUPPETMASTERURL = "tcp://" + getIP() + ":" + PORT + "/PM";
                    puppetMaster = new PMaster(PUPPETMASTERURL);

                    RemotingServices.Marshal(puppetMaster, "PM");
                    puppetMasterTB.Text = PUPPETMASTERURL;
                    AddToLog("PuppetMasterService registered at:" + PUPPETMASTERURL);
                }
                catch (FormatException)
                {
                    AddToLog("Invalid Port: " + portTB.Text);
                }
            }
        }
    }
}
