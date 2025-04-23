
public class ContentRegistry
{
    private readonly Dictionary<string, object> _items = new(StringComparer.OrdinalIgnoreCase);

    public bool Register<T>(string id, T item)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("ID cannot be null or whitespace.", nameof(id));
        if (_items.ContainsKey(id))
            return false;
        _items[id] = item;
        return true;
    }

    public bool TryResolve<T>(string id, out T item)
    {
        if (!string.IsNullOrWhiteSpace(id)
            && _items.TryGetValue(id, out object? obj)
            && obj is T cast)
        {
            item = cast;
            return true;
        }
        item = default;
        return false;
    }

    public List<T> GetAllOfType<T>()
        => _items.Values.OfType<T>().ToList();

    public List<string> ValidateReferences<T>(List<string> ids)
    {
        List<string> missing = new List<string>();
        foreach (string id in ids)
            if (!TryResolve<T>(id, out _))
                missing.Add(id);
        return missing;
    }
}
