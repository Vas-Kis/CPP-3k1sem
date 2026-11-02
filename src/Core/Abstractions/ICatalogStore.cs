using Core.Domain; 

namespace Core.Abstractions;

//Definition of done 1
public interface ICatalogStore
{
    IReadOnlyList<Product> List();
    Product? GetById(string id);
    void Add(Product item);
    void Update(Product item);
    bool Remove(string id);
}
