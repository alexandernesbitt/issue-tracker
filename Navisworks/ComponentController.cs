using ARUP.IssueTracker.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ARUP.IssueTracker.Navisworks
{
    public class ComponentController : IComponentController
    {
        private NavisWindow window;

        public ComponentController(NavisWindow window)
        {
            this.window = window;
        }

        public override void selectElements(List<Classes.BCF2.Component> components)
        {
            window.SelectElements(components);
        }
    }
}
