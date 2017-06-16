using ARUP.IssueTracker.Classes.BCF2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ARUP.IssueTracker.IPC
{
    [Serializable]
    public class AddIssueResponse
    {
        public string documentName { get; set; }
        public string documentGuid { get; set; }
        public string tempSnapshotPath { get; set; }
        public VisualizationInfo visualizationInfo { get; set; }
        public bool isValidRequest { get; set; }
    }
    
}
