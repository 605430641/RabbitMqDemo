using System.Text;
using RabbitMQ.Client;

namespace Zhaoxi.RabbitSample.ProducerApp
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

            await channel.QueueDeclareAsync("order.queue", false, false, false, 
                arguments: new Dictionary<string, object?>
                {
                    { "x-message-ttl", 60000 }
                });

            var message = "用户已下单";
            var body = Encoding.UTF8.GetBytes(message);

            var props = new BasicProperties
            {
                // 设置消息的TTL为30秒
                Expiration = "30000"
            };

            await channel.BasicPublishAsync("", "order.queue", true, props, body);
            Console.WriteLine($"消息已发送：{message} ");
        }
    }
}
