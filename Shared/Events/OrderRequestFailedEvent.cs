using Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Events
{
    //Siparişin başarısız olduğunu ifade eden event’tir.
    //Order.API tarafından ilgili siparişin Fail durumuna çekilmesini sağlayacaktır.
    public class OrderRequestFailedEvent : IOrderRequestFailedEvent
    {
        public int OrderId { get ; set ; }
        public string Reason { get; set; }
    }
}
