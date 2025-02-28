using System.Collections.Concurrent;

namespace LottaCashMummy.Common;
public interface IStatsDictionary<TKey, TValue>
{
    bool TryGetValue(TKey key, out TValue value);
    void AddOrUpdate(TKey key, TValue value, Func<TKey, TValue, TValue> updateValueFactory);
    int Count { get; }
    void Clear();

    TValue GetValueOrDefault(TKey key);

    IEnumerable<KeyValuePair<TKey, TValue>> GetItems();
}

public class StatsDictionary<TKey, TValue> : IStatsDictionary<TKey, TValue> where TKey : notnull
{
    private readonly ConcurrentDictionary<TKey, TValue> dict = new ConcurrentDictionary<TKey, TValue>();

    public bool TryGetValue(TKey key, out TValue value) => dict.TryGetValue(key, out value!);

    public void AddOrUpdate(TKey key, TValue value, Func<TKey, TValue, TValue> updateValueFactory) =>
        dict.AddOrUpdate(key, value, updateValueFactory);

    public int Count => dict.Count;

    public void Clear()
    {
        foreach (var key in dict.Keys)
        {
            dict[key] = default!;
        }
    }

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

    public void Clear() => dict.Clear();

    public IEnumerable<KeyValuePair<TKey, TValue>> GetItems()
    {
        return dict.ToList();
    }

    public TValue GetValueOrDefault(TKey key)
    {
        return dict.TryGetValue(key, out var value) ? value : default!;
    }
}