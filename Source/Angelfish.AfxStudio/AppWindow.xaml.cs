using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Xceed.Wpf.AvalonDock.Layout;

using Angelfish.AfxSystem.A.Common.Services;
using Angelfish.AfxSystem.A.Common.Workflows;
using Angelfish.AfxSystem.A.Common.Plugins.Metadata;
using Angelfish.AfxSystem.A.Common.Ui.Plugins.Metadata;

using Angelfish.AfxSystem.A.Common.Ui.Workflows;

namespace Angelfish.AfxStudio
{
    /// <summary>
    /// Interaction logic for AppWindow.xaml
    /// </summary>
    public partial class AppWindow : Window
    {
        public AppWindow()
        {
            InitializeComponent();
        }

        public void File_New_Executed(object sender, ExecutedRoutedEventArgs args)
        {
            // Create a new instance of a worflow deisgn surface and add it
            // to the docking container's collection of document panes:
            var documentView = new AfxWorkflowView(new AfxWorkflow());

            var documentPane = new LayoutDocument();
            documentPane.Content = documentView;

            // NOTE: This is hard-coded for now, we'll revisit this a little
            // later in the tutorial, when we start thinking about how we're
            // going to persist workflow data out to a file...
            documentPane.Title = "New Workflow";
            _Docking_Layout_Document_Pane.Children.Add(documentPane);
        }

        public void File_New_CanExecute(object sender, CanExecuteRoutedEventArgs args)
        {
            args.CanExecute = true;
        }

        public void File_Open_Executed(object sender, ExecutedRoutedEventArgs args)
        {

        }

        public void File_Open_CanExecute(object sender, CanExecuteRoutedEventArgs args)
        {
            args.CanExecute = true;
        }

        public void File_Save_Executed(object sender, ExecutedRoutedEventArgs args)
        {

        }

        public void File_Save_CanExecute(object sender, CanExecuteRoutedEventArgs args)
        {
            args.CanExecute = false;
        }

        public void File_Save_As_Executed(object sender, ExecutedRoutedEventArgs args)
        {

        }

        public void File_Save_As_CanExecute(object sender, CanExecuteRoutedEventArgs args)
        {
            args.CanExecute = false;
        }

        public void File_Exit_Executed(object sender, ExecutedRoutedEventArgs args)
        {
            Application.Current.Shutdown();
        }

        public void File_Exit_CanExecute(object sender, CanExecuteRoutedEventArgs args)
        {
            args.CanExecute = true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Retrieve a reference to the application's service container:
            var appServices = Application.Current.Properties["App.Services"] as AfxServices;
            if (appServices != null)
            {
                // Retrieve a reference to the plug-in catalog service:
                var pluginCatalog = appServices.GetService(typeof(IAfxComponentCatalog))
                    as IAfxComponentCatalog;

                // Instantiate a view model for the plug-in catalog user
                // control and bind it so that we can display all of the
                // plug-in prototypes that are loaded into the system:
                if (pluginCatalog != null)
                {
                    var viewModel = new AfxComponentCatalogViewModel(pluginCatalog);
                    _Component_Catalog_View.DataContext = viewModel;
                }
            }
        }
    }

    /// <summary>
    /// The routed command declarations for all of the commands
    /// that are supported by the main menu and toolbars in the
    /// visual designer application.
    /// </summary>
    public static class AppCommands
    {
        public static RoutedCommand File_New =
            new RoutedCommand("File New", typeof(AppCommands));

        public static RoutedUICommand File_Open =
            new RoutedUICommand("Open", "Open", typeof(AppCommands));

        public static RoutedUICommand File_Save =
            new RoutedUICommand("Save", "Save As", typeof(AppCommands));

        public static RoutedUICommand File_Save_As =
            new RoutedUICommand("Save As", "Save As", typeof(AppCommands));

        public static RoutedUICommand File_Exit =
            new RoutedUICommand("Exit", "Exit", typeof(AppCommands));
    }
}
