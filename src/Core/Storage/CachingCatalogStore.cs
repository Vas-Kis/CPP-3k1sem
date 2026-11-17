using Core.Abstractions;
using Core.Domain;

namespace Core.Storage;

public sealed class CachingCatalogStore : ICatalogStore
{
    private readonly ICatalogStore _inner;
    private readonly Dictionary<string, Product?> _byId = new(StringComparer.OrdinalIgnoreCase);
    private IReadOnlyList<Product>? _list;

    public CachingCatalogStore(ICatalogStore inner)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
    }

    public int Hits { get; private set; }
    public int Misses { get; private set; }

    public IReadOnlyList<Product> List()
    {
        if (_list is not null)
        {
            Hits++;
            return _list;
        }

        Misses++;
        _list = _inner.List();
        return _list;
    }

    public Product? GetById(string id)
    {
        if (_byId.TryGetValue(id, out Product? cached))
        {
            Hits++;
            return cached;
        }

        Misses++;
        Product? product = _inner.GetById(id);
        _byId[id] = product;
        return product;
    }

    public void Add(Product item)
    {
        _inner.Add(item);
        Invalidate();
    }

    public void Update(Product item)
    {
        _inner.Update(item);
        Invalidate();
    }

    public bool Remove(string id)
    {
        bool removed = _inner.Remove(id);
        if (removed)
            Invalidate();
        return removed;
    }

    private void Invalidate()
    {
        _list = null;
        _byId.Clear();
    }
}
