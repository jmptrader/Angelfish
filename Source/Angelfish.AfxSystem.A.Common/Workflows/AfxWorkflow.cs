using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

using Angelfish.AfxSystem.A.Common.Plugins;
using Angelfish.AfxSystem.A.Common.Serialization;

namespace Angelfish.AfxSystem.A.Common.Workflows
{
    public class AfxWorkflow : IAfxSerializable
    {
        public Guid Id { get; private set; }

        /// <summary>
        /// The collection of plug-in component instances that have
        /// been added to an instance of a workflow.
        /// </summary>
        public ObservableCollection<AfxComponent> Components
        {
            get { return _components; }
        }

        private ObservableCollection<AfxComponent> _components = new ObservableCollection<AfxComponent>();

        /// <summary>
        /// The collection of plug-in component connections that have
        /// been added to an instance of a workflow.
        /// </summary>
        public ObservableCollection<AfxConnector> Connectors
        {
            get { return _connectors; }
        }

        private ObservableCollection<AfxConnector> _connectors = new ObservableCollection<AfxConnector>();

        /// <summary>
        /// The map of all logical connections that have been established
        /// between components, keyed by the connector definition that was
        /// used to originally create them; the system needs to remember the
        /// delegates that were used to establish logical connections between
        /// components, in case the end user wants to remove them.
        /// </summary>
        private Dictionary<AfxConnector, Delegate> _delegates = new Dictionary<AfxConnector, Delegate>();

        public AfxWorkflow()
        {
            this.Id = Guid.NewGuid();
            InitializeCollectionBindings();
        }

        /// <summary>
        /// The serialization constructor that is required in order
        /// to provide compatibility with the serialization system.
        /// </summary>
        /// <param name="id"></param>
        public AfxWorkflow(Guid id)
        {
            this.Id = id;
            InitializeCollectionBindings();
        }

        private void InitializeCollectionBindings()
        {
            // Register a collection change event handler to monitor any
            // changes that are made to the collection of components:
            _components.CollectionChanged += _components_CollectionChanged;

            // Register a collection change event handler to monitor any
            // chances that are made to the collection of connectors:
            _connectors.CollectionChanged += _connectors_CollectionChanged;
        }


        private void _components_CollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            // If a component is being removed from the workflow, then the
            // system also needs to remove any existing connections that have
            // been established between the component being removed and other
            // components in the workflow:
            if (args.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (var item in args.OldItems)
                {
                    var component = item as AfxComponent;
                    if (component != null)
                    {
                        // Get a list of all incoming bindings that have been
                        // established between this component and any other:
                        var incomingConnectors = GetIncomingConnectors(component.Id);
                        foreach (var connector in incomingConnectors)
                        {
                            // Removing the connector will trigger the deletion
                            // of the corresponding binding to the component:
                            _connectors.Remove(connector);
                        }


                        // Get a list of all outgoing bindings that have been
                        // established between this component and any other:
                        var outgoingConnectors = GetOutgoingConnectors(component.Id);
                        foreach (var connector in outgoingConnectors)
                        {
                            // Removing the connector will trigger the deletion
                            // of the corresponding binding from the component:
                            _connectors.Remove(connector);
                        }
                    }
                }
            }
        }

        private void _connectors_CollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            // If a connection is being added to the workflow, the system
            // needs to create a corresponding logical connection between
            // the actual components in the workflow:
            if (args.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (AfxConnector connector in args.NewItems)
                {
                    // Resolve the source operator instance:
                    var sourceOperator = GetComponent(connector.SourceOperator);
                    // Resolve the source operator endpoint:
                    var sourceEndpoint = sourceOperator.GetOutgoingPort(connector.SourceEndpoint);

                    // Resolve the target operator instance:
                    var targetOperator = GetComponent(connector.TargetOperator);
                    // Resolve the target operator endpoint:
                    var targetEndpoint = targetOperator.GetIncomingPort(connector.TargetEndpoint);

                    // Outgoing Ports are .NET events:
                    var sourceEventInfo = sourceEndpoint.Metadata as EventInfo;
                    var sourceEventType = sourceEventInfo.EventHandlerType;

                    // Incoming Ports are .NET event handlers:
                    var targetMethodInfo = targetEndpoint.Metadata as MethodInfo;

                    // Construct a delegate that matches the requirements
                    // for the destination endpoint's event handler:
                    var binding = Delegate.CreateDelegate(
                        sourceEventType,
                        targetOperator.Instance,
                        targetMethodInfo);

                    // Add the delegate event handler to the collection of
                    // event handlers on the source endpoint's event:
                    sourceEventInfo.AddEventHandler(sourceOperator.Instance, binding);

                    // Add the delegate to the internal dictionary so that it
                    // can be located if the system needs to remove it:
                    _delegates.Add(connector, binding);

                }
            }
            else if (args.Action == NotifyCollectionChangedAction.Remove)
            {
                // The user is removing a connection from the workflow, so the
                // system needs to also remove the logical connection that was
                // established between the source and target components:
                foreach (var item in args.OldItems)
                {
                    var connector = item as AfxConnector;
                    if (connector != null)
                    {
                        var sourceOperator = GetComponent(connector.SourceOperator);
                        var sourceEndpoint = sourceOperator.GetOutgoingPort(connector.SourceEndpoint);

                        var targetOperator = GetComponent(connector.TargetOperator);

                        var sourceEventInfo = sourceEndpoint.Metadata as EventInfo;
                        if (sourceEventInfo != null)
                        {
                            // Detach the delegate from the event:
                            var binding = _delegates[connector];
                            sourceEventInfo.RemoveEventHandler(targetOperator.Instance, binding);

                            // After the delegate has been detached, it can be
                            // removed from the internal dictionary as well:
                            _delegates.Remove(connector);
                        }
                    }
                }
            }
        }

        private AfxComponent GetComponent(Guid component)
        {
            return _components.FirstOrDefault(s => (s.Id.CompareTo(component) == 0));
        }

        private IEnumerable<AfxConnector> GetIncomingConnectors(Guid component)
        {
            return _connectors.Where(s => (s.TargetOperator.CompareTo(component) == 0));
        }

        private IEnumerable<AfxConnector> GetOutgoingConnectors(Guid component)
        {
            return _connectors.Where(s => (s.SourceOperator.CompareTo(component) == 0));
        }

        public void Serialize(IAfxObjectWriter serializer, IServiceProvider services)
        {
            serializer.WriteObject("Components", _components as Collection<AfxComponent>);
            serializer.WriteObject("Connectors", _connectors as Collection<AfxConnector>);
        }

        public void Deserialize(IAfxObjectReader serializer, IServiceProvider services)
        {
            serializer.ReadObject<Collection<AfxComponent>>("Components", components =>
            {
                foreach(var component in components)
                {
                    _components.Add(component);
                }
            });

            serializer.ReadObject<Collection<AfxConnector>>("Connectors", connectors =>
            {
                foreach(var connector in connectors)
                {
                    _connectors.Add(connector);
                }
            });
        }
    }

    
}