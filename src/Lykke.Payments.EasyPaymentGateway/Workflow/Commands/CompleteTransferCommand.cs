using ProtoBuf;

namespace Lykke.Payments.EasyPaymentGateway.Workflow.Commands
{
    [ProtoContract]
    public class CompleteTransferCommand
    {
        [ProtoMember(1)]
        public string Id { get; set; }
        [ProtoMember(2)]
        public double Amount { get; set; }
    }
}
