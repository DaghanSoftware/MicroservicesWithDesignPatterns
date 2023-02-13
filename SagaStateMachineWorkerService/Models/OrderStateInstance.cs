using MassTransit;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SagaStateMachineWorkerService.Models
{
    //Bir state machine verisini temsil eden sınıftır.
    //Misal olarak aynı anda yapılan iki farklı sipariş isteğinin her biri bir state instance olarak veritabanında tutulacaktır.
    //Haliyle gelen bu istekleri birbirlerinden ayırt edebilmek için CorrelationId değeri kullanılmaktadır.
    //Bu değer tekilleştirici niteliğe sahiptir.
    //Bir sınıfın state instance olabilmesi için SagaStateMachineInstance arayüzünü uygulaması gerekmektedir.
    //Orchestration implementasyonunda dört temel event vardır.Bu event’ler;

    //Tetikleyici Event: İlgili service’i tetikleyecek/çalıştıracak/işlevsel hale getirecek olan event’tir.
    //Başarılı Event: İşlevin başarıyla sonuçlandığını ifade eden event’tir.
    //Başarısız Event: İşlevin başarısızlıkla sonuçlandığını ifade eden event’tir.
    //Compensable Event: Yapılan işlemlerin geri alınmasını bildiren event’tir.
    //Dolayısıyla gelen her tetikleyici event için yeni bir state instance oluşturulacak ve diğer event’ler de ise önceden oluşturulmuş state instance CorrelationId üzerinden tespit edilip üzerinden işlem gerçekleştirilecektir.
    public class OrderStateInstance : SagaStateMachineInstance
    {
        // Her bir State Instance özünde bir siparişe özeldir. Haliyle bu State Instance'ları
        // birbirinden ayırabilmek için CorrelationId(yani bildiğiniz unique id) kullanılmaktadır
        public Guid CorrelationId { get; set; }
        public string CurrentState { get; set; }
        public string BuyerId { get; set; }
        public int OrderId { get; set; }

        public string CardName { get; set; }
        public string CardNumber { get; set; }
        public string Expiration { get; set; }
        public string CVV { get; set; }
        [Column(TypeName ="decimal(18,2)")]
        public decimal TotalPrice { get; set; }

        public DateTime CreatedDate { get; set; }

        public override string ToString()
        {
            var properties = GetType().GetProperties();
            var sb = new StringBuilder();

            properties.ToList().ForEach(p => 
            {
                var value = p.GetValue(this, null);
                sb.AppendLine($"{p.Name}:{value}");
            });
            sb.Append("--------------------------");
            return sb.ToString();
        }
    }
}
