using Core.Dto;
using System;
using System.Collections.Generic;
namespace Core.Domain;
public sealed class Product
{
private int _quantity;
public string Id { get; }
public string Sku { get; }
public string Name { get; }
public string Unit { get; }
public int Quantity => _quantity;
private Product(string id, string sku, string name, string unit, int quantity)
    {
    Id = id;
    Sku = sku;
    Name = name;
    Unit = unit;
    _quantity = quantity;
    }
// Єдиний спосіб створити товар: усі перевірки тут.
public static Product Create(string id, string sku, string name, string unit, int quantity)
    {
    if (string.IsNullOrWhiteSpace(id))
throw new ArgumentException($"Ідентифікатор обов'язковий; отримано {Describe(id)}", nameof(id));
    if (string.IsNullOrWhiteSpace(sku))
throw new ArgumentException($"SKU не може бути порожнім; отримано {Describe(sku)}", nameof(sku));
    if (string.IsNullOrWhiteSpace(name))
throw new ArgumentException($"Назва не може бути порожньою; отримано {Describe(name)}", nameof(name));
if (unit is null)
throw new ArgumentException("Одиниця виміру не може бути null; отримано <null>", nameof(unit));
if (quantity < 0)
throw new ArgumentOutOfRangeException(nameof(quantity), quantity,
$"Початковий залишок не може бути від'ємним; отримано {quantity}");
    return new Product(id.Trim(), sku.Trim().ToUpperInvariant(),
    name.Trim(), unit.Trim(), quantity);
    }
public void RegisterArrival(int amount)
    {
    if (amount <= 0)
    throw new ArgumentOutOfRangeException(nameof(amount), amount,
    $"Кількість приходу має бути більшою за нуль; отримано {amount}");
    if (amount > int.MaxValue - _quantity)
    throw new InvalidOperationException(
    $"Неможливо зареєструвати прихід {amount} для SKU '{Sku}': залишок {_quantity} перевищить максимальне значення {int.MaxValue}");
    _quantity += amount;
    }
public void Issue(int amount)
    {
    if (amount <= 0)
    throw new ArgumentOutOfRangeException(nameof(amount), amount,
    $"Кількість видачі має бути більшою за нуль; отримано {amount}");
    if (amount > _quantity)
    throw new InvalidOperationException(
    $"Недостатній залишок для SKU '{Sku}': запитано {amount}, доступно {_quantity}");
    _quantity -= amount;
    }
public ProductDto ToDto() => new(Id, Sku, Name, Unit, Quantity);
public static Product FromDto(ProductDto dto) =>
Create(dto.Id, dto.Sku, dto.Name, dto.Unit, dto.Quantity);
public static ImportResult<Product> FromImportResult(ImportResult<ProductDto> importResult)
    {
    ArgumentNullException.ThrowIfNull(importResult);
    if (importResult.ItemRows is not null && importResult.ItemRows.Count != importResult.Items.Count)
        throw new ArgumentException(
            $"Кількість номерів рядків ({importResult.ItemRows.Count}) має відповідати кількості записів ({importResult.Items.Count})",
            nameof(importResult));

    var products = new List<Product>();
    var errors = new List<string>(importResult.Errors);
    var itemRows = new List<int>();
    for (int i = 0; i < importResult.Items.Count; i++)
        {
        ProductDto dto = importResult.Items[i];
        int row = importResult.ItemRows is null ? i + 1 : importResult.ItemRows[i];
        if (dto is null)
            {
            errors.Add($"рядок {row}: товарний запис не може бути null; отримано <null>");
            continue;
            }
        try
            {
            products.Add(FromDto(dto));
            itemRows.Add(row);
            }
        catch (ArgumentException exception)
            {
            errors.Add($"рядок {row}: {exception.Message}");
            }
        }

    return new ImportResult<Product>(products, errors, itemRows);
    }
private static string Describe(string? value) => value is null ? "<null>" : $"'{value}'";
public override string ToString() => $"{Id} [{Sku}] {Name} — {Quantity} {Unit}";
}