using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Diagnostics;
using Newtonsoft.Json.Linq;

using Angelfish.AfxSystem.X.Common.Plugins;

namespace Angelfish.AfxSystem.A.Common.Plugins.Metadata
{
    /// <summary>
    /// The AfxComponentPackage class encapsulates all of the 
    /// information about a collection of related components that
    /// has been registered in the component catalog.
    /// </summary>
    public class AfxComponentPackage : IAfxComponentPackage
    {
        public string Title { get; private set; }

        public string Homepage { get; private set; }

        public string Description { get; private set; }

        /// <summary>
        /// Reference to the loaded assembly that contains all the
        /// component implementations for the package:
        /// </summary>
        private Assembly _operatorAssembly { get; set; }

        /// <summary>
        /// Reference to the designer assembly, which contains the
        /// artifacts and resolvers that are required for using this
        /// package within the visual designer application.
        /// </summary>
        private Assembly _designerAssembly { get; set; }

        public ReadOnlyCollection<AfxComponentTemplate> Templates
        {
            get { return _templates.AsReadOnly(); }
        }

        /// <summary>
        /// The list of all component templates that were discovered
        /// in the component package's operator assembly.
        /// </summary>
        private List<AfxComponentTemplate> _templates = new List<AfxComponentTemplate>();

        public ReadOnlyCollection<AfxComponentResolver> Resolvers
        {
            get { return _resolvers.AsReadOnly(); }
        }

        /// <summary>
        /// The list of all component resolves that were discovered
        /// in the component package's designer assembly.
        /// </summary>
        private List<AfxComponentResolver> _resolvers = new List<AfxComponentResolver>();

        /// <summary>
        /// Initializes a new instance of a component package based
        /// on the information in the specified manifest file.
        /// </summary>
        /// <param name="filename">
        /// The fully-qualified path to the location of a component
        /// package manifest file that defines all of the information
        /// that is needed to initialize a new instance of a package.
        /// </param>
        public AfxComponentPackage(string filename)
        {
            var manifestData = JObject.Parse(File.ReadAllText(filename));
            var manifestInfo = manifestData["Angelfish"]["Package"];

            this.Title = manifestInfo["Title"]?.ToString();
            this.Homepage = manifestInfo["Homepage"]?.ToString();
            this.Description = manifestInfo["Description"]?.ToString();

            // Retrieve the name of the assembly DLL from the manifest
            // and use it to build the full path to the assembly file:
            var assemblyFile = manifestInfo["Assembly"]?.ToString();
            var assemblyPath = Path.Combine(Path.GetDirectoryName(filename), assemblyFile);

            // Attempt to retrieve all of the unique identification
            // properties for the specified assembly file:
            var assemblyData = AssemblyName.GetAssemblyName(assemblyPath);

            // Attempt to load the actual assembly into memory, using the
            // full name of the assembly so that the assembly loader probes
            // the PrivatePath specified in App.config, and loads the assembly
            // into the regular application "Load" context:
            this._operatorAssembly = Assembly.Load(assemblyData.FullName);

            // Iterate over all of the classes that are defined in 
            // the source assembly and identify any that support the
            // system's plug-in interface:
            foreach (Type t in _operatorAssembly.GetTypes())
            {
                // Retrieve the identity attribute for this component:
                var identityAttribute = GetOperatorIdentityAttribute(t);

                // If the attribute is present, then consider this class
                // to be an instance of a plug-in component implementation:
                if (identityAttribute != null)
                {
                    // The component prototype class will handle the rest:
                    AfxComponentTemplate prototype = new AfxComponentTemplate(_operatorAssembly, t);

                    // Add this new component to the internal collection:
                    _templates.Add(prototype);
                }
            }

            // After the component assembly has been loaded and scanned
            // for all of the plug-in implementations in the package, the
            // system attempts to load the designer assembly, which should
            // contain the artifacts and resolver(s) that are needed for use
            // with the visual designer application:
            var designerRoot = Path.GetDirectoryName(filename);

            // The name of the designer assembly is implicit, and is based on
            // the operator assembly's name, with ".Ui" tacked on to it:
            var designerFile = String.Format("{0}.Ui.dll", Path.GetFileNameWithoutExtension(assemblyFile));
            var designerPath = Path.Combine(designerRoot, designerFile);

            // The designer assembly may not be present, especially if this
            // package is being loaded on the server side as opposed to being
            // loaded in the visual designer application:
            if (File.Exists(designerPath))
            {
                // Retrieve the unique identification information for the designer
                // assembly, without actually loading it into the application:
                var designerData = AssemblyName.GetAssemblyName(designerPath);

                // Attempt to load the designer assembly into memory, using the
                // full name of the assembly so that the loader uses the private
                // probe path in App.config and subsequently loads the assembly
                // into the normal application load context:
                _designerAssembly = Assembly.Load(designerData.FullName);

                // Iterate over all of the types that are declared in the
                // component's designer assembly and locate any types that
                // implement the IAfxComponentResolver interface:
                foreach (Type t in _designerAssembly.GetTypes())
                {
                    // Retrieve the identity attribute for this component:
                    var identityAttribute = GetResolverIdentityAttribute(t);

                    // If the attribute is present, then consider this class
                    // to be an instance of a plug-in component implementation:
                    if (identityAttribute != null)
                    {
                        // Add this new component to the internal collection:
                        _resolvers.Add(new AfxComponentResolver(_designerAssembly, t));
                    }
                }
            }
        }

        public IEnumerator<AfxComponentTemplate> GetEnumerator()
        {
            foreach (AfxComponentTemplate template in _templates)
            {
                yield return template;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private static AfxOperatorIdentityAttribute GetOperatorIdentityAttribute(Type type)
        {
            var relevantType = typeof(AfxOperatorIdentityAttribute);
            return type.GetCustomAttribute(relevantType) as AfxOperatorIdentityAttribute;
        }

        private static AfxResolverIdentityAttribute GetResolverIdentityAttribute(Type type)
        {
            var relevantType = typeof(AfxResolverIdentityAttribute);
            return type.GetCustomAttribute(relevantType) as AfxResolverIdentityAttribute;
        }
    }

    /// <summary>
    /// The interface for a component package definition is intended
    /// to provide read-only access to information about a collection
    /// of related components that is registed in the component catalog.
    /// </summary>
    public interface IAfxComponentPackage : IEnumerable<AfxComponentTemplate>
    {
        string Title { get; }

        string Homepage { get; }

        string Description { get; }

        ReadOnlyCollection<AfxComponentTemplate> Templates { get; }

        ReadOnlyCollection<AfxComponentResolver> Resolvers { get; }
    }
}
