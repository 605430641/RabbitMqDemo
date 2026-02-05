using System.Text;
using RabbitMQ.Client;

namespace Zhaoxi.RabbitSample.ProducerApp
{
    /// <summary>
    /// 主体模式生产者
    /// </summary>
    public static class Sample07
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
            const string exchangeName = "sample.recommendation.topic";
            // 声明（创建）一个交换机（Exchange）
            // exchangeName：交换机名称（字符串变量）
            // ExchangeType.Direct：交换机类型为 Direct（直连模式）
            // Direct 类型的特点是：消息会根据 routingKey 精确匹配队列进行投递
            // 如果交换机已存在，则不会重复创建；如果不存在，则会自动创建
            // Async 方法表示这是一个异步操作，不会阻塞当前线程
            await channel.ExchangeDeclareAsync(
                                               exchangeName,       // 交换机名称
                                               ExchangeType.Topic // 交换机类型：直连交换机
                                              );

            while (true)
            {

                Console.WriteLine("请输入要发送的推荐消息");
                var msg = Console.ReadLine();
                
                Console.WriteLine("请输入要发布的主体,以逗号分隔：");
                var topicInput = Console.ReadLine();
                var topics     = topicInput.Split(',');
                
                //发布推荐信息到多个主题
                foreach (var topic in topics)
                {
                    
                    var body=Encoding.UTF8.GetBytes(msg);
                    await channel.BasicPublishAsync(exchangeName,topic.Trim(),body);
                    Console.WriteLine("推荐信息已发送到主题："+topic.Trim());
                }
            }
        }

     
    }
}
