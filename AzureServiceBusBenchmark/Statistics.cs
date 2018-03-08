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
            Metrics["elapsed"] = 0;
            Metrics["producedCount"] = 0;
            Metrics["producedAvg"] = 0;
            Metrics["consumedCount"] = 0;
            Metrics["consumedAvg"] = 0;
        }

        public async Task UpdateAsync()
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
                    var weightedProduction = Metrics["producedAvg"] * Metrics["elapsed"];
                    var weightedConsumed = Metrics["consumedAvg"] * Metrics["elapsed"];

                    Metrics["elapsed"] += 1;

                    Metrics["producedAvg"] = (weightedProduction + Metrics["producedCount"]) / Metrics["elapsed"];
                    Metrics["consumedAvg"] = (weightedConsumed + Metrics["consumedCount"]) / Metrics["elapsed"];

                    Metrics["producedCount"] = 0;
                    Metrics["consumedCount"] = 0;
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
                    Console.ForegroundColor = ConsoleColor.Yellow;

                    foreach (string key in keys)
                    {
                        Console.WriteLine($"{key} = {Metrics[key]}");
                    }
                    Console.ResetColor();
                    Console.SetCursorPosition(0, Console.CursorTop - keys.Count);
                }

                await Task.Delay(100);
            }
        }
    }
}
