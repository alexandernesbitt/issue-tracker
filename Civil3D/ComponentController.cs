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

namespace ARUP.IssueTracker.Civil3D
{
    public class ComponentController : IComponentController
    {
        private Civil3DWindow window;

        public ComponentController(Civil3DWindow window) 
        {
            this.window = window;
        }

        public void selectElements(List<Component> components)
        {

        }

    }
}
