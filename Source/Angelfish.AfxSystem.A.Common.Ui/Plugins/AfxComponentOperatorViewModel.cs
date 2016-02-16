using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

using Angelfish.AfxSystem.A.Common.Plugins;
using Angelfish.AfxSystem.A.Common.Plugins.Metadata;

namespace Angelfish.AfxSystem.A.Common.Ui.Plugins
{
    public class AfxComponentOperatorViewModel : INotifyPropertyChanged
    {
        public Guid Id { get; private set; }
        
        /// <summary>
        /// The title of the component is displayed as a label on
        /// the workflow design surface:
        /// </summary>
        public string Title { get; private set; }

        /// <summary>
        /// The image that acts as the icon for the component when
        /// it is displayed on the workflow design surface:
        /// </summary>
        public BitmapImage Image { get; set; }

        public bool IsSelected
        {
            get { return _isSelected; }

            set { _isSelected = value; OnPropertyChanged("IsSelected"); }
        }

        private bool _isSelected { get; set; }

        /// <summary>
        /// The accessor for binding the collection of incoming
        /// port representations to the component view:
        /// </summary>
        public ObservableCollection<AfxComponentEndpointViewModel> IncomingPorts
        {
            get { return _incomingPorts; }
            
        }

        /// <summary>
        /// The collection of incoming port view models that represent
        /// the incoming endpoints on the underlying component:
        /// </summary>
        private ObservableCollection<AfxComponentEndpointViewModel> _incomingPorts =
            new ObservableCollection<AfxComponentEndpointViewModel>();

        /// <summary>
        /// The accessor for binding the collection of incoming
        /// port representations to the component view:
        /// </summary>
        public ObservableCollection<AfxComponentEndpointViewModel> OutgoingPorts
        {
            get { return _outgoingPorts; }
        }

        /// <summary>
        /// The collection of incoming port view models that represent
        /// the incoming endpoints on the underlying component:
        /// </summary>
        private ObservableCollection<AfxComponentEndpointViewModel> _outgoingPorts =
            new ObservableCollection<AfxComponentEndpointViewModel>();

        public event PropertyChangedEventHandler PropertyChanged;


        public AfxComponent Model { get; private set; }

        public AfxComponentOperatorViewModel(AfxComponent component, BitmapImage icon)
        {
            Model = component;

            this.Id = component.Id;
            this.Title = component.Template.Name;
            this.Image = icon;

            foreach (var incomingPort in component.Template.IncomingPorts)
            {
                _incomingPorts.Add(new AfxComponentEndpointViewModel(incomingPort));
            }

            foreach (var outgoingPort in component.Template.OutgoingPorts)
            {
                _outgoingPorts.Add(new AfxComponentEndpointViewModel(outgoingPort));
            }
        }

        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
