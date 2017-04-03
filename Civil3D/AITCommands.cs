// (C) Copyright 2017 by Microsoft 
//
using System;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using System.Windows;

// This line is not mandatory, but improves loading performances
[assembly: CommandClass(typeof(ARUP.IssueTracker.Civil3D.AITCommands))]

namespace ARUP.IssueTracker.Civil3D
{

    // This class is instantiated by AutoCAD for each document when
    // a command is called by the user the first time in the context
    // of a given document. In other words, non static data in this class
    // is implicitly per-document!
    public class AITCommands
    {
        // The CommandMethod attribute can be applied to any public  member 
        // function of any public class.
        // The function should take no arguments and return nothing.
        // If the method is an intance member then the enclosing class is 
        // intantiated for each document. If the member is a static member then
        // the enclosing class is NOT intantiated.
        //
        // NOTE: CommandMethod has overloads where you can provide helpid and
        // context menu.

        private bool _isRunning;
        private Civil3DWindow window;

        // Modal Command with localized name
        [CommandMethod("AIT", "AITSNAPSHOT", "AITSNAPSHOT", CommandFlags.Modal)]
        public void GenerateSnapshot() // This method can have any name
        {
            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            PromptResult pr = ed.GetString("\nOutput Path: ");
            if (pr.Status != PromptStatus.OK)
            {
                ed.WriteMessage("No path was provided.\n");
                return;
            }            

            var result = doc.Editor.SelectAll();
            if (result.Status == PromptStatus.OK)
            {
                Autodesk.AutoCAD.Internal.Utils.SelectObjects(result.Value.GetObjectIds());
            }

            doc.Editor.Command("_.PNGOUT", pr.StringResult, "");
        }

        /// <summary>
        /// Main Command Entry Point
        /// </summary>
        // Modal Command with localized name
        [CommandMethod("AIT", "AITRUN", "AITRUN", CommandFlags.Modal)]
        public void Run()
        {
            try
            {
#if C3D2014
                string versionNumber = "19.1";
#elif C3D2015
                string versionNumber = "20.0";
#elif C3D2016
                string versionNumber = "20.1";
#elif C3D2017
                string versionNumber = "21.0";
#endif

                // Version
                if (!Autodesk.AutoCAD.ApplicationServices.Application.Version.ToString().Contains(versionNumber))
                {
                    MessageBox.Show(string.Format("This Add-In was built for Civil 3D {0}, please find the Arup Issue Tracker group in Yammer for assistance...", versionNumber), "Incompatible Civil 3D Version");
                    return;
                }

                // Form Running?
                if (_isRunning && window != null && window.IsLoaded)
                {
                    window.Focus();
                    return;
                }

                _isRunning = true;

                window = new Civil3DWindow(Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument);
                window.Show();

                // register a document closed event
                Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.BeginDocumentClose += MdiActiveDocument_BeginDocumentClose;

            }
            catch (System.Exception ex)
            {
                MessageBox.Show("exception: " + ex);
            }

        }

        void MdiActiveDocument_BeginDocumentClose(object sender, DocumentBeginCloseEventArgs e)
        {
            window.Close();
        }

    }

}
