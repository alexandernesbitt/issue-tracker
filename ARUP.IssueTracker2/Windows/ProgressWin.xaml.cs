using System;
using System.Windows;
using System.Windows.Threading;

namespace ARUP.IssueTracker.Windows
{
    /// <summary>
    /// Interaction logic for ProgressWin.xaml
    /// </summary>
    public partial class ProgressWin : Window
    {
        public event EventHandler killWorker;

        // for updating progress window
        private static Action EmptyDelegate = delegate() { };

        public ProgressWin()
        {
            InitializeComponent();
        }

        public void SetProgress(int i, string s) {
            progress.Value = i;
            taskProgress.Content = s;
            progress.Dispatcher.Invoke(DispatcherPriority.Input, EmptyDelegate);
        }

        private void cancelBtn_Click(object sender, RoutedEventArgs e)
        {
            if (killWorker != null)
            {
                killWorker(this, e);
                this.Close();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (killWorker != null)
            {
                killWorker(this, e);
            }
        }
    }
}
