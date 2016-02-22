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
    class AppWindowDocument_Workflow : IAppWindowDocument
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

        public AppWindowDocument_Workflow()
        {
            _documentPane = new LayoutDocument();
            _documentPane.Content = new AfxWorkflowView(new AfxWorkflow());
            _documentPane.Title = "New Workflow";
        }

        public AppWindowDocument_Workflow(string path)
        {
            if(string.IsNullOrEmpty(path))
            {
                throw new ArgumentException();
            }

            _documentPath = path;
            _documentPane = new LayoutDocument();
            _documentPane.Content = new AfxWorkflowView(ImportDocument(path));
            _documentPane.Title = System.IO.Path.GetFileNameWithoutExtension(path);
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
            var appServices = Application.Current.Properties["App.Services"] as IServiceProvider;
            if (appServices == null)
            {
                throw new InvalidOperationException();
            }

            var documentView = _documentPane.Content as AfxWorkflowView;
            if(documentView != null)
            {
                var documentData = documentView.Model;
                if(documentData != null)
                {
                    var serializer = new AfxSerializer(appServices);
                    serializer.Serialize(path, documentData);
                }
            }
        }

        private AfxWorkflow ImportDocument(string path)
        {
            var appServices = Application.Current.Properties["App.Services"] as IServiceProvider;
            if(appServices == null)
            {
                throw new InvalidOperationException();
            }

            var serializer = new AfxSerializer(appServices);
            return serializer.Deserialize(path) as AfxWorkflow;
        }
    }
}
