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

            List<OrderInbox> orderInboxes = await stockDbContext.OrderInboxes.Where(x => x.Processed == false).ToListAsync();
            foreach (var ordInbox in orderInboxes)
            {
                OrderCreatedEvent? orderCreatedEvent = JsonSerializer.Deserialize<OrderCreatedEvent>(ordInbox.Payload);
                await Console.Out.WriteLineAsync($"{orderCreatedEvent.OrderId} order id değerine sahip olan siparişin stok işlemleri tamamlandı.");
                ordInbox.Processed = true;
                await stockDbContext.SaveChangesAsync();
            }
        }
    }
}