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
        /// <summary>
        /// Flag that indicates whether or not the left
        /// mouse button is currently depressed.
        /// </summary>
        private bool _mouseLeftButtonDown { get; set; }

        /// <summary>
        /// The position of the mouse when the left button
        /// was originally depressed; used for detecting if
        /// the mouse movement is past the system threshold
        /// for indicating the start of drag-and-drop.
        /// </summary>
        private Point _mouseLeftButtonDownPosition { get; set; }


        /// <summary>
        /// The collection of workflow items that have been
        /// selected by the end user.
        /// </summary>
        private List<object> _selectedItems = new List<object>();

        /// <summary>
        /// The collection of selected items that may need to
        /// be processed when the mouse button is released.
        /// </summary>
        private List<object> _selectedItemsPending = new List<object>();

        /// <summary>
        /// Flag that indicated whether or not the current 
        /// drag operation involves one or more instances of
        /// component views; the system does not allow views
        /// and ports to be dragged at the same time.
        /// </summary>
        private bool _draggingComponentOperatorView { get; set; }
        
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

        

        private void _ScrollViewer_Content_MouseLeftButtonDown(object sender, MouseButtonEventArgs args)
        {
            // The design surface simulates drag and drop operations
            // in some circumstances, so it's best to capture the mouse
            // when buttons are pressed and then release that capture when
            // the button is released, to ensure the workflow surface will
            // have exclusive access to all mouse events that occur between
            // the mouse down and mouse up events being signaled:
            _ScrollViewer_Content.CaptureMouse();

            _mouseLeftButtonDown = true;
            _mouseLeftButtonDownPosition = args.GetPosition(_ScrollViewer_Content);

            var hitResult = VisualTreeHelper.HitTest(_ScrollViewer_Content, _mouseLeftButtonDownPosition);
            if (hitResult.VisualHit != null)
            {
                // Check if the user has clicked on a component operator view:
                var componentView = AfxVisualTree.FindAncestor<AfxComponentOperatorView>(hitResult.VisualHit);
                if (componentView != null)
                {
                    Surface_ComponentView_MouseDown(componentView);
                    return;
                }
            }

            // If the user clicks on an area of the design surface that does
            // not contain any items that can be selected, then the currently
            // selected items will be released:
            Surface_ResetSelectedItems();
        }

        private void Surface_ComponentView_MouseDown(AfxComponentOperatorView item)
        {
            // If the user is selecting a component view, then any selected items
            // that are not component views need to be reset:
            if (_selectedItems.Count > 0 && !(_selectedItems[0] is AfxComponentOperatorView))
            {
                Surface_ResetSelectedItems();
            }

            // Determine whether or not the control key is down; if it is then we
            // need to handle mutiple selection, if not, just single selection:
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                if (!_selectedItems.Contains(item))
                {
                    item.Model.IsSelected = true;
                    _selectedItems.Add(item);
                }
                else
                {
                    _selectedItemsPending.Add(item);
                }
            }
            else
            {
                // If there is more than one item in the list of all
                // items that are currently selected, then we deselect
                // the other items and select this one instead:
                if (_selectedItems.Count > 1)
                {
                    Surface_ResetSelectedItems();
                }

                // If the component view has not already been selected
                // then it can be added to the selected items list:
                if (!_selectedItems.Contains(item))
                {
                    Surface_ResetSelectedItems();

                    item.Model.IsSelected = true;
                    _selectedItems.Add(item);
                }
                else
                {
                    // The component has already been selected, so it
                    // will be added to the pending list to ensure that
                    // it is deselected unless this mouse down event is
                    // the start of a drag operation:
                    _selectedItemsPending.Add(item);
                }
            }
        }

        private void _ScrollViewer_Content_MouseMove(object sender, MouseEventArgs args)
        {
            if (_mouseLeftButtonDown)
            {
                var mousePosition = args.GetPosition(_ScrollViewer_Control);
                var mouseTraveled = mousePosition - _mouseLeftButtonDownPosition;

                if (_draggingComponentOperatorView == true)
                {
                    foreach (var item in _selectedItems)
                    {
                        var componentView = item as AfxComponentOperatorView;

                        double adjustedY = mouseTraveled.Y + componentView.Margin.Top;
                        double adjustedX = mouseTraveled.X + componentView.Margin.Left;

                        // Adjust the item's position:
                        componentView.Margin = new Thickness(adjustedX, adjustedY, 0, 0);

                        // Adjust the persisted X,Y coordinates that are configured
                        // as properties on the underlying component instance:
                        componentView.Model.Model.Properties["Surface.X"] = adjustedX.ToString();
                        componentView.Model.Model.Properties["Surface.Y"] = adjustedY.ToString();
                    }

                    _mouseLeftButtonDownPosition = mousePosition;
                    return;
                }

                double threshY = SystemParameters.MinimumVerticalDragDistance;
                double threshX = SystemParameters.MinimumHorizontalDragDistance;

                if (Math.Abs(mouseTraveled.X) > threshX || Math.Abs(mouseTraveled.Y) > threshY)
                {
                    if ((_selectedItems.Count > 0) && (_selectedItems[0] is AfxComponentOperatorView))
                    {
                        _draggingComponentOperatorView = true;
                        return;
                    }

                    _selectedItemsPending.Clear();
                }
            }
        }

        private void _ScrollViewer_Content_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _ScrollViewer_Content.ReleaseMouseCapture();
            _mouseLeftButtonDown = false;

            if (_draggingComponentOperatorView)
            {
                _draggingComponentOperatorView = false;
                return;
            }

            Surface_ResetPendingItems();
        }

        /// <summary>
        /// This is a helper method for drag-and-drop support on the
        /// workflow design surface. This method resets the selection
        /// state of every selected item, then removes every item from
        /// the selected items list.
        /// </summary>
        private void Surface_ResetSelectedItems()
        {
            // Iterate over every selected item, convert it to the
            // appropriate type, reset its selection state to false:
            foreach (object item in _selectedItems)
            {
                if (item is AfxComponentOperatorView)
                {
                    var componentView = item as AfxComponentOperatorView;
                    componentView.Model.IsSelected = false;
                }
                else if (item is AfxComponentEndpointView)
                {
                    var componentPort = item as AfxComponentEndpointView;
                    componentPort.Model.IsSelected = false;
                }
            }

            _selectedItems.Clear();
        }


        /// <summary>
        /// This is a helper method that clears the list of items
        /// that are awaiting deselection on the design surface.
        /// </summary>
        private void Surface_ResetPendingItems()
        {
            foreach (object item in _selectedItemsPending)
            {
                if (item is AfxComponentOperatorView)
                {
                    var componentView = item as AfxComponentOperatorView;
                    componentView.Model.IsSelected = false;
                }

                _selectedItems.Remove(item);
            }

            _selectedItemsPending.Clear();
        }

       
    }
}
