using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ARUP.IssueTracker.Classes
{
    // This is a class to convert data between BCF 1.0 and 2.0 
    public class BcfAdapter
    {
        /// <summary>
        /// Save BCF 1.0 as BCF 2.0
        /// </summary>
        /// <param name="bcf1">BCF 1.0 file</param>
        public static void SaveBcf2(BCF bcf1)
        {
            BCF2.BcfFile bcf2 = new BCF2.BcfFile();
            bcf2.HasBeenSaved = bcf1.HasBeenSaved;
            bcf2.Filename = bcf1.Filename;
            bcf2.Fullname = bcf1.path;
            bcf2.TempPath = bcf1.path;
            bcf2.Issues = new ObservableCollection<BCF2.Markup>();
            // bcf2.Id = null;     // check if this is correct
            // bcf2.ProjectName = ;    // from Jira
            // bcf2.ProjectId = ;      // from Jira


            // Add issues (markups)
            foreach (IssueBCF bcf1Issue in bcf1.Issues)
            {
                // Convert header files
                List<BCF2.HeaderFile> bcf2Headers = new List<BCF2.HeaderFile>();
                foreach (HeaderFile bcf1Header in bcf1Issue.markup.Header)
                {
                    bcf2Headers.Add(new BCF2.HeaderFile()
                    {
                        Date = bcf1Header.Date,
                        // DateSpecified = bcf1Header.DateSpecified,  // default ignored
                        Filename = bcf1Header.Filename,
                        IfcProject = bcf1Header.IfcProject,
                        IfcSpatialStructureElement = bcf1Header.IfcSpatialStructureElement,
                        isExternal = true, // default true for now
                        Reference = "" // default empty for now
                    });
                }

                // Convert Comments
                ObservableCollection<BCF2.Comment> bcf2Comments = new ObservableCollection<BCF2.Comment>();
                foreach (CommentBCF bcf1Comment in bcf1Issue.markup.Comment)
                {
                    bcf2Comments.Add(new BCF2.Comment()
                    {
                        Author = bcf1Comment.Author,
                        Comment1 = bcf1Comment.Comment1,
                        Date = bcf1Comment.Date,
                        Guid = bcf1Comment.Guid,
                        ModifiedAuthor = bcf1Comment.Author,  // default the same as author
                        //ModifiedDate = null, //// default null
                        //ModifiedDateSpecified = false,   // default ignored
                        ReplyToComment = null, // default null
                        Status = bcf1Comment.Status.ToString(), // check if it accepts user input
                        Topic = null, // all referenced to markup's topic
                        VerbalStatus = bcf1Comment.VerbalStatus,
                        Viewpoint = null // check if there's a CommentViewPoint
                    });
                }

                // Convert Topic
                BCF2.Topic bcf2Topic = new BCF2.Topic() {
                    AssignedTo = null,
                    BimSnippet = null,
                    CreationAuthor = null,
                    //CreationDate = null,
                    //CreationDateSpecified = false,    // default ignored
                    Description = null,
                    DocumentReferences = null,
                    Guid = bcf1Issue.markup.Topic.Guid,
                    Index = null,
                    Labels = null,
                    ModifiedAuthor = null,
                    //ModifiedDate = ,
                    //ModifiedDateSpecified = false,    // default ignored
                    Priority = null,
                    ReferenceLink = bcf1Issue.markup.Topic.ReferenceLink,
                    RelatedTopics = null,
                    Title = bcf1Issue.markup.Topic.Title,
                    TopicStatus = null,
                    TopicType = null
                };

                // Convert ClippingPlane
                List<BCF2.ClippingPlane> bcf2ClippingPlanes = new List<BCF2.ClippingPlane>();
                foreach (ClippingPlane bcf1ClippingPlane in bcf1Issue.viewpoint.ClippingPlanes)
                {
                    bcf2ClippingPlanes.Add(new BCF2.ClippingPlane()
                    {
                        Direction = new BCF2.Direction() { X = bcf1ClippingPlane.Direction.X,
                                                           Y = bcf1ClippingPlane.Direction.Y,
                                                           Z = bcf1ClippingPlane.Direction.Z },
                        Location = new BCF2.Point() { X = bcf1ClippingPlane.Location.X,
                                                      Y = bcf1ClippingPlane.Location.Y,
                                                      Z = bcf1ClippingPlane.Location.Z } 
                    });
                }

                // Convert Components
                List<BCF2.Component> bcf2Components = new List<BCF2.Component>();
                foreach (Component bcf1Component in bcf1Issue.viewpoint.Components)
                {
                    bcf2Components.Add(new BCF2.Component() {
                        AuthoringToolId = bcf1Component.AuthoringToolId,
                        // Color = bcf1Component,    // check if this is correct
                        IfcGuid = bcf1Component.IfcGuid,
                        OriginatingSystem = bcf1Component.OriginatingSystem,
                        // Selected = bcf1Component,   // check if this is correct
                        // SelectedSpecified = ,    // check if this is correct
                        // Visible = bcf1Component.v    // check if this is correct
                    });                    
                }

                // Convert Lines
                List<BCF2.Line> bcf2Lines = new List<BCF2.Line>();
                foreach (Line bcf1Line in bcf1Issue.viewpoint.Lines)
                {
                    bcf2Lines.Add(new BCF2.Line() {
                        StartPoint = new BCF2.Point() { X = bcf1Line.StartPoint.X,
                                                        Y = bcf1Line.StartPoint.Y,
                                                        Z = bcf1Line.StartPoint.Z },
                        EndPoint = new BCF2.Point() { X = bcf1Line.EndPoint.X,
                                                      Y = bcf1Line.EndPoint.Y,
                                                      Z = bcf1Line.EndPoint.Z }
                    });
                }

                // Convert VisualizationInfo
                BCF2.VisualizationInfo bcf2VizInfo = new BCF2.VisualizationInfo()
                {
                    Bitmaps = null, // default null 
                    ClippingPlanes = bcf2ClippingPlanes.ToArray(),
                    Components = bcf2Components,
                    Lines = bcf2Lines.ToArray(),
                    OrthogonalCamera = new BCF2.OrthogonalCamera() { CameraDirection = new BCF2.Direction() { X = bcf1Issue.viewpoint.OrthogonalCamera.CameraDirection.X,
                                                                                                              Y = bcf1Issue.viewpoint.OrthogonalCamera.CameraDirection.Y,
                                                                                                              Z = bcf1Issue.viewpoint.OrthogonalCamera.CameraDirection.Z },
                                                                     CameraUpVector = new BCF2.Direction() { X = bcf1Issue.viewpoint.OrthogonalCamera.CameraUpVector.X,
                                                                                                             Y = bcf1Issue.viewpoint.OrthogonalCamera.CameraUpVector.Y,
                                                                                                             Z = bcf1Issue.viewpoint.OrthogonalCamera.CameraUpVector.Z },
                                                                     CameraViewPoint = new BCF2.Point() { X = bcf1Issue.viewpoint.OrthogonalCamera.CameraViewPoint.X,
                                                                                                          Y = bcf1Issue.viewpoint.OrthogonalCamera.CameraViewPoint.Y,
                                                                                                          Z = bcf1Issue.viewpoint.OrthogonalCamera.CameraViewPoint.Z },
                                                                     ViewToWorldScale = bcf1Issue.viewpoint.OrthogonalCamera.ViewToWorldScale },
                    PerspectiveCamera = new BCF2.PerspectiveCamera() { CameraDirection = new BCF2.Direction() { X = bcf1Issue.viewpoint.PerspectiveCamera.CameraDirection.X,
                                                                                                                Y = bcf1Issue.viewpoint.PerspectiveCamera.CameraDirection.Y,
                                                                                                                Z = bcf1Issue.viewpoint.PerspectiveCamera.CameraDirection.Z },
                                                                       CameraUpVector = new BCF2.Direction() { X = bcf1Issue.viewpoint.PerspectiveCamera.CameraUpVector.X,
                                                                                                               Y = bcf1Issue.viewpoint.PerspectiveCamera.CameraUpVector.Y,
                                                                                                               Z = bcf1Issue.viewpoint.PerspectiveCamera.CameraUpVector.Z },
                                                                       CameraViewPoint = new BCF2.Point() { X = bcf1Issue.viewpoint.PerspectiveCamera.CameraViewPoint.X,
                                                                                                            Y = bcf1Issue.viewpoint.PerspectiveCamera.CameraViewPoint.Y,
                                                                                                            Z = bcf1Issue.viewpoint.PerspectiveCamera.CameraViewPoint.Z },
                                                                       FieldOfView = bcf1Issue.viewpoint.PerspectiveCamera.FieldOfView },
                    SheetCamera = new BCF2.SheetCamera() { SheetID = bcf1Issue.viewpoint.SheetCamera.SheetID,
                                                           SheetName = null, // default null
                                                           TopLeft = new BCF2.Point() { X = bcf1Issue.viewpoint.SheetCamera.TopLeft.X,
                                                                                        Y = bcf1Issue.viewpoint.SheetCamera.TopLeft.Y,
                                                                                        Z = bcf1Issue.viewpoint.SheetCamera.TopLeft.Z },
                                                           BottomRight = new BCF2.Point(){ X = bcf1Issue.viewpoint.SheetCamera.BottomRight.X,
                                                                                           Y = bcf1Issue.viewpoint.SheetCamera.BottomRight.Y,
                                                                                           Z = bcf1Issue.viewpoint.SheetCamera.BottomRight.Z }}
                }; 

                // Convert viewpoints
                // BCF 1.0 can only have one viewpoint
                ObservableCollection<BCF2.ViewPoint> bcf2ViewPoints = new ObservableCollection<BCF2.ViewPoint>();
                bcf2ViewPoints.Add(new BCF2.ViewPoint() {
                    //Guid = null,    // check if this is correct
                    Snapshot = bcf1Issue.snapshot,    // check if this is correct
                    //SnapshotPath = bcf1Issue.snapshot,    //not for serialization
                    Viewpoint = "viewpoint.bcfv",
                    VisInfo = bcf2VizInfo
                });

                // Add BCF 2.0 issues/markups
                bcf2.Issues.Add(new BCF2.Markup()
                {
                    Header = bcf2Headers,
                    Comment = bcf2Comments,
                    Topic = bcf2Topic,
                    //ViewComments = ,    // default ignored
                    Viewpoints = bcf2ViewPoints
                });
            }


            // Save BCF 2.0 file
            BCF2.BcfContainer.SaveBcfFile(bcf2);
            
        }

        /// <summary>
        /// Load from BCF 2.0 as BCF 1.0
        /// </summary>
        /// <param name="bcf2">BCF 2.0 file</param>
        /// <returns></returns>
        public static BCF LoadBcf1FromBcf2(BCF2.BcfFile bcf2)
        {
            return null;
        }
    }
}
