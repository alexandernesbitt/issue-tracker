using ARUP.IssueTracker.Classes;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ARUP.IssueTracker.Windows
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {

        public Settings()
        {            
            InitializeComponent();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            List<JiraAccount> accounts = jiraAccounts.ItemsSource as List<JiraAccount>;
            if (accounts.FindAll(ac => ac.active).Count > 1)
            {
                MessageBox.Show("You can set only one active account!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            DialogResult = true;
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void DataGridCell_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DataGridCell cell = sender as DataGridCell;

            // Set focus for single click editing
            if (cell != null && !cell.IsEditing && !cell.IsReadOnly)
            {
                if (!cell.IsFocused)
                {
                    cell.Focus();
                }
                DataGrid dataGrid = FindParent<DataGrid>(cell);
                if (dataGrid != null)
                {
                    if (dataGrid.SelectionUnit != DataGridSelectionUnit.FullRow)
                    {
                        if (!cell.IsSelected)
                            cell.IsSelected = true;
                    }
                    else
                    {
                        DataGridRow row = FindParent<DataGridRow>(cell);
                        if (row != null && !row.IsSelected)
                        {
                            row.IsSelected = true;
                        }
                    }
                }
            }
        }

        private void PasswordBox_LostFocus_1(object sender, RoutedEventArgs e)
        {
            PasswordBox pb = sender as PasswordBox;
            int currentIndex = FindParent<DataGridRow>(pb).GetIndex();
            ((JiraAccount)(jiraAccounts.Items[currentIndex])).password = pb.Password;
        }

        private T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            //get parent item
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            //we've reached the end of the tree
            if (parentObject == null) return null;

            //check if the parent matches the type we're looking for
            T parent = parentObject as T;
            if (parent != null)
                return parent;
            else
                return FindParent<T>(parentObject);
        }

    }
}
