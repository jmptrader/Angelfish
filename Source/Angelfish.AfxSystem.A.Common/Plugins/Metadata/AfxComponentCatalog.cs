using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Angelfish.AfxSystem.A.Common.Plugins.Metadata
{
    public class AfxComponentCatalog : IAfxComponentCatalog
    {
        /// <summary>
        /// The catalog maintains a collection of all of the
        /// packages that have been loaded into the system.
        /// </summary>
        private Collection<AfxComponentPackage> _packages =
            new Collection<AfxComponentPackage>();

        /// <summary>
        /// The catalog maintains a global dictionary of all the
        /// component prototypes, keyed by their respective ids:
        /// </summary>
        private Dictionary<Guid, AfxComponentTemplate> _templates =
            new Dictionary<Guid, AfxComponentTemplate>();

        private Dictionary<Guid, AfxComponentResolver> _resolvers =
            new Dictionary<Guid, AfxComponentResolver>();

        /// <summary>
        /// Constructs an instance of a component catalog by scanning
        /// through all the assemblies in the specified path and importing
        /// all of the plug-in packages that are defined in them.
        /// </summary>
        /// <param name="path"></param>
        public AfxComponentCatalog(string path)
        {
            // Scan all of the folders beneath the plugins folder
            // and load any component assemblies that are found there:
            foreach (string folder in Directory.GetDirectories(path))
            {
                var manifestPath = System.IO.Path.Combine(folder, "Manifest.json");
                if (File.Exists(manifestPath))
                {
                    var componentPackage = new AfxComponentPackage(manifestPath);
                    foreach (AfxComponentTemplate template in componentPackage)
                    {
                        // Add a reference to all of the components in the package
                        // to the local dictionary so that the system can use this 
                        // catalog interface to resolve component identifiers back
                        // to their corresponding component templates.
                        _templates.Add(template.Id, template);
                    }

                    foreach (AfxComponentResolver resolver in componentPackage.Resolvers)
                    {
                        _resolvers.Add(resolver.Id, resolver);
                    }

                    // Maintain a reference to this package, mainly because the 
                    // design application will need to be able to enumerate all
                    // the packages in order to display them in the UI.
                    _packages.Add(componentPackage);
                }
            }
        }

        /// <summary>
        /// Returns a reference to the component prototype that corresponds
        /// to the supplied component identifier; the system supports the use
        /// of any text string for this purpose, as long as it is only assigned
        /// to a single plug-in component implementation.
        /// </summary>
        /// <param name="identifier"></param>
        /// <returns></returns>
        public AfxComponentTemplate GetComponentTemplate(Guid component)
        {
            if (_templates.ContainsKey(component))
            {
                return _templates[component];
            }

            throw new ArgumentOutOfRangeException();
        }

        public AfxComponentResolver GetComponentResolver(Guid component)
        {
            if (_resolvers.ContainsKey(component))
            {
                return _resolvers[component];
            }

            throw new ArgumentOutOfRangeException();

        }

        public IEnumerator<IAfxComponentPackage> GetEnumerator()
        {
            foreach (AfxComponentPackage package in _packages)
            {
                yield return package;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    /// <summary>
    /// The IAfxComponentCatalog interface provides for read-only access
    /// to the system's plug-in component catalog.
    /// </summary>
    public interface IAfxComponentCatalog : IEnumerable<IAfxComponentPackage>
    {
        AfxComponentTemplate GetComponentTemplate(Guid identifier);

        AfxComponentResolver GetComponentResolver(Guid identifier);
    }
}
