using Cli;
using Core.Abstractions;
using Core.Domain;
using Core.Services;
using Core.Storage;

//Definition of done 2
//Definition of done 4
// lab05 add03 - вибір сховища винесено у StoreFactory.Create(args) (src/Cli/StoreFactory.cs).
// Раніше тут був тернарний вибір FileCatalogStore / InMemoryCatalogStore прямо в Program.cs.
ICatalogStore store = StoreFactory.Create(args);
var service = new CatalogService(store);

Console.WriteLine($"Сховище: {store.GetType().Name}");

try
{
    //Definition of done 5
    Product created = service.Add("SKU-101", "Кабель UTP cat6", "м", 50);
    service.Receive(created.Id, 25);
    service.Issue(created.Id, 10);

    Console.WriteLine($"Додано товар: {created.Id}; після приходу та видачі: {created.Quantity} {created.Unit}");

    Product? found = service.Find(created.Id);
    Console.WriteLine(found is null ? "Товар не знайдено." : $"Знайдено: {found}");

    Console.WriteLine("Каталог:");
    foreach (Product product in service.All())
        Console.WriteLine($"  {product.Id}  {product.Sku,-10} {product.Name,-24} {product.Quantity,5} {product.Unit}");

    // lab05 add02 - пошук за умовою Func<Product, bool>.
    Console.WriteLine("Пошук: товари з залишком менше 50:");
    foreach (Product product in service.Find(p => p.Quantity < 50))
        Console.WriteLine($"  {product.Id}  {product.Sku,-10} {product.Name,-24} {product.Quantity,5} {product.Unit}");

    TryDo("отримання неіснуючого товару", () => service.Receive("missing-product", 1));

    // lab05 add01 - повторні читання обслуговує декоратор CachingCatalogStore (режим --cache).
    if (store is CachingCatalogStore caching)
    {
        service.All();
        service.Find(created.Id);
        Console.WriteLine($"Кеш: влучань {caching.Hits}, промахів {caching.Misses}");
    }
}
catch (ArgumentException exception)
{
    Console.WriteLine($"Помилка аргументу: {exception.Message}");
}
catch (InvalidOperationException exception)
{
    Console.WriteLine($"Помилка операції: {exception.Message}");
}

return 0;

static void TryDo(string title, Action action)
{
    try
    {
        action();
        Console.WriteLine($" {title}: помилка НЕ спрацювала.");
    }
    catch (InvalidOperationException exception)
    {
        Console.WriteLine($" {title}: {exception.Message}");
    }
}
