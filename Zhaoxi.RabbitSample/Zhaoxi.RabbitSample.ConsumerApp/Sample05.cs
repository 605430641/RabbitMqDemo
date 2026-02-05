using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Zhaoxi.RabbitSample.ConsumerApp
{
    /// <summary>
    /// 发布订阅模式消费者
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
            };

            await using var connection = await factory.CreateConnectionAsync();
            await using var channel = await connection.CreateChannelAsync();

            // 声明换机
            const string exchangeName = "sample.msg.fanout";
            //下面这句代码的作用是创建一个交换机，如果该交换机已经存在，则不会重复创建。
            await channel.ExchangeDeclareAsync(exchangeName, ExchangeType.Fanout);

            Console.WriteLine("请输入要监听的队列名称：");
            var queue = Console.ReadLine();
            //注释：这里没有设置队列持久化、排他性和自动删除
            await channel.QueueDeclareAsync(queue,false,false,false);

            // 绑定队列到交换机
            await channel.QueueBindAsync(queue, exchangeName, "");

            //注释：设置每次只接收一条未确认的消息，处理完一条再接收下一条
            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine($"{message}已接收，当前时间：{DateTime.Now:T}");

                // 处理过期的订单逻辑
                await channel.BasicAckAsync(ea.DeliveryTag, multiple:false);

            };
            //注释：开启手动消息确认
            await channel.BasicConsumeAsync(queue, false, consumer);
            Console.ReadLine();
        }
    }
}
