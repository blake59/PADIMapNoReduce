using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
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
        private static string PUPPETMASTERURL = "192.169.1.1"; //TODO change this url
        public PuppetMasterForm()
        {
            InitializeComponent();
            puppetMasterTB.Text = PUPPETMASTERURL;
            AddToLog("Welcome to PuppetMaster");
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
                    if (splitLines.Length != 5)
                        AddToLog("Invalid Command: " + line);
                    else
                        try
                        {
                          workerCmd(Convert.ToInt32(splitLines[1]), splitLines[2],
                                    splitLines[3], splitLines[4]);
                        }
                        catch (FormatException e)
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
                        catch (FormatException e)
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
                        catch (FormatException e)
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
                        catch (FormatException e)
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
                        catch (FormatException e)
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
                        catch (FormatException e)
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
                        catch (FormatException e)
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
                        catch (FormatException e)
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
        private void workerCmd(int id, string puppetUrl, string serviceUrl, string entryUrl)
        {
            String command = "WORKER " + id + " " + puppetUrl + " " + serviceUrl + " " + entryUrl;
            AddToLog(command);
            //TODO add code here
        }

        private void submitCmd(string entryUrl, string file, string output, int splitsNumber, 
                               string mapClass, string dllPath)
        {
            String command = "SUBMIT " + entryUrl + " " + file + " " + output + " " + 
                              splitsNumber + " " + mapClass + " " + dllPath;
            AddToLog(command);
            
            //TODO add code here
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
            puppetMasterUrlBtn.Enabled = true;
        }

        private void puppetMasterUrlBtn_Click(object sender, EventArgs e)
        {
            if (puppetMasterTB.Text != null && puppetMasterTB.Text != "")
            {
                PUPPETMASTERURL = puppetMasterTB.Text;
                puppetMasterUrlBtn.Enabled = false;
                AddToLog("PuppetMaster URL changed to: " + PUPPETMASTERURL);
                //TODO add code here to change puppetmasterurl if it's possible
            }
        }
    }
}
