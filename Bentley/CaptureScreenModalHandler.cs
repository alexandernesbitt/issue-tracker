using Bentley.Interop.MicroStationDGN;
#if select
using BMI = Bentley.MicroStation.InteropServices;
#elif connect
using BMI = Bentley.MstnPlatformNET.InteropServices;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ARUP.IssueTracker.Bentley
{
    public class CaptureScreenModalHandler : IModalDialogEvents
    {
        private string tempFolder;
        private string tempFilename;
        private int activeViewNumber;

        public CaptureScreenModalHandler(string tempFilePath, int activeViewNumber) 
        {
            this.tempFolder = Path.GetDirectoryName(tempFilePath);
            this.tempFilename = Path.GetFileName(tempFilePath);
            this.activeViewNumber = activeViewNumber;
        }

        public void OnDialogClosed(string DialogBoxName, MsdDialogBoxResult DialogResult)
        {
            // do nothing
        }

        public void OnDialogOpened(string DialogBoxName, ref MsdDialogBoxResult DialogResult)
        {
            if (DialogBoxName == "Save Image")
            {
                BMI.Utilities.ComApp.SetCExpressionValue("msDialogState.modalData.saveImage.viewNum", activeViewNumber-1, "MPTOOLS");
                BMI.Utilities.ComApp.SetCExpressionValue("msDialogState.modalData.saveImage.format", 28, "MPTOOLS");
                BMI.Utilities.ComApp.SetCExpressionValue("msDialogState.modalData.saveImage.colorMode", 1, "MPTOOLS");
                BMI.Utilities.ComApp.SetCExpressionValue("msDialogState.modalData.saveImage.renderMode", 3, "MPTOOLS");
                BMI.Utilities.ComApp.SetCExpressionValue("msDialogState.modalData.saveImage.size.x", 1000, "MPTOOLS");
                BMI.Utilities.ComApp.SetCExpressionValue("msDialogState.modalData.saveImage.flags.freeAspect", 0, "MPTOOLS");
                DialogResult = MsdDialogBoxResult.OK;
            }
            else if (DialogBoxName == "Save Image As")
            {
                BMI.Utilities.ComApp.CadInputQueue.SendCommand(string.Format("MDL COMMAND MGDSHOOK,fileList_setDirectoryCmd {0}", tempFolder));
                BMI.Utilities.ComApp.CadInputQueue.SendCommand(string.Format("MDL COMMAND MGDSHOOK,fileList_setFileNameCmd {0}", tempFilename));
                DialogResult = MsdDialogBoxResult.OK;
            }
        }
    }
}
