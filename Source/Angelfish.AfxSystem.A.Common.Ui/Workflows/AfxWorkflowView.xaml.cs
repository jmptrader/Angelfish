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

using Angelfish.AfxSystem.A.Common.Plugins;
using Angelfish.AfxSystem.A.Common.Plugins.Metadata;

using Angelfish.AfxSystem.A.Common.Ui.Plugins;

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
            // Access the application's global service container:
            var appServices = Application.Current.Properties["App.Services"]
                as IServiceProvider;

            // Access the plug-in catalog which will have already
            // been added to the application's service container:
            var pluginCatalog = appServices.GetService(typeof(IAfxComponentCatalog))
                as IAfxComponentCatalog;

            // Retrieve the prototype from the catalog, based on the
            // unique identifier that was dropped onto the workflow:
            var componentType = pluginCatalog.GetComponentTemplate(template);

            // Create a new instance of a component data model, using
            // the corresponding component prototype:
            var componentImpl = new AfxComponent(componentType);

            // Set the surface coordinates on the component instance
            // so that they can be serialized later, when the workflow
            // is saved to a file or deployed to the server side:
            componentImpl.Properties.Add("Surface.X", position.X.ToString());
            componentImpl.Properties.Add("Surface.Y", position.Y.ToString());

            BitmapImage componentIcon = null;

            // Attempt to retrieve the resolver for the specified component
            // so that we can get the bitmap image associated with it:
            var resolver = pluginCatalog.GetComponentResolver(componentType.Resolver);
            if(resolver != null)
            {
                componentIcon = resolver.GetProperty(componentType.Id, "Component.Bitmap") 
                    as BitmapImage;
            }

            // Create the corresponding visual representation of the
            // component on the design surface:
            Surface_CreateComponent(componentImpl, componentIcon);
        }

        /// <summary>
        /// Creates the visual representation of a single
        /// instance of a workflow component on the design
        /// surface, at the coordinates that are specified
        /// in the component's properties.
        /// </summary>
        /// <param name="component"></param>
        private void Surface_CreateComponent(AfxComponent component, BitmapImage icon)
        {
            // Create the view model for the component:
            var componentModel = new AfxComponentOperatorViewModel(component, icon);

            // Create the view, assign the view model to it:
            var componentView = new AfxComponentOperatorView(componentModel);

            var X = double.Parse(component.Properties["Surface.X"]);
            var Y = double.Parse(component.Properties["Surface.Y"]);

            componentView.Margin = new Thickness(X, Y, 0, 0);
            componentView.VerticalAlignment = VerticalAlignment.Top;
            componentView.HorizontalAlignment = HorizontalAlignment.Left;

            _ScrollViewer_Content.Children.Add(componentView);
        }

    }
}
