using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Angelfish.AfxSystem.A.Common.Ui.Plugins.Metadata
{
    /// <summary>
    /// Interaction logic for AfxComponentCatalogView.xaml
    /// </summary>
    public partial class AfxComponentCatalogView : UserControl
    {
        private Point _mouseOrigin { get; set; }

        public AfxComponentCatalogView()
        {
            InitializeComponent();
        }

        private void _TreeView_Control_MouseLeftButtonDown(object sender, MouseButtonEventArgs args)
        {
            _mouseOrigin = args.GetPosition(null);
        }

        private void _TreeView_Control_MouseMove(object sender, MouseEventArgs args)
        {
            if(args.LeftButton == MouseButtonState.Pressed)
            {
                var mousePosition = args.GetPosition(null);
                var mouseTraveled = _mouseOrigin - mousePosition;
                
                var thresholdY = SystemParameters.MinimumVerticalDragDistance;
                var thresholdX = SystemParameters.MinimumHorizontalDragDistance;

                // Determine if the mouse has traveled far enough from the original
                // position to trigger the start of a drag-and-drop operation:
                if ((Math.Abs(mouseTraveled.Y) < thresholdY) && (Math.Abs(mouseTraveled.X) < thresholdY))
                {
                    return;
                } 

                var treeNode = args.OriginalSource as DependencyObject;
                if (treeNode == null)
                {
                    return;
                }

                var treeItem = AfxVisualTree.FindAncestor<TreeViewItem>(treeNode);
                if (treeItem == null)
                {
                    return;
                }

                var itemData = treeItem.DataContext as AfxComponentCatalogViewModel.ComponentTemplate;

                // The only data that we need, in order to drop the
                // component onto the workflow, is the unique identifier
                // associated with the selected component prototype:
                var relevantData = new DataObject("Component.Template.Id", itemData.Id.ToString());
                DragDrop.DoDragDrop(treeItem, relevantData, DragDropEffects.Copy);
            }
        }
    }
}
