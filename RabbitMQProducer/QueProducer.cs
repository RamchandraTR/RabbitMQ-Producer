using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using System.Threading;

namespace RabbitMQProducer
{
    public static class QueProducer
    {
        public static void Publish(IModel channel)
        {
            channel.QueueDeclare("Ram-Queue",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);
            var count = 0;
            while(true)
            {
                var message = new { Name = "Ram", message = $"Hello consumer: {count}" };
                var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

                channel.BasicPublish("", "Ram-Queue", null, body);
                count++;
                Thread.Sleep(1000);
            }

           
        }
    }
}
