using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ARUP.IssueTracker.Windows;
using System.Windows;
using System.IO;
using ARUP.IssueTracker.Classes.BCF2;
using System.Xml.Serialization;
using System.Windows.Controls;

namespace ARUP.IssueTracker.Civil3D
{
    public class CommentController : ICommentController
    {
        private Civil3DWindow window;

        public CommentController(Civil3DWindow window) 
        {
            this.window = window;
        }

        public override Tuple<string, string> getSnapshotAndViewpoint(int elemCheck)
        {
            if (false)
            {
                MessageBox.Show("I'm sorry,\nonly 3D and 2D views are supported.", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }

            string tempPath = Path.Combine(Path.GetTempPath(), "BCFtemp", Path.GetRandomFileName());
            string issueFolder = Path.Combine(tempPath, Guid.NewGuid().ToString());
            if (!Directory.Exists(issueFolder))
                Directory.CreateDirectory(issueFolder);

            Guid viewpointGuid = Guid.NewGuid();
            string snapshotFilePath = Path.Combine(issueFolder, string.Format("{0}.png", viewpointGuid.ToString()));
            string viewpointFilePath = Path.Combine(issueFolder, string.Format("{0}.bcfv", viewpointGuid.ToString()));

            // save snapshot

            // save viewpoint
            VisualizationInfo vi = window.generateViewpoint(elemCheck);
            XmlSerializer serializerV = new XmlSerializer(typeof(VisualizationInfo));
            Stream writerV = new FileStream(viewpointFilePath, FileMode.Create);
            serializerV.Serialize(writerV, vi);
            writerV.Close();

            return new Tuple<string, string>(snapshotFilePath, viewpointFilePath);
        }
    
        public override void comboVisuals_SelectionChanged(object sender, SelectionChangedEventArgs e, AddComment addCommentWindow)
        {
            try
            {
                System.Windows.Controls.ComboBox comboVisuals = sender as System.Windows.Controls.ComboBox;
                
                addCommentWindow.captureModelViewpointButton_Click(null, null);
            }
            catch (System.Exception ex1)
            {
                
            }
        }
}
}
