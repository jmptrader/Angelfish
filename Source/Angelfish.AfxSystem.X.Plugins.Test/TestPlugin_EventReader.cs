using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Angelfish.AfxSystem.X.Common.Plugins;

namespace Angelfish.AfxSystem.X.Plugins.Test
{
    [AfxOperatorIdentity(
        "Event Reader", 
        "67160b85-b9df-46d2-8a77-83f5ff76f8f7", 
        "a9e5855c-e558-4a43-b816-772795daffcd")]
    public class TestPlugin_EventReader : IAfxComponent
    {
        [AfxOperatorOutgoingPort(
            "Outgoing Events", 
            "76c96339-ca46-4dd3-b6c4-797265298510", 
            "message")]
        public event EventHandler<EventArgs> OutgoingPort1;

        public void Activate(IServiceProvider services)
        {
            // TODO: Implement activation logic.
        }

        public void Init(IServiceProvider services)
        {
            // TODO: Implement initialization logic.
        }

        public void Shutdown(IServiceProvider services)
        {
            // TODO: Implement shutdown logic.
        }

    }
}
