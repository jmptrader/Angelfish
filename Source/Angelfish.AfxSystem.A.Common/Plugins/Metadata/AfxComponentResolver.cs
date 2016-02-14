using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

using Angelfish.AfxSystem.X.Common.Plugins;


namespace Angelfish.AfxSystem.A.Common.Plugins.Metadata
{
    public class AfxComponentResolver
    {
        public Guid Id { get; private set; }

        public string Name { get; private set; }

        private IAfxComponentResolver _instance { get; set; }

        /// <summary>
        /// Initializes a new instance of a component resolver
        /// based on the implementation located in a component
        /// package's designer assembly.
        /// </summary>
        /// <param name="asm"></param>
        /// <param name="type"></param>
        public AfxComponentResolver(Assembly asm, Type type)
        {
            var identityAttribute = GetIdentityAttribute(type);
            if (identityAttribute != null)
            {
                this.Id = new Guid(identityAttribute.Id);
                this.Name = identityAttribute.Name;
            }

            // Create an instance of the resolver interface
            // implementation and assign it to the internal
            // reference for future use:
            _instance = asm.CreateInstance(type.FullName) as IAfxComponentResolver;

        }

        public object GetProperty(Guid component, string property)
        {
            return _instance.GetProperty(component, property);
        }


        private static AfxResolverIdentityAttribute GetIdentityAttribute(Type type)
        {
            var relevantType = typeof(AfxResolverIdentityAttribute);
            return type.GetCustomAttribute(relevantType) as AfxResolverIdentityAttribute;
        }

    }
}