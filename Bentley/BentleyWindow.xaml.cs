using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.IO;
using ARUP.IssueTracker.Classes;
using ARUP.IssueTracker.Classes.BCF2;
using System.ComponentModel;
using ARUP.IssueTracker.Windows;
using System.Windows.Controls;
using System.Text;
using BCOM = Bentley.Interop.MicroStationDGN;
using BM = Bentley.MicroStation;
using BMI = Bentley.MicroStation.InteropServices;

namespace ARUP.IssueTracker.Bentley
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
    public partial class BentleyWindow : Window
  {
    private CommentController commentController;
    private ComponentController componentController;
    private BCOM.Application MSApp;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="_uiapp"></param>
    /// <param name="exEvent"></param>
    /// <param name="handler"></param>
    public BentleyWindow(BCOM.Application MSApp)
    {
      try
      {
        InitializeComponent();
        this.Title = AITPlugin.getBentleyProductName() + " | Arup Issue Tracker by CASE";

        mainPan.jiraPan.AddIssueBtn.Click += new RoutedEventHandler(AddIssueJira);
        mainPan.bcfPan.AddIssueBtn.Click += new RoutedEventHandler(AddIssueBCF);

        mainPan.bcfPan.open3dViewEvent += new RoutedEventHandler(Open3dViewBCF);
        mainPan.jiraPan.open3dViewEvent += new RoutedEventHandler(Open3dViewJira);

        // for ICommentController callback
        commentController = new CommentController(this);
        commentController.client = AuthoringTool.Bentley;
        mainPan.jiraPan.AddCommBtn.Tag = commentController;
        mainPan.bcfPan.AddCommBtn.Tag = commentController;

        // for IComponentController
        componentController = new ComponentController(this);
        mainPan.componentController = this.componentController;

        // for open 3d view and show components
        //mainPan.jiraPan.open3dView.Visibility = System.Windows.Visibility.Visible;
        //mainPan.jiraPan.showComponents.Visibility = System.Windows.Visibility.Visible;
        //mainPan.bcfPan.isShowBcfFirstViewpointButtons = true;

        this.MSApp = MSApp;
      }

      catch (System.Exception ex1)
      {
        MessageBox.Show("exception: " + ex1);
      }

    }

    /// <summary>
    /// Add Issue to Jira
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void AddIssueJira(object sender, EventArgs e)
    {
      try
      {
          string path = Path.Combine(Path.GetTempPath(), "BCFtemp", Path.GetRandomFileName());
          Tuple<Markup, Issue> tup = AddIssue(path, false);
          if (tup == null)
              return;
          Markup issue = tup.Item1;
          Issue issueJira = tup.Item2;

          List<Markup> issues = new List<Markup>();
          List<Issue> issuesJira = new List<Issue>();

          issues.Add(issue);
          issuesJira.Add(issueJira);

          if (issue != null)
              mainPan.doUploadIssue(issues, path, true, mainPan.jiraPan.projIndex, issuesJira);
      }

      catch (System.Exception ex1)
      {
        MessageBox.Show("exception: " + ex1);
      }

    }

    /// <summary>
    /// Add Issue to BCF
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void AddIssueBCF(object sender, EventArgs e)
    {
      try
      {
          Tuple<Markup, Issue> tup = AddIssue(mainPan.jira.Bcf.TempPath, true);
        if (tup == null)
          return;
        Markup issue = tup.Item1;

          if (issue != null)
          {
              mainPan.jira.Bcf.Issues.Add(issue);
              mainPan.jira.Bcf.HasBeenSaved = false;
              mainPan.bcfPan.issueList.SelectedIndex = mainPan.jira.Bcf.Issues.Count - 1;
          }
      }

      catch (System.Exception ex1)
      {
        MessageBox.Show("exception: " + ex1);
      }
    }

    /// <summary>
    /// Add Issue
    /// </summary>
    /// <param name="path"></param>
    /// <param name="isBcf"></param>
    /// <returns></returns>
    private Tuple<Markup, Issue> AddIssue(string path, bool isBcf)
    {
      try
      {
        if (MSApp.ActiveModelReference.Type != BCOM.MsdModelType.Normal)
        {
            MessageBox.Show("Please switch to model space as paper space is not supported.", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            return null;
        }

        Markup issue = new Markup(DateTime.Now);

        string folderIssue = Path.Combine(path, issue.Topic.Guid);
        if (!Directory.Exists(folderIssue))
          Directory.CreateDirectory(folderIssue);

        var types = new ObservableCollection<Issuetype>();
        var assignees = new List<User>();
        var components = new ObservableCollection<IssueTracker.Classes.Component>();
        var priorities = new ObservableCollection<Priority>();
        var noCom = true;
        var noPrior = true;
        var noAssign = true;

        if (!isBcf)
        {
            types = mainPan.jira.TypesCollection;
            assignees = mainPan.getAssigneesIssue();
            components = mainPan.jira.ComponentsCollection;
            priorities = mainPan.jira.PrioritiesCollection;
            noCom =
                mainPan.jira.ProjectsCollection[mainPan.jiraPan.projIndex].issuetypes[0].fields.components ==
                null;
            noPrior =
                mainPan.jira.ProjectsCollection[mainPan.jiraPan.projIndex].issuetypes[0].fields.priority ==
                null;
            noAssign =
                mainPan.jira.ProjectsCollection[mainPan.jiraPan.projIndex].issuetypes[0].fields.assignee ==
                null;

        }

        // get snapshot by AITSNAPSHOT command
        string snapshotPath = Path.Combine(folderIssue, "snapshot.png");
        // FIXME: add snapshot

        AddIssueBentley aic = new AddIssueBentley(MSApp, folderIssue, types, assignees, components, priorities, noCom, noPrior, noAssign);
        aic.Title = "Add Jira Issue";
        if (!isBcf)
        {
            aic.JiraFieldsBox.Visibility = System.Windows.Visibility.Visible;
            aic.VerbalStatus.Visibility = System.Windows.Visibility.Collapsed;
            aic.BcfFieldsBox.Visibility = System.Windows.Visibility.Collapsed;
        }
        else 
        {
            aic.JiraFieldsBox.Visibility = System.Windows.Visibility.Collapsed;
            aic.BcfFieldsBox.Visibility = System.Windows.Visibility.Visible;
        }

        aic.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
        aic.ShowDialog();
          if (aic.DialogResult.HasValue && aic.DialogResult.Value)
          {
              ViewPoint vp = new ViewPoint(true);
              vp.SnapshotPath = snapshotPath;
              int elemCheck = 2;
              //if (aic.all.IsChecked.Value)
              //    elemCheck = 0;
              //else if (aic.selected.IsChecked.Value)
              //    elemCheck = 1;
              vp.VisInfo = generateViewpoint(elemCheck);

              //Add annotations for description with snapshot/viewpoint
              StringBuilder descriptionText = new StringBuilder();
              if (!string.IsNullOrWhiteSpace(aic.CommentBox.Text))
              {
                  descriptionText.AppendLine(aic.CommentBox.Text);
              }
              if (!isBcf) 
              {
                  if (vp.VisInfo != null)
                  {
                      descriptionText.AppendLine(string.Format("{{anchor:<Viewpoint>[^{0}]</Viewpoint>}}", "viewpoint.bcfv"));
                  }
                  if (!string.IsNullOrWhiteSpace(vp.SnapshotPath))
                  {
                      descriptionText.AppendLine(string.Format("!{0}|thumbnail!", "snapshot.png"));
                      descriptionText.AppendLine(string.Format("{{anchor:<Snapshot>[^{0}]</Snapshot>}}", "snapshot.png"));
                  }
              }              

              Issue issueJira = new Issue();
              if (!isBcf)
              {
                  issueJira.fields = new Fields();
                  issueJira.fields.description = descriptionText.ToString().Trim();
                  issueJira.fields.issuetype =  (Issuetype) aic.issueTypeCombo.SelectedItem;
                  issueJira.fields.priority = (Priority) aic.priorityCombo.SelectedItem;
                  if (!string.IsNullOrEmpty(aic.ChangeAssign.Content.ToString()) &&
                      aic.ChangeAssign.Content.ToString() != "none")
                  {
                      issueJira.fields.assignee = new User();
                      issueJira.fields.assignee.name = aic.ChangeAssign.Content.ToString();
                  }

                  if (aic.SelectedComponents != null && aic.SelectedComponents.Any())
                  {
                      issueJira.fields.components = aic.SelectedComponents;
                  }

                  if (!string.IsNullOrWhiteSpace(aic.JiraLabels.Text))
                  {
                      var labels = aic.JiraLabels.Text.Split(',').ToList();
                      labels.ForEach(s => s.Trim());
                      issueJira.fields.labels = labels;
                  }
              }
              
              issue.Viewpoints.Add(vp);
              issue.Topic.Title = aic.TitleBox.Text;
              issue.Topic.Description = descriptionText.ToString().Trim();
              issue.Topic.AssignedTo = aic.BcfAssignee.Text;
              issue.Topic.CreationAuthor = string.IsNullOrWhiteSpace(MySettings.Get("username")) ? MySettings.Get("BCFusername") : MySettings.Get("username");
              issue.Topic.Priority = aic.BcfPriority.Text;
              issue.Topic.TopicStatus = aic.VerbalStatus.Text;
              issue.Topic.TopicType = aic.BcfIssueType.Text;
              if (!string.IsNullOrWhiteSpace(aic.BcfLabels.Text))
              {
                  var labels = aic.BcfLabels.Text.Split(',').ToList();
                  labels.ForEach(s => s.Trim());
                  issue.Topic.Labels = labels.ToArray();
              }

              // FIXME: issue.Header[0].IfcProject = 
              issue.Header[0].Filename = MSApp.ActiveDesignFile.FullName;
              issue.Header[0].Date = DateTime.Now;

              return new Tuple<Markup, Issue>(issue, issueJira);
          }
          else
          {
              mainPan.DeleteDirectory(folderIssue);
          }
           
      }

      catch (System.Exception ex1)
      {
        MessageBox.Show("exception: " + ex1);
      }
      return null;

    }

    /// <summary>
    /// Open 3D View - BCF
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Open3dViewBCF(object sender, EventArgs e)
    {
      try
      {
          VisualizationInfo VisInfo = (VisualizationInfo)((Button)sender).Tag;
          doOpen3DView(VisInfo);
      }
      catch (System.Exception ex1)
      {
        MessageBox.Show("exception: " + ex1);
      }
    }

    /// <summary>
    /// Open 3D View - Jira
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Open3dViewJira(object sender, RoutedEventArgs e)
    {
        try
        {
            string url = (string)((Button)sender).Tag;
            VisualizationInfo v = mainPan.getVisInfo(url);
            if (v == null) 
            {
                MessageBox.Show("Failed to open the viewpoint", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            doOpen3DView(v);
        }
        catch (System.Exception ex1)
        {
            MessageBox.Show("exception: " + ex1);
        }
    }

    /// <summary>
    /// Open a 3D View
    /// </summary>
    /// <param name="v"></param>
    private void doOpen3DView(VisualizationInfo v)
    {
        try
        {
            if (MSApp.ActiveModelReference.Type != BCOM.MsdModelType.Normal)
            {
                MessageBox.Show("Please switch to model space as paper space is not supported.", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // TODO
        }
        catch (System.Exception ex1)
        {
            MessageBox.Show("exception: " + ex1, "Error!");
        }
    }

    #region viewpoint operations

    /// <summary>
    /// Generate Viewpoint
    /// </summary>
    /// <param name="elemCheck"></param>
    /// <returns></returns>
    public VisualizationInfo generateViewpoint(int elemCheck)
    {
        try
        {
            // TODO

            // set up BCF viewpoint
            VisualizationInfo v = new VisualizationInfo();            

            /*if (currentView.PerspectiveEnabled)
            {
                v.PerspectiveCamera = new PerspectiveCamera();
                v.PerspectiveCamera.CameraViewPoint.X = cameraLocation.X;
                v.PerspectiveCamera.CameraViewPoint.Y = cameraLocation.Y;
                v.PerspectiveCamera.CameraViewPoint.Z = cameraLocation.Z;
                v.PerspectiveCamera.CameraUpVector.X = upVector.X;
                v.PerspectiveCamera.CameraUpVector.Y = upVector.Y;
                v.PerspectiveCamera.CameraUpVector.Z = upVector.Z;
                v.PerspectiveCamera.CameraDirection.X = direction.GetNormal().X;
                v.PerspectiveCamera.CameraDirection.Y = direction.GetNormal().Y;
                v.PerspectiveCamera.CameraDirection.Z = direction.GetNormal().Z;
                v.PerspectiveCamera.FieldOfView = 40;
            }
            else 
            {
                v.OrthogonalCamera = new OrthogonalCamera();
                v.OrthogonalCamera.CameraViewPoint.X = cameraLocation.X;
                v.OrthogonalCamera.CameraViewPoint.Y = cameraLocation.Y;
                v.OrthogonalCamera.CameraViewPoint.Z = cameraLocation.Z;
                v.OrthogonalCamera.CameraUpVector.X = upVector.X;
                v.OrthogonalCamera.CameraUpVector.Y = upVector.Y;
                v.OrthogonalCamera.CameraUpVector.Z = upVector.Z;
                v.OrthogonalCamera.CameraDirection.X = direction.GetNormal().X;
                v.OrthogonalCamera.CameraDirection.Y = direction.GetNormal().Y;
                v.OrthogonalCamera.CameraDirection.Z = direction.GetNormal().Z;
                v.OrthogonalCamera.ViewToWorldScale = h;
            }*/

            return v;
        }
        catch (System.Exception ex1)
        {
            MessageBox.Show("exception: " + ex1, "Error!");
        }
        return null;
    }

    #endregion
    
    /// <summary>
    /// passing event to the user control
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Window_Closing(object sender, CancelEventArgs e)
    {
      e.Cancel = mainPan.onClosing(e);
    }

  }
}