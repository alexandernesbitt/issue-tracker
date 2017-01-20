using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ARUP.IssueTracker.Classes;
using ARUP.IssueTracker.Classes.JIRA;

namespace ARUP.IssueTracker.Windows
{
    /// <summary>
    /// Interaction logic for ChangeAssignee.xaml
    /// </summary>
    public partial class ChangeAssignee : Window
    {
        public readonly CollectionViewSource viewSource = new CollectionViewSource();
        public ChangeAssignee()
        {
            InitializeComponent();
        }

        public void SetList(List<User> assignees)
        {
            viewSource.Source = assignees;

            Binding binding = new Binding();
            binding.Source = viewSource;
            BindingOperations.SetBinding(valuesList, ListView.ItemsSourceProperty, binding);
        }

        public void SetWatchers(List<Watcher> watchers)
        {
            valuesList.SelectedItems.Clear();
            var users = viewSource.Source as List<User>;
            watchers.ForEach(watcher => {
                if (!string.IsNullOrWhiteSpace(watcher.name))
                {
                    var user = users.Find(u => u.name == watcher.name);
                    if (user != null)
                    {
                        valuesList.SelectedItems.Add(user);
                    }
                }                
            });            
        }
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void Search_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            viewSource.Filter += SearchText;
            viewSource.Filter += SearchText;
        }
        private void SearchText(object sender, System.Windows.Data.FilterEventArgs e)
        {
            // see Notes on Filter Methods:
            var src = e.Item as User;
            if (src == null)
                e.Accepted = false;
            else if (src.displayName != null && !(src.displayName).ToLowerInvariant().Contains(search.Text.ToLowerInvariant())
                && src.emailAddress != null && !(src.emailAddress).ToLowerInvariant().Contains(search.Text.ToLowerInvariant())
                && src.name != null && !(src.name).ToLowerInvariant().Contains(search.Text.ToLowerInvariant()))// here is FirstName a Property in my YourCollectionItem
                e.Accepted = false;
          
        }
    }
}
