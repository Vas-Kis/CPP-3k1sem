using System.Text.Json;
using Core.Abstractions;
using Core.Domain;
using Core.Dto;

namespace Core.Storage;

public sealed class FileCatalogStore : ICatalogStore
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };
    private readonly Dictionary<string, Product> _cache = new(StringComparer.OrdinalIgnoreCase);
    private readonly string _path;
    private bool _loaded;

    public FileCatalogStore(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        _path = Path.GetFullPath(path);
    }

    public IReadOnlyList<Product> List()
    {
        EnsureLoaded();
        return _cache.Values.ToList();
    }

    public Product? GetById(string id)
    {
        EnsureLoaded();
        return _cache.GetValueOrDefault(id);
    }

    public void Add(Product item)
    {
        ArgumentNullException.ThrowIfNull(item);
        EnsureLoaded();

        if (_cache.ContainsKey(item.Id))
            throw new InvalidOperationException($"Запис з id={item.Id} уже існує.");

        _cache.Add(item.Id, item);
        Flush();
    }

    public void Update(Product item)
    {
        ArgumentNullException.ThrowIfNull(item);
        EnsureLoaded();
        _cache[item.Id] = item;
        Flush();
    }

    public bool Remove(string id)
    {
        EnsureLoaded();
        if (!_cache.Remove(id))
            return false;

        Flush();
        return true;
    }

    private void EnsureLoaded()
    {
        if (_loaded)
            return;

        if (File.Exists(_path))
        {
            List<ProductDto> dtos = JsonSerializer.Deserialize<List<ProductDto>>(File.ReadAllText(_path)) ?? [];
            var loadedItems = new Dictionary<string, Product>(StringComparer.OrdinalIgnoreCase);

            foreach (ProductDto dto in dtos)
            {
                if (dto is null)
                    throw new InvalidDataException($"Файл каталогу '{_path}' містить порожній запис.");

                Product product = Product.FromDto(dto);
                if (!loadedItems.TryAdd(product.Id, product))
                    throw new InvalidDataException($"Файл каталогу '{_path}' містить повторний id={product.Id}.");
            }

            foreach (KeyValuePair<string, Product> item in loadedItems)
                _cache.Add(item.Key, item.Value);
        }

        _loaded = true;
    }

    private void Flush()
    {
        string? directory = Path.GetDirectoryName(_path);
        if (directory is null)
            throw new InvalidOperationException($"Не вдалося визначити каталог для файлу '{_path}'.");

        Directory.CreateDirectory(directory);
        List<ProductDto> dtos = _cache.Values.Select(product => product.ToDto()).ToList();
        File.WriteAllText(_path, JsonSerializer.Serialize(dtos, Options));
    }
}
