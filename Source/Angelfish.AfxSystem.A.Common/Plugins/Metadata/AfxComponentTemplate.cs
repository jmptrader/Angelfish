using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

using Angelfish.AfxSystem.X.Common.Plugins;

namespace Angelfish.AfxSystem.A.Common.Plugins.Metadata
{
    public class AfxComponentTemplate
    {
        /// <summary>
        /// The unique identifier associated with the corresponding
        /// component's implementation; retrieved from the operator
        /// identity attribute on the plug-in class.
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// The human-readable name associated with the corresponding
        /// component's implementation; retrieved from the operator 
        /// identity attribute on the plug-in class.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// The actual .NET type of the corresponding component.
        /// </summary>
        public Type Type { get; private set; }

        /// <summary>
        /// The actual .NET assembly that the corresponding component
        /// implementation can be found in.
        /// </summary>
        public Assembly Assembly { get; private set; }

        /// <summary>
        /// The collection of incoming port definitions that are defined
        /// on the corresponding plug-in component's implementation.
        /// </summary>
        public ReadOnlyCollection<AfxComponentEndpoint> IncomingPorts
        {
            get { return new ReadOnlyCollection<AfxComponentEndpoint>(this._incomingPorts); }
        }

        private Collection<AfxComponentEndpoint> _incomingPorts = new Collection<AfxComponentEndpoint>();

        /// <summary>
        /// The collection of outgoing port definitions that are defined
        /// on the corresponding plug-in component's implementation.
        /// </summary>
        public ReadOnlyCollection<AfxComponentEndpoint> OutgoingPorts
        {
            get { return new ReadOnlyCollection<AfxComponentEndpoint>(this._outgoingPorts); }
        }

        private Collection<AfxComponentEndpoint> _outgoingPorts = new Collection<AfxComponentEndpoint>();

        /// <summary>
        /// Construct a new instance of AfxComponentType, using the assembly
        /// and type information of the plug-in class. This constructor uses
        /// reflection to retrieve metadata from custom attributes that the
        /// plug-in author will have added to their plug-in implementation.
        /// </summary>
        /// <param name="asm"></param>
        /// <param name="type"></param>
        public AfxComponentTemplate(Assembly asm, Type type)
        {
            this.Type = type;
            this.Assembly = asm;

            // Retrieve the AfxComponentIdentityAttribute from the
            // plug-in implementation's type information and extract
            // the relevant metadata from it.
            var identityAttribute = GetIdentityAttribute(type);
            if (identityAttribute != null)
            {
                this.Id = new Guid(identityAttribute.Id);
                this.Name = identityAttribute.Name;
            }

            // Retrieve the AfxOperatorIncomingPortAttribute(s) from the
            // plug-in implementation's incoming port declarations and use
            // the metadata to create definitions for each incoming port:
            foreach (MethodInfo m in type.GetMethods())
            {
                var attribute = GetIncomingPortAttribute(m);
                if (attribute != null)
                {
                    var port = new AfxComponentEndpoint(new Guid(attribute.Id), attribute.Name, attribute.Type, m);
                    this._incomingPorts.Add(port);
                }
            }

            // Retrieve the AfxOperatorOutgoingPortAttribute(s) from the
            // plug-in implementation's outgoing port declarations and use
            // the metadata to create definitions for each outgoing port:
            foreach (EventInfo e in type.GetEvents())
            {
                var attribute = GetOutgoingPortAttribute(e);
                if (attribute != null)
                {
                    var port = new AfxComponentEndpoint(new Guid(attribute.Id), attribute.Name, attribute.Type, e);
                    this._outgoingPorts.Add(port);
                }
            }
        }

        private static AfxOperatorIdentityAttribute GetIdentityAttribute(Type type)
        {
            var relevantType = typeof(AfxOperatorIdentityAttribute);
            return type.GetCustomAttribute(relevantType) as AfxOperatorIdentityAttribute;
        }

        private static AfxOperatorIncomingPortAttribute GetIncomingPortAttribute(MethodInfo metadata)
        {
            var relevantType = typeof(AfxOperatorIncomingPortAttribute);
            return metadata.GetCustomAttribute(relevantType) as AfxOperatorIncomingPortAttribute;
        }

        private static AfxOperatorOutgoingPortAttribute GetOutgoingPortAttribute(EventInfo metadata)
        {
            var relevantType = typeof(AfxOperatorOutgoingPortAttribute);
            return metadata.GetCustomAttribute(relevantType) as AfxOperatorOutgoingPortAttribute;
        }
    }
}
