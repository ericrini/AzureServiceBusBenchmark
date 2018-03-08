using Microsoft.Azure.ServiceBus;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    public class Program
    {
        static void Main(string[] args)
        {
            int messages = args.Length > 0 ? int.Parse(args[0]) : 1000;
            int clients = args.Length > 1 ? int.Parse(args[1]) : 1;
            int parallelism = args.Length > 2 ? int.Parse(args[2]) : 1;
            int delay = args.Length > 3 ? int.Parse(args[3]) : 10;
            new Program(messages, clients, parallelism, delay);
            Thread.Sleep(Timeout.Infinite);
        }

        public Program(int messages, int clients, int parallelism, int delay)
        {
            var cs = "Endpoint=sb://sb-usw-capability-01-dev.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=0dT07rqQc+kzk0FBLJv80wDcrA4pFF7aptHlpvwMEmM=";
            var topic = "performance-benchmark";

            InitializeReceiver(new SubscriptionClient(cs, topic, "subscription-1"), parallelism);

            for (int i = 0; i < clients; i++)
            {
                Task sendLoop = SendMessagesAsync(new TopicClient(cs, topic), i, messages, delay);
            }

            Task updateLoop = new Statistics().UpdateAsync();
        }

        private async Task SendMessagesAsync(TopicClient client, int index, int messages, int delay)
        {
            var key = $"client-{index}-remaining-messages";
            Statistics.Metrics[key] = messages;

            while (Statistics.Metrics[key] > 0)
            {
                var correlation = Guid.NewGuid();
                var message = new Message(Encoding.UTF8.GetBytes("Now is the time!"))
                {
                    MessageId = correlation.ToString(),
                    TimeToLive = TimeSpan.FromMinutes(2)
                };

                await client.SendAsync(message);

                Statistics.Metrics["producedCount"] += 1;
                Statistics.Metrics[key] -= 1;

                await Task.Delay(TimeSpan.FromMilliseconds(delay));
            }
        }

        private void InitializeReceiver(SubscriptionClient client, int parallelism)
        {
            MessageHandlerOptions options = new MessageHandlerOptions(args =>
            {
                Console.WriteLine(args.Exception.Message);
                return Task.CompletedTask;
            })
            {
                MaxConcurrentCalls = parallelism
            };

            client.RegisterMessageHandler((message, cancellationToken) =>
            {
                Statistics.Metrics["consumedCount"] += 1;
                return Task.CompletedTask;
            }, options);
        }
    }
}
