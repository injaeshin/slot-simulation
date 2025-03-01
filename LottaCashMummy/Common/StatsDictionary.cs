using System.Collections.Concurrent;

namespace LottaCashMummy.Common;
public interface IStatsDictionary<TKey, TValue>
{
    bool TryGetValue(TKey key, out TValue value);
    void AddOrUpdate(TKey key, TValue value, Func<TKey, TValue, TValue> updateValueFactory);
    int Count { get; }
    
    //void Clear();

    TValue this[TKey key] { get; set; }

    TValue GetValueOrDefault(TKey key);

    IEnumerable<KeyValuePair<TKey, TValue>> GetItems();
}

public class StatsDictionary<TKey, TValue> : IStatsDictionary<TKey, TValue> where TKey : notnull
{
    private List<TKey>? keys;

    private readonly Dictionary<TKey, TValue> dict = new();

    public bool TryGetValue(TKey key, out TValue value) => dict.TryGetValue(key, out value!);

    public void Init()
    {
        keys = dict.Keys.ToList();
    }

    public TValue this[TKey key]
    {
        get => dict[key];
        set => dict[key] = value;
    }

    public void AddOrUpdate(TKey key, TValue value, Func<TKey, TValue, TValue> updateValueFactory)
    {
        if (!dict.ContainsKey(key))
        {
            throw new Exception($"Key {key} not found");
        }

        // 키가 존재하면 업데이트
        dict[key] = updateValueFactory(key, dict[key]);

        // else
        // {
        //     // 키가 존재하지 않으면 추가
        //     dict.Add(key, value);
        //     keys ??= new List<TKey>();
        //     keys.Add(key);
        // }
    }
        //dict.AddOrUpdate(key, value, updateValueFactory);

    public int Count => dict.Count;

    // public void Clear()
    // {
    //     if (keys == null)
    //     {
    //         throw new Exception("Keys not initialized");
    //     }

    //     for (int i = 0; i < keys.Count; i++)
    //     {
    //         dict[keys[i]] = default!;
    //     }
    // }

    public IEnumerable<KeyValuePair<TKey, TValue>> GetItems()
    {
        return dict.ToList();
    }

    public TValue GetValueOrDefault(TKey key)
    {
        return dict.TryGetValue(key, out var value) ? value : default!;
    }
}

public class ConcurrentStatsDictionary<TKey, TValue> : IStatsDictionary<TKey, TValue> where TKey : notnull
{
    private readonly ConcurrentDictionary<TKey, TValue> dict = new ConcurrentDictionary<TKey, TValue>();

    public bool TryGetValue(TKey key, out TValue value) => dict.TryGetValue(key, out value!);

    public void AddOrUpdate(TKey key, TValue value, Func<TKey, TValue, TValue> updateValueFactory) =>
        dict.AddOrUpdate(key, value, updateValueFactory);

    public int Count => dict.Count;

    public TValue this[TKey key]
    {
        get => dict[key];
        set => dict[key] = value;
    }

    //public void Clear() => dict.Clear();

    public IEnumerable<KeyValuePair<TKey, TValue>> GetItems()
    {
        return dict.ToList();
    }

    public TValue GetValueOrDefault(TKey key)
    {
        return dict.TryGetValue(key, out var value) ? value : default!;
    }
}