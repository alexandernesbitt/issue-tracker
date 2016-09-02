using System.Windows;

namespace ARUP.IssueTracker.Windows
{
    /// <summary>
    /// Interaction logic for SnapWin.xaml
    /// </summary>
    /// 

    public partial class SnapWin : Window
    {
        public string snapshot;
        public SnapWin(string snapshotPath, string windowTitle)
        {
            snapshot = snapshotPath;
            
            InitializeComponent();
            DataContext = snapshot;
            this.Title = windowTitle;

            //  Converters.UriToImageConv ui = new Converters.UriToImageConv();
            // snap.Source = (BitmapImage)ui.Convert(snapshot,typeof(String),null,null);
        }
    }
}
