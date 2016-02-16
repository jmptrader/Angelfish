using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using Angelfish.AfxSystem.A.Common.Plugins;
using Angelfish.AfxSystem.A.Common.Plugins.Metadata;

namespace Angelfish.AfxSystem.A.Common.Ui.Plugins
{
    public class AfxComponentEndpointViewModel : INotifyPropertyChanged
    {
        private AfxComponentEndpoint _model { get; set; }

        public Guid Id
        {
            get { return _model.Id; }
        }

        public string Name
        {
            get { return _model.Name; }
        }

        public string Type
        {
            get { return _model.Type; }
        }

        public object Metadata
        {
            get { return _model.Metadata; }
        }

        public Point Centerpoint
        {
            get { return _centerpoint; }
            set { _centerpoint = value; OnPropertyChanged("Centerpoint"); }
        }

        private Point _centerpoint { get; set; }

        public bool IsSelected
        {

            get { return _isSelected; }
            set { _isSelected = value; OnPropertyChanged("IsSelected"); }

        }

        private bool _isSelected { get; set; }

        public AfxComponentEndpointViewModel(AfxComponentEndpoint model)
        {
            _model = model;
        }

        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
