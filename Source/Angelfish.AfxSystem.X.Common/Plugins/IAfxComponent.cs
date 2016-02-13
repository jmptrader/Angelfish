using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Angelfish.AfxSystem.X.Common.Plugins
{
    /// <summary>
    /// The basic interface for Angelfish plug-in operators. All operators that are
    /// developed for the system are required to implement this interface, and to use
    /// the associated custom attributes (defined below) to provide the corresponding
    /// metadata that the plug-in system will use to locate and import plug-ins.
    /// </summary>
    public interface IAfxComponent
    {
        /// <summary>
        /// The initialization method that is invoked when a plug-in operator is first
        /// added to a workflow. The services that are provided to the component when it
        /// is initialized will be documented externally.
        /// </summary>
        /// <param name="services"></param>
        void Init(IServiceProvider services);

        /// <summary>
        /// The activation method is invoked when the workflow that a plug-in operator
        /// belongs to is activated by the system (or the end user). This is where you
        /// would start background threads or carry out any other activities that need
        /// to be completed in order to start processing events.
        /// </summary>
        /// <param name="services"></param>
        void Activate(IServiceProvider services);

        /// <summary>
        /// The shutdown method is invoked when the workflow that a plug-in operator
        /// belongs to is being shutdown by the system (or the end user). This would
        /// be where you shutdown any threads or carry out any other activities that
        /// need to be completed in order to completely shutdown the plug-in.
        /// </summary>
        /// <param name="services"></param>
        void Shutdown(IServiceProvider services);
    }

    /// <summary>
    /// The AfxOperatorIdentityAttribute is used to decorate a plug-in operator with
    /// a display name (for use in the Visual Designer) and a GUID that will be used
    /// to identify the plug-in when it is added to workflow definitions.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class AfxOperatorIdentityAttribute : Attribute
    {
        public string Id { get; private set; }

        public string Name { get; private set; }

        public string Resolver { get; private set; }

        public AfxOperatorIdentityAttribute(string name, string id, string resolver)
        {
            this.Id = id;
            this.Name = name;
            this.Resolver = resolver;
        }
    }

    /// <summary>
    /// The AfxOperatorIncomingPortAttribute is used to decorate event handlers on
    /// a plug-in implementation so that they can be exposed as incoming endpoints
    /// in the Visual Designer, and accept connections from plug-in operators that
    /// implement compatible outgoing endpoints.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class AfxOperatorIncomingPortAttribute : Attribute
    {
        public string Id { get; private set; }

        public string Name { get; private set; }

        public string Type { get; private set; }

        public AfxOperatorIncomingPortAttribute(string name, string id, string type)
        {
            this.Id = id;
            this.Name = name;
            this.Type = type;
        }
    }

    /// <summary>
    /// The AfxOperatorOutgoingPortAttribute is used to decorate event implementations
    /// on plug-in operators so that they can be exposed as outgoing endpoints (ports)
    /// in the Visual Designer, and allow for the establishment of connections between
    /// the outgoing port and compatible incoming ports on other plug-ins.
    /// </summary>
    [AttributeUsage(AttributeTargets.Event)]
    public class AfxOperatorOutgoingPortAttribute : Attribute
    {
        public string Id { get; private set; }

        public string Name { get; private set; }

        public string Type { get; private set; }

        public AfxOperatorOutgoingPortAttribute(string name, string id, string type)
        {
            this.Id = id;
            this.Name = name;
            this.Type = type;
        }
    }
}

