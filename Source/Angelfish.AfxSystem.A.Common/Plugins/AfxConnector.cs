using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Angelfish.AfxSystem.A.Common.Serialization;

namespace Angelfish.AfxSystem.A.Common.Plugins
{
    public class AfxConnector : IAfxSerializable
    {
        public Guid Id { get; private set; }

        public Guid SourceOperator { get; private set; }

        public Guid SourceEndpoint { get; private set; }

        public Guid TargetOperator { get; private set; }

        public Guid TargetEndpoint { get; private set; }

        public AfxConnector(Guid id)
        {
            this.Id = id;
        }

        public AfxConnector(Guid sourceOperator, Guid sourceEndpoint, Guid TargetOperator, Guid TargetEndpoint)
        {
            this.Id = System.Guid.NewGuid();

            this.SourceOperator = sourceOperator;
            this.SourceEndpoint = sourceEndpoint;
            this.TargetOperator = TargetOperator;
            this.TargetEndpoint = TargetEndpoint;
        }

        public void Serialize(IAfxObjectWriter serializer, IServiceProvider services)
        {
            serializer.WriteObject("SourceOperator", SourceOperator.ToString());
            serializer.WriteObject("SourceEndpoint", SourceEndpoint.ToString());
            serializer.WriteObject("TargetOperator", TargetOperator.ToString());
            serializer.WriteObject("TargetEndpoint", TargetEndpoint.ToString());
        }

        public void Deserialize(IAfxObjectReader serializer, IServiceProvider services)
        {
            this.SourceOperator = new Guid(serializer.ReadObject<string>("SourceOperator"));
            this.SourceEndpoint = new Guid(serializer.ReadObject<string>("SourceEndpoint"));
            this.TargetOperator = new Guid(serializer.ReadObject<string>("TargetOperator"));
            this.TargetEndpoint = new Guid(serializer.ReadObject<string>("TargetEndpoint"));
        }
    }
}
