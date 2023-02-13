using Shared.Interfaces;

namespace Shared.Events
{
    //Sipariş oluşturulduktan sonra Stock.API‘ı tetikleyebilmek için yayınlanacak event’tir.
    //Burada dikkatinizi çekmek istediğim iki husus mevcuttur.
    //Bunlardan ilki, bu event’in artık hangi kullanıcının hangi siparişi için yayınlandığına dair bir veri taşımasına gerek yoktur!
    //Çünkü bu veriler zaten State Machine’de tutulmaktadır.
    //İkinci husus ise, ilgili siparişin State Machine’de hangi korelasyon değerine sahip olan instance’a
    //karşılık geldiğini ifade edecek olan ‘CorrelationId’ property’sini uygulayacak CorrelatedBy<Guid> arayüzüdür!

    //Bu property’de ki korelasyon değeri sayesinde tetikleyici event (OrderStartedEvent) geldikten
    //sonra oluşturulan State Instance hangisiyse onun üzerinde state bilgisi değiştirilecektir.
    public class OrderCreatedEvent : IOrderCreatedEvent
    {
        public OrderCreatedEvent(Guid correlationId)
        {
            CorrelationId = correlationId;
        }
        public List<OrderItemMessage> OrderItems { get; set; }

        public Guid CorrelationId { get; }
    }
}
