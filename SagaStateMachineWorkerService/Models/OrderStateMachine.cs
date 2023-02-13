using MassTransit;
using Shared;
using Shared.Interfaces;
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
        public Event<IOrderCreatedRequestEvent> OrderCreatedRequestEvent { get; set; }
        public State OrderCreated { get; private set; }
        public OrderStateMachine()
        {
            InstanceState(x => x.CurrentState);
            Event(() => OrderCreatedRequestEvent, y => y.CorrelateBy<int>(x => x.OrderId, z => z.Message.OrderId).SelectId(context =>
            Guid.NewGuid()));

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
        }
    }
}
