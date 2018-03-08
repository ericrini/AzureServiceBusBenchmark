using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    public class Statistics
    {
        public static IDictionary<string, double> Metrics { get; set; } = new ConcurrentDictionary<string, double>();

        public Statistics()
        {
            Metrics["elapsed-seconds"] = 0;
            Metrics["produced-count"] = 0;
            Metrics["produced-average"] = 0;
            Metrics["consumed-count"] = 0;
            Metrics["consumed-average"] = 0;
        }

        public void Start()
        {
            Task metricTask = UpdateMetricsAsync();
            Task consoleTask = UpdateConsoleAsync();
        }

        private async Task UpdateMetricsAsync()
        {
            while (true)
            {
                lock (Metrics)
                {
                    Metrics["elapsed-seconds"] += 1;
                    Metrics["produced-average"] = Metrics["produced-count"] / Metrics["elapsed-seconds"];
                    Metrics["consumed-average"] = Metrics["consumed-count"] / Metrics["elapsed-seconds"];
                }

                await Task.Delay(1000);
            }
        }

        private async Task UpdateConsoleAsync()
        {
            while (true)
            {
                List<string> keys = new List<string>(Metrics.Keys);
                keys.Sort();

                lock (Console.Out)
                {
                    foreach (string key in keys)
                    {
                        if (key == "produced-average" || key == "consumed-average")
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                        }
                        Console.WriteLine($"{key} = {Metrics[key]}");
                        Console.ResetColor();
                    }
                    Console.SetCursorPosition(0, Console.CursorTop - keys.Count);
                }

                await Task.Delay(100);
            }
        }
    }
}
