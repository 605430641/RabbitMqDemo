using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Zhaoxi.RabbitSample.ConsumerApp
{
    /// <summary>
    /// 主体模式消费者
    /// </summary>
    public static class Sample07
    {
        public static async Task Run()
        {
            var factory = new ConnectionFactory
                          {
                              HostName = "34.92.235.102",Port = 5672,UserName = "admin",Password = "123123",
                          };

            await using var connection = await factory.CreateConnectionAsync();
            await using var channel    = await connection.CreateChannelAsync();

            // 声明交换机
            const string exchangeName = "sample.recommendation.topic";

            // 声明（创建）一个交换机（Exchange）
            // exchangeName：交换机名称（字符串变量）
            // ExchangeType.Direct：交换机类型为 Direct（直连模式）
            // Direct 类型的特点是：消息会根据 routingKey 精确匹配队列进行投递
            // 如果交换机已存在，则不会重复创建；如果不存在，则会自动创建
            // Async 方法表示这是一个异步操作，不会阻塞当前线程
            await channel.ExchangeDeclareAsync(
                                               exchangeName,      // 交换机名称
                                               ExchangeType.Topic // 交换机类型：直连交换机
                                              );

            Console.WriteLine("请输入要订阅的主题,以逗号分隔：");
            var topicInput = Console.ReadLine();
            var topics     = topicInput.Split(',');
            //通过路由键来分发信息 所以队列名称不重要由mq随机生成
            var queueName  = (await channel.QueueDeclareAsync()).QueueName;

            //订阅多个主题的推荐信息
            foreach (var topic in topics)
            {
                await channel.QueueBindAsync(queueName,exchangeName,topic.Trim());
                Console.WriteLine("已订阅主题：" + topic.Trim());
            }

            Console.WriteLine("等待接收推荐信息...");

            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.ReceivedAsync +=  (model,ea) =>
                                      {
                                          var body       = ea.Body.ToArray();
                                          var message    = Encoding.UTF8.GetString(body);
                                          var routingKey = ea.RoutingKey;
                                          Console.WriteLine($"收到推荐信息：主题 [{routingKey}]，内容：{message}");
                                          return Task.CompletedTask;
                                      };

            await channel.BasicConsumeAsync(queueName,true,consumer);
            Console.ReadLine();
        }
    }
}