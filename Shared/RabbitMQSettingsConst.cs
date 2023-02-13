using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    //Servislerimizle State Machine arasındaki iletişimleri sağlayacak olan kuyruk isimlerinin ayarlanması için
    //Shared class library’sinde ‘RabbitMQSettingsConst.cs’ isminde bir static class oluşturalım ve içerisini aşağıdaki gibi dolduralım.
    public class RabbitMQSettingsConst
    {
        public const string OrderSaga = "order-saga-queue";
        public const string StockOrderCreatedEventQueueName = "stock-order-created-queue";
        public const string PaymentStockReservedRequestQueueName = "payment-stock-reserved-request-queue";
        public const string OrderRequestCompletedEventQueueName = "order-request-completed-queue";
        public const string OrderRequestFailedEventQueueName = "order-request-failed-queue";
        public const string StockRollBackMessageQueueName = "stock-rollback-message-queue";

    }
}
