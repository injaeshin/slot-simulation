using System.Text.Json;
using System.Xml.Linq;
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
            //.SetBasePath(Directory.GetCurrentDirectory())
            .SetBasePath(Environment.CurrentDirectory)
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
    private readonly JsonReader jsonReader;

    public Application(IConfiguration conf)
    {
        this.jsonReader = new JsonReader(conf.GetSection("file").Value ?? throw new Exception("File not found in configuration"));
    }

    public async Task RunAsync()
    {
        if (!jsonReader.ReadJson())
        {
            throw new Exception("Failed to read JSON file");
        }

        var file = jsonReader.GetElement("file").GetString() ?? throw new Exception("File not found in JSON");
        var reader = new ExcelReader(file);
        if (!reader.Open())
        {
            throw new Exception("Failed to open file");
        }

        var writer = new JsonWriter();
        var data = jsonReader.GetElement("data");
        foreach (JsonProperty sheet in data.EnumerateObject())
        {
            var sheetName = sheet.Name;//.GetProperty("sheet").GetString() ?? throw new Exception("Sheet name not found in JSON");
            Console.WriteLine($"Processing sheet: > {sheetName}");

            foreach (var range in sheet.Value.EnumerateObject())
            {
                var rangeName = range.Name;
                var rangeValue = range.Value.GetString();

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

                dt.TableName = $"{sheetName.Replace(" ", "")}_{rangeName.Replace(" ", "")}";

                if (!writer.AddDataAsColumnBase(dt))
                {
                    Console.WriteLine($"Failed to write data for {sheetName} {rangeName}");
                    continue;
                }
            }
        }

        var output = jsonReader.GetElement("output").GetString() ?? throw new Exception("Output not found in configuration");
        writer.SaveToFile(output);

        await Task.CompletedTask;
    }
}