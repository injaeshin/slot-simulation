using System.Text.Json;

namespace LineAndFreeGame.Common;
public class JsonOptions
{
    public static JsonSerializerOptions Opt = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
    };
}

public class GameDataLoader : Dictionary<string, object>
{
    public static JsonSerializerOptions GetOptions()
    {
        return new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString,
            WriteIndented = false
        };
    }

    public static GameDataLoader? Read(string jsonPath)
    {
        try
        {
            var jsonString = File.ReadAllText(jsonPath);
            var options = GetOptions();
            var jsonObject = JsonSerializer.Deserialize<GameDataLoader>(jsonString, options);
            return jsonObject;
        }
        catch (JsonException jsonEx)
        {
            Console.WriteLine($"JSON Error: {jsonEx.Message}");
            Console.WriteLine($"JSON Path: {jsonEx.Path}");
            Console.WriteLine($"JSON Line Number: {jsonEx.LineNumber}");
            Console.WriteLine($"JSON Byte Position: {jsonEx.BytePositionInLine}");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading JSON file: {ex.Message}");
            return null;
        }
    }
}
