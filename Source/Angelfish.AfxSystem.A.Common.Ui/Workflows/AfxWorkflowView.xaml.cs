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

        /// <summary>
        /// Flag that indicates whether or not the the current
        /// drag operation involves an instance of a port:
        /// </summary>
        private bool _draggingComponentEndpointView { get; set; }

        /// <summary>
        /// The line that is drawn on the workflow surface
        /// to represent a potential connection between an
        /// outgoing and incoming port - tracks the cursor
        /// position while a user is dragging an outgoing
        /// port around on the design surface.
        /// </summary>
        private Line _draggingComponentEndpointLine = new Line();

        /// <summary>
        /// The map of all component operator views that are
        /// present in the workflow, keyed by their respective
        /// instance identifiers.
        /// </summary>
        private Dictionary<Guid, AfxComponentOperatorView> _mapOperatorViews =
            new Dictionary<Guid, AfxComponentOperatorView>();

        /// <summary>
        /// The map of all connection lines that have been added
        /// to the design surface, keyed by the unique identifier
        /// of the component that owns the outgoing endpoint that
        /// the connection originates from.
        /// </summary>
        private Dictionary<Guid, List<Line>> _mapOutgoingLinesByOperatorView =
            new Dictionary<Guid, List<Line>>();

        /// <summary>
        /// The map of all connection lines that have been added
        /// to the design surface, keyed by the unique identifier
        /// of the component that owns the incoming endpoint that
        /// the connection line terminates on.
        /// </summary>
        private Dictionary<Guid, List<Line>> _mapIncomingLinesByOperatorView =
            new Dictionary<Guid, List<Line>>();


        public AfxWorkflowView()
        {
            InitializeComponent();

            _draggingComponentEndpointLine.Stroke = Brushes.LightBlue;
            _draggingComponentEndpointLine.StrokeThickness = 2;
            _draggingComponentEndpointLine.Fill = Brushes.Black;

            _draggingComponentEndpointLine.Visibility = Visibility.Hidden;
            _ScrollViewer_Content.Children.Add(_draggingComponentEndpointLine);
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

            // Maintain a reference to the component operator view, keyed by the
            // unique identifier associated with the underlying component instance:
            _mapOperatorViews.Add(component.Id, componentView);

            // Add the new component operator view to the design surface:
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
                // Determine if the user has clicked on an endpoint view:
                var endpointView = AfxVisualTree.FindAncestor<AfxComponentEndpointView>(hitResult.VisualHit);
                if (endpointView != null)
                {
                    Surface_ComponentEndpointView_MouseDown(endpointView);
                    return;
                }

                // Determine if the user has clicked on an operator view:
                var componentView = AfxVisualTree.FindAncestor<AfxComponentOperatorView>(hitResult.VisualHit);
                if (componentView != null)
                {
                    Surface_ComponentOperatorView_MouseDown(componentView);
                    return;
                }
            }

            // If the user clicks on an area of the design surface that does
            // not contain any items that can be selected, then the currently
            // selected items will be released:
            Surface_ResetSelectedItems();
        }

        private void Surface_ComponentOperatorView_MouseDown(AfxComponentOperatorView target)
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
                if (!_selectedItems.Contains(target))
                {
                    target.Model.IsSelected = true;
                    _selectedItems.Add(target);
                }
                else
                {
                    _selectedItemsPending.Add(target);
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
                if (!_selectedItems.Contains(target))
                {
                    Surface_ResetSelectedItems();

                    target.Model.IsSelected = true;
                    _selectedItems.Add(target);
                }
                else
                {
                    // The component has already been selected, so it
                    // will be added to the pending list to ensure that
                    // it is deselected unless this mouse down event is
                    // the start of a drag operation:
                    _selectedItemsPending.Add(target);
                }
            }
        }

        private void Surface_ComponentEndpointView_MouseDown(AfxComponentEndpointView target)
        {
            // If the user is selecting an endpoint, then any selected items
            // that are not component ports need to be reset:
            if (_selectedItems.Count > 0 && !(_selectedItems[0] is AfxComponentEndpointView))
            {
                Surface_ResetSelectedItems();
            }
           
            // If the component port has not already been selected
            // then it can be added to the selected items list:
            if (!_selectedItems.Contains(target))
            {
                Surface_ResetSelectedItems();

                target.Model.IsSelected = true;
                _selectedItems.Add(target);
            }
            else
            {
                // The component has already been selected, so it
                // will be added to the pending list to ensure that
                // it is deselected unless this mouse down event is
                // the start of a drag operation:
                _selectedItemsPending.Add(target);
            }
        }

        private void _ScrollViewer_Content_MouseMove(object sender, MouseEventArgs args)
        {
            if (_mouseLeftButtonDown)
            {
                var mousePosition = args.GetPosition(_ScrollViewer_Content);
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

                        // Adjust the position of any connection lines
                        // that originate from the component being dragged:
                        if (_mapIncomingLinesByOperatorView.ContainsKey(componentView.Model.Id))
                        {
                            var relevantLines = _mapIncomingLinesByOperatorView[componentView.Model.Id];
                            foreach (var relevantLine in relevantLines)
                            {
                                relevantLine.X2 += mouseTraveled.X;
                                relevantLine.Y2 += mouseTraveled.Y;
                            }
                        }

                        // Adjust the position of any connection lines
                        // that terminate at the component being dragged:
                        if (_mapOutgoingLinesByOperatorView.ContainsKey(componentView.Model.Id))
                        {
                            var relevantLines = _mapOutgoingLinesByOperatorView[componentView.Model.Id];
                            foreach (var relevantLine in relevantLines)
                            {
                                relevantLine.X1 += mouseTraveled.X;
                                relevantLine.Y1 += mouseTraveled.Y;
                            }
                        }
                    }
                    
                    // Adjust the mouse down position so that it reflects the
                    // distance traveled during this update:
                    _mouseLeftButtonDownPosition = mousePosition;
                    return;
                }
                
                if(_draggingComponentEndpointView == true)
                {
                    _draggingComponentEndpointLine.X2 = args.GetPosition(_ScrollViewer_Content).X;
                    _draggingComponentEndpointLine.Y2 = args.GetPosition(_ScrollViewer_Content).Y;

                    // Adjust the mouse down position so that it reflects the
                    // distance traveled during this update:
                    _mouseLeftButtonDownPosition = mousePosition;
                    return;
                }

                double threshY = SystemParameters.MinimumVerticalDragDistance;
                double threshX = SystemParameters.MinimumHorizontalDragDistance;

                if (Math.Abs(mouseTraveled.X) > threshX || Math.Abs(mouseTraveled.Y) > threshY)
                {
                    // Determine if an operator view is being dragged:
                    if ((_selectedItems.Count > 0) && (_selectedItems[0] is AfxComponentOperatorView))
                    {
                        _draggingComponentOperatorView = true;
                        return;
                    }

                    // Determine if an endpoint view is being dragged:
                    if ((_selectedItems.Count > 0) && (_selectedItems[0] is AfxComponentEndpointView))
                    {
                        Surface_ComponentEndpoint_DragStarting();
                        return;
                    }

                    _selectedItemsPending.Clear();
                }
            }
        }

        private void Surface_ComponentEndpoint_DragStarting()
        {
            if (_selectedItems.Count == 1)
            {
                var item = _selectedItems[0] as AfxComponentEndpointView;
                if (item != null)
                {
                    var origin = item.TranslatePoint(new Point(0, 0), _ScrollViewer_Content);
                    origin.X += (item.ActualWidth - 1) / 2;
                    origin.Y += (item.ActualHeight - 1) / 2;

                    _draggingComponentEndpointLine.X1 = origin.X;
                    _draggingComponentEndpointLine.Y1 = origin.Y;
                    _draggingComponentEndpointLine.X2 = origin.X;
                    _draggingComponentEndpointLine.Y2 = origin.Y;

                    _draggingComponentEndpointLine.Visibility = Visibility.Visible;
                    _draggingComponentEndpointView = true;
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }
            else
            {
                throw new InvalidOperationException();
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

            if (_draggingComponentEndpointView)
            {
                Surface_ComponentEndpoint_DragComplete();
                return;
            }

            Surface_ResetPendingItems();
        }

        private void Surface_ComponentEndpoint_DragComplete()
        {
            // No matter what, clear the dragging component port
            // flag and hide the indicator line:
            _draggingComponentEndpointView = false;
            _draggingComponentEndpointLine.Visibility = Visibility.Hidden;

            // Retrieve the current mouse position, relative to
            // the workflow's design surface:
            var mousePosition = Mouse.GetPosition(_ScrollViewer_Content);

            // Determine if the mouse button was released over an
            // instance of a component view on the design surface:
            var hit = VisualTreeHelper.HitTest(_ScrollViewer_Content, mousePosition);
            if (hit.VisualHit != null)
            {
                var targetPort = Surface_GetRelatedEndpointView(hit.VisualHit);
                if (targetPort != null)
                {
                    var sourcePort = _selectedItems[0] as AfxComponentEndpointView;
                    if (sourcePort == null)
                    {
                        throw new InvalidOperationException();
                    }

                    var sourceView = Surface_GetRelatedOperatorView(sourcePort);
                    if(sourceView == null)
                    {
                        throw new InvalidOperationException();
                    }

                    var targetView = Surface_GetRelatedOperatorView(targetPort);
                    if(targetView == null)
                    {
                        throw new InvalidOperationException();
                    }
                    
                    // Ensure that the user is not dragging a connector
                    // from an outgoing port to an incoming port on the
                    // same component view:
                    if (sourceView == targetView)
                    {
                        return;
                    }

                    // Ensure that the user is dropping an outgoing endpoint
                    // onto an instance of an incoming endpoint; a connection
                    // must be between an outgoing and incoming endpoints:
                    if(!targetView.Model.IncomingPorts.Contains(targetPort.Model))
                    {
                        return;
                    }
                    
                    var connection = new AfxConnector
                        (
                            sourceView.Model.Id,
                            sourcePort.Model.Id,
                            targetView.Model.Id,
                            targetPort.Model.Id
                        );

                    // Create the visual representation of the connection
                    // between the two components on the design surface:
                    Surface_CreateConnection(connection);
                }
            }
        }

        private void Surface_CreateConnection(AfxConnector connection)
        {
            var sourceView = Surface_GetComponentOperatorView(connection.SourceOperator);
            if (sourceView == null)
            {
                throw new InvalidOperationException();
            }

            var targetView = Surface_GetComponentOperatorView(connection.TargetOperator);
            if (targetView == null)
            {
                throw new InvalidOperationException();
            }

            // Retreive the X,Y coordinate for the center position of the
            // specified outgoing port on the source component:
            var point1 = sourceView.GetOutgoingPortPosition(connection.SourceEndpoint);
            point1 = sourceView.TranslatePoint(point1, _ScrollViewer_Content);

            // Retrieve the X,Y coordinate for the center position of the
            // specific incoming port on the target component:
            var point2 = targetView.GetIncomingPortPosition(connection.TargetEndpoint);
            point2 = targetView.TranslatePoint(point2, _ScrollViewer_Content);

            Line connectionLine = new Line();
            connectionLine.Stroke = Brushes.Black;


            connectionLine.X1 = point1.X;
            connectionLine.Y1 = point1.Y;
            connectionLine.X2 = point2.X;
            connectionLine.Y2 = point2.Y;

            // Ensure that connector lines are drawn at a lower z-index than
            // any of the other items on the design surface:
            Panel.SetZIndex(connectionLine, -1);


            // Create an entry in the map of incoming lines, so that we can find
            // the line attached to this component operator view; if the user moves
            // the source operator, we'll need to update this line's coordinates:
            if (!_mapOutgoingLinesByOperatorView.ContainsKey(connection.SourceOperator))
            {
                _mapOutgoingLinesByOperatorView.Add(connection.SourceOperator, new List<Line>());
            }

            _mapOutgoingLinesByOperatorView[connection.SourceOperator].Add(connectionLine);


            // Create an entry in the map of incoming lines, so that we can find
            // the line attached to this component operator view; if the user moves
            // the target operator, we'll need to update this line's coordinates:
            if (!_mapIncomingLinesByOperatorView.ContainsKey(connection.TargetOperator))
            {
                _mapIncomingLinesByOperatorView.Add(connection.TargetOperator, new List<Line>());
            }

            _mapIncomingLinesByOperatorView[connection.TargetOperator].Add(connectionLine);


            _ScrollViewer_Content.Children.Add(connectionLine);
        }

        private AfxComponentOperatorView Surface_GetComponentOperatorView(Guid id)
        {
            if(_mapOperatorViews.ContainsKey(id))
            {
                return _mapOperatorViews[id];
            }

            return null;
        }

        /// <summary>
        /// Helper method that retrieves the parent operator view
        /// for a specific dependecy object in the visual tree.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private AfxComponentOperatorView Surface_GetRelatedOperatorView(DependencyObject item)
        {
            return AfxVisualTree.FindAncestor<AfxComponentOperatorView>(item);
        }

        /// <summary>
        /// Helper method that retrieves the parent endpoint view
        /// for a specific dependency object in the visual tree.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private AfxComponentEndpointView Surface_GetRelatedEndpointView(DependencyObject item)
        {
            return AfxVisualTree.FindAncestor<AfxComponentEndpointView>(item);
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
