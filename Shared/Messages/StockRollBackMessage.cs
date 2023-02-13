using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Messages
{
    //PaymentFailedEvent’ event’i yayınlanırsa eğer Stock.API‘da yapılan işlemlerin geri alınmasını sağlayacak olan message’dır.
    public class StockRollBackMessage : IStockRollBackMessage
    {
        public List<OrderItemMessage> OrderItems { get; set; }
    }
}
