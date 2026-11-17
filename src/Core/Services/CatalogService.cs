using Core.Abstractions;
using Core.Domain;
using Core.Events;

namespace Core.Services;

// lab06 DoD 3 - Core не пише ні в консоль, ні в лог-файл: сервіс лише піднімає подію,
// а спосіб оброблення сповіщень визначає зовнішній код (Cli).
public sealed class CatalogService
{
    private readonly ICatalogStore _store;

    public CatalogService(ICatalogStore store)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
    }

    // lab06 DoD 2 - подія оголошена як event (а не публічне поле-делегат):
    // зовні доступні лише += та -=, викликати її може тільки CatalogService.
    public event EventHandler<CatalogChangedEventArgs>? Changed;

    // lab06 DoD 2 - єдиний метод підняття події; ?.Invoke пропускає виклик, якщо підписників немає.
    private void OnChanged(Product p, CatalogChangeKind kind, int delta) =>
        Changed?.Invoke(this, new CatalogChangedEventArgs(p.Id, kind, delta, p.Quantity));

    public Product Add(string sku, string name, string unit, int quantity)
    {
        Product product = Product.Create(Guid.NewGuid().ToString("N")[..8], sku, name, unit, quantity);
        _store.Add(product);
        // lab06 DoD 2 - подія лише після успішної зміни: якщо Create або Add кинуть виняток, події не буде.
        OnChanged(product, CatalogChangeKind.Added, quantity);
        return product;
    }

    public void Receive(string id, int quantity)
    {
        Product product = GetRequired(id);
        product.RegisterArrival(quantity);   // інваріант кинув виняток -> подія не піднімається
        _store.Update(product);
        OnChanged(product, CatalogChangeKind.Received, quantity);
    }

    public void Issue(string id, int quantity)
    {
        Product product = GetRequired(id);
        product.Issue(quantity);
        _store.Update(product);
        // lab06 - для видачі дельта від'ємна.
        OnChanged(product, CatalogChangeKind.Issued, -quantity);
    }

    // lab06 - операція видалення, щоб вид події Removed також піднімався.
    public bool Remove(string id)
    {
        Product? product = _store.GetById(id);
        if (product is null || !_store.Remove(id))
            return false;

        Changed?.Invoke(this, new CatalogChangedEventArgs(product.Id, CatalogChangeKind.Removed, -product.Quantity, 0));
        return true;
    }

    public IReadOnlyList<Product> All() => _store.List();

    public Product? Find(string id) => _store.GetById(id);

    // lab06 DoD 6 - делегат Func<Product, bool> у сигнатурі методу: тут потрібне значення у відповідь,
    // тому використано Func, а не подію. Критерій пошуку задає той, хто викликає.
    public IReadOnlyList<Product> FindAll(Func<Product, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return _store.List().Where(predicate).ToList();
    }

    private Product GetRequired(string id) =>
        _store.GetById(id) ?? throw new InvalidOperationException($"Немає запису з id={id}.");
}
