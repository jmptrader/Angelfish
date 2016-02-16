using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Angelfish.AfxSystem.A.Common.Plugins
{
    public class AfxConnector
    {
        public Guid SourceOperator { get; private set; }

        public Guid SourceEndpoint { get; private set; }

        public Guid TargetOperator { get; private set; }

        public Guid TargetEndpoint { get; private set; }

        public AfxConnector(Guid sourceOperator, Guid sourceEndpoint, Guid TargetOperator, Guid TargetEndpoint)
        {
            this.SourceOperator = sourceOperator;
            this.SourceEndpoint = sourceEndpoint;
            this.TargetOperator = TargetOperator;
            this.TargetEndpoint = TargetEndpoint;
        }
    }
}
