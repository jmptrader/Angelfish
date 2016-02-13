using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

using Angelfish.AfxSystem.A.Common.Services;

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
            // Add the application service container to the application's global
            // properties so that it can be accessed anywhere in the system:
            Application.Current.Properties.Add("App.Services", _appServices);
        }
    }
}
