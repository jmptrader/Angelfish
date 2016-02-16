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

using Angelfish.AfxSystem.A.Common.Plugins;

namespace Angelfish.AfxSystem.A.Common.Ui.Plugins
{
    public partial class AfxComponentEndpointView : UserControl
    {
        /// <summary>
        /// The component port view updates the data model with the
        /// coordinate of its exact center; this is used by the workflow
        /// to position connector lines between the ports on components.
        /// </summary>
        public static readonly DependencyProperty ConnectionPointProperty =
            DependencyProperty.Register("ConnectionPoint", typeof(Point), typeof(AfxComponentEndpointView));


        public Point ConnectionPoint
        {
            get { return (Point)GetValue(ConnectionPointProperty); }
            set { SetValue(ConnectionPointProperty, value); }
        }

        public AfxComponentEndpointViewModel Model
        {
            get { return DataContext as AfxComponentEndpointViewModel; }
        }

        public AfxComponentEndpointView()
        {
            InitializeComponent();
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var context = AfxVisualTree.FindAncestor<UserControl>(this);
            Point position = this.TranslatePoint(new Point(0, 0), context);
            position.X += (this.ActualWidth / 2) + 1;
            position.Y += (this.ActualHeight / 2) + 1;
            ConnectionPoint = position;
        }
    }
}
