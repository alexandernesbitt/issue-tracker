using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ARUP.IssueTracker.Windows;
using System.Windows;
using System.IO;
using ARUP.IssueTracker.Classes.BCF2;
using System.Xml.Serialization;
using System.Windows.Controls;

namespace ARUP.IssueTracker.Bentley
{
    public class ComponentController : IComponentController
    {
        private BentleyWindow window;

        public ComponentController(BentleyWindow window) 
        {
            this.window = window;
        }

        public void selectElements(List<Component> components)
        {

        }

    }
}
