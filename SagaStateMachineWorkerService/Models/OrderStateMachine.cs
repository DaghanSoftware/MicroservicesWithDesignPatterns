using MassTransit;
using Shared;
using Shared.Events;
using Shared.Interfaces;
using Shared.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SagaStateMachineWorkerService.Models
{
    //	State Machine yapılanmasını bizlere sunan sınıftır.
    //	Sorumluluk olarak state’leri, event’leri ve belirli davranışları belirleyen merkezi bir rol oynar.
    //	Yani distributed transaction’ı yönetecek olan sınıftır diyebiliriz.
    //	Bir sınıfın state machine olabilmesi için MassTransitStateMachine<T> arayüzünü uygulaması gerekmektedir.
    //	Generic olarak belirtilen T ise bir State Instance almaktadır.
    public class OrderStateMachine:MassTransitStateMachine<OrderStateInstance>
    {
        //State Machine üzerinde bir event’i temsil edebilmek için 22. satırdaki gibi Event<T> türünden bir property oluşturulmalıdır
        public Event<IOrderCreatedRequestEvent> OrderCreatedRequestEvent { get; set; }
        public Event<IStockReservedEvent> StockReservedEvent { get; set; }
        public Event<IStockNotReservedEvent> StockNotReservedEvent { get; set; }
        public Event<IPaymentCompletedEvent> PaymentCompletedEvent { get; set; }
        public Event<IPaymentFailedEvent> PaymentFailedEvent { get; set; }

        public State OrderCreated { get; private set; }
        public State StockReserved { get; private set; }
        public State StockNotReserved { get; private set; }
        public State PaymentCompleted { get; private set; }
        public State PaymentFailed { get; private set; }
        public OrderStateMachine()
        {
            //State Instance'da ki hangi property'nin sipariş sürecindeki state'i tutacağı bildiriliyor.
            //Yani artık tüm event'ler CurrentState property'sin de tutulacaktır!
            InstanceState(x => x.CurrentState);

            //Eğer gelen event OrderStartedEvent ise CorrelateBy metodu ile veritabanında(database)
            //tutulan Order State Instance'da ki OrderId'si ile gelen event'te ki(@event) OrderId'yi
            //kıyasla. Bu kıyas neticesinde eğer ilgili instance varsa kaydetme. Yani yeni bir korelasyon
            //üretme! Yok eğer yoksa yeni bir korelasyon üret(SelectId)
            Event(() => OrderCreatedRequestEvent, y => y.CorrelateBy<int>(x => x.OrderId, z => z.Message.OrderId).SelectId(context =>
            Guid.NewGuid()));

            //StockReservedEvent fırlatıldığında veritabanındaki hangi correlationid değerine sahip state
            //instance'ın state'ini değiştirecek bunu belirtmiş olduk!
            Event(() => StockReservedEvent, x => x.CorrelateById(y => y.Message.CorrelationId));

            //StockNotReservedEvent fırlatıldığında veritabanındaki hangi correlationid değerine sahip state
            //instance'ın state'ini değiştirecek bunu belirtmiş olduk!
            Event(() => StockNotReservedEvent, x => x.CorrelateById(y => y.Message.CorrelationId));

            // //PaymentCompletedEvent fırlatıldığında veritabanındaki hangi correlationid değerine sahip state
            //instance'ın state'ini değiştirecek bunu belirtmiş olduk!
            Event(() => PaymentCompletedEvent, x => x.CorrelateById(y => y.Message.CorrelationId));

            //İlgili instance'ın state'i initial/başlangıç aşamasındayken(Initially) 'OrderStartedEvent'
            //tetikleyici event'i geldiyse(When) şu işlemleri yap(Then). Ardından bu işlemler yapıldıktan
            //sonra ilgili instance'ı 'OrderCreated' state'ine geçir(TransitionTo). Ardından 'Stock.API'ı
            //tetikleyebilmek/haberdar edebilmek için 'OrderCreatedEvent' event'ini gönder(Publish/Send)
            Initially(When(OrderCreatedRequestEvent).Then(context =>
            {
                context.Instance.BuyerId = context.Data.BuyerId;
                context.Instance.OrderId = context.Data.OrderId;
                context.Instance.CreatedDate=DateTime.Now;
                context.Instance.CardName = context.Data.Payment.CardName;
                context.Instance.CardNumber = context.Data.Payment.CardNumber;
                context.Instance.CVV = context.Data.Payment.CVV;
                context.Instance.Expiration = context.Data.Payment.Expiration;
                context.Instance.TotalPrice = context.Data.Payment.TotalPrice;

            }).Then(context => { Console.WriteLine($"OrderCreatedRequestEvent before {context.Instance}"); })
            .Publish(context=>new OrderCreatedEvent(context.Instance.CorrelationId) {OrderItems=context.Data.OrderItems})
            .TransitionTo(OrderCreated)
            .Then(context => { Console.WriteLine($"OrderCreatedRequestEvent after {context.Instance}"); }));


            //Eğer state 'OrderCreated' ise(During) ve o anda 'StockReservedEvent' event'i geldiyse(When)
            //o zaman state'i 'StockReserved' olarak değiştir(TransitionTo) ve belirtilen kuyruğa 
            //'PaymentStartedEvent' event'ini gönder(Send)
            During(OrderCreated,
                When(StockReservedEvent)
                .TransitionTo(StockReserved)
                .Send(new Uri($"queue:{RabbitMQSettingsConst.PaymentStockReservedRequestQueueName}"), 
                context => new StockReservedRequestPayment(context.Instance.CorrelationId)
                {
                    OrderItems = context.Data.OrderItems,
                    Payment= new PaymentMessage() 
                    { 
                        CardName=context.Instance.CardName,
                        CardNumber = context.Instance.CardNumber,
                        CVV = context.Instance.CVV,
                        Expiration = context.Instance.Expiration,
                        TotalPrice = context.Instance.TotalPrice
                    },
                    BuyerId = context.Instance.BuyerId
                })
                .Then(context => { Console.WriteLine($"StockNotReservedEvent after {context.Instance}"); }),

                //Yok eğer State 'OrderCreated' iken(During) 'StockNotReservedEvent' event'i geldiyse(When)
                //o zaman state'i 'StockNotReserved' olarak değiştir(TransitionTo) ve belirtilen
                //kuyruğa 'OrderFailedEvent' event'ini gönder.
                When(StockNotReservedEvent).TransitionTo(StockNotReserved).Publish(context=>new OrderRequestFailedEvent()
                {
                    OrderId= context.Instance.OrderId,Reason=context.Data.Reason
                }).Then(context => { Console.WriteLine($"StockNotReservedEvent after {context.Instance}"); }));


            //Eğer ilgili sipariş 'StockReserved' durumunda iken(During) 'PaymentCompletedEvent' event'i geldiyse(When)
            //'PaymentCompleted' state'i olarak değiştir(TransitionTo) ve ardından belirtilen kuyruğa 
            //'OrderRequestCompletedEvent' event'ini gönder. Ayrıca artık bu sipariş başarılı olacağından dolayı
            //State Machine tarafından bu State Instance'ı başarıyla sonlandır(Finalize) Haliyle böylece sonuç olarak
            //ilgili instance'ın state'inde 'Final' yazacaktır!
            During(StockReserved, When(PaymentCompletedEvent).TransitionTo(PaymentCompleted).Publish(context=>new OrderRequestCompletedEvent()
            {
                OrderId = context.Instance.OrderId
            })
            .Then(context => { Console.WriteLine($"PaymentCompletedEvent after {context.Instance}"); })
            .Finalize(),

            //Yok eğer mevcut state 'StockReserved' iken(During) 'PaymentFailedEvent' event'i gelirse(When)
            //o zaman state'i 'PaymentFailed' olarak değiştir(TransitionTo) ve belirtilen kuyruklara 
            //'OrderRequestFailedEvent' ve 'StockRollBackMessage' event'lerini gönder(Send).
            When(PaymentFailedEvent)
            .Publish(context => new OrderRequestFailedEvent()
            {
                OrderId = context.Instance.OrderId,
                Reason = context.Data.Reason
            })
            .Send(new Uri($"queue:{RabbitMQSettingsConst.StockRollBackMessageQueueName}"), context=>new StockRollBackMessage()
            {
                OrderItems = context.Data.OrderItems
            })
            .TransitionTo(PaymentFailed)
            .Then(context => { Console.WriteLine($"PaymentFailedEvent after {context.Instance}"); }));

            
            //Finalize olan instance'ları veritabanından kaldırıyoruz!
            SetCompletedWhenFinalized();

        }
    }
}
