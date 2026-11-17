using Core.Dto;

namespace Core.Domain;

public sealed class Warehouse
{
    private readonly List<Product> _products = [];

    public string Id { get; }
    public string Name { get; }
    public string Address { get; }
    public IReadOnlyList<Product> Products => _products.AsReadOnly(); 

    private Warehouse(string id, string name, string address)
    {
        Id = id;
        Name = name;
        Address = address;
    }

    public static Warehouse Create(string id, string name, string address)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException($"Ідентифікатор складу обов'язковий; отримано {Describe(id)}", nameof(id));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException($"Назва складу обов'язкова; отримано {Describe(name)}", nameof(name));
        if (address is null)
            throw new ArgumentException("Адреса складу не може бути null; отримано <null>", nameof(address));

        return new Warehouse(id.Trim(), name.Trim(), address.Trim());
    }

    public void AddProduct(Product product)
    {
        if (product is null)
            throw new ArgumentNullException(nameof(product), "Товар обов'язковий; отримано <null>");

        if (_products.Any(existing => string.Equals(existing.Sku, product.Sku, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException(
                $"Склад '{Name}' (ID '{Id}') вже містить товар із SKU '{product.Sku}'; SKU має бути унікальним у межах складу");

        _products.Add(product);
    }

    public WarehouseDto ToDto() => new(Id, Name, Address);

    public static Warehouse FromDto(WarehouseDto dto)
    {
        if (dto is null)
            throw new ArgumentNullException(nameof(dto), "DTO складу обов'язковий; отримано <null>");
        return Create(dto.Id, dto.Name, dto.Address);
    }

    private static string Describe(string? value) => value is null ? "<null>" : $"'{value}'";
}
