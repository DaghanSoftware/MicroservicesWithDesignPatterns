using MassTransit;
using Shared;
using Shared.Events;
using Shared.Interfaces;

namespace Payment.API.Consumer
{
    public class StockReservedRequestPaymentConsumer : IConsumer<IStockReservedRequestPayment>
    {
        private ILogger<StockReservedRequestPayment> _logger;

        private readonly IPublishEndpoint _publishEndpoint;

        public StockReservedRequestPaymentConsumer(ILogger<StockReservedRequestPayment> logger, IPublishEndpoint publishEndpoint)
        {
            _logger = logger;
            _publishEndpoint = publishEndpoint;
        }

        public async Task Consume(ConsumeContext<IStockReservedRequestPayment> context)
        {
            var balance = 3000m;
            if (balance > context.Message.Payment.TotalPrice)
            {
                _logger.LogInformation($"{context.Message.Payment.TotalPrice} TL was withdrawn from credit card for user id = {context.Message.BuyerId}");
                await _publishEndpoint.Publish(new PaymentCompletedEvent(context.Message.CorrelationId));
            }
            else
            {
                _logger.LogInformation($"{context.Message.Payment.TotalPrice} TL was not withhdraw from credit card for  user id = {context.Message.BuyerId}");
                await _publishEndpoint.Publish(new PaymentFailedEvent(context.Message.CorrelationId) { OrderItems = context.Message.OrderItems, Reason = "not enough balance" });
            }
        }
    }
}
