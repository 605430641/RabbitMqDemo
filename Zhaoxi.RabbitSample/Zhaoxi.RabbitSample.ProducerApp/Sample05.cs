using System.Text;
using RabbitMQ.Client;

namespace Zhaoxi.RabbitSample.ProducerApp
{
    /// <summary>
    /// 发布订阅模式生产者
    /// </summary>
    public static class Sample05
    {
        public static async Task Run()
        {
            var factory = new ConnectionFactory
            {
                HostName = "34.92.235.102",
                Port = 5672,
                UserName = "admin",
                Password = "123123",
                VirtualHost = "/"
            };

            await using var connection = await factory.CreateConnectionAsync();
            await using var channel = await connection.CreateChannelAsync();

            // 声明交换机
            const string exchangeName = "sample.msg.fanout";
            
            //下面这句代码的作用是创建一个交换机，如果该交换机已经存在，则不会重复创建。
            await channel.ExchangeDeclareAsync(exchangeName, ExchangeType.Fanout);

            for (int i = 0; i < 100; i++)
            {
                var message = $"publish:计数 {i}";
                var body = Encoding.UTF8.GetBytes(message);

           
                Console.WriteLine($"已发送消息：{message}");
                await channel.BasicPublishAsync(exchangeName,"",body);

            }
        }
    }
}
