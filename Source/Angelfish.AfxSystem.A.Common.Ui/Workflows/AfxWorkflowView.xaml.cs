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

namespace Angelfish.AfxSystem.A.Common.Ui.Workflows
{
    /// <summary>
    /// Interaction logic for AfxWorkflowView.xaml
    /// </summary>
    public partial class AfxWorkflowView : UserControl
    {
        public AfxWorkflowView()
        {
            InitializeComponent();
        }

        private void _ScrollViewer_Control_DragEnter(object sender, DragEventArgs args)
        {
            args.Effects = DragDropEffects.None;

            // Retrieve the source data from the drag operation and determine
            // whether or not it's compatible with the workflow design surface:
            var sourceData = args.Data as DataObject;
            if (sourceData != null)
            {
                // Check the payload of the drag-and-drop data to determine
                // if this represents a drag operation involving a template
                // from the component catalog's user control; if it is, the
                // payload will contain the GUID for a component template:
                if (args.Data.GetDataPresent("Component.Template.Id"))
                {
                    args.Effects = DragDropEffects.Copy;
                }
            }

            args.Handled = true;
        }

        private void _ScrollViewer_Control_DragOver(object sender, DragEventArgs args)
        {
            args.Effects = DragDropEffects.None;

            // Retrieve the source data from the drag operation and determine
            // whether or not it's compatible with the workflow design surface:
            var sourceData = args.Data as DataObject;
            if (sourceData != null)
            {
                // Check the payload of the drag-and-drop data to determine
                // if this represents a drag operation involving a template
                // from the component catalog's user control; if it is, the
                // payload will contain the GUID for a component template:
                if(args.Data.GetDataPresent("Component.Template.Id"))
                {
                    args.Effects = DragDropEffects.Copy;
                }
            }

            args.Handled = true;
        }

        private void _ScrollViewer_Control_Drop(object sender, DragEventArgs args)
        {
            var xPos = System.Math.Round(args.GetPosition(_ScrollViewer_Content).X);
            var yPos = System.Math.Round(args.GetPosition(_ScrollViewer_Content).Y);

            // Determine if a plug-in component is being dropped on the
            // workflow design surface, and respond accordingly:
            var sourceData = args.Data as DataObject;
            var sourceGuid = sourceData.GetData("Component.Template.Id") as string;
            if (sourceGuid != null)
            {
                HandleDrop_Component(new Point(xPos, yPos), new Guid(sourceGuid));
            }
        }

        private void HandleDrop_Component(Point position, Guid template)
        {

        }
    }
}
