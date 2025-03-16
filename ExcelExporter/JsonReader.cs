using System.Text.Json;

namespace ExcelExport;

public class JsonReader
{
    private string jsonFilePath;
    private JsonDocument? jsonDocument;

    public JsonReader(string jsonFilePath)
    {
        this.jsonFilePath = jsonFilePath;
    }

    public bool ReadJson()
    {
        try
        {
            using var stream = File.OpenRead(jsonFilePath);
            jsonDocument = JsonDocument.Parse(stream);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading JSON file: {ex.Message}");
            return false;
        }

        return true;
    }

    public JsonElement GetElement(string key)
    {
        if (jsonDocument == null)
        {
            throw new Exception("Json object is empty");
        }

        if (!jsonDocument.RootElement.TryGetProperty(key, out var value))
        {
            throw new Exception($"Key '{key}' not found in JSON object.");
        }

        return value;
    }
}
