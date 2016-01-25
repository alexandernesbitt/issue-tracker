using System;
using System.Windows;
using System.Windows.Controls;
using ARUP.IssueTracker.Classes;

namespace ARUP.IssueTracker.UserControls
{
    /// <summary>
    /// Interaction logic for MainPanel.xaml
    /// </summary>
    public partial class JiraPanel : UserControl
    {
        public event EventHandler<IntArg> ComponentsShowBCFEH;

        public JiraPanel()
        {
            InitializeComponent();

        }
        public int projIndex
        {
            get { return projCombo.SelectedIndex; }
            set { projCombo.SelectedIndex = value; }
        }
       

        public int listIndex
        {
            get { return issueList.SelectedIndex; }
            set { issueList.SelectedIndex = value; }
        }
        private void ComponentsShow(object sender, RoutedEventArgs e)
        {

            try
            {
                if (issueList.SelectedIndex != -1)
                {
                    if (ComponentsShowBCFEH != null)
                    {
                        ComponentsShowBCFEH(this, new IntArg(issueList.SelectedIndex));
                    }
                }
            }
            catch (System.Exception ex1)
            {
                MessageBox.Show("exception: " + ex1);
            }
        }

        public string Filters
        {
            get
            {
                string filters = "";
                if (customFilters.IsEnabled)
                {
                    filters = statusfilter.Result + typefilter.Result + priorityfilter.Result;
                }
                return filters;
            }

        }

        public string Assignation
        {
            get
            {
                string assignation = "";
                if (unassignedRadioButton.IsChecked.Value)
                {
                    assignation = "+AND+assignee=EMPTY";
                }
                else if (assignedRadioButton.IsChecked.Value)
                {
                    assignation = "+AND+assignee!=EMPTY";
                }
                else if (assignedToMeRadioButton.IsChecked.Value)
                {
                    assignation = "+AND+assignee=currentUser()";
                }
                else if (allAssignationRadioButton.IsChecked.Value)
                {
                    assignation = "";
                }
                return assignation;
            }
        }

        public string Creator
        {
            get
            {
                string creator = "";
                if (meCreatorRadioButton.IsChecked.Value)
                {
                    creator = "+AND+creator=currentUser()";
                }
                else if (othersCreatorRadioButton.IsChecked.Value)
                {
                    creator = "+AND+creator!=currentUser()";
                }
                else if (allCreatorRadioButton.IsChecked.Value)
                {
                    creator = "";
                }
                
                return creator;
            }
        }

        public string Order
        {
            get
            {
                string ordertype = "DESC";
                string orderdate = "Updated";
                foreach (RadioButton rb in grouptype.Children)
                {
                    if (rb.IsChecked.Value)
                        ordertype = rb.Content.ToString();

                }
                foreach (RadioButton rb in groupdate.Children)
                {
                    if (rb.IsChecked.Value)
                    {
                        orderdate = rb.Content.ToString();
                    }

                }

                return "+ORDER+BY+" + orderdate + "+" + ordertype;
            }

        }
        public void clearFilters_Click(object sender, RoutedEventArgs e)
        {
            statusfilter.Clear();
            typefilter.Clear();
            priorityfilter.Clear();
           
                //IM.getIssues();
        }
        private void ChangeStatus_Click(object sender, RoutedEventArgs e)
        {
            //   IM.ChangeStatus_Click();
        }
        private void ChangePriority_Click(object sender, RoutedEventArgs e)
        {
            //  IM.ChangePriority_Click();
        }
       
        
        private void ChangeType_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ChangeAssign_Click(object sender, RoutedEventArgs e)
        {

        }


        
    }
       
}
