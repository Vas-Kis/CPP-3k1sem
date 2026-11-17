using Cli;
using Core.Abstractions;
using Core.Domain;
using Core.Events;
using Core.Services;
using Core.Storage;

ICatalogStore store = StoreFactory.Create(args);
var service = new CatalogService(store);

Console.WriteLine($"Сховище: {store.GetType().Name}");

// lab06 DoD 7 - шлях до logs/app.log будується через Path.Combine, каталог створюється кодом.
string logPath = Path.Combine(AppContext.BaseDirectory, "logs", "app.log");
Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);

// lab06 DoD 4 - перший підписник: локальний метод (стале ім'я, тому від нього можна відписатися).
// Виводить сповіщення в консоль і дописує його в logs/app.log.
void OnCatalogChanged(object? sender, CatalogChangedEventArgs e)
{
    string line = $"{e.At:HH:mm:ss} {e.Kind,-8} {e.Id} {e.Delta,4:+0;-0} → залишок {e.QuantityAfter}";
    Console.WriteLine(line);

    // lab06 - ризикована робота (файл) обгорнута в try/catch усередині підписника,
    // щоб збій журналу не зламав бізнес-операцію.
    try
    {
        File.AppendAllText(logPath, line + Environment.NewLine);
    }
    catch (IOException exception)
    {
        Console.WriteLine($" Не вдалося записати лог: {exception.Message}");
    }
}

service.Changed += OnCatalogChanged;

// lab06 DoD 4 - другий підписник: лямбда із замиканням над локальною змінною changes.
// Обробники викликаються в порядку підписки: спершу OnCatalogChanged, потім лічильник.
int changes = 0;
service.Changed += (_, _) => changes++;

try
{
    // lab06 - сценарій: додати запис і зробити кілька змін; кожна успішна операція дає рівно одну подію.
    Product created = service.Add("SKU-101", "Кабель UTP cat6", "м", 50);
    service.Receive(created.Id, 25);
    service.Issue(created.Id, 10);
    service.Add("SKU-102", "Розетка подвійна", "шт", 8);
    Console.WriteLine($"Подій отримано: {changes}");

    // lab06 - сценарій відмови: виняток є, рядка події немає, лічильник не змінився.
    TryDo("прихід з від'ємною кількістю", () => service.Receive(created.Id, -5));
    TryDo("прихід для неіснуючого товару", () => service.Receive("missing-product", 1));
    Console.WriteLine($"Подій отримано після відмов: {changes}");

    // lab06 DoD 5 - відписка -= тим самим локальним методом: операція виконується,
    // дані змінюються, але рядка в консолі та в лозі немає; лічильник-лямбда ще підписана (+1).
    service.Changed -= OnCatalogChanged;
    service.Receive(created.Id, 5);
    Console.WriteLine($"Подій отримано: {changes}; залишок {created.Id}: {created.Quantity} {created.Unit}");

    Product? found = service.Find(created.Id);
    Console.WriteLine(found is null ? "Товар не знайдено." : $"Знайдено: {found}");

    Console.WriteLine("Каталог:");
    foreach (Product product in service.All())
        Console.WriteLine($"  {product.Id}  {product.Sku,-10} {product.Name,-24} {product.Quantity,5} {product.Unit}");

    // lab06 DoD 6 - делегат замість події там, де потрібне значення: FindAll(Func<Product, bool>).
    Console.WriteLine("Пошук: товари з залишком менше 10:");
    IReadOnlyList<Product> lowStock = service.FindAll(p => p.Quantity < 10);
    if (lowStock.Count == 0)
        Console.WriteLine("  (немає)");
    foreach (Product product in lowStock)
        Console.WriteLine($"  {product.Id}  {product.Sku,-10} {product.Name,-24} {product.Quantity,5} {product.Unit}");

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
    catch (Exception exception) when (exception is InvalidOperationException or ArgumentException)
    {
        Console.WriteLine($" {title}: {exception.Message}");
    }
}
