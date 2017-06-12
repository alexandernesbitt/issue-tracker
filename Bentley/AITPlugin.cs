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

namespace ARUP.IssueTracker.Bentley
{
    /// <summary>
    /// MicroStation looks for this class that is
    /// derived from Bentley.MicroStation.AddIn.
    /// </summary>
    [BM.AddIn(MdlTaskID = "AIT")]
    public class AITPlugin : BM.AddIn
    {
        private string _path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static AITPlugin MSAddin = null;
        public static BCOM.Application MSApp = null;
        public static BentleyWindow mainWindow;

        //static AITPlugin()
        //{
        //    AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        //}

        //public static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        //{
        //    string dllFileName = new AssemblyName(args.Name).Name + ".dll";
        //    string dllPath = Path.Combine(ProgramFilesx86(), "CASE", "ARUP Issue Tracker", dllFileName);

        //    Assembly theAsm = null;
        //    if (!string.IsNullOrEmpty(dllPath) && File.Exists(dllPath))
        //    {
        //        theAsm = Assembly.LoadFrom(dllPath);
        //    }
        //    return theAsm;
        //}

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
                string versionNumber = "V8i";

                // Version
                // FIXME
                if (true)
                {
                    MessageBox.Show(string.Format("This Add-In was built for {0} {1}, please find the Arup Issue Tracker group in Yammer for assistance...", versionNumber, getBentleyProductName()), "Incompatible Version");
                }

                // Assembly Paths
                string m_issuetracker = Path.Combine(ProgramFilesx86(), "CASE", "ARUP Issue Tracker", "ARUP.IssueTracker.dll");

                // Check that File Exists
                if (!File.Exists(m_issuetracker))
                {
                    MessageBox.Show(m_issuetracker, "Required Issue Tracker Library Not Found");
                }

                // Load Assemblies
                Assembly.LoadFrom(m_issuetracker);
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
            //  Register reload and unload events, and show the form
            ReloadEvent += new ReloadEventHandler(AIT_ReloadEvent);
            UnloadedEvent += new UnloadedEventHandler(AIT_UnloadedEvent);

            mainWindow = new BentleyWindow(MSApp);
            mainWindow.Show();

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
            //TODO: add specific handling For this Event here       
            if (mainWindow == null || !mainWindow.IsLoaded)
            {
                mainWindow = new BentleyWindow(MSApp);
                mainWindow.Show();
            }
        }

        /// <summary>
        /// Handles MDL UNLOAD requests after the application has been unloaded.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void AIT_UnloadedEvent(BM.AddIn sender, UnloadedEventArgs eventArgs)
        {
            mainWindow.Close();
            mainWindow = null;
        }

        /// <summary>
        /// Handles MDL ONUNLOAD requests when the application is being unloaded.
        /// </summary>
        /// <param name="eventArgs"></param>
        protected override void OnUnloading(UnloadingEventArgs eventArgs)
        {
            base.OnUnloading(eventArgs);
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
