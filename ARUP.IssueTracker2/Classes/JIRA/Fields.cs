using System.Collections.Generic;
using ARUP.IssueTracker.Classes.JIRA;

namespace ARUP.IssueTracker.Classes
{
    public class Fields
    {
        public Resolution resolution { get; set; }
        public Priority priority { get; set; }
        public User creator { get; set; }
        public Project project { get; set; }
        public string summary { get; set; }
        public Status status { get; set; }
        public string created { get; set; }
        public string updated { get; set; }
        public string description { get; set; }
        public User assignee { get; set; }
        public Watches watches { get; set; }
        public Issuetype issuetype { get; set; }
        public List<Attachment> attachment { get; set; }
        public Comment comment { get; set; }
        public List<Component> components { get; set; }
        public List<string> labels { get; set; }
        public string guid { get; set; }

        // for filtering attachments without viewpoints/snapshots
        public List<Attachment> filteredAttachments 
        {
            get 
            {
                List<Attachment> filtered = new List<Attachment>();
                attachment.ForEach(o => {
                    if (o.filename != "markup.bcf" && o.filename != "viewpoint.bcfv" && o.filename != "snapshot.png" && !comment.comments.Exists(c => c.snapshotFileName == o.filename) && !comment.comments.Exists(c => c.viewpointFileName == o.filename))
                    {
                        filtered.Add(o);
                    }
                });

                return filtered;
            } 
        }

        //public List<Component> components { get; set; }
        
    }


}
