using MassTransit.Transports;
using Order.Inbox.Table.Consumer.Service.Entities;
using Quartz;
using Shared.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Order.Inbox.Table.Consumer.Service.Jobs
{
    public class OrderInboxConsumerJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            if (OrderInboxSingletonDatabase.DataReaderState)
            {
                OrderInboxSingletonDatabase.DataReaderBusy();

                List<OrderInbox> orderInboxes = (await OrderInboxSingletonDatabase.QueryAsync<OrderInbox>(
                    $@"SELECT * FROM ORDERINBOXES WHERE PROCESSED = 0"))
                    .ToList();

                foreach (var orderInbox in orderInboxes)
                {
                    OrderCreatedEvent? orderCreatedEvent = JsonSerializer.Deserialize<OrderCreatedEvent>(orderInbox.Payload);
                    if (orderCreatedEvent is not null)
                    {
                        await OrderInboxSingletonDatabase.ExecuteAsync(
                            $"UPDATE ORDERINBOXES SET PROCESSED = 1 WHERE IDEMPOTENTTOKEN = '{orderInbox.IdempotentToken}'");
                    }
                }

                OrderInboxSingletonDatabase.DataReaderReady();
                await Console.Out.WriteLineAsync("Order inbox table checked!");
            }
        }

    }
}
