using System.Collections.Generic;
using System.Linq;
using GreenPipes;

namespace MassTransit.OpenTracing
{
    public class OpenTracingPipeSpecification : IPipeSpecification<ConsumeContext>, IPipeSpecification<PublishContext>, IPipeSpecification<ReceiveContext>
    {
        public IEnumerable<ValidationResult> Validate()
        {
            return Enumerable.Empty<ValidationResult>();
        }

        public void Apply(IPipeBuilder<ReceiveContext> builder)
        {
            builder.AddFilter(new OpenTracingReceiveFilter());
        }

        public void Apply(IPipeBuilder<ConsumeContext> builder)
        {
            builder.AddFilter(new OpenTracingConsumeFilter());
        }

        public void Apply(IPipeBuilder<PublishContext> builder)
        {
            builder.AddFilter(new OpenTracingPublishFilter());
        }
    }
}
