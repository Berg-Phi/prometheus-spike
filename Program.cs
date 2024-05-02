// See https://aka.ms/new-console-template for more information
using System.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Prometheus;

Console.WriteLine("Hello, World!");

using var server = new MetricServer(port: 1234);
server.Start();


Counter ProcessedJobCount = Metrics
    .CreateCounter("weatherforecast_time", "Number of Wather forecasts calls.");

Counter counter = Metrics
    .CreateCounter("count", "Number of Wather forecasts calls.", new CounterConfiguration() { LabelNames = new string[] { "User", "StatusCode" } });

var histogram = Metrics.CreateHistogram("weatherforecast_histogram", "Istrogrammma",  new HistogramConfiguration
        {
            // We divide measurements in 10 buckets of $100 each, up to $1000.
            Buckets = Histogram.LinearBuckets(start: 0, width: 3000, count: 10)
        });


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.UseHttpClientMetrics();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseHttpMetrics();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

var rand = new Random();

app.MapGet("/weatherforecast", async () =>
{
    using(var timer = histogram.NewTimer())
    {
        var random = rand.Next(0, 3000);
        await Task.Delay(random);
        var forecast =  Enumerable.Range(1, 5).Select(index =>
            new WeatherForecast
            (
                DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                Random.Shared.Next(-20, 55),
                summaries[Random.Shared.Next(summaries.Length)]
            ))
            .ToArray();
        return forecast;
    }
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.MapGet("/count", ([FromQuery] string name) =>
{
    var val = Random.Shared.Next(0, 3);

    if(val == 0)
    {
        counter.WithLabels(name, StatusCodes.Status200OK.ToString()).Inc();    
    }

    if(val == 1)
    {
        counter.WithLabels(name, StatusCodes.Status305UseProxy.ToString()).Inc();    
    }

    if(val == 2)
    {
        counter.WithLabels(name, StatusCodes.Status100Continue.ToString()).Inc();    
    }

    if(val == 3)
    {
        counter.WithLabels(name, StatusCodes.Status408RequestTimeout.ToString()).Inc();    
    }

    return "Ciao";   
})
.WithName("Conta")
.WithOpenApi();

app.MapMetrics();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
