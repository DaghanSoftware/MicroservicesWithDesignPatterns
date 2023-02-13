using Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Events
{
    //Siparişin başarıyla sonlandığını ifade eden event’tir.
    //Bu event State Machine tarafından yayıldığında Order.API servisinde ilgili
    //‘OrderId’ye karşılık gelen siparişin durumu Completed‘a çekilecektir.
    //Ayrıca dikkat ederseniz bu event’te herhangi bir korelasyon değeri bulunmamaktadır.
    //Çünkü bu event, siparişin sonlandığını ifade ettiğinden dolayı State Machine tarafından da bu event
    //geldiğinde ilgili siparişe dair kayıt sonlandırılmış(Finilize) olacaktır.
    public class OrderRequestCompletedEvent : IOrderRequestCompletedEvent
    {
        public int OrderId { get; set; }
    }
}
