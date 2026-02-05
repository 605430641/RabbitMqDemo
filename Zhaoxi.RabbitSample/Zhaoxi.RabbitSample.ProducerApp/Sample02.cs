using System.Text;
using RabbitMQ.Client;

namespace Zhaoxi.RabbitSample.ProducerApp
{
    /// <summary>
    /// 延迟队列
    /// </summary>
    public static class Sample02
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

            // 声明订单队列作为延迟队列
            const string orderQueueName = "sample.order.queue";
            await channel.QueueDeclareAsync(orderQueueName, false, false, false,
                new Dictionary<string, object?>
                {
                    // 设置默认 TTL 60秒
                    { "x-message-ttl", 60000 },
                    // 死信交换机
                    { "x-dead-letter-exchange", "sample.dl.exchange" },
                    // 死信路由键
                    { "x-dead-letter-routing-key", "sample.dl.order" }
                });

            var props = new BasicProperties()
            {
                // 设置消息的TTL为30秒
                Expiration = "30000"
            };

            // 发送100条订单消息
            for (int i = 0; i < 100; i++)
            {
                var message = $"用户订单：{i}，时间：{DateTime.Now:T}";
                var body = Encoding.UTF8.GetBytes(message);
                await channel.BasicPublishAsync("", orderQueueName,true, props , body);
                Console.WriteLine($"消息已发送：{message} ");
            }
        }
    }
}
