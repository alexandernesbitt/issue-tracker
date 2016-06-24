using ARUP.IssueTracker.Classes.BCF2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ARUP.IssueTracker.Windows
{
    //This is an abstraction for attached components within various (versions) authoring tools
    public interface IComponentController
    {
        void selectElements(List<Component> components);
    }
}
