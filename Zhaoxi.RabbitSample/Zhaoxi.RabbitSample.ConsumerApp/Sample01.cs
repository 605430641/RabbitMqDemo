using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Zhaoxi.RabbitSample.ConsumerApp
{
    /// <summary>
    /// TTL 消息存活时间 
    /// </summary>
    public static class Sample01
    {
        public static async Task Run()
        {
            var factory = new ConnectionFactory
            {
                HostName = "192.168.2.5",
                Port = 5672,
                UserName = "admin",
                Password = "admin"
            };

            await using var connection = await factory.CreateConnectionAsync();
            await using var channel = await connection.CreateChannelAsync();

            const string orderQueueName = "order.queue";
            await channel.QueueDeclareAsync(orderQueueName, false, false, false, 
                new Dictionary<string, object?>
                {
                    { "x-message-ttl", 60000 }
                });

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine($"收到消息：{message}");

                // 处理订单支付逻辑

                await channel.BasicAckAsync(ea.DeliveryTag, false);

            };
            await channel.BasicConsumeAsync(orderQueueName, false, consumer);
            Console.ReadLine();
        }
    }
}
