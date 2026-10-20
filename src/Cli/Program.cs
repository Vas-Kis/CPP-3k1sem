using Core.Domain;

Console.WriteLine("=== Сценарій 1: успіх ===");
Product product = Product.Create("P-001", "sku-001", "Цемент М400 25кг", "шт", -5);
Console.WriteLine(product);
product.RegisterArrival(50);
product.Issue(30);
Console.WriteLine(product);
Console.WriteLine();
Console.WriteLine("=== Сценарій 2: порушення інваріантів ===");
TryDo("видача більша за залишок", () => product.Issue(1000));
TryDo("порожній SKU", () => Product.Create("P-002", " ", "Пісок", "т", 10));
TryDo("від'ємний залишок", () => Product.Create("P-003", "SKU-003", "Цегла", "шт", -5));
/*product._quantity = 999;   // CS0122: '_quantity' is inaccessible due to its protection level
product.Quantity = 999;    // CS0200: Property 'Quantity' cannot be assigned to -- it is read only
product.Sku = "x";         // CS0200: властивість лише для читання
new Product("1","s","n","u",1); // CS0122: конструктор private*/


static void TryDo(string title, Action action)
{
    try
    {
        action();
        Console.WriteLine($" {title}: виняток НЕ спрацював — інваріант відсутній!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($" {title}: {ex.GetType().Name} — {ex.Message}");
    }
}
