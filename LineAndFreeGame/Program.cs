using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using LineAndFreeGame.Service;

namespace LineAndFreeGame;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                config.SetBasePath(Directory.GetCurrentDirectory());
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            })
            .ConfigureLogging((context, logging) =>
            {
                logging.AddConsole();
            })
            .ConfigureServices((context, services) =>
            {
                services.AddTransient<GameSimulation>();
                services.AddTransient<GameService>();
                services.AddTransient<StatsService>();
            })
            .Build();

        var simulation = host.Services.GetRequiredService<GameSimulation>();
        await simulation.RunAsync();
    }
}