using System.Text;
using RabbitMQ.Client;

namespace Zhaoxi.RabbitSample.ProducerApp
{
    /// <summary>
    /// 路由模式生产者
    /// </summary>
    public static class Sample06
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
            const string exchangeName = "sample.order.direct";
            // 声明（创建）一个交换机（Exchange）
            // exchangeName：交换机名称（字符串变量）
            // ExchangeType.Direct：交换机类型为 Direct（直连模式）
            // Direct 类型的特点是：消息会根据 routingKey 精确匹配队列进行投递
            // 如果交换机已存在，则不会重复创建；如果不存在，则会自动创建
            // Async 方法表示这是一个异步操作，不会阻塞当前线程
            await channel.ExchangeDeclareAsync(
                                               exchangeName,       // 交换机名称
                                               ExchangeType.Direct // 交换机类型：直连交换机
                                              );
            
            //创建供应商队列并绑定到交换机
            await CreateSupplierQueueAsync(channel,"supplier1", exchangeName);
            await CreateSupplierQueueAsync(channel,"supplier2", exchangeName);
            //模拟订单发送
            await SendOrderAsync(channel,exchangeName,"supplier1","订单1001");
            await SendOrderAsync(channel,exchangeName,"supplier2","订单1002");
            
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

        private static async Task CreateSupplierQueueAsync(IChannel channel,string supplierId,string exchangeName)
        {
          //声明队列
          var queueName = $"supplier.queue.{supplierId}";
          await channel.QueueDeclareAsync(queueName,false,false,false);
          //绑定队列到交换机，使用供应商ID作为路由键
          await channel.QueueBindAsync(queueName,exchangeName,supplierId);
        }

        /// <summary>
        /// 发送订单消息
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="exchangeName"></param>
        /// <param name="routingkey"></param>
        /// <param name="orderMessage"></param>
        private static async Task SendOrderAsync(IChannel channel,string exchangeName,string routingkey,string orderMessage)
        {
            var mesg = Encoding.UTF8.GetBytes(orderMessage);
            //发送消息到交换机
            await channel.BasicPublishAsync(exchangeName,routingkey,mesg);
            Console.WriteLine($"发送订单{orderMessage}>供应商{routingkey}");
        }
    }
}
