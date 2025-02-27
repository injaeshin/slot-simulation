using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ExcelExport;

class Program
{
    static async Task Main()
    {
        var services = CreateServices();

        using var scope = services.CreateScope();
        Application app = scope.ServiceProvider.GetRequiredService<Application>();
        await app.RunAsync();
    }

    private static ServiceProvider CreateServices()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            //.SetBasePath(Environment.CurrentDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        var serviceProvider = new ServiceCollection() { }
            .AddSingleton<IConfiguration>(configuration)
            .AddTransient<Application>()
            .BuildServiceProvider();

        return serviceProvider;
    }
}

public class Application
{
    private readonly IConfiguration configuration;

    public Application(IConfiguration conf)
    {
        this.configuration = conf;
    }

    public async Task RunAsync()
    {
        var file = configuration.GetSection("file").Value ?? throw new Exception("File not found in configuration");

        var writer = new JsonWriter();
        var reader = new ExcelReader(file);
        if (!reader.Open())
        {
            Console.WriteLine("Failed to open file");
            return;
        }

        var data = configuration.GetSection("data") ?? throw new Exception("Sheet section not found in configuration");
        
        foreach (var sheet in data.GetChildren())
        {
            var sheetName = sheet.Key;
            Console.WriteLine($"Processing sheet: > {sheetName}");

            foreach (var range in sheet.GetChildren())
            {
                var rangeName = range.Key;
                var rangeValue = range.Value;

                if (string.IsNullOrEmpty(rangeValue))
                {
                    Console.WriteLine($"Warning: Empty range value for {rangeName} in {sheetName}");
                    continue;
                }

                Console.WriteLine($"Processing range: {rangeName} = {rangeValue}");

                var dt = reader.GetRangeAsDataTable(sheetName, rangeValue);
                if (dt == null)
                {
                    Console.WriteLine($"No data found in the specified range: {sheetName} {rangeValue}");
                    continue;
                }

                dt.TableName = $"{rangeName}";

                if (!writer.AddDataAsColumnBase(dt))
                {
                    Console.WriteLine($"Failed to write data for {sheetName} {rangeName}");
                    continue;
                }
            }
        }

        var output = configuration.GetSection("output").Value ?? throw new Exception("Output not found in configuration");
        writer.SaveToFile(output);

        await Task.CompletedTask;
    }
}