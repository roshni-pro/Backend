using AngularJSAuthentication.BatchManager.Helpers;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Configuration;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.BatchManager
{
    public class RabbitMqHelperNew
    {
        public IConnection Connection { get; }
        public IModel channel { get; set; }
        public RabbitMqHelperNew()
        {
            Connection = RabbitMqSingleton.Instance;
            //channel = Connection.CreateModel();
        }

        public async Task<bool> PublishAsync<T>(string queueName, T data) where T : class
        {
            using (var channel = Connection.CreateModel())
            {
                channel.ConfirmSelect();
                channel.ExchangeDeclare(exchange: queueName, type: ExchangeType.Direct);
                //channel.ExchangeDeclare(exchange: queueName, type: ExchangeType.Direct, durable: true);

                channel.QueueDeclare(queueName, true, false, false, null);

                channel.QueueBind(queue: queueName, exchange: queueName, routingKey: "");

                byte[] byteData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data));
                var properties = channel.CreateBasicProperties();

                properties.Persistent = true;
                channel.BasicPublish(exchange: queueName, queueName, properties, byteData);
                channel.WaitForConfirmsOrDie(new TimeSpan(0, 0, 5));
            }

            return true;
        }

        public bool Publish<T>(string queueName, T data) where T : class
        {
            try
            {
                using (var channel = Connection.CreateModel())
                {
                    channel.ConfirmSelect();
                    channel.ExchangeDeclare(exchange: queueName, type: ExchangeType.Direct);
                    //channel.ExchangeDeclare(exchange: queueName, type: ExchangeType.Direct, durable:true);

                    channel.QueueDeclare(queueName, true, false, false, null);

                    channel.QueueBind(queue: queueName, exchange: queueName, routingKey: "");

                    byte[] byteData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data));
                    var properties = channel.CreateBasicProperties();

                    properties.Persistent = true;
                    channel.BasicPublish(exchange: queueName, "", properties, byteData);
                    channel.WaitForConfirmsOrDie(new TimeSpan(0, 0, 5));
                }
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLogSync(new DataContracts.ServiceRequestParam.ErrorLog { ForwardedIps = JsonConvert.SerializeObject(data), CoRelationId = queueName, Message = ex.ToString() });
                throw ex;
            }
            return true;
        }

        //public void Subscribe<T>(string queueName, Action<T> CallBackFunction) where T : class
        //{
        //    //using (var channel = Connection.CreateModel())
        //    //{
        //    channel.ExchangeDeclare(exchange: queueName, type: ExchangeType.Direct);

        //    channel.QueueDeclare(queueName, true, false, false, null);

        //    channel.QueueBind(queue: queueName, exchange: queueName, routingKey: "");

        //    var consumer = new AsyncEventingBasicConsumer(channel);

        //    consumer.Received += async (model, ea) =>
        //    {
        //        var body = ea.Body.ToArray();
        //        CallBackFunction(JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(body)));
        //    };

        //    channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);

        //    //}
        //}

        //public async Task SubscribeAsync<T>(string queueName, Func<T, Task> CallBackFunction) where T : class
        //{

        //    channel.ExchangeDeclare(exchange: queueName, type: ExchangeType.Direct);

        //    channel.QueueDeclare(queueName, true, false, false, null);

        //    channel.QueueBind(queue: queueName, exchange: queueName, routingKey: "");

        //    var consumer = new AsyncEventingBasicConsumer(channel);

        //    consumer.Received += async (model, ea) =>
        //    {
        //        var body = ea.Body.ToArray();
        //        await CallBackFunction(JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(body)));
        //    };

        //    channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);
        //}

    }

    public sealed class RabbitMqSingleton
    {
        private static string HostName { get { return ConfigurationManager.AppSettings["RabbitHost"]; ; } }
        private static string UserName { get { return ConfigurationManager.AppSettings["RabbitUserName"]; } }
        private static string Password { get { return ConfigurationManager.AppSettings["RabbitPassword"]; } }
        private static string VirtualHost { get { return ConfigurationManager.AppSettings["VirtualHost"]; } }

        public static ConnectionFactory connectionFactory => new ConnectionFactory
        {
            //HostName = ConfigurationManager.AppSettings["Environment"] == "development" ? "rabbitdev" : ConfigurationManager.AppSettings["RabbitHost"],
            //UserName = ConfigurationManager.AppSettings["Environment"] == "development" ? "guest" : ConfigurationManager.AppSettings["RabbitUserName"],
            //Password = ConfigurationManager.AppSettings["Environment"] == "development" ? "guest" : ConfigurationManager.AppSettings["RabbitPassword"],
            //Port = ConfigurationManager.AppSettings["Environment"] == "development" ? 5672 : Convert.ToInt32(ConfigurationManager.AppSettings["RabbitPort"]),
            DispatchConsumersAsync = true,
            //VirtualHost="testing"
            //Uri = new Uri("amqp://guest:guest@localhost:5672/testing")

            //Uri = new Uri(@"amqp://skbackend:skbackend@smart-blond-chicken.rmq4.cloudamqp.com/Test"),

            Uri = new Uri(@"amqp://" + UserName + ":" + Password + "@" + HostName + "/" + VirtualHost),
        };

        private static readonly Lazy<IConnection> Lazy = new Lazy<IConnection>(() =>

            connectionFactory.CreateConnection()
        );

        public static IConnection Instance
        {
            get { return Lazy.Value; }
        }
    }
}
