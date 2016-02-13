using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Angelfish.AfxSystem.X.Common.Plugins
{
    /// <summary>
    /// The resolver interface is used to provide for a loose coupling
    /// between plug-in components and any GUI elements that they provide
    /// for use within the visual designer application. This interface is
    /// intended to be implemented in a separate assembly, along with all
    /// the plug-in component's GUI elements, and is then associated with
    /// components via a custom attribute on each component that ties the
    /// implementation of the plug-in component back to its corresponding
    /// resolver implementation.
    /// </summary>
    public interface IAfxComponentResolver
    {
        /// <summary>
        /// Retrieves a specific named property for a specific
        /// implementation of a plug-in component.
        /// </summary>
        /// <param name="id">
        /// The identity of the plug-in component that is relevant
        /// to the property resolution request; this should be the
        /// identifier that was assigned to the identity attribute
        /// on the component in the package it was loaded from.
        /// </param>
        /// <param name="property">
        /// The named property that is to be resolved. This can be
        /// any kind of object, based on the future development of
        /// the system, and the property names and expected return
        /// values should be documented in the developer's guide.
        /// </param>
        /// <returns></returns>
        object GetProperty(Guid component, string property);

    }

    /// <summary>
    /// The resolver identity attribute is used to assign a unique
    /// identifier to an implementation of a resolver, which is then
    /// used to correlate the resolver to its relevant components by
    /// specifying this same identifier as the 'resolver' parameter in
    /// the component's operator identity attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class AfxResolverIdentityAttribute : Attribute
    {
        public string Id { get; private set; }

        public string Name { get; private set; }

        public AfxResolverIdentityAttribute(string name, string id)
        {
            this.Id = id;
            this.Name = name;
        }
    }
}
