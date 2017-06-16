using Bentley.Interop.MicroStationDGN;
using BCOM = Bentley.Interop.MicroStationDGN;
using BM = Bentley.MicroStation;
using BMI = Bentley.MicroStation.InteropServices;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Diagnostics;
using System.IO.Pipes;
using System.Text;
using ARUP.IssueTracker.IPC;

namespace ARUP.IssueTracker.Bentley
{
    /// <summary>
    /// MicroStation looks for this class that is
    /// derived from Bentley.MicroStation.AddIn.
    /// </summary>
    [BM.AddIn(MdlTaskID = "AIT")]
    public class AITPlugin : BM.AddIn
    {
        private static string aitProcessName = "ARUP.IssueTracker.Win";
        private static string aitAppPath = Path.Combine(ProgramFilesx86(), @"CASE\ARUP Issue Tracker\ARUP.IssueTracker.Win.exe");
        private static string pipeName = Guid.NewGuid().ToString();
        private string _path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static AITPlugin MSAddin = null;
        public static BCOM.Application MSApp = null;

        /// <summary>
        /// Private constructor required for all AddIn classes derived from 
        /// Bentley.MicroStation.AddIn.
        /// </summary>
        private AITPlugin(System.IntPtr mdlDesc)
            : base(mdlDesc)
        {
            MSAddin = this;
            
            try
            {
                // Assembly Paths
                string m_issuetracker = Path.Combine(ProgramFilesx86(), @"CASE\ARUP Issue Tracker\ARUP.IssueTracker.dll");

                // Check that File Exists
                if (!File.Exists(m_issuetracker))
                {
                    MessageBox.Show(m_issuetracker, "Required Issue Tracker Library Not Found");
                }
            }
            catch (System.Exception ex1)
            {
                MessageBox.Show("exception: " + ex1);
            }
        }

        /// <summary>
        /// Initializes the AddIn Called by the AddIn loader after
        /// it has created the instance of this AddIn class
        /// </remarks>
        /// <param name="commandLine"></param>
        /// <returns>0 on success</returns>
        protected override int Run(string[] commandLine)
        {
            MSApp = BMI.Utilities.ComApp;

            // Version check
            string versionNumber = "V8i";
            if (!getBentleyProductName().Contains(versionNumber))
            {
                MessageBox.Show(string.Format("This Add-In was built for {0} {1}, please find the Arup Issue Tracker group in Yammer for assistance...", versionNumber, getBentleyProductName()), "Incompatible Version");
                return -1;
            }

            //  Register reload and unload events, and show the form
            ReloadEvent += new ReloadEventHandler(AIT_ReloadEvent);
            UnloadedEvent += new UnloadedEventHandler(AIT_UnloadedEvent);

            // Run IPC app
            RunAIT();

            return 0;
        }

        /// <summary>
        /// Static property that the rest of the application uses 
        /// get the reference to the AddIn.
        /// </summary>
        internal static AITPlugin Addin
        {
            get { return MSAddin; }
        }

        /// <summary>
        /// Static property that the rest of the application uses to
        /// get the reference to the COM object model's main application object.
        /// </summary>
        internal static BCOM.Application ComApp
        {
            get { return MSApp; }
        }

        /// <summary>
        /// Handles MDL LOAD requests after the application has been loaded.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void AIT_ReloadEvent(BM.AddIn sender, ReloadEventArgs eventArgs)
        {
            RunAIT();
        }

        /// <summary>
        /// Handles MDL UNLOAD requests after the application has been unloaded.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void AIT_UnloadedEvent(BM.AddIn sender, UnloadedEventArgs eventArgs)
        {
            foreach (var process in Process.GetProcessesByName(aitProcessName))
            {
                process.Kill();
            }
        }

        /// <summary>
        /// Handles MDL ONUNLOAD requests when the application is being unloaded.
        /// </summary>
        /// <param name="eventArgs"></param>
        protected override void OnUnloading(UnloadingEventArgs eventArgs)
        {
            base.OnUnloading(eventArgs);

            foreach (var process in Process.GetProcessesByName(aitProcessName))
            {
                process.Kill();
            }
        }

        private void RunAIT() 
        {
            // avoid muliple AIT instances
            if (Process.GetProcessesByName(aitProcessName).Length > 0)
            {
                MessageBox.Show("Arup Issue Tracker is already running!", "Arup Issue Tracker", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // create separate process of the standalone app
            var anotherProcess = new Process
            {
                StartInfo =
                {
                    FileName = aitAppPath,
                    CreateNoWindow = true,
                    UseShellExecute = true,
                    Arguments = string.Format("IPC \"{0}\" {1}", getBentleyProductName(), pipeName)
                }
            };
            anotherProcess.Start();

            try
            {
                // create the new async pipe 
                NamedPipeServerStream pipeServer = new NamedPipeServerStream(pipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);

                // wait for a connection
                pipeServer.BeginWaitForConnection(new AsyncCallback(WaitForConnectionCallBack), pipeServer);
            }
            catch (IOException ex)
            {
                // do nothing since server instance has been set up             
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void WaitForConnectionCallBack(IAsyncResult iar)
        {
            try
            {
                // Get the pipe
                NamedPipeServerStream pipeServer = (NamedPipeServerStream)iar.AsyncState;
                // End waiting for the connection
                pipeServer.EndWaitForConnection(iar);

                byte[] buffer = new byte[1024];

                // Read the incoming message
                pipeServer.Read(buffer, 0, 1024);

                // Convert byte buffer to string
                string jsonRequestMsg = Encoding.UTF8.GetString(buffer, 0, buffer.Length);

                IpcOperationType type = IpcMessageStore.getIpcOperationType(jsonRequestMsg);

                string jsonResponseMsg = null;
                if (type == IpcOperationType.AddIssueRequest)
                {
                    AddIssueResponse response = handleAddIssueRequest();
                    jsonResponseMsg = IpcMessageStore.savePayload(IpcOperationType.AddIssueResponse, response);
                }
                else if (type == IpcOperationType.OpenViewpointRequest)
                {
                
                }

                // Send response
                byte[] _buffer = Encoding.UTF8.GetBytes(jsonResponseMsg);
                pipeServer.Write(_buffer, 0, _buffer.Length);
                pipeServer.Flush();

                // Kill original sever and create new wait server
                pipeServer.Close();
                pipeServer = null;
                pipeServer = new NamedPipeServerStream(pipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);

                // Recursively wait for the connection again and again....
                pipeServer.BeginWaitForConnection(new AsyncCallback(WaitForConnectionCallBack), pipeServer);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private AddIssueResponse handleAddIssueRequest() 
        {
            return new AddIssueResponse() 
            {
                isValidRequest = true,
                documentGuid = "guid",
                documentName = "docName", 
                tempSnapshotPath = @"C:\test.png", 
                visualizationInfo = new Classes.BCF2.VisualizationInfo() 
            };
        }

        #region Static Members

        public static string getBentleyProductName()
        {
            string windowTitle = MSApp.Caption;
            int lastIndexOfDash = windowTitle.LastIndexOf('-');
            return windowTitle.Substring(lastIndexOfDash + 1).Trim();
        }

        /// <summary>
        /// Get System Architecture
        /// </summary>
        /// <returns></returns>
        static string ProgramFilesx86()
        {
            if (8 == IntPtr.Size || (!String.IsNullOrEmpty(Environment.GetEnvironmentVariable("PROCESSOR_ARCHITEW6432"))))
                return Environment.GetEnvironmentVariable("ProgramFiles(x86)");

            return Environment.GetEnvironmentVariable("ProgramFiles");
        }

        #endregion

    }

}
