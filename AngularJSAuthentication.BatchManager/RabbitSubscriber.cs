using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Configuration;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.BatchManager
{
   
    public class RabbitSubscriber
    {
        public IConnection Connection { get; }
        public IModel channel { get; set; }
        public RabbitSubscriber()
        {
            Connection = RabbitMqSingleton.Instance;
            channel = Connection.CreateModel();
        }



        public void Subscribe<T>(string queueName, Action<T> CallBackFunction) where T : class
        {
            //using (var channel = Connection.CreateModel())
            //{
            //channel.ExchangeDeclare(exchange: queueName, type: ExchangeType.Direct, durable: true);
            channel.ExchangeDeclare(exchange: queueName, type: ExchangeType.Direct);


            channel.QueueDeclare(queueName, true, false, false, null);

            channel.QueueBind(queue: queueName, exchange: queueName, routingKey: "");

            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                CallBackFunction(JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(body)));
            };

            channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);

            //}
        }

        public async Task SubscribeAsync<T>(string queueName, Func<T, Task> CallBackFunction) where T : class
        {

            //channel.ExchangeDeclare(exchange: queueName, type: ExchangeType.Direct, durable: true);
            channel.ExchangeDeclare(exchange: queueName, type: ExchangeType.Direct);


            channel.QueueDeclare(queueName, true, false, false, null);

            channel.QueueBind(queue: queueName, exchange: queueName, routingKey: "");

            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                await CallBackFunction(JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(body)));
            };

            channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);
        }

    }

    
}
