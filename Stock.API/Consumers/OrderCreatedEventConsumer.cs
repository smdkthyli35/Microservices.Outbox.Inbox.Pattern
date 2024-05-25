using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared.Events;
using Stock.API.Models.Contexts;
using Stock.API.Models.Entities;
using System.Text.Json;

namespace Stock.API.Consumers
{
    public class OrderCreatedEventConsumer(StockDbContext stockDbContext) : IConsumer<OrderCreatedEvent>
    {
        public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
        {
            OrderInbox orderInbox = new()
            {
                Processed = false,
                Payload = JsonSerializer.Serialize(context.Message)
            };

            await stockDbContext.OrderInboxes.AddAsync(orderInbox);
            await stockDbContext.SaveChangesAsync();
        }
    }
}