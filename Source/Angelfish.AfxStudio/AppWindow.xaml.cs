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

using Microsoft.Win32;

using Angelfish.AfxSystem.A.Common.Services;
using Angelfish.AfxSystem.A.Common.Workflows;
using Angelfish.AfxSystem.A.Common.Plugins.Metadata;
using Angelfish.AfxSystem.A.Common.Serialization;
using Angelfish.AfxSystem.A.Common.Ui.Plugins.Metadata;

using Angelfish.AfxSystem.A.Common.Ui.Workflows;

namespace Angelfish.AfxStudio
{
    /// <summary>
    /// Interaction logic for AppWindow.xaml
    /// </summary>
    public partial class AppWindow : Window
    {
        /// <summary>
        /// The map of all documents that have been loaded into the
        /// visual designer, keyed by their respective views:
        /// </summary>
        private Dictionary<object, IAppWindowDocument> _mapDocumentsByView = 
            new Dictionary<object, IAppWindowDocument>();

        /// <summary>
        /// The map of all documents that have been loaded into the
        /// visual designer, keyed by their respective paths.
        /// </summary>
        private Dictionary<string, IAppWindowDocument> _mapDocumentsByPath =
            new Dictionary<string, IAppWindowDocument>();

        /// <summary>
        /// Reference to the most recently active document in the
        /// docking container; this is used to keep the reference
        /// so that the user can hit File->Save even if the focus
        /// in the docking container is on a toolbar or whatever.
        /// </summary>
        private IAppWindowDocument _activeDocument { get; set; }

        public AppWindow()
        {
            InitializeComponent();
        }

        public void File_New_Executed(object sender, ExecutedRoutedEventArgs args)
        {
            _activeDocument = new AppWindowDocument_Workflow();
            _mapDocumentsByView.Add(_activeDocument.DocumentPane.Content, _activeDocument);

            _Docking_Layout_Document_Pane.Children.Add(_activeDocument.DocumentPane);
            _activeDocument.DocumentPane.IsActive = true;
            
        }

        public void File_New_CanExecute(object sender, CanExecuteRoutedEventArgs args)
        {
            args.CanExecute = true;
        }

        

        private void DocumentPane_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        public void File_Open_Executed(object sender, ExecutedRoutedEventArgs args)
        {
            var fileDialog = new OpenFileDialog();
            
            fileDialog.DefaultExt = ".json";
            fileDialog.Filter = "Angelfish Workflows (*.json)|*.json";
            fileDialog.InitialDirectory = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            if(fileDialog.ShowDialog() == true)
            {
                // Check if the user is attempting to open a file that has
                // already been loaded into the application; if that's the
                // case, then just bring the existing document into view:
                if(_mapDocumentsByPath.ContainsKey(fileDialog.FileName))
                {
                    var appDocument = _mapDocumentsByPath[fileDialog.FileName];
                    appDocument.DocumentPane.IsActive = true;
                    return;
                }

                _activeDocument = new AppWindowDocument_Workflow(fileDialog.FileName);

                _mapDocumentsByPath.Add(_activeDocument.DocumentPath, _activeDocument);
                _mapDocumentsByView.Add(_activeDocument.DocumentPane.Content, _activeDocument);

                _Docking_Layout_Document_Pane.Children.Add(_activeDocument.DocumentPane);
                _activeDocument.DocumentPane.IsActive = true;
                
            }
        }

        public void File_Open_CanExecute(object sender, CanExecuteRoutedEventArgs args)
        {
            args.CanExecute = true;
        }

        public void File_Save_Executed(object sender, ExecutedRoutedEventArgs args)
        {
            if (_activeDocument != null)
            {
                _activeDocument.OnDocumentSave();
            }
        }

        public void File_Save_CanExecute(object sender, CanExecuteRoutedEventArgs args)
        {
            args.CanExecute = (_activeDocument != null) ? true : false;
        }

        public void File_Save_As_Executed(object sender, ExecutedRoutedEventArgs args)
        {
            if(_activeDocument != null)
            {
                var previousPath = _activeDocument.DocumentPath;
                _activeDocument.OnDocumentSaveAs();
                var selectedPath = _activeDocument.DocumentPath;

                if(previousPath != null)
                {
                    _mapDocumentsByPath.Remove(previousPath);
                }

                if(selectedPath != null)
                {
                    _mapDocumentsByPath.Add(selectedPath, _activeDocument);
                }
            }
        }

        public void File_Save_As_CanExecute(object sender, CanExecuteRoutedEventArgs args)
        {
            args.CanExecute = (_activeDocument != null) ? true : false;
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

        

        /// <summary>
        /// The event handler that is triggered by the docking manager when
        /// the active pane in the docking container is changed. Note that this
        /// event occurs not just for documents, but for any pane in the docking
        /// container, so there's no guarantee the active content represents one
        /// of the documents - it could be anything.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void _Docking_Manager_ActiveContentChanged(object sender, EventArgs args)
        {
            // The docking manager returns a reference to the actual
            // user control that is contained in the layout pane, so
            // the object reference is used to lookup the corresponding
            // document details structure in the dictionary:
            object activeContent = _Docking_Manager.ActiveContent;
            if(activeContent is AfxWorkflowView)
            {
                var documentView = activeContent as AfxWorkflowView;
                if(documentView != null)
                {
                    if(_mapDocumentsByView.ContainsKey(documentView))
                    {
                        _activeDocument = _mapDocumentsByView[documentView];
                    }
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
