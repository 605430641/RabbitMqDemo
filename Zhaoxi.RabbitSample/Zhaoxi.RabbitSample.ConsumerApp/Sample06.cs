using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Zhaoxi.RabbitSample.ConsumerApp
{
    /// <summary>
    /// 路由模式消费者
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
            
            // 创建供应商队列并绑定到交换机
            await CreateSupplierQueueAsync(channel, "supplier1", exchangeName);
            await CreateSupplierQueueAsync(channel, "supplier2", exchangeName);
            
            
            // 注册消费者监听供应商1的队列
            await registerConsumer(channel, "supplier1");
            // 注册消费者监听供应商2的队列
            await registerConsumer(channel, "supplier2");

            Console.ReadLine();
        }

        private static async Task CreateSupplierQueueAsync(IChannel channel,string supplierId,string sampleOrderDirect)
        {
          //声明队列
          var queueName = $"supplier.queue.{supplierId}";
          await channel.QueueDeclareAsync(queueName,false,false,false);
          //绑定队列到交换机，使用供应商ID作为路由键
          await channel.QueueBindAsync(queueName,sampleOrderDirect,supplierId);
        }
        
        private static async Task registerConsumer(IChannel channel,string supplierId)
        {
            var queueName = $"supplier.queue.{supplierId}";
            //创建消费者
            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);    
                Console.WriteLine($"供应商{supplierId}接收到订单消息：{message}，当前时间：{DateTime.Now:T}");
                await channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
            };
            //开启手动消息确认
            await channel.BasicConsumeAsync(queueName, false, consumer);
            
        }
    }
}
