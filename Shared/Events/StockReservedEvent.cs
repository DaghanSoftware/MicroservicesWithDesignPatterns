using Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Events
{
    //Stock.API‘da ilgili stok işlemlerinin başarıyla gerçekleştirildiğini ifade eden event’tir.
    //CorrelatedBy<Guid> arayüzü sayesinde ‘CorrelationId’ property’si uygulanarak State Machine’de hangi
    //instance’ın mevzu bahis olduğunu taşımaktadır.
    public class StockReservedEvent : IStockReservedEvent
    {
        public StockReservedEvent(Guid correlationId)
        {
            CorrelationId = correlationId;
        }

        public List<OrderItemMessage> OrderItems { get; set; }

        public Guid CorrelationId { get; }
    }
}
