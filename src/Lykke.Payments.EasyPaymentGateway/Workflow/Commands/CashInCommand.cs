using ProtoBuf;

namespace Lykke.Payments.EasyPaymentGateway.Workflow.Commands
{
    [ProtoContract]
    public class CashInCommand
    {
        [ProtoMember(1)]
        public string OrderId { get; set; }

        [ProtoMember(2)]
        public string Request { get; set; }
    }
}
