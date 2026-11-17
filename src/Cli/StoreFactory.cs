using Core;
using Core.Abstractions;
using Core.Storage;

namespace Cli;

public static class StoreFactory
{
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
