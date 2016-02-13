using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Angelfish.AfxSystem.X.Common.Plugins;

namespace Angelfish.AfxSystem.X.Plugins.Test
{
    [AfxOperatorIdentity(
        "Event Writer", 
        "91ffa5cd-3c21-421f-93f9-1a07b1fabc0d", 
        "a9e5855c-e558-4a43-b816-772795daffcd"
     )]
    public class TestPlugin_EventWriter : IAfxComponent
    {

        public void Activate(IServiceProvider services)
        {
            // TODO: Implement activation logic.
        }

        public void Init(IServiceProvider services)
        {
            // TODO: Implement initialization logic.
        }

        [AfxOperatorIncomingPort(
            "Incoming Events", 
            "d5ea04ab-b9ce-4374-a00b-6baf791a8503", 
            "message"
        )]
        public void IncomingPort1(object sender, EventArgs args)
        {

        }

        public void Shutdown(IServiceProvider services)
        {
            // TODO: Implement shutdown logic.
        }

    }
}
