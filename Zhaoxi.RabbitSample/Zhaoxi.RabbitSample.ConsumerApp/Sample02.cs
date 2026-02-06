using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Zhaoxi.RabbitSample.ConsumerApp
{
    /// <summary>
    /// 死信队列消费者
    /// </summary>
    public static class Sample02
    {
        public static async Task Run()
        {
            var factory = new ConnectionFactory
            {
                HostName = "34.92.235.102",
                Port     = 5672,
                UserName = "admin",
                Password = "123123",
            };

            await using var connection = await factory.CreateConnectionAsync();
            await using var channel = await connection.CreateChannelAsync();

            // 声明死信交换机
            const string deadLetterExchangeName = "sample.dl.exchange";
            await channel.ExchangeDeclareAsync(deadLetterExchangeName, ExchangeType.Direct);

            // 声明死信队列
            const string deadLetterQueueName = "sample.dl.queue";
            await channel.QueueDeclareAsync(deadLetterQueueName, false, false, false);

            // 绑定死信队列到死信交换机
            await channel.QueueBindAsync(deadLetterQueueName, deadLetterExchangeName, "sample.dl.order");

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine($"收到过期消息：{message}，当前时间：{DateTime.Now:T}");

                // 处理过期的订单逻辑
                await channel.BasicAckAsync(ea.DeliveryTag, false);

            };
            await channel.BasicConsumeAsync(deadLetterQueueName, false, consumer);
            Console.ReadLine();
        }
    }
}
