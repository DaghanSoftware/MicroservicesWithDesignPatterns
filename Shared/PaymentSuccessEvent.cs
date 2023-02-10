namespace Shared
{
    public class PaymentSuccessEvent
    {
        public int OrderId { get; set; }
        public string BuyerId { get; set; }
    }
}
