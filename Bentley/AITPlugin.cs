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
using ARUP.IssueTracker.Classes.BCF2;

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
                    VisualizationInfo visInfo = IpcMessageStore.getPayload<VisualizationInfo>(jsonRequestMsg);
                    jsonResponseMsg = "{}";
                    doOpen3DView(visInfo);
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
                isValidRequest = MSApp.ActiveModelReference.Type == MsdModelType.Normal,
                documentGuid = MSApp.ActiveDesignFile.GetHashCode().ToString(),
                documentName = MSApp.ActiveDesignFile.Name,
                tempSnapshotPath = generateSnapshot(),
                visualizationInfo = generateViewpoint(2)
            };
        }

        private int getActiveViewNumber() 
        {
            for (int i = 1; i <= 8; i++)
            {
                if(MSApp.ActiveDesignFile.Views[i].IsSelected)
                {
                    return i;
                }
            }
            return 0;
        }

        private string generateSnapshot() 
        {
            MSApp.CadInputQueue.SendKeyin("null");
            string tempSnapshotPath = Path.GetTempFileName();
            CaptureScreenModalHandler handler = new CaptureScreenModalHandler(tempSnapshotPath);
            MSApp.AddModalDialogEventsHandler(handler);
            MSApp.CadInputQueue.SendCommand("MDL KEYIN mptools save image");
            MSApp.RemoveModalDialogEventsHandler(handler);
            MSApp.CommandState.StartDefaultCommand();
            return tempSnapshotPath;
        }

        /// <summary>
        /// Generate Viewpoint
        /// </summary>
        /// <param name="elemCheck"></param>
        /// <returns></returns>
        private VisualizationInfo generateViewpoint(int elemCheck)
        {
            try
            {
                // get current view
                View currentView = MSApp.ActiveDesignFile.Views[getActiveViewNumber()];
                double unitFactor = 1 / GetGunits();

                // enable perspective camera back and forth to get correct view attributes, see the post below
                // https://communities.bentley.com/products/programming/microstation_programming/f/343173/t/80064
                MSApp.CadInputQueue.SendKeyin("MDL KEYIN BENTLEY.VIEWATTRIBUTESDIALOG,VAD VIEWATTRIBUTESDIALOG SETATTRIBUTE 0 Camera True");
                MSApp.CadInputQueue.SendKeyin("MDL KEYIN BENTLEY.VIEWATTRIBUTESDIALOG,VAD VIEWATTRIBUTESDIALOG SETATTRIBUTE 0 Camera False");

                // force view center to be identical as camera target
                Point3d center = new Point3d();
                center = currentView.get_Center();
                Point3d extents = new Point3d();
                extents = currentView.get_Extents();
                Point3d translation = new Point3d();
                translation = MSApp.Point3dSubtract(center, currentView.get_CameraTarget());
                ViewCameraParameters vcp = new ViewCameraParametersClass();
                vcp.set_CameraPosition(MSApp.Point3dAdd(currentView.get_CameraPosition(), translation));
                vcp.set_CameraTarget(MSApp.Point3dAdd(currentView.get_CameraTarget(), translation));
                currentView.SetCameraProperties(vcp);
                currentView.set_Extents(extents);
                currentView.set_Center(center);
                currentView.Redraw();
                
                // camera direction
                Point3d direction = MSApp.Point3dNormalize(MSApp.Point3dSubtract(currentView.get_CameraTarget(), currentView.get_CameraPosition()));

                // camera scale
                double h = currentView.get_Extents().Y * unitFactor;
                double w = currentView.get_Extents().X *unitFactor;
                double fov = 180 * currentView.CameraAngle / Math.PI;

                // camera location
                Point3d cameraLocation = MSApp.Point3dScale(currentView.get_CameraPosition(), unitFactor);

                // camera up vector
                Point3d upVector = currentView.get_CameraUpVector();

                // set up BCF viewpoint
                VisualizationInfo v = new VisualizationInfo();

                // FIXME: ignore perspective view for now
                /*if (currentView.isPerspective)
                {
                    v.PerspectiveCamera = new PerspectiveCamera();
                    v.PerspectiveCamera.CameraViewPoint.X = cameraLocation.X;
                    v.PerspectiveCamera.CameraViewPoint.Y = cameraLocation.Y;
                    v.PerspectiveCamera.CameraViewPoint.Z = cameraLocation.Z;
                    v.PerspectiveCamera.CameraUpVector.X = upVector.X;
                    v.PerspectiveCamera.CameraUpVector.Y = upVector.Y;
                    v.PerspectiveCamera.CameraUpVector.Z = upVector.Z;
                    v.PerspectiveCamera.CameraDirection.X = direction.X;
                    v.PerspectiveCamera.CameraDirection.Y = direction.Y;
                    v.PerspectiveCamera.CameraDirection.Z = direction.Z;
                    v.PerspectiveCamera.FieldOfView = fov;
                }
                else
                {*/
                    v.OrthogonalCamera = new OrthogonalCamera();
                    v.OrthogonalCamera.CameraViewPoint.X = cameraLocation.X;
                    v.OrthogonalCamera.CameraViewPoint.Y = cameraLocation.Y;
                    v.OrthogonalCamera.CameraViewPoint.Z = cameraLocation.Z;
                    v.OrthogonalCamera.CameraUpVector.X = upVector.X;
                    v.OrthogonalCamera.CameraUpVector.Y = upVector.Y;
                    v.OrthogonalCamera.CameraUpVector.Z = upVector.Z;
                    v.OrthogonalCamera.CameraDirection.X = direction.X;
                    v.OrthogonalCamera.CameraDirection.Y = direction.Y;
                    v.OrthogonalCamera.CameraDirection.Z = direction.Z;
                    v.OrthogonalCamera.ViewToWorldScale = h;
                //}

                return v;
            }
            catch (System.Exception ex1)
            {
                MessageBox.Show("exception: " + ex1, "Error!");
            }
            return null;
        }

        /// <summary>
        /// Open a 3D View
        /// </summary>
        /// <param name="v"></param>
        private void doOpen3DView(VisualizationInfo v)
        {
            try
            {
                if (MSApp.ActiveModelReference.Type != MsdModelType.Normal)
                {
                    MessageBox.Show("This operation is not allowed in paper space.\nPlease go to model space and retry.",
                        "Warning!", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                Point3d cameraPos, viewDirection, upVector;
                double zoomValue;

                // IS ORTHOGONAL
                if (v.OrthogonalCamera != null)
                {
                    if (v.OrthogonalCamera.CameraViewPoint == null || v.OrthogonalCamera.CameraUpVector == null || v.OrthogonalCamera.CameraDirection == null)
                        return;

                    cameraPos = MSApp.Point3dFromXYZ(v.OrthogonalCamera.CameraViewPoint.X, v.OrthogonalCamera.CameraViewPoint.Y, v.OrthogonalCamera.CameraViewPoint.Z);
                    viewDirection = MSApp.Point3dNormalize(MSApp.Point3dFromXYZ(v.OrthogonalCamera.CameraDirection.X, v.OrthogonalCamera.CameraDirection.Y, v.OrthogonalCamera.CameraDirection.Z));
                    upVector = MSApp.Point3dNormalize(MSApp.Point3dFromXYZ(v.OrthogonalCamera.CameraUpVector.X, v.OrthogonalCamera.CameraUpVector.Y, v.OrthogonalCamera.CameraUpVector.Z));
                    zoomValue = v.OrthogonalCamera.ViewToWorldScale;
                }
                else if (v.PerspectiveCamera != null)
                {
                    if (v.PerspectiveCamera.CameraViewPoint == null || v.PerspectiveCamera.CameraUpVector == null || v.PerspectiveCamera.CameraDirection == null)
                        return;

                    cameraPos = MSApp.Point3dFromXYZ(v.PerspectiveCamera.CameraViewPoint.X, v.PerspectiveCamera.CameraViewPoint.Y, v.PerspectiveCamera.CameraViewPoint.Z);
                    viewDirection = MSApp.Point3dNormalize(MSApp.Point3dFromXYZ(v.PerspectiveCamera.CameraDirection.X, v.PerspectiveCamera.CameraDirection.Y, v.PerspectiveCamera.CameraDirection.Z));
                    upVector = MSApp.Point3dNormalize(MSApp.Point3dFromXYZ(v.PerspectiveCamera.CameraUpVector.X, v.PerspectiveCamera.CameraUpVector.Y, v.PerspectiveCamera.CameraUpVector.Z));
                    zoomValue = v.PerspectiveCamera.FieldOfView;
                }
                else
                {
                    MessageBox.Show("No camera information was found within this viewpoint.",
                        "Warning!", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // get current view
                View currentView = MSApp.ActiveDesignFile.Views[getActiveViewNumber()];
                double unitFactor = GetGunits();                

                // set camera properties
                Point3d scaledCameraPos = MSApp.Point3dScale(cameraPos, unitFactor);
                Point3d scaledCameraTarget = MSApp.Point3dAdd(scaledCameraPos, MSApp.Point3dScale(viewDirection, unitFactor));
                currentView.set_CameraPosition(scaledCameraPos);
                currentView.set_CameraTarget(scaledCameraTarget);
                currentView.set_Center(scaledCameraTarget);
                currentView.set_CameraUpVector(upVector);                               
                if (v.PerspectiveCamera != null)
                {
                    currentView.CameraAngle = zoomValue * unitFactor;
                }
                else if (v.OrthogonalCamera != null)
                {
                    Point3d currentExtent = currentView.get_Extents();                    
                    currentView.Zoom(zoomValue * unitFactor / currentExtent.Y);                    
                }
                
                // redraw current view
                currentView.Redraw();
            }
            catch (System.Exception ex1)
            {
                MessageBox.Show("exception: " + ex1, "Error!");
            }
        }

        private double GetGunits()
        {
            string units = MSApp.ActiveModelReference.get_MasterUnit().Label;
            double factor = 1;
            switch (units)
            {
                case "cm":
                    factor = 100;
                    break;
                case "ft":
                    factor = 3.28084;
                    break;
                case "im":
                    factor = 39.3701;
                    break;
                case "km":
                    factor = 0.001;
                    break;
                case "m":
                    factor = 1;
                    break;
                case "um":
                    factor = 1000000;
                    break;
                case "mi":
                    factor = 0.000621371;
                    break;
                case "mm":
                    factor = 1000;
                    break;
                case "mil":
                    factor = 39370.0787;
                    break;
                case "yd":
                    factor = 1.09361;
                    break;
                default:
                    MessageBox.Show("Units " + units + " not recognized.");
                    factor = 1;
                    break;
            }
            return factor;
        }

        public string getBentleyProductName()
        {
            string windowTitle = MSApp.Caption;
            int lastIndexOfDash = windowTitle.LastIndexOf('-');
            return windowTitle.Substring(lastIndexOfDash + 1).Trim();
        }

        /// <summary>
        /// Get System Architecture
        /// </summary>
        /// <returns></returns>
        public static string ProgramFilesx86()
        {
            if (8 == IntPtr.Size || (!String.IsNullOrEmpty(Environment.GetEnvironmentVariable("PROCESSOR_ARCHITEW6432"))))
                return Environment.GetEnvironmentVariable("ProgramFiles(x86)");

            return Environment.GetEnvironmentVariable("ProgramFiles");
        }
    }

}
