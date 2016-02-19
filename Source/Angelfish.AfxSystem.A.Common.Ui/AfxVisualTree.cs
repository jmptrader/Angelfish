using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Angelfish.AfxSystem.A.Common.Ui
{
    /// <summary>
    /// This is just a helper class for methods that relate
    /// to finding elements in the WPF visual tree.
    /// </summary>
    public class AfxVisualTree
    {
        /// <summary>
        /// This is a helper method used that searches the visual
        /// tree for an ancestor element of a specific type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="current"></param>
        /// <param name="parentName"></param>
        /// <returns></returns>
        public static T FindAncestor<T>(DependencyObject current, string parent=null) 
            where T : DependencyObject
        {
            if (current != null)
            {
                current = VisualTreeHelper.GetParent(current);
            }

            while (current != null)
            {
                if (!string.IsNullOrEmpty(parent))
                {
                    var element = current as FrameworkElement;
                    if (element != null)
                    {
                        if(current is T && element.Name == parent)
                        {
                            return (T)current;
                        }
                    }
                }
                else if (current is T)
                {
                    return (T)current;
                }
                current = VisualTreeHelper.GetParent(current);
            };

            return null;
        }
    }
}
