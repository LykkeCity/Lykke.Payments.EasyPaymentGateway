using System.Collections.Generic;
using System.Xml.Serialization;

namespace Lykke.Payments.EasyPaymentGateway.Models
{
    [XmlRoot(ElementName = "payfrex-response")]
    public class CallbackStatusModel
    {
        [XmlElement(ElementName = "message")]
        public string Message { get; set; }

        [XmlArray(ElementName = "operations")]
        [XmlArrayItem(ElementName = "operation")]
        public List<Operation> Operations { get; set; }

        [XmlElement(ElementName = "status")]
        public string Status { get; set; }

        public CallbackStatusModel()
        {
            Operations = new List<Operation>();
        }
    }

    public class Operation
    {
        [XmlElement(ElementName = "amount")]
        public decimal Amount { get; set; }

        [XmlElement(ElementName = "currency")]
        public string Currency { get; set; }

        [XmlElement(ElementName = "details")]
        public string Details { get; set; }

        [XmlElement(ElementName = "merchantTransactionId")]
        public string MerchantTransactionId { get; set; }

        [XmlElement(ElementName = "payFrexTransactionId")]
        public string PayFrexTransactionId { get; set; }

        [XmlElement(ElementName = "message")]
        public string Message { get; set; }

        [XmlElement(ElementName = "operationType")]
        public string OperationType { get; set; }

        [XmlElement(ElementName = "paymentDetails")]
        public PaymentDetails PaymentDetails { get; set; }

        [XmlElement(ElementName = "paymentSolution")]
        public string PaymentSolution { get; set; }

        /// <summary>
        /// Operation status, possible values are
        /// "INITIATED"
        /// "PENDING"
        /// "SUCCESS"
        /// "FAIL"
        /// "ERROR"
        /// "VOIDED"
        /// "REBATED"
        /// "N/A"
        /// "REJECTED"
        /// "GROUPED"
        /// "SUCCESS_WARNING"
        /// "REDIRECTED"
        /// "AWAITING_PAYSOL"
        /// "SUCCESS3DS"
        /// "ERROR3DS"
        /// "REVOKE_SUCCESS"
        /// "TO_CAPTURE"
        /// "CANCELLED"
        /// </summary>
        [XmlElement(ElementName = "status")]
        public string Status { get; set; }

        public bool Succeeded
        {
            get
            {
                var statusValue = Status?.ToUpper();

                return (statusValue == "SUCCESS" || statusValue == "VOIDED");
            }
        }
    }

    public class PaymentDetails
    {
        [XmlElement(ElementName = "cardHolderName")]
        public string CardHolderName { get; set; }

        [XmlElement(ElementName = "cardNumber")]
        public string CardNumber { get; set; }

        [XmlElement(ElementName = "cardNumberToken")]
        public string CardNumberToken { get; set; }

        [XmlElement(ElementName = "cardType")]
        public string CardType { get; set; }

        [XmlElement(ElementName = "expDate")]
        public string ExpDate { get; set; }

        [XmlElement(ElementName = "issuerBank")]
        public string IssuerBank { get; set; }

        [XmlElement(ElementName = "issuerCountry")]
        public string IssuerCountry { get; set; }
    }
}
