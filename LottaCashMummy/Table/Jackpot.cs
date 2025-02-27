using System.Text.Json;
using LottaCashMummy.Common;

namespace LottaCashMummy.Table;

public class JackpotParser
{
    public static JackpotType ParseJackpotType(string value)
    {
        return value.ToLower() switch
        {
            "none" => JackpotType.None,
            "mini" => JackpotType.Mini,
            "minor" => JackpotType.Minor,
            "major" => JackpotType.Major,
            "mega" => JackpotType.Mega,
            "grand" => JackpotType.Grand,
            _ => throw new Exception("Invalid JackpotType")
        };
    }
}

public class JackpotModel(string[] name, string[] value)
{
    public string[] Name => name;
    public string[] Value => value;
}

public interface IJackpot
{
    bool TryGetJackpotType(int value, out JackpotType jackpotType);
}

public class Jackpot : IJackpot
{
    private readonly Dictionary<int, JackpotType> jackpot;

    public Jackpot(GameDataLoader kv)
    {
        if (!kv.TryGetValue("Jackpot", out var jackpotJson))
        {
            throw new Exception("Jackpot not found in json object");
        }

        var model = JsonSerializer.Deserialize<JackpotModel>(jackpotJson.ToString()!, JsonOptions.Opt)
            ?? throw new Exception("Invalid Jackpot format");

        this.jackpot = new Dictionary<int, JackpotType>();

        for (int i = 0; i < model.Name.Length; i++)
        {
            jackpot.Add(int.Parse(model.Value[i]), JackpotParser.ParseJackpotType(model.Name[i]));
        }
    }

    public bool TryGetJackpotType(int value, out JackpotType jackpotType)
    {
        return jackpot.TryGetValue(value, out jackpotType);
    }
}
