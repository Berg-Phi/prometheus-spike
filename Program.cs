// See https://aka.ms/new-console-template for more information
using Prometheus;

Console.WriteLine("Hello, World!");

using var server = new Prometheus.MetricServer(port: 1234);
server.Start();


Counter ProcessedJobCount = Metrics
    .CreateCounter("myapp_jobs_processed_total", "Number of processed jobs.");

ProcessedJobCount.Inc();
ProcessedJobCount.Inc();
ProcessedJobCount.Inc();
ProcessedJobCount.Inc();
ProcessedJobCount.Inc();
ProcessedJobCount.Inc();


Console.WriteLine("Open http://localhost:1234/metrics in a web browser.");
Console.WriteLine("Press enter to exit.");
Console.ReadLine();
