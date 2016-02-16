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

namespace Angelfish.AfxSystem.A.Common.Ui.Plugins
{
    public partial class AfxComponentOperatorView : UserControl
    {
        public AfxComponentOperatorViewModel Model
        {
            get { return DataContext as AfxComponentOperatorViewModel; }
        }


        public AfxComponentOperatorView(AfxComponentOperatorViewModel model)
        {
            InitializeComponent();
            this.DataContext = model;
        }

        /// <summary>
        /// Returns the X,Y coordinate that is closest to the center of the
        /// visual representation of one of the component's incoming ports.
        /// </summary>
        /// <param name="identifier"></param>
        /// <returns></returns>
        public Point GetIncomingPortPosition(string identifier)
        {
            var portModel = Model.IncomingPorts.FirstOrDefault(port => port.Id.CompareTo(identifier) == 0 ? true : false);
            if (portModel != null)
            {
                return portModel.Centerpoint;
            }

            throw new ArgumentOutOfRangeException();

        }
        /// <summary>
        /// 
        /// Returns the X,Y coordinate that is closest to the center of the
        /// visual representation of one of the component's outgoing ports.
        /// </summary>
        /// <param name="identifier"></param>
        /// <returns></returns>
        public Point GetOutgoingPortPosition(string identifier)
        {
            var portModel = Model.OutgoingPorts.FirstOrDefault(port => port.Id.CompareTo(identifier) == 0 ? true : false);
            if (portModel != null)
            {
                return portModel.Centerpoint;
            }

            throw new ArgumentOutOfRangeException();
        }
    }
}
