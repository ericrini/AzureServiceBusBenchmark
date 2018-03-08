# Azure ServiceBus Benchmark
Run it using the dotnet Core 2.0 runtime.

```shell
> dotnet AzureServiceBusBenchmark.dll {1} {2} {3} {4} {5} {6} {7}
```

The input arguments should be like this.

1. The connection string to the namespace. (required)  
1. The topic to publish to. (default "performance-benchmark")  
1. The subscrption to listen to. (default "subscription-1")  
1. The number of messages to send per producer. (default 1000)  
1. The number of producer clients. (default 3)  
1. The number of consumer clients. (default 3)  
1. The number of minutes for each sent message to live. (default 1)

You should get output like this.

```text
consumed-average = 12.6734693877551
consumed-count = 1254
consumer-0-recieved-count = 417
consumer-1-recieved-count = 417
consumer-2-recieved-count = 420
elapsed-seconds = 98
produced-average = 30.5816326530612
produced-count = 3000
producer-0-send-count = 1000
producer-1-send-count = 1000
producer-2-send-count = 1000
```  

The consumed-average and produced-average metrics indicate the average number of messages per second across all async fibers. The other statistics are for validation of those numbers.