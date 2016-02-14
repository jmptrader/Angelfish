using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Reflection;

using Angelfish.AfxSystem.A.Common.Services;
using Angelfish.AfxSystem.A.Common.Plugins.Metadata;

namespace Angelfish.AfxStudio
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// The application service container provides a global container
        /// for all of the services that are shared throughout the system.
        /// </summary>
        private AfxServices _appServices = new AfxServices();

        /// <summary>
        /// An event handler for the WPF application's "Startup" event which
        /// is invoked before the main window has been displayed. This is where
        /// the application will initialize common services and settings.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void Application_Startup(object sender, StartupEventArgs args)
        {
            // Initialize any shared services that will need to be added to the
            // application service container prior to the UI being loaded:
            InitializeApplicationServices();
        }

        private void InitializeApplicationServices()
        {
            // Retrieve the fully-qualified path to the current assembly:
            var pathAsm = Assembly.GetExecutingAssembly().Location;

            // Isolate the path to the application, sans the filename:
            var pathApp = System.IO.Path.GetDirectoryName(pathAsm);

            // Combine to form the fully-qualified path to the plugins:
            var pathPlugins = System.IO.Path.Combine(pathApp, "Plugins");

            // Initialize the plug-in catalog and add it's interface to the
            // application's global service container:
            var pluginCatalog = new AfxComponentCatalog(pathPlugins);

            // Add the plug-in system to the application service container:
            _appServices.AddService(typeof(IAfxComponentCatalog), pluginCatalog);

            // Add the application service container to the application's global
            // properties so that it can be accessed anywhere in the system:
            Application.Current.Properties.Add("App.Services", _appServices);
        }
    }
}
