using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

using Angelfish.AfxSystem.X.Common.Plugins;

namespace Angelfish.AfxSystem.X.Plugins.Test.Ui
{
    [AfxResolverIdentity("Resolver for Test Operators", "a9e5855c-e558-4a43-b816-772795daffcd")]
    public class Resolver : IAfxComponentResolver
    {
        /// <summary>
        /// The map of all component properties that the resolver can
        /// return to the system, keyed by the unique identifier of the
        /// plug-in component implementation and the property's name.
        /// </summary>
        private Dictionary<Guid, Dictionary<string, object>> _properties = 
            new Dictionary<Guid, Dictionary<string, object>>();

        public object GetProperty(Guid component, string property)
        {
            object result = null;

            // The resolver implementation should cache properties
            // after it has resolved them, so that the next lookup
            // can be done immediately:
            if(_properties.ContainsKey(component))
            {
                if(_properties[component].ContainsKey(property))
                {
                    return _properties[component][property];
                }
            }

            switch (property)
            {
                case "Component.Bitmap":
                    result = GetComponentBitmap(component);
                    break;

                case "Component.Dialog":
                    result = GetComponentDialog(component);
                    break;

                case "Component.Window":
                    result = GetComponentWindow(component);
                    break;
            }

            // If the requested property for the specified component
            // was successfully resolved, then add it to the internal
            // dictionary so that the next time it is requested it can
            // be resolved immediately:
            if(result != null)
            {
                if(!_properties.ContainsKey(component))
                {
                    _properties.Add(component, new Dictionary<string, object>());
                }

                if(!_properties[component].ContainsKey(property))
                {
                    _properties[component].Add(property, result);
                }
            }

            return result;
        }

        public object GetComponentBitmap(Guid component)
        {
            string path = "pack://application:,,,/Angelfish.AfxSystem.X.Plugins.Testing.Ui;component/Media/";
            switch (component.ToString())
            {
                // Get the bitmap image for the EventReader plug-in:
                case "67160b85-b9df-46d2-8a77-83f5ff76f8f7":
                    return new BitmapImage(new Uri(path + "TestPlugin_EventReader.png"));

                // Get the bitmap image for the EventWriter plug-in:
                case "91ffa5cd-3c21-421f-93f9-1a07b1fabc0d":
                    return new BitmapImage(new Uri(path + "TestPlugin_EventWriter.png"));

                // Get the bitmap image for the EventHandler plug-in:
                case "bd6a4419-ad37-4489-a3d9-3a3ada9dcb88":
                    return new BitmapImage(new Uri(path + "TestPlugin_EventHandler.png"));
            }

            return null;
        }

        public object GetComponentDialog(Guid component)
        {
            // TODO: Implement configuration dialogs for the
            // components in the testing package:
            return null;
        }

        public object GetComponentWindow(Guid component)
        {
            // TODO: Implement status windows for all of the
            // components in the testing package:
            return null;
        }
    }
}
