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
