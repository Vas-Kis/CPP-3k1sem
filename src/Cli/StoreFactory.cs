using Core;
using Core.Abstractions;
using Core.Storage;

namespace Cli;

// lab05 add03 - фабрика сховищ. Лежить у проєкті Cli, тому конкретні класи
// сховищ, як і раніше, згадуються лише в composition root, а не в Core.
public static class StoreFactory
{
    // lab05 add03 - вибір реалізації за аргументами командного рядка:
    // --file  -> FileCatalogStore (data/catalog.json), інакше InMemoryCatalogStore;
    // --cache -> вибране сховище загортається в декоратор CachingCatalogStore (add01).
    public static ICatalogStore Create(string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);

        bool useFile = args.Contains("--file", StringComparer.OrdinalIgnoreCase);
        bool useCache = args.Contains("--cache", StringComparer.OrdinalIgnoreCase);

        string dataPath = Path.Combine(AppContext.BaseDirectory, "data", "catalog.json");
        ICatalogStore store = useFile
            ? new FileCatalogStore(dataPath)
            : new InMemoryCatalogStore(SampleData.Products());

        return useCache ? new CachingCatalogStore(store) : store;
    }
}
