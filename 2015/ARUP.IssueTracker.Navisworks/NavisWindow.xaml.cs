using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
//using System.Threading.Tasks;
using System.Windows;
using System.IO;
using System.Windows.Controls;

//using System.Windows.Data;
//using System.Windows.Documents;
//using System.Windows.Input;
//using System.Windows.Navigation;

//Add two new namespaces
using ARUP.IssueTracker.Classes;
using Autodesk.Navisworks.Api;
using ComBridge = Autodesk.Navisworks.Api.ComApi.ComApiBridge;
using ComApi = Autodesk.Navisworks.Api.Interop.ComApi;
using Com = Autodesk.Navisworks.Api.Interop.ComApi;

namespace ARUP.IssueTracker.Navisworks
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class NavisWindow : UserControl
    {
        List<SavedViewpoint> _savedViewpoints = new List<SavedViewpoint>();
        //const double Feet = 3.2808;
        List<ModelItem> _elementList;
        readonly Document _oDoc = Autodesk.Navisworks.Api.Application.ActiveDocument;

        public NavisWindow()
        {
            InitializeComponent();
            //mainPan.jiraPan.AddIssueBtn.Visibility = System.Windows.Visibility.Collapsed;
            mainPan.bcfPan.AddIssueBtn.Click += new RoutedEventHandler(AddIssueBCF);
            mainPan.jiraPan.AddIssueBtn.Click += new RoutedEventHandler(AddIssueJira);
            // mainPan.bcfPan.AddIssueBtn.ToolTip = "Load issues as Saved Viewpoints";

            mainPan.bcfPan.Open3dViewBtn.Click += new RoutedEventHandler(Open3dViewBCF);
            mainPan.jiraPan.Open3dViewBtn.Click += new RoutedEventHandler(Open3dViewJira);

            //enable create saved viewpoint button
            mainPan.bcfPan.CreateSavedViewpointBtn.Visibility = System.Windows.Visibility.Visible;
            mainPan.jiraPan.CreateSavedViewpointBtn.Visibility = System.Windows.Visibility.Visible;
            mainPan.jiraPan.CreateSavedViewpointBtn.Click += new RoutedEventHandler(CreateSavedViewpointJira);
            mainPan.bcfPan.CreateSavedViewpointBtn.Click += new RoutedEventHandler(CreateSavedViewpointBCF);

   
        }
        private void AddIssueJira(object sender, EventArgs e)
        {
            try
            {
                string path = Path.Combine(Path.GetTempPath(), "BCFtemp", Path.GetRandomFileName());
                Tuple<List<IssueBCF>, List<Issue>> tup = AddIssue(path, false);
                if (tup == null)
                    return;
                List<IssueBCF> issues = tup.Item1;
                List<Issue> issuesJira = tup.Item2;

                if (issues != null && issues.Any())
                    mainPan.doUploadIssue(issues, path, true, mainPan.jiraPan.projIndex, issuesJira);
            }

            catch (System.Exception ex1)
            {
                MessageBox.Show("exception: " + ex1);
            }

        }
        private void AddIssueBCF(object sender, EventArgs e)
        {
            try
            {

                //mainPan.NewBCF(null, null);
                //if (mainPan.jira.Bcf != null && !mainPan.jira.Bcf.HasBeenSaved && mainPan.jira.Bcf.Issues.Any())
                //    return;
                Tuple<List<IssueBCF>, List<Issue>> tup = AddIssue(mainPan.jira.Bcf.path, true);
                if (tup == null)
                    return;
                List<IssueBCF> issues = tup.Item1; ;
                //int typeInt = tup.Item2;
                if (issues != null && issues.Any())
                {
                    foreach (var i in issues)
                    {
                        mainPan.jira.Bcf.Issues.Add(i);
                    }
                    //mainPan.jira.Bcf.Issues = new System.Collections.ObjectModel.ObservableCollection<IssueBCF>(issues);
                    mainPan.jira.Bcf.HasBeenSaved = false;
                    //string projFilename = !string.IsNullOrEmpty(_oDoc.FileName) ? System.IO.Path.GetFileNameWithoutExtension(_oDoc.FileName) : "New BCF Report";
                    //mainPan.jira.Bcf.Filename = projFilename;
                }
            }

            catch (System.Exception ex1)
            {
                MessageBox.Show("exception: " + ex1);
            }
        }
        private Tuple<List<IssueBCF>, List<Issue>> AddIssue(string path, bool isBcf)
        {
            try
            {
                // set image export settings
                ComApi.InwOaPropertyVec options = ComBridge.State.GetIOPluginOptions("lcodpimage");
                // configure the option "export.image.format" to export png and image size
                foreach (ComApi.InwOaProperty opt in options.Properties())
                {
                    if (opt.name == "export.image.format")
                        opt.value = "lcodpexpng";
                    if (opt.name == "export.image.width")
                        opt.value = 1600;
                    if (opt.name == "export.image.height")
                        opt.value = 900;

                }

                _savedViewpoints = new List<SavedViewpoint>();

                foreach (SavedItem oSI in _oDoc.SavedViewpoints.ToSavedItemCollection())
                {
                    RecurseItems(oSI);
                }

                var types = new ObservableCollection<Issuetype>();
                var assignees = new List<User>();
                var components = new ObservableCollection<Component>();
                var priorities = new ObservableCollection<Priority>();
                var noCom = true;
                var noPrior = true;
                var noAssign = true;

                if (!isBcf)
                {
                    types = mainPan.jira.TypesCollection;
                    assignees = mainPan.getAssigneesProj();
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



                AddIssueNavis ain = new AddIssueNavis(_savedViewpoints, types, assignees, components, priorities, noCom, noPrior, noAssign);
                if (isBcf)
                    ain.JiraFieldsBox.Visibility = System.Windows.Visibility.Collapsed;
                ain.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                ain.ShowDialog();
                if (ain.DialogResult.HasValue && ain.DialogResult.Value)
                {
                    int elemCheck = 2;
                    if (ain.all.IsChecked.Value)
                        elemCheck = 0;
                    else if (ain.selected.IsChecked.Value)
                        elemCheck = 1;

                    List<SavedViewpoint> savedViewpointsImport = new List<SavedViewpoint>();

                    for (int i = 0; i < ain.issueList.SelectedItems.Count; i++)
                    {
                        int index = ain.issueList.Items.IndexOf(ain.issueList.SelectedItems[i]);
                        savedViewpointsImport.Add(_savedViewpoints[index]);
                    }
                    if (!savedViewpointsImport.Any())
                        return null;
                    //get selection only once!
                    if (elemCheck == 1)
                        _elementList = _oDoc.CurrentSelection.SelectedItems.Where(o => o.InstanceGuid != Guid.Empty).ToList<ModelItem>();

                    List<IssueBCF> issues = new List<IssueBCF>();
                    List<Issue> issuesJira = new List<Issue>();
                    foreach (var sv in savedViewpointsImport)
                    {
                        Issue issueJira = new Issue();
                        if (!isBcf)
                        {
                            issueJira.fields = new Fields();
                            issueJira.fields.issuetype = (Issuetype)ain.issueTypeCombo.SelectedItem;
                            issueJira.fields.priority = (Priority)ain.priorityCombo.SelectedItem;
                            if (!string.IsNullOrEmpty(ain.ChangeAssign.Content.ToString()) &&
                                ain.ChangeAssign.Content.ToString() != "none")
                            {
                                issueJira.fields.assignee = new User();
                                issueJira.fields.assignee.name = ain.ChangeAssign.Content.ToString();
                            }

                            if (ain.SelectedComponents != null && ain.SelectedComponents.Any())
                            {
                                issueJira.fields.components = ain.SelectedComponents;
                            }
                        }

                        IssueBCF issue = new IssueBCF();
                        string folderIssue = Path.Combine(path, issue.guid.ToString());
                        if (!Directory.Exists(folderIssue))
                            Directory.CreateDirectory(folderIssue);

                        issue.snapshot = Path.Combine(folderIssue, "snapshot.png");
                        // set the currtent saved viewpoint and then generate sna and BCF viewpoint
                        _oDoc.SavedViewpoints.CurrentSavedViewpoint = sv;
                        issue.viewpoint = generateViewpoint(sv.Viewpoint, elemCheck);
                        generateSnapshot(folderIssue);

                        issue.markup.Topic.Title = sv.DisplayName;
                        issue.markup.Header[0].IfcProject = "";
                        string projFilename = !string.IsNullOrEmpty(_oDoc.FileName) ? System.IO.Path.GetFileName(_oDoc.FileName) : "";
                        issue.markup.Header[0].Filename = projFilename;
                        issue.markup.Header[0].Date = DateTime.Now;

                        //comment
                        if (sv.Comments.Any())
                        {

                            foreach (var comm in sv.Comments)
                            {
                                string commentMetadata = string.Empty;
                                commentMetadata += comm.Body + Environment.NewLine;
                                commentMetadata += string.Format("<OriginalAuthor>{0}</OriginalAuthor>{1}", comm.Author, Environment.NewLine);
                                commentMetadata += string.Format("<CommentStatus>{0}</CommentStatus>{1}", comm.Status, Environment.NewLine);
                                commentMetadata += string.Format("<CreationDate>{0}</CreationDate>{1}", comm.CreationDate, Environment.NewLine);
                                var c = new CommentBCF
                                {
                                    Comment1 = commentMetadata,
                                    Topic = new CommentTopic {Guid = issue.guid.ToString()}
                                };
                                ;
                                c.Date = DateTime.Now;
                                c.VerbalStatus = comm.Status.ToString();
                                c.Author = (string.IsNullOrWhiteSpace(mainPan.jira.Self.displayName)) ? MySettings.Get("BCFusername") : mainPan.jira.Self.displayName;
                                issue.markup.Comment.Add(c);
                            }
                        }
                        issues.Add(issue);
                        issuesJira.Add(issueJira);
                    } // end foreach
                    return new Tuple<List<IssueBCF>, List<Issue>>(issues, issuesJira);
                }
            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            return null;
        }
        public void generateSnapshot(string folderIssue)
        {
            try
            {
                string snapshot = Path.Combine(folderIssue, "snapshot.png");

                // get the state of COM
                ComApi.InwOpState10 oState = ComBridge.State;
                // get the IO plugin for image
                ComApi.InwOaPropertyVec options = oState.GetIOPluginOptions("lcodpimage");

                //export the viewpoint to the image
                oState.DriveIOPlugin("lcodpimage", snapshot, options);
                System.Drawing.Bitmap oBitmap = new System.Drawing.Bitmap(snapshot);
                System.IO.MemoryStream ImageStream = new System.IO.MemoryStream();
                oBitmap.Save(ImageStream, System.Drawing.Imaging.ImageFormat.Jpeg);
                oBitmap.Dispose();
                //IM.postAttach(issueKey, File.ReadAllBytes(snapshot), IM.ConvertToBytes(v), g);
                //   postAttach2(issueKey, ConvertToBytes(v), "viewpoint.bcfv");
            }
            catch (System.Exception ex1)
            {
                MessageBox.Show("exception: " + ex1);
            }
        }
        private VisualizationInfo generateViewpoint(Viewpoint oVP, int elemCheck)
        {
            double units = GetGunits();
            VisualizationInfo v = new VisualizationInfo();
            try
            {

                Vector3D vi = getViewDir(oVP);
                Vector3D up = getViewUp(oVP);
                Point3D center = new Point3D(oVP.Position.X / units, oVP.Position.Y / units, oVP.Position.Z / units);
                double zoomValue = 1;


                oVP = oVP.CreateCopy();
                if (!oVP.HasFocalDistance)
                    oVP.FocalDistance = 1;

                if (oVP.Projection == ViewpointProjection.Orthographic) //IS ORTHO
                {
                    // **** CUSTOM VALUE FOR TEKLA **** //
                    // otherwise = 1
                    // **** CUSTOM VALUE FOR TEKLA **** //


                    double dist = oVP.VerticalExtentAtFocalDistance / 2 / units;
                    zoomValue = 3.125 * dist / (up.Length * 1.25);
                    //zoomValue = Math.Tan(oVP.HeightField / 2) * oVP.FarPlaneDistance / _feet *1.25;
                    //  MessageBox.Show(oVP.HeightField.ToString() + "  " + oVP.FarPlaneDistance.ToString() + "   " + zoomValue + "   " + oVP.HasFocalDistance.ToString() + "   " + oVP.VerticalExtentAtFocalDistance.ToString());
                    v.OrthogonalCamera = new OrthogonalCamera();
                    v.OrthogonalCamera.CameraViewPoint.X = center.X;
                    v.OrthogonalCamera.CameraViewPoint.Y = center.Y;
                    v.OrthogonalCamera.CameraViewPoint.Z = center.Z;
                    v.OrthogonalCamera.CameraUpVector.X = up.X;
                    v.OrthogonalCamera.CameraUpVector.Y = up.Y;
                    v.OrthogonalCamera.CameraUpVector.Z = up.Z;
                    v.OrthogonalCamera.CameraDirection.X = vi.X;
                    v.OrthogonalCamera.CameraDirection.Y = vi.Y;
                    v.OrthogonalCamera.CameraDirection.Z = vi.Z;
                    v.OrthogonalCamera.ViewToWorldScale = zoomValue;
                }
                else // it is a perspective view
                {
                    double f = oVP.FocalDistance;
                    //there is an issue when using vewpoint generated from clashes
                    //their VerticalExtentAtFocalDistance is correct but the HorizontalExtentAtFocalDistance is too small!
                    //so results that the aspect ratio in that case is <1. In which I try to get an approximate valut of the HorizontalExtentAtFocalDistance
                    // by multiplying the vercial by 1.35
                    //double hfov = (oVP.AspectRatio < 1) ? oVP.VerticalExtentAtFocalDistance * 1.23245 / 2 : oVP.HorizontalExtentAtFocalDistance / 2;
                    //double angle = Math.Atan(hfov * (1 / f)) * 2;
                    //double angled = (180 * angle / Math.PI);


                    //NAVIS USES HFOV
                    //double vfov = oVP.VerticalExtentAtFocalDistance / 2;
                    //double fov = Math.Sqrt(hfov * hfov + vfov * vfov);
                    //double angle = Math.Atan(fov*(1/f)) *2;
                    //double angled = (180 * angle / Math.PI);
                    //MessageBox.Show(angled.ToString() + "   " + oVP.FarDistance + "   " +f + "   " + oVP.NearDistance );// + "\n zoom" + zoom);

                    zoomValue = f;

                    v.PerspectiveCamera = new PerspectiveCamera();
                    v.PerspectiveCamera.CameraViewPoint.X = center.X;
                    v.PerspectiveCamera.CameraViewPoint.Y = center.Y;
                    v.PerspectiveCamera.CameraViewPoint.Z = center.Z;
                    v.PerspectiveCamera.CameraUpVector.X = up.X;
                    v.PerspectiveCamera.CameraUpVector.Y = up.Y;
                    v.PerspectiveCamera.CameraUpVector.Z = up.Z;
                    v.PerspectiveCamera.CameraDirection.X = vi.X;
                    v.PerspectiveCamera.CameraDirection.Y = vi.Y;
                    v.PerspectiveCamera.CameraDirection.Z = vi.Z;
                    v.PerspectiveCamera.FieldOfView = zoomValue;
                }



                if (elemCheck == 0)//visible (0)
                    _elementList = _oDoc.Models.First.RootItem.DescendantsAndSelf.Where(o => o.InstanceGuid != Guid.Empty && ChechHidden(o.AncestorsAndSelf) && o.FindFirstGeometry() != null && !o.FindFirstGeometry().Item.IsHidden).ToList<ModelItem>();

                if (null != _elementList && _elementList.Any() && elemCheck != 2)//not if none (2)
                {
                    v.Components = new Classes.Component[_elementList.Count];
                    string appname = Autodesk.Navisworks.Api.Application.Title;
                    for (var i = 0; i < _elementList.Count; i++)
                    {
                        string ifcguid = IfcGuid.ToIfcGuid(_elementList.ElementAt(i).InstanceGuid).ToString();
                        v.Components[i] = new ARUP.IssueTracker.Classes.Component(appname, "", ifcguid);

                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            return v;
        }

        private bool ChechHidden(ModelItemEnumerableCollection items)
        {
            if(items.Any(o => o.IsHidden))
                return false; //an anchestor is hidden, so it the item
            return true; // all anchestors are visible
           
            
        }
        private void RecurseItems(SavedItem oSI)
        {
            try
            {
                Autodesk.Navisworks.Api.GroupItem group = oSI as Autodesk.Navisworks.Api.GroupItem;
                if (null != group)//is a group
                {
                    foreach (SavedItem oSII in group.Children)
                    {
                        RecurseItems(oSII);
                    }
                }
                else
                {
                    _savedViewpoints.Add((SavedViewpoint)oSI);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

        }
        private void Open3dViewBCF(object sender, EventArgs e)
        {
            try
            {
                VisualizationInfo v = mainPan.jira.Bcf.Issues[mainPan.bcfPan.listIndex].viewpoint;
                Open3DView(v);
            }
            catch (System.Exception ex1)
            {
                MessageBox.Show("exception: " + ex1);
            }
        }
        private void Open3dViewJira(object sender, EventArgs e)
        {
            try
            {
                VisualizationInfo v = mainPan.getVisInfo();
                if (null != v)
                    Open3DView(v);
            }
            catch (System.Exception ex1)
            {
                MessageBox.Show("exception: " + ex1);
            }
        }
        // for creating saved viewpoints using BCF 2.0 VisualizationInfo
        private Viewpoint GetViewpointFromVisualizationInfo(object visInfo)
        {
            try
            {
                Tuple<Point3D, Vector3D, Vector3D, ViewpointProjection, double> tuple = null;

                if (visInfo is ARUP.IssueTracker.Classes.BCF2.VisualizationInfo)
                {
                    tuple = GetViewCoordinatesFromBcf2VisInfo((ARUP.IssueTracker.Classes.BCF2.VisualizationInfo)visInfo);
                }
                else if (visInfo is ARUP.IssueTracker.Classes.VisualizationInfo)
                {
                    tuple = GetViewCoordinates((ARUP.IssueTracker.Classes.VisualizationInfo)visInfo);
                }

                if (tuple == null)
                {
                    MessageBox.Show("Viewpoint not formatted correctly.", "Viewpoint Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }

                Document oDoc = Autodesk.Navisworks.Api.Application.ActiveDocument;

                // get current viewpoint
                // Viewpoint oCurVP = oDoc.vi.CurrentViewpoint.ToViewpoint;
                // get copy viewpoint
                Viewpoint oCopyVP = new Viewpoint();



                oCopyVP.AlignDirection(tuple.Item3);
                oCopyVP.AlignUp(tuple.Item2);
                oCopyVP.Projection = tuple.Item4;



                // **** CUSTOM VALUE FOR TEKLA **** //
                // otherwise = 1
                // **** CUSTOM VALUE FOR TEKLA **** //
                const double TEKLA = 1.25;

                double x = tuple.Item5 / TEKLA;


                if (oCopyVP.Projection == ViewpointProjection.Orthographic)
                {

                    oCopyVP.Position = tuple.Item1;
                    oCopyVP.FocalDistance = 1;
                    //top center point of view
                    Point3D xyzTL = oCopyVP.Position.Add(tuple.Item2.Multiply(x));
                    oCopyVP.SetExtentsAtFocalDistance(1, xyzTL.DistanceTo(oCopyVP.Position));
                }
                else
                {
                    //double angle = tuple.Item5 * Math.PI / 180;
                    // MessageBox.Show(tuple.Item5.ToString() + "  " +(Math.Tan(angle / 2)*2).ToString());
                    oCopyVP.FocalDistance = tuple.Item5;
                    //oCopyVP.SetExtentsAtFocalDistance(Math.Tan(angle / 2) * 2, Math.Tan(angle / 2) * 2 / oCopyVP.AspectRatio);
                    oCopyVP.Position = tuple.Item1;
                }

                //SavedViewpoint sv = new SavedViewpoint(oCopyVP);
                //sv.DisplayName = "test view";
                //sv.Guid = Guid.NewGuid();
                //oDoc.SavedViewpoints.AddCopy(sv);
                return oCopyVP;


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return null;
            }
        }
        private void Open3DView(VisualizationInfo v)
        {
            try
            {
                //    {
                //      
                Tuple<Point3D, Vector3D, Vector3D, ViewpointProjection, double> tuple = GetViewCoordinates(v);

                if (tuple == null)
                {
                    MessageBox.Show("Viewpoint not formatted correctly.", "Viewpoint Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                Document oDoc = Autodesk.Navisworks.Api.Application.ActiveDocument;

                // get current viewpoint
               // Viewpoint oCurVP = oDoc.vi.CurrentViewpoint.ToViewpoint;
                // get copy viewpoint
                Viewpoint oCopyVP = new Viewpoint();



                oCopyVP.AlignDirection(tuple.Item3);
                oCopyVP.AlignUp(tuple.Item2);
                oCopyVP.Projection = tuple.Item4;



                // **** CUSTOM VALUE FOR TEKLA **** //
                // otherwise = 1
                // **** CUSTOM VALUE FOR TEKLA **** //
                const double TEKLA = 1.25;

                double x = tuple.Item5 / TEKLA;


                if (oCopyVP.Projection == ViewpointProjection.Orthographic)
                {

                    oCopyVP.Position = tuple.Item1;
                    oCopyVP.FocalDistance = 1;
                    //top center point of view
                    Point3D xyzTL = oCopyVP.Position.Add(tuple.Item2.Multiply(x));
                    oCopyVP.SetExtentsAtFocalDistance(1, xyzTL.DistanceTo(oCopyVP.Position));
                }
                else
                {
                    //double angle = tuple.Item5 * Math.PI / 180;
                    // MessageBox.Show(tuple.Item5.ToString() + "  " +(Math.Tan(angle / 2)*2).ToString());
                    oCopyVP.FocalDistance = tuple.Item5;
                    //oCopyVP.SetExtentsAtFocalDistance(Math.Tan(angle / 2) * 2, Math.Tan(angle / 2) * 2 / oCopyVP.AspectRatio);
                    oCopyVP.Position = tuple.Item1;
                }

                //SavedViewpoint sv = new SavedViewpoint(oCopyVP);
                //sv.DisplayName = "test view";
                //sv.Guid = Guid.NewGuid();
                //oDoc.SavedViewpoints.AddCopy(sv);
                oDoc.CurrentViewpoint.CopyFrom(oCopyVP);

                if (v.Components != null && v.Components.Any())
                {
                    // ModelItemCollection selected = new ModelItemCollection();
                    List<ModelItem> attachedElems = new List<ModelItem>();

                    List<ModelItem> elems = oDoc.Models.First.RootItem.DescendantsAndSelf.ToList<ModelItem>();
     

                    foreach (var item in elems.Where(o => o.InstanceGuid != Guid.Empty))
                    {
                        string ifcguid = IfcGuid.ToIfcGuid(item.InstanceGuid).ToString();
                        if (v.Components.Any(o => o.IfcGuid == ifcguid))
                            attachedElems.Add(item);

                    }
                    if (attachedElems.Any())//avoid to hide everything if no elements matches
                    {
                        if (MySettings.Get("selattachedelems") == "0")
                        {
                            List<ModelItem> elemsVisible = new List<ModelItem>();
                            foreach (var item in attachedElems)
                            {
                                elemsVisible.AddRange(item.AncestorsAndSelf);
                            }
                            foreach (var item in elemsVisible)
                                elems.Remove(item);

                            oDoc.Models.ResetAllHidden();
                            oDoc.Models.SetHidden(elems, true);
                        }

                        else
                        {
                            oDoc.CurrentSelection.Clear();
                            oDoc.CurrentSelection.AddRange(attachedElems);
                        }
                    }


                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            //setVisibility(v);
           

        }
        public Tuple<Point3D, Vector3D, Vector3D, Autodesk.Navisworks.Api.ViewpointProjection, double> GetViewCoordinates(VisualizationInfo viewport)
        {
            try
            {
                double units = GetGunits();

                Point3D Position = new Point3D();
                Vector3D VectorUp = new Vector3D();
                Vector3D VectorTo = new Vector3D();
                ViewpointProjection vp = ViewpointProjection.Perspective;
                double zoom = 0;
                if (viewport.OrthogonalCamera != null)
                {
                    if (viewport.OrthogonalCamera.CameraViewPoint == null || viewport.OrthogonalCamera.CameraUpVector == null || viewport.OrthogonalCamera.CameraDirection == null)
                        return null;

                    vp = ViewpointProjection.Orthographic;
                    zoom = units * viewport.OrthogonalCamera.ViewToWorldScale;
                    Position = GetXYZ(viewport.OrthogonalCamera.CameraViewPoint.X, viewport.OrthogonalCamera.CameraViewPoint.Y, viewport.OrthogonalCamera.CameraViewPoint.Z);
                    VectorUp = GetXYZ(viewport.OrthogonalCamera.CameraUpVector.X, viewport.OrthogonalCamera.CameraUpVector.Y, viewport.OrthogonalCamera.CameraUpVector.Z).ToVector3D().Normalize();
                    VectorTo = GetXYZ(viewport.OrthogonalCamera.CameraDirection.X, viewport.OrthogonalCamera.CameraDirection.Y, viewport.OrthogonalCamera.CameraDirection.Z).ToVector3D().Normalize();
                }
                else if (viewport.PerspectiveCamera != null)
                {
                    if (viewport.PerspectiveCamera.CameraViewPoint == null || viewport.PerspectiveCamera.CameraUpVector == null || viewport.PerspectiveCamera.CameraDirection == null)
                        return null;

                    zoom = viewport.PerspectiveCamera.FieldOfView;
                    Position = GetXYZ(viewport.PerspectiveCamera.CameraViewPoint.X, viewport.PerspectiveCamera.CameraViewPoint.Y, viewport.PerspectiveCamera.CameraViewPoint.Z);
                    VectorUp = GetXYZ(viewport.PerspectiveCamera.CameraUpVector.X, viewport.PerspectiveCamera.CameraUpVector.Y, viewport.PerspectiveCamera.CameraUpVector.Z).ToVector3D().Normalize();
                    VectorTo = GetXYZ(viewport.PerspectiveCamera.CameraDirection.X, viewport.PerspectiveCamera.CameraDirection.Y, viewport.PerspectiveCamera.CameraDirection.Z).ToVector3D().Normalize();

                }
                else
                    return null;

                return new Tuple<Point3D, Vector3D, Vector3D, ViewpointProjection, double>(Position, VectorUp, VectorTo, vp, zoom);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            return null;
        }
        public Tuple<Point3D, Vector3D, Vector3D, Autodesk.Navisworks.Api.ViewpointProjection, double> GetViewCoordinatesFromBcf2VisInfo(ARUP.IssueTracker.Classes.BCF2.VisualizationInfo viewport)
        {
            try
            {
                double units = GetGunits();

                Point3D Position = new Point3D();
                Vector3D VectorUp = new Vector3D();
                Vector3D VectorTo = new Vector3D();
                ViewpointProjection vp = ViewpointProjection.Perspective;
                double zoom = 0;
                if (viewport.OrthogonalCamera != null)
                {
                    if (viewport.OrthogonalCamera.CameraViewPoint == null || viewport.OrthogonalCamera.CameraUpVector == null || viewport.OrthogonalCamera.CameraDirection == null)
                        return null;

                    vp = ViewpointProjection.Orthographic;
                    zoom = units * viewport.OrthogonalCamera.ViewToWorldScale;
                    Position = GetXYZ(viewport.OrthogonalCamera.CameraViewPoint.X, viewport.OrthogonalCamera.CameraViewPoint.Y, viewport.OrthogonalCamera.CameraViewPoint.Z);
                    VectorUp = GetXYZ(viewport.OrthogonalCamera.CameraUpVector.X, viewport.OrthogonalCamera.CameraUpVector.Y, viewport.OrthogonalCamera.CameraUpVector.Z).ToVector3D().Normalize();
                    VectorTo = GetXYZ(viewport.OrthogonalCamera.CameraDirection.X, viewport.OrthogonalCamera.CameraDirection.Y, viewport.OrthogonalCamera.CameraDirection.Z).ToVector3D().Normalize();
                }
                else if (viewport.PerspectiveCamera != null)
                {
                    if (viewport.PerspectiveCamera.CameraViewPoint == null || viewport.PerspectiveCamera.CameraUpVector == null || viewport.PerspectiveCamera.CameraDirection == null)
                        return null;

                    zoom = viewport.PerspectiveCamera.FieldOfView;
                    Position = GetXYZ(viewport.PerspectiveCamera.CameraViewPoint.X, viewport.PerspectiveCamera.CameraViewPoint.Y, viewport.PerspectiveCamera.CameraViewPoint.Z);
                    VectorUp = GetXYZ(viewport.PerspectiveCamera.CameraUpVector.X, viewport.PerspectiveCamera.CameraUpVector.Y, viewport.PerspectiveCamera.CameraUpVector.Z).ToVector3D().Normalize();
                    VectorTo = GetXYZ(viewport.PerspectiveCamera.CameraDirection.X, viewport.PerspectiveCamera.CameraDirection.Y, viewport.PerspectiveCamera.CameraDirection.Z).ToVector3D().Normalize();

                }
                else
                    return null;

                return new Tuple<Point3D, Vector3D, Vector3D, ViewpointProjection, double>(Position, VectorUp, VectorTo, vp, zoom);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            return null;
        }
        private Point3D GetXYZ(double x, double y, double z)
        {
            double units = GetGunits();

            Point3D myXYZ = new Point3D(x * units, y * units, z * units);
            return myXYZ;
        }

        private void CreateSavedViewpointJira(object sender, EventArgs e)
        {
            if (mainPan.jiraPan.issueList.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please select one or more Isses first.", "No Issue selected", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            CreateSavedViewpoint(true);
        }

        private void CreateSavedViewpointBCF(object sender, EventArgs e)
        {
            CreateSavedViewpoint(false);
        }

        private void CreateSavedViewpoint(bool isJira)
        {
            List<Autodesk.Navisworks.Api.SavedViewpoint> savedViewpoints = new List<Autodesk.Navisworks.Api.SavedViewpoint>();
            int errors = 0;

            if (isJira)
            {
                if (mainPan.jiraPan.issueList.SelectedItems.Count == 0)
                {
                    MessageBox.Show("Please select an issue.", "No Issue", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                foreach (object t in mainPan.jiraPan.issueList.SelectedItems)
                {
                    int index = mainPan.jiraPan.issueList.Items.IndexOf(t);
                    Issue issue = mainPan.jira.IssuesCollection[index];
                    if (string.IsNullOrWhiteSpace(issue.viewpoint))
                    {
                        errors++;
                        continue;
                    }
                    // Save viewpoint
                    string viewpointPath = string.Empty;
                    try
                    {
                        string ReportFolder = Path.Combine(Path.GetTempPath(), "BCFtemp");
                        if (!Directory.Exists(ReportFolder))
                            Directory.CreateDirectory(ReportFolder);
                        string issueFolder = Path.Combine(ReportFolder, issue.fields.guid);
                        if (!Directory.Exists(issueFolder))
                            Directory.CreateDirectory(issueFolder);
                        viewpointPath = Path.Combine(issueFolder, "viewpoint.bcfv");
                        mainPan.saveSnapshotViewpoint(issue.viewpoint, viewpointPath);
                    }
                    catch (Exception ex)
                    {
                        errors++;
                        continue;
                    }
                    if (File.Exists(viewpointPath))
                    {
                        try
                        {
                            ARUP.IssueTracker.Classes.BCF2.VisualizationInfo issueViewpoint = ARUP.IssueTracker.Classes.BCF2.BcfContainer.DeserializeViewpoint(viewpointPath);
                            if (issueViewpoint == null)
                            {
                                errors++;
                                continue;
                            }
                            Viewpoint navisworksViewpoint = GetViewpointFromVisualizationInfo(issueViewpoint);
                            if (navisworksViewpoint == null)
                            {
                                errors++;
                                continue;
                            }
                            else
                            {
                                SavedViewpoint savedViewpoint = new SavedViewpoint(navisworksViewpoint);
                                // save issue metadata to a comment
                                string issueMetadata = string.Empty;
                                if (!string.IsNullOrWhiteSpace(issue.fields.summary))
                                    issueMetadata += string.Format("<Summary>{0}</Summary>{1}", issue.fields.summary, Environment.NewLine);
                                if (!string.IsNullOrWhiteSpace(issue.fields.description))
                                    issueMetadata += string.Format("<Description>{0}</Description>{1}", issue.fields.description, Environment.NewLine);
                                if (issue.fields.issuetype != null)
                                    issueMetadata += string.Format("<Type>{0}</Type>{1}", issue.fields.issuetype.name, Environment.NewLine);
                                if (issue.fields.status != null)
                                    issueMetadata += string.Format("<Status>{0}</Status>{1}", issue.fields.status.name, Environment.NewLine);
                                if (issue.fields.priority != null)
                                    issueMetadata += string.Format("<Priority>{0}</Priority>{1}", issue.fields.priority.name, Environment.NewLine);
                                if (issue.fields.creator != null)
                                    issueMetadata += string.Format("<Creator>{0}</Creator>{1}", issue.fields.creator.displayName, Environment.NewLine);
                                if (!string.IsNullOrWhiteSpace(issue.fields.created))
                                    issueMetadata += string.Format("<Created>{0}</Created>{1}", issue.fields.created, Environment.NewLine);
                                if (issue.fields.assignee != null)
                                    issueMetadata += string.Format("<Assignee>{0}</Assignee>{1}", issue.fields.assignee.displayName, Environment.NewLine);
                                if (!string.IsNullOrWhiteSpace(issue.fields.updated))
                                    issueMetadata += string.Format("<Updated>{0}</Updated>{1}", issue.fields.updated, Environment.NewLine);

                                savedViewpoint.Comments.Add(new Autodesk.Navisworks.Api.Comment(issueMetadata, Autodesk.Navisworks.Api.CommentStatus.New, issue.fields.creator.displayName));

                                // save issue comments to Navis comments
                                if (issue.fields.comment.total > 0)
                                {
                                    foreach (Comment2 jiraComment in issue.fields.comment.comments)
                                    {
                                        if (!string.IsNullOrWhiteSpace(jiraComment.body))
                                        {
                                            string commentMetadata = string.Empty;
                                            commentMetadata += jiraComment.body;
                                            commentMetadata += string.Format("{0}<Updated>{1}</Updated>", Environment.NewLine, jiraComment.updated);
                                            Autodesk.Navisworks.Api.Comment navisworksComment = new Autodesk.Navisworks.Api.Comment(commentMetadata, Autodesk.Navisworks.Api.CommentStatus.New, jiraComment.updateAuthor.displayName);
                                            savedViewpoint.Comments.Add(navisworksComment);
                                        }
                                    }
                                }
                                savedViewpoint.DisplayName = string.Format("{0}_{1}", mainPan.jira.ProjectsCollection[mainPan.jiraPan.projIndex].name, issue.key);
                                savedViewpoints.Add(savedViewpoint);
                            }
                        }
                        catch (Exception ex)
                        {
                            errors++;
                            continue;
                        }
                    }
                }
            }
            else
            {
                if (mainPan.bcfPan.issueList.SelectedItems.Count == 0)
                {
                    MessageBox.Show("Please select an issue.", "No Issue", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                foreach (object t in mainPan.bcfPan.issueList.SelectedItems)
                {
                    int index = mainPan.bcfPan.issueList.Items.IndexOf(t);
                    IssueBCF issueBcf = mainPan.jira.Bcf.Issues[index];
                    if (issueBcf.viewpoint == null)
                    {
                        errors++;
                        continue;
                    }
                    try
                    {
                        ARUP.IssueTracker.Classes.VisualizationInfo issueViewpoint = issueBcf.viewpoint;
                        if (issueViewpoint == null)
                        {
                            errors++;
                            continue;
                        }
                        Viewpoint navisworksViewpoint = GetViewpointFromVisualizationInfo(issueViewpoint);
                        if (navisworksViewpoint == null)
                        {
                            errors++;
                            continue;
                        }
                        else
                        {
                            SavedViewpoint savedViewpoint = new SavedViewpoint(navisworksViewpoint);
                            // save issue metadata to a comment
                            string issueMetadata = string.Empty;
                            if (issueBcf.bcf2Markup != null)
                            {
                                if (!string.IsNullOrWhiteSpace(issueBcf.bcf2Markup.Topic.Title))
                                    issueMetadata += string.Format("<Title>{0}</Title>{1}", issueBcf.bcf2Markup.Topic.Title, Environment.NewLine);
                                if (!string.IsNullOrWhiteSpace(issueBcf.bcf2Markup.Topic.Description))
                                    issueMetadata += string.Format("<Description>{0}</Description>{1}", issueBcf.bcf2Markup.Topic.Description, Environment.NewLine);
                                if (!string.IsNullOrWhiteSpace(issueBcf.bcf2Markup.Topic.TopicType))
                                    issueMetadata += string.Format("<TopicType>{0}</TopicType>{1}", issueBcf.bcf2Markup.Topic.TopicType, Environment.NewLine);
                                if (!string.IsNullOrWhiteSpace(issueBcf.bcf2Markup.Topic.TopicStatus))
                                    issueMetadata += string.Format("<TopicStatus>{0}</TopicStatus>{1}", issueBcf.bcf2Markup.Topic.TopicStatus, Environment.NewLine);
                                if (!string.IsNullOrWhiteSpace(issueBcf.bcf2Markup.Topic.Priority))
                                    issueMetadata += string.Format("<Priority>{0}</Priority>{1}", issueBcf.bcf2Markup.Topic.Priority, Environment.NewLine);
                                if (!string.IsNullOrWhiteSpace(issueBcf.bcf2Markup.Topic.CreationAuthor))
                                    issueMetadata += string.Format("<CreationAuthor>{0}</CreationAuthor>{1}", issueBcf.bcf2Markup.Topic.CreationAuthor, Environment.NewLine);
                                if (issueBcf.bcf2Markup.Topic.CreationDateSpecified)
                                    issueMetadata += string.Format("<CreationDate>{0}</CreationDate>{1}", issueBcf.bcf2Markup.Topic.CreationDate.ToString(), Environment.NewLine);
                                if (!string.IsNullOrWhiteSpace(issueBcf.bcf2Markup.Topic.AssignedTo))
                                    issueMetadata += string.Format("<AssignedTo>{0}</AssignedTo>{1}", issueBcf.bcf2Markup.Topic.AssignedTo, Environment.NewLine);
                                if (!string.IsNullOrWhiteSpace(issueBcf.bcf2Markup.Topic.ModifiedAuthor))
                                    issueMetadata += string.Format("<ModifiedAuthor>{0}</ModifiedAuthor>{1}", issueBcf.bcf2Markup.Topic.ModifiedAuthor, Environment.NewLine);
                                if (issueBcf.bcf2Markup.Topic.ModifiedDateSpecified)
                                    issueMetadata += string.Format("<ModifiedDate>{0}</ModifiedDate>{1}", issueBcf.bcf2Markup.Topic.ModifiedDate.ToString(), Environment.NewLine);
                                if (!string.IsNullOrWhiteSpace(issueBcf.bcf2Markup.Topic.ReferenceLink))
                                    issueMetadata += string.Format("<ReferenceLink>{0}</ReferenceLink>{1}", issueBcf.bcf2Markup.Topic.ReferenceLink.ToString(), Environment.NewLine);
                            }

                            if (!string.IsNullOrWhiteSpace(issueMetadata))
                                savedViewpoint.Comments.Add(new Autodesk.Navisworks.Api.Comment(issueMetadata, Autodesk.Navisworks.Api.CommentStatus.New, issueBcf.bcf2Markup.Topic.CreationAuthor));

                            // save issue comments to Navis comment
                            if (issueBcf.bcf2Markup.Comment.Count > 0)
                            {
                                foreach (ARUP.IssueTracker.Classes.BCF2.Comment bcf2Comment in issueBcf.bcf2Markup.Comment)
                                {
                                    if (!string.IsNullOrWhiteSpace(bcf2Comment.Comment1))
                                    {
                                        string commentMetadata = string.Empty;
                                        commentMetadata += bcf2Comment.Comment1;
                                        commentMetadata += string.Format("{0}<Author>{1}</Author>", Environment.NewLine, bcf2Comment.Author);
                                        commentMetadata += string.Format("{0}<Date>{1}</Date>", Environment.NewLine, bcf2Comment.Date);
                                        commentMetadata += string.Format("{0}<Status>{1}</Status>", Environment.NewLine, bcf2Comment.Status);
                                        if (!string.IsNullOrWhiteSpace(bcf2Comment.VerbalStatus))
                                            commentMetadata += string.Format("{0}<VerbalStatus>{1}</VerbalStatus>", Environment.NewLine, bcf2Comment.VerbalStatus);
                                        if (!string.IsNullOrWhiteSpace(bcf2Comment.ModifiedAuthor))
                                            commentMetadata += string.Format("{0}<ModifiedAuthor>{1}</ModifiedAuthor>", Environment.NewLine, bcf2Comment.ModifiedAuthor);
                                        if (bcf2Comment.ModifiedDateSpecified)
                                            commentMetadata += string.Format("{0}<ModifiedDate>{1}</ModifiedDate>", Environment.NewLine, bcf2Comment.ModifiedDate);
                                        Autodesk.Navisworks.Api.Comment navisworksComment = new Autodesk.Navisworks.Api.Comment(commentMetadata, Autodesk.Navisworks.Api.CommentStatus.New, bcf2Comment.Author);
                                        savedViewpoint.Comments.Add(navisworksComment);
                                    }
                                }
                            }
                            savedViewpoint.DisplayName = string.Format("{0}_{1}", mainPan.jira.ProjectsCollection[mainPan.jiraPan.projIndex].name, issueBcf.guid.ToString());
                            savedViewpoints.Add(savedViewpoint);
                        }
                    }
                    catch (Exception ex)
                    {
                        errors++;
                        continue;
                    }
                }
            }

            savedViewpoints.ForEach(v => _oDoc.SavedViewpoints.AddCopy(v));

            if (errors != 0)
            {
                MessageBox.Show(errors + " viewpoints(s) were not generated because of missing files or wrong formats of viewpoint.bcfv.",
                    "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        //public void setVisibility(XDocument v)
        //{
        //    try
        //    {
        //        if (v.Element("VisualizationInfo").Elements("Components").Any() && v.Element("VisualizationInfo").Elements("Components").Elements("Component").Any())
        //        {
        //            System.Collections.Generic.IEnumerable<string> guidList = from item in v.Element("VisualizationInfo").Elements("Components").Elements("Component")
        //                                                                      select (string)item.Attribute("IfcGuid");
        //            System.Collections.Generic.IEnumerable<string> authList = from item in v.Element("VisualizationInfo").Elements("Components").Elements("Component")
        //                                                                      select (string)item.Element("AuthoringToolId");


        //            //COMPONENTS PART
        //            ModelItemCollection hidden = new ModelItemCollection();

        //            //mark as invisible all the siblings of the visible items as well as the visible items
        //            foreach (var item in Autodesk.Navisworks.Api.Application.ActiveDocument.Models.First.RootItem.DescendantsAndSelf.Where(o => o.InstanceGuid != Guid.Empty))
        //            {
        //                string ifcguid = IfcGuid.ToIfcGuid(item.InstanceGuid).ToString();
        //                if (!guidList.Contains(ifcguid))//there is not its ifcguid, check for authoring tool ID
        //                {
        //                    if (null != item.PropertyCategories.FindCategoryByDisplayName("Element ID")) //has an auto tool id
        //                    {
        //                        string t = item.PropertyCategories.FindCategoryByDisplayName("Element ID")
        //                            .Properties.FindPropertyByDisplayName("Value").Value.ToDisplayString();
        //                        if (!authList.Contains(ifcguid))//does not have its auth tool id
        //                            hidden.Add(item);
        //                    }
        //                    else //has no auth tool id
        //                        hidden.Add(item);

        //                }
        //            }
        //            Autodesk.Navisworks.Api.Application.ActiveDocument.Models.ResetAllHidden();
        //            Autodesk.Navisworks.Api.Application.ActiveDocument.CurrentSelection.Clear();
        //            Autodesk.Navisworks.Api.Application.ActiveDocument.Models.SetHidden(hidden, true);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(ex.ToString());
        //    }

        //}
        #region math rotations and stuff
        private Vector3D getViewDir(Viewpoint oVP)
        {
            double units = GetGunits();

            Rotation3D oRot = oVP.Rotation;
            // calculate view direction
            Rotation3D oNegtiveZ = new Rotation3D(0, 0, -1, 0);
            Rotation3D otempRot = MultiplyRotation3D(oNegtiveZ, oRot.Invert());
            Rotation3D oViewDirRot = MultiplyRotation3D(oRot, otempRot);
            // get view direction
            Vector3D oViewDir = new Vector3D(oViewDirRot.A, oViewDirRot.B, oViewDirRot.C);

            return oViewDir.Normalize();
        }
        private Vector3D getViewUp(Viewpoint oVP)
        {
            double units = GetGunits();

            Rotation3D oRot = oVP.Rotation;
            // calculate view direction
            Rotation3D oNegtiveZ = new Rotation3D(0, 1, 0, 0);
            Rotation3D otempRot = MultiplyRotation3D(oNegtiveZ, oRot.Invert());
            Rotation3D oViewDirRot = MultiplyRotation3D(oRot, otempRot);
            // get view direction
            Vector3D oViewDir = new Vector3D(oViewDirRot.A, oViewDirRot.B, oViewDirRot.C);

            return oViewDir.Normalize();
        }

        // help function: Multiply two Rotation3D
        private Rotation3D MultiplyRotation3D(
            Rotation3D r2,
            Rotation3D r1)
        {

            Rotation3D oRot =
                new Rotation3D(r2.D * r1.A + r2.A * r1.D +
                                    r2.B * r1.C - r2.C * r1.B,
                                r2.D * r1.B + r2.B * r1.D +
                                    r2.C * r1.A - r2.A * r1.C,
                                r2.D * r1.C + r2.C * r1.D +
                                    r2.A * r1.B - r2.B * r1.A,
                                r2.D * r1.D - r2.A * r1.A -
                                    r2.B * r1.B - r2.C * r1.C);

            oRot.Normalize();

            return oRot;

        }
            private double GetGunits()
    {
        string units = _oDoc.Units.ToString();
        double factor = 1;
        switch (units)
        {
            case "Centimeters":
                factor = 100;
                break;
            case "Feet":
                factor = 3.28084;
                break;
            case "Inches":
                factor = 39.3701;
                break;
            case "Kilometers":
                factor = 0.001;
                break;
            case "Meters":
                factor = 1;
                break;
            case "Micrometers":
                factor = 1000000;
                break;
            case "Miles":
                factor = 0.000621371;
                break;
            case "Millimeters":
                factor = 1000;
                break;
            case "Mils":
                factor = 39370.0787;
                break;
            case "Yards":
                factor = 1.09361;
                break;
            default:
                MessageBox.Show("Units " + units + " not recognized.");
                factor = 1;
                break;
        }
        return factor;
    }

        #endregion
    }
}
