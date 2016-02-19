using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Angelfish.AfxSystem.X.Common.Plugins;

using Angelfish.AfxSystem.A.Common.Services;
using Angelfish.AfxSystem.A.Common.Plugins.Metadata;

namespace Angelfish.AfxSystem.A.Common.Plugins
{
    /// <summary>
    /// The AfxComponent class represents a single instance
    /// of a plug-in component that has been instantiated in
    /// the context of a specific event processing workflow.
    /// </summary>
    public class AfxComponent 
    {
        public Guid Id { get; private set; }

        public string Title { get; private set; }

        /// <summary>
        /// A reference to the live instance of the component
        /// that corresponds to the template that this class
        /// instance was originally constructed with:
        /// </summary>
        public IAfxComponent Instance { get; private set; }

        /// <summary>
        /// A reference to the component implementation's
        /// template, which allows the system to instantiate
        /// an actual instance of the corresponding type, and
        /// to discover information about the component, such
        /// as it's incoming and outgoing endpoints, etc.
        /// </summary>
        public AfxComponentTemplate Template { get; set; }

        /// <summary>
        /// The collection of properties that are associated
        /// with an instance of a component, mainly used for
        /// persisting attributes for the visual designer.
        /// </summary>
        public Dictionary<string, string> Properties = new Dictionary<string, string>();

        /// <summary>
        /// Serialization constructor required to provide
        /// compatibility with the serialization system.
        /// </summary>
        /// <param name="id"></param>
        public AfxComponent(Guid id)
        {
            this.Id = id;
        }

        /// <summary>
        /// Constructs a new instance of the class, based on
        /// the specified plug-in component prototype.
        /// </summary>
        /// <param name="template"></param>
        public AfxComponent(AfxComponentTemplate template)
        {
            // Assign a unique identifier to the component
            // instance, for use in keeping track of connections
            // betweeen individual components, and when the workflow
            // needs to serializer a workflow out to a file, whatever:
            this.Id = System.Guid.NewGuid();

            // Retrieve the plug-in component's title from the
            // prototype information:
            this.Title = template.Name;

            // Maintain a reference to the plug-in component's
            // original prototype for use by the system later on:
            this.Template = template;

            // Retrieve the fully-qualified type name from the
            // plug-in component's prototype:
            var typeName = template.Type.FullName;

            // Instantiate a new instance of the corresponding
            // plug-in implementation type and maintain a local
            // reference to it for later use:
            this.Instance = Template.Assembly.CreateInstance(typeName)
                as IAfxComponent;

        }

        /// <summary>
        /// The initialization method is invoked to initialize
        /// the underlying plug-in component implementation with
        /// any context-specific services that it might need.
        /// </summary>
        /// <param name="services"></param>
        public void Init(IServiceProvider services)
        {
            // We'll build this out in more detail as the tutorial
            // series moves further along; for now we're just going
            // to pass an empty service container to it:
            var componentServices = new AfxServices();
            if (this.Instance != null)
            {
                Instance.Init(componentServices);
            }
        }

        public AfxComponentEndpoint GetOutgoingPort(Guid id)
        {
            return Template.OutgoingPorts.First(s => s.Id.CompareTo(id) == 0);
        }

        public AfxComponentEndpoint GetIncomingPort(Guid id)
        {
            return Template.IncomingPorts.First(s => s.Id.CompareTo(id) == 0);
        }
        

    }
}
