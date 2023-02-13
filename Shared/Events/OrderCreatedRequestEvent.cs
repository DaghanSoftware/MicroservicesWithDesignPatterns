using Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Events
{
    //Tetikleyici event’tir. Bir sipariş sürecinin başladığını ifade eder.
    //Haliyle tetikleyici olduğu için State Machine’de bir Order’a karşılık State Instance satırı ekleyecektir.
    //Dikkat ederseniz içerisinde ‘OrderId’, ‘BuyerId’ gibi State Instance’da belirttiğimiz verilere karşılık property’ler mevcuttur.
    //Haliyle bir sipariş süreci bu veriler eşliğinde başlatılacaktır.
    public class OrderCreatedRequestEvent : IOrderCreatedRequestEvent
    {
        public int OrderId { get; set; }
        public string BuyerId { get; set; }
        public List<OrderItemMessage> OrderItems { get; set; } = new List<OrderItemMessage>();
        public PaymentMessage Payment { get; set; }
    }
}
