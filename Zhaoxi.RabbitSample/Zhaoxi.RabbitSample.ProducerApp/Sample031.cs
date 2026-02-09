using System.Text;
using RabbitMQ.Client;

namespace Zhaoxi.RabbitSample.ProducerApp
{
    /// <summary>
    /// 消息持久化
    /// </summary>
    public static class Sample031
    {
        public static async Task Run()
        {
            var factory = new ConnectionFactory
            {
                HostName = "34.92.235.102",
                Port     = 5672,
                UserName = "admin",
                Password = "123123",
                VirtualHost = "my_vhost"
            };

            await using var connection = await factory.CreateConnectionAsync();
            await using var channel = await connection.CreateChannelAsync();

            // 声明持久化交换机
            await channel.ExchangeDeclareAsync("sample.exchange2", ExchangeType.Direct, true);

            // 声明持久化队列
            await channel.QueueDeclareAsync("sample.queue2", true, false, false);

            await channel.QueueBindAsync("sample.queue2", "sample.exchange2", "sample.queue");
            
            var message = "测试消息";
            var body = Encoding.UTF8.GetBytes(message);
            
            var props = new BasicProperties()
            {
                // 设置消息的持久化
                Persistent = true
            };
            
            await channel.BasicPublishAsync("sample.exchange2", "sample.queue", true, props, body);
            Console.WriteLine($"消息已发送：{message} ");
        }
    }
}
