using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;

using Xceed.Wpf.AvalonDock.Layout;

using Angelfish.AfxSystem.A.Common.Workflows;
using Angelfish.AfxSystem.A.Common.Serialization;

using Angelfish.AfxSystem.A.Common.Ui.Workflows;

namespace Angelfish.AfxStudio
{
    /// <summary>
    /// An implementation of the application window
    /// wrapper for instances of workflows that have
    /// been loaded into the system.
    /// </summary>
    public class AppWindowDocument_Workflow : IAppWindowDocument
    {
        public string DocumentPath
        {
            get { return _documentPath; }
        }

        public LayoutDocument DocumentPane
        {
            get { return _documentPane; }
        }

        /// <summary>
        /// The fully qualified path that the document was
        /// most recently loaded from or saved to.
        /// </summary>
        private string _documentPath { get; set; }

        private LayoutDocument _documentPane { get; set; }

        /// <summary>
        /// The workflow document instances need a reference to
        /// the application's service container, because loading
        /// a serialized workflow requires access to the component
        /// catalog in order to reconstitute the plug-in operators
        /// that are present on the workflow. 
        /// </summary>
        private IServiceProvider _documentSvcs { get; set; }

        public AppWindowDocument_Workflow(IServiceProvider services)
        {
            _documentPane = new LayoutDocument();
            _documentPane.Content = new AfxWorkflowView(new AfxWorkflow());
            _documentPane.Title = "New Workflow";
            _documentSvcs = services;
        }

        public void OnDocumentOpen(string path)
        {
            _documentPane.Content = ImportDocument(path);
            _documentPane.Title = System.IO.Path.GetFileNameWithoutExtension(path);
            _documentPath = path;
        }

        public void OnDocumentClose()
        {
            OnDocumentSave();
        }

        public void OnDocumentSave()
        {
            if (!string.IsNullOrEmpty(_documentPath))
            {
                ExportDocument(_documentPath);
            }
            else
            {
                OnDocumentSaveAs();
            }
        }

        public void OnDocumentSaveAs()
        {
            var fileDialog = new SaveFileDialog();
            if (!string.IsNullOrEmpty(_documentPath))
            {
                fileDialog.InitialDirectory = System.IO.Path.GetDirectoryName(_documentPath);

                fileDialog.FileName = System.IO.Path.GetFileNameWithoutExtension(_documentPath);
            }
            else
            {
                fileDialog.InitialDirectory = 
                    System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                fileDialog.FileName = MakeFilename(fileDialog.InitialDirectory);
            }

            fileDialog.DefaultExt = ".json";
            fileDialog.Filter = "Angelfish Workflows (*.json)|*.json";

            if (fileDialog.ShowDialog() == true)
            {
                ExportDocument(fileDialog.FileName);

                _documentPath = fileDialog.FileName;
                _documentPane.Title = System.IO.Path.GetFileNameWithoutExtension(_documentPath);
            }
        }

        private string MakeFilename(string path)
        {
            var filename = "AFX Workflow.json";
            var filepath = System.IO.Path.Combine(path, filename);

            int attempts = 1;
            while (System.IO.File.Exists(filepath))
            {
                filename = String.Format("AFX Workflow {0}.json", attempts++);
                filepath = System.IO.Path.Combine(path, filename);
            }

            return filename;
        }

        private void ExportDocument(string path)
        {
            var documentView = _documentPane.Content as AfxWorkflowView;
            if(documentView != null)
            {
                var documentData = documentView.Model;
                if(documentData != null)
                {
                    var serializer = new AfxSerializer(_documentSvcs);
                    serializer.Serialize(path, documentData);
                }
            }
        }

        private AfxWorkflow ImportDocument(string path)
        {
            var serializer = new AfxSerializer(_documentSvcs);
            return serializer.Deserialize(path) as AfxWorkflow;
        }
    }
}
