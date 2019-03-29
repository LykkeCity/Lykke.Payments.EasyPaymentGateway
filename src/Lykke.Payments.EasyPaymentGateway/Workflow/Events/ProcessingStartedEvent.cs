using ProtoBuf;

namespace Lykke.Payments.EasyPaymentGateway.Workflow.Events
{
    [ProtoContract]
    public class ProcessingStartedEvent
    {
        [ProtoMember(1)]
        public string OrderId { get; set; }
    }
}
