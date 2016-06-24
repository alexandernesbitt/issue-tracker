using System.Windows;
using ARUP.IssueTracker.Classes;
using System.Collections.Generic;
using System.Linq;
using ARUP.IssueTracker.UserControls;

namespace ARUP.IssueTracker.Windows
{
    /// <summary>
    /// Interaction logic for Components.xaml
    /// </summary>
    public partial class ComponentsList : Window
    {
        List<ARUP.IssueTracker.Classes.BCF2.Component> components;
        MainPanel mainPan;

        public ComponentsList(ARUP.IssueTracker.Classes.BCF2.Component[] components, MainPanel mainPan)
        {
            InitializeComponent();
            this.mainPan = mainPan;
            this.components = components.ToList();
            componentsList.ItemsSource = this.components;
            
        }

        private void selectElementsButton_Click(object sender, RoutedEventArgs e)
        {
            if (mainPan.componentController != null)
            {
                mainPan.componentController.selectElements(components);
            }
            else if (mainPan.selectElements != null)
            {
                mainPan.selectElements(components);
            }
        }
    }
}
