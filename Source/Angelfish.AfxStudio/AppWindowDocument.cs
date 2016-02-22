using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xceed.Wpf.AvalonDock.Layout;

namespace Angelfish.AfxStudio
{
    /// <summary>
    /// A simple interface for managing documents
    /// that are being displayed in the application.
    /// </summary>
    public interface IAppWindowDocument
    {
        /// <summary>
        /// The fully-qualified path to the file that the
        /// document was originally loaded from; this can
        /// be used to ensure that the user does not load
        /// the same file into the app twice, or to check
        /// the file for changes in real-time.
        /// </summary>
        string DocumentPath { get; }

        /// <summary>
        /// The docking pane that contains the view for the
        /// document so that it can be added into the docking
        /// container in the visual designer application.
        /// </summary>
        LayoutDocument DocumentPane { get; }

        void OnDocumentClose();

        void OnDocumentSave();

        void OnDocumentSaveAs();
    }
}
