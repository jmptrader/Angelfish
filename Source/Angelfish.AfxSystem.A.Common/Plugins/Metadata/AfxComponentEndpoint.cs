using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Angelfish.AfxSystem.A.Common.Plugins.Metadata
{
    public class AfxComponentEndpoint
    {
        /// <summary>
        /// The unique identifier assigned to the port, used to keep track of
        /// connections between components when they are added to a workflow.
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// The human-readable name assigned to the port, used to provide 
        /// information to the end user in the Visual Designer application.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// The actual .NET type of the plug-in implementation that this port
        ///  is associated with; used to assist when using .NET reflection.
        /// </summary>
        public string Type { get; private set; }

        /// <summary>
        /// The Metadata property is a generic placeholder that references 
        /// either a MethodInfo or EventInfo instance, depending on whether
        /// the corresponding port is an incoming or outgoing port.
        /// </summary>
        public object Metadata { get; private set; }

        public AfxComponentEndpoint(Guid id, string name, string type, object metadata)
        {
            this.Id = id;
            this.Name = name;
            this.Type = type;
            this.Metadata = metadata;
        }
    }
}