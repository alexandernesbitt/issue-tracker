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
using System.ComponentModel;

namespace ARUP.IssueTracker.Windows
{
    /// <summary>
    /// Interaction logic for ChangeAssignee.xaml
    /// </summary>
    public partial class ChangeAssignee : Window
    {
        public readonly CollectionViewSource viewSource = new CollectionViewSource();
        private List<Watcher> watchers;

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
            this.watchers = watchers;
            RefreshWatchers();                       
        }

        private void RefreshWatchers() 
        {
            valuesList.SelectedItems.Clear();
            var users = viewSource.Source as List<User>;
            watchers.ForEach(watcher =>
            {
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
            try
            {
                TextBox t = (TextBox)sender;
                string filter = t.Text;
                ICollectionView cv = CollectionViewSource.GetDefaultView(valuesList.ItemsSource);
                if (filter == string.Empty || filter == null)
                {
                    cv.Filter = null;
                }
                else
                {
                    cv.Filter = o =>
                    {
                        User user = o as User;
                        if (user == null)
                        {
                            return false;
                        }
                        bool displayNameFilter = (user.displayName == null) ? false : (user.displayName).ToLowerInvariant().Contains(search.Text.ToLowerInvariant());
                        bool emailFilter = (user.emailAddress == null) ? false : (user.emailAddress).ToLowerInvariant().Contains(search.Text.ToLowerInvariant());
                        bool nameFilter = (user.name == null) ? false : (user.name).ToLowerInvariant().Contains(search.Text.ToLowerInvariant());
                        return (displayNameFilter || emailFilter || nameFilter);
                    };
                }
                if (watchers != null)
                {
                    RefreshWatchers();
                }                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}
