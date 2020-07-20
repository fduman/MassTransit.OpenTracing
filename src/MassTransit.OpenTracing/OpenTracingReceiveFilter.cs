using System;
using System.Linq;
using System.Threading.Tasks;
using GreenPipes;
using MassTransit.Internals.Extensions;
using OpenTracing;
using OpenTracing.Propagation;
using OpenTracing.Util;

namespace MassTransit.OpenTracing
{
    public class OpenTracingReceiveFilter : IFilter<ReceiveContext>
    {
        public async Task Send(ReceiveContext context, IPipe<ReceiveContext> next)
        {
            var operationName = $"Receiving Message: {context.InputAddress.GetExchangeName()}";

            ISpanBuilder spanBuilder;

            try
            {
                var headers = context.TransportHeaders.GetAll().ToDictionary(pair => pair.Key, pair => pair.Value.ToString());
                var parentSpanCtx = GlobalTracer.Instance.Extract(BuiltinFormats.TextMap, new TextMapExtractAdapter(headers));

                spanBuilder = GlobalTracer.Instance.BuildSpan(operationName);
                if (parentSpanCtx != null)
                {
                    spanBuilder = spanBuilder.AsChildOf(parentSpanCtx);
                }
            }
            catch (Exception)
            {
                spanBuilder = GlobalTracer.Instance.BuildSpan(operationName);
            }

            spanBuilder
                .WithTag("redelivered", context.Redelivered)
                .WithTag("is-faulted", context.IsFaulted)
                .WithTag("is-delivered", context.IsDelivered)
                .WithTag("elapsed-time", context.ElapsedTime.ToFriendlyString())
                .WithTag("input-address", context.InputAddress.ToString());

            using (var scope = spanBuilder.StartActive(true))
            {
                await next.Send(context);
            }
        }

        public void Probe(ProbeContext context)
        {
        }
    }
}
