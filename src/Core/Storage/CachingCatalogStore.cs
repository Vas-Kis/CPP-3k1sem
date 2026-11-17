using Core.Abstractions;
using Core.Domain;

namespace Core.Storage;

// lab05 add01 - декоратор: приймає інший ICatalogStore у конструкторі,
// реалізує той самий контракт і додає нову поведінку — кешування читань.
public sealed class CachingCatalogStore : ICatalogStore
{
    private readonly ICatalogStore _inner;
    private readonly Dictionary<string, Product?> _byId = new(StringComparer.OrdinalIgnoreCase);
    private IReadOnlyList<Product>? _list;

    // lab05 add01 - залежність від абстракції, а не від конкретного сховища.
    public CachingCatalogStore(ICatalogStore inner)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
    }

    // lab05 add01 - лічильники для демонстрації роботи кешу.
    public int Hits { get; private set; }
    public int Misses { get; private set; }

    // lab05 add01 - повторні виклики List() не звертаються до внутрішнього сховища.
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

    // lab05 add01 - кешується і знайдений запис, і його відсутність (null).
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

    // lab05 add01 - зміни передаються внутрішньому сховищу, після чого кеш інвалідується.
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
