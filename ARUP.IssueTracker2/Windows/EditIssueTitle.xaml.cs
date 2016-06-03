using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ARUP.IssueTracker.Windows
{
    /// <summary>
    /// Edit issue title for Jita
    /// </summary>
    public partial class EditIssueTitle : Window
    {
        public EditIssueTitle(string currentTitle)
        {          
            InitializeComponent();
            issueTitleTextBox.Text = currentTitle;
        }

        private void OKBtnClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(issueTitleTextBox.Text))
            {
                MessageBox.Show("Please write an issue title.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            DialogResult = true;
        }

        private void CancelBtnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
