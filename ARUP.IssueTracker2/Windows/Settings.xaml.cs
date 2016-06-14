using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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
            DialogResult = true;
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void DataGridCell_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

            DataGridCell cell = sender as DataGridCell;

            if (!cell.IsEditing)
            {
                // enables editing on single click

                if (!cell.IsFocused)

                    cell.Focus();

                if (!cell.IsSelected)

                    cell.IsSelected = true;
            }

        }


    }
}
