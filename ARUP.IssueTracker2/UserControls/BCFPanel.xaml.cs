using System;
using System.Windows;
using System.Windows.Controls;
using ARUP.IssueTracker.Classes;
using ARUP.IssueTracker.Classes.BCF2;
using System.Linq;

namespace ARUP.IssueTracker.UserControls
{
    /// <summary>
    /// Interaction logic for BCFPanel.xaml
    /// </summary>
    public partial class BCFPanel : UserControl
    {
        //public event EventHandler<IntArg> ComponentsShowBCFEH;
        //public event EventHandler<StringArg> OpenImageEH;

        public event RoutedEventHandler open3dViewEvent;
        public MainPanel mainPanel = null;
        public bool isShowBcfFirstViewpointButtons = false;

        public BCFPanel()
        {
            InitializeComponent();
        }

        //private void ComponentsShow(object sender, RoutedEventArgs e)
        //{

        //    try
        //    {
        //        if (issueList.SelectedIndex != -1)
        //        {
        //            if (ComponentsShowBCFEH != null)
        //            {
        //                ComponentsShowBCFEH(this, new IntArg(issueList.SelectedIndex));
        //            }
        //        }
        //    }
        //    catch (System.Exception ex1)
        //    {
        //        MessageBox.Show("exception: " + ex1);
        //    }
        //}

        //private void OpenImage(object sender, RoutedEventArgs e)
        //{
        //    if (OpenImageEH != null)
        //    {
        //        OpenImageEH(this, new StringArg((string)((Button)sender).Tag));
        //    }
        //}
        public int listIndex
        {
            get { return issueList.SelectedIndex; }
            set { issueList.SelectedIndex = value; }
        }

        private void popup_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.Primitives.Popup popup = sender as System.Windows.Controls.Primitives.Popup;
            if (popup != null)
                popup.IsOpen = false;
        }

        private void OpenImageBtn_Click(object sender, RoutedEventArgs e)
        {
            if (mainPanel != null)
            {
                mainPanel.OpenImage(sender, e);
            }
        }

        private void showComponents_Click(object sender, RoutedEventArgs e)
        {
            if (mainPanel != null)
            {
                mainPanel.ComponentsShowBCF(sender, e);
            }
        }

        private void open3dView_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (mainPanel != null && open3dViewEvent != null)
                {
                    open3dViewEvent(sender, e);
                }
                else
                {
                    MessageBox.Show("3D views can only be opened in Revit or Navisworks", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void OpenLinkProjBtn_Click(object sender, RoutedEventArgs e)
        {
            string url = (string)((Button)sender).Tag;
            System.Diagnostics.Process.Start(url);
        }
    }
}
