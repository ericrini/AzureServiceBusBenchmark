using Microsoft.Azure.ServiceBus;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    public class Program
    {
        static void Main(string[] args)
        {
            string connectionString = args.Length > 0 ? args[0] : "";
            string topicName = args.Length > 1 ? args[1] : "performance-benchmark";
            string subscriptionName = args.Length > 2 ? args[2] : "subscription-1";
            int messages = args.Length > 3 ? int.Parse(args[3]) : 1000;
            int producerClientCount = args.Length > 4 ? int.Parse(args[4]) : 3;
            int consumerClientCount = args.Length > 5 ? int.Parse(args[5]) : 3;
            int ttl = args.Length > 6 ? int.Parse(args[6]) : 1;
            new Program(connectionString, messages, producerClientCount, consumerClientCount, ttl);
            Thread.Sleep(Timeout.Infinite);
        }

        public Program(string connectionString, int messages, int producerClientCount, int consumerClientCount, int ttl)
        {
            var topic = "performance-benchmark";
            var subscription = "subscription-1";

            for (int j = 0; j < producerClientCount; j++)
            {
                InitializeReceiver(new SubscriptionClient(connectionString, topic, subscription), j);
            }

            for (int i = 0; i < producerClientCount; i++)
            {
                Task sendLoop = SendMessagesAsync(new TopicClient(connectionString, topic), i, messages, int ttl);
            }

            new Statistics().Start();
        }

        private async Task SendMessagesAsync(TopicClient client, int index, int messages, int ttl)
        {
            string key = $"producer-{index}-send-count";
            Statistics.Metrics[key] = 0;
            Random generate = new Random();

            while (messages > 0)
            {
                var correlation = Guid.NewGuid();
                var buffer = new byte[1024];
                generate.NextBytes(buffer);
                var message = new Message(buffer)
                {
                    MessageId = correlation.ToString(),
                    TimeToLive = TimeSpan.FromMinutes(ttl)
                };

                await client.SendAsync(message);

                lock (Statistics.Metrics)
                {
                    Statistics.Metrics[key] += 1;
                    Statistics.Metrics["produced-count"] += 1;
                }

                messages--;
            }
        }

        private void InitializeReceiver(SubscriptionClient client, int index)
        {
            string key = $"consumer-{index}-recieved-count";
            Statistics.Metrics[key] = 0;

            MessageHandlerOptions options = new MessageHandlerOptions(args =>
            {
                Console.WriteLine(args.Exception.Message);
                return Task.CompletedTask;
            });

            client.RegisterMessageHandler((message, cancellationToken) =>
            {
                lock (Statistics.Metrics)
                {
                    Statistics.Metrics[key] += 1;
                    Statistics.Metrics["consumed-count"] += 1;
                }
                return Task.CompletedTask;
            }, options);
        }
    }
}
