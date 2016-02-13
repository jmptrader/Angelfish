using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Angelfish.AfxSystem.X.Common.Plugins;

namespace Angelfish.AfxSystem.X.Plugins.Test
{
    [AfxOperatorIdentity(
        "Event Handler",
        "bd6a4419-ad37-4489-a3d9-3a3ada9dcb88",
        "a9e5855c-e558-4a43-b816-772795daffcd"
     )]
    public class TestPlugin_EventHandler : IAfxComponent
    {
        [AfxOperatorOutgoingPort(
            "Outgoing Events",
            "8519badb-6ec8-45d0-b635-764a2177cdc5", 
            "message"
        )]
        public event EventHandler<EventArgs> OutgoingPort1;

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
            "125919e3-0d0d-4f00-bb64-ddd2af2e7007",
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