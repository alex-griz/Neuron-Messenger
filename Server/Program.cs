using Confluent.Kafka;
using System.Text.Json;
using MySql.Data.MySqlClient;

namespace NeuronServer
{
    internal class Program
    {
        private IConsumer<string, string> consumer;
        static void Main()
        {
            ConsumerConfig consumerConfig = new ConsumerConfig
            {
                BootstrapServers = "localhost:9092",
                GroupId = "server-consumer",
                AutoOffsetReset = AutoOffsetReset.Latest
            };
            var consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();
            consumer.Subscribe("chat-messages");

            while (true)
            {
                var message = consumer.Consume();
                if (message != null)
                {
                    ChatMessage chat_message = JsonSerializer.Deserialize<ChatMessage>(message.Message.Value);
                    SQL_Inject();
                }
            }
        }
        private static void SQL_Inject()
        {

        }
    }
}
public class ChatMessage()
{
    public int ChatID { get; set; }
    public string Sender { get; set; }
    public string Message { get; set; }
    public string Time { get; set; }
    public string Date { get; set; }
}
