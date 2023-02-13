using Shared.Interfaces;

namespace Shared.Events
{
    //Ödeme işleminin başarılı olduğunu ifade eden event’tir.
    public class PaymentCompletedEvent : IPaymentCompletedEvent
    {
        public PaymentCompletedEvent(Guid correlationId)
        {
            CorrelationId = correlationId;
        }

        public Guid CorrelationId { get; set; }
    }
}
