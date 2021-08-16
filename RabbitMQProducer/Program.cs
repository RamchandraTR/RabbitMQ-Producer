using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;

namespace RabbitMQProducer
{
    static class Program
    {
        static void Main(string[] args)
        {
            //var connectionFactory = new ConnectionFactory
            //{
            //     Uri = new Uri("amqp://guest:guest@localhost:5672")
            //};
            //using var connection = connectionFactory.CreateConnection();
            //using var channel = connection.CreateModel();

            //QueProducer.Publish(channel);
            //channel.QueueDeclare("Ram-Queue",
            //    durable: true,
            //    exclusive: false,
            //    autoDelete: false,
            //    arguments: null);
            //var message = new { Name = "Ram", message = "Hello consumer" };
            //var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

            //channel.BasicPublish("", "Ram-Queue", null, body);
            RunRpcQueue();
        }
        private static void RunRpcQueue()
        {
            ConnectionFactory connectionFactory = new ConnectionFactory()
            {
                Uri = new Uri("amqp://guest:guest@localhost:5672")
            };

            //connectionFactory.Port = 5672;
            //connectionFactory.HostName = "localhost";
            //connectionFactory.UserName = "accountant";
            //connectionFactory.Password = "accountant";
            //connectionFactory.VirtualHost = "accounting";

            IConnection connection = connectionFactory.CreateConnection();
            IModel channel = connection.CreateModel();

            channel.QueueDeclare("Ram-Queue", true, false, false, null);
            SendRpcMessagesBackAndForth(channel);

            channel.Close();
            connection.Close();
        }

        private static void SendRpcMessagesBackAndForth(IModel channel)
        {
            string rpcResponseQueue = channel.QueueDeclare().QueueName;

            string correlationId = Guid.NewGuid().ToString();
            string responseFromConsumer = null;

            IBasicProperties basicProperties = channel.CreateBasicProperties();
            basicProperties.ReplyTo = rpcResponseQueue;
            basicProperties.CorrelationId = correlationId;
            Console.WriteLine("Enter your message and press Enter.");
            string message = Console.ReadLine();
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            channel.BasicPublish("", "Ram-Queue", basicProperties, messageBytes);

            EventingBasicConsumer rpcEventingBasicConsumer = new EventingBasicConsumer(channel);
            rpcEventingBasicConsumer.Received += (sender, basicDeliveryEventArgs) =>
            {
                IBasicProperties props = basicDeliveryEventArgs.BasicProperties;
                if (props != null
                    && props.CorrelationId == correlationId)
                {
                    string response = Encoding.UTF8.GetString(basicDeliveryEventArgs.Body.ToArray());
                    responseFromConsumer = response;
                }
                channel.BasicAck(basicDeliveryEventArgs.DeliveryTag, false);
                Console.WriteLine("Response: {0}", responseFromConsumer);
                Console.WriteLine("Enter your message and press Enter.");
                message = Console.ReadLine();
                messageBytes = Encoding.UTF8.GetBytes(message);
                channel.BasicPublish("", "Ram-Queue", basicProperties, messageBytes);
            };
            channel.BasicConsume(rpcResponseQueue, false, rpcEventingBasicConsumer);
        }

        private static void SetUpFanoutExchange()
        {
            //code ignored from previous posts
        }

        private static void SetUpDirectExchange()
        {
            //code ignored from previous posts
        }
    }
}
