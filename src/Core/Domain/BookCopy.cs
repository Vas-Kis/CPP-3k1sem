namespace Core.Domain;

public sealed class BookCopy
{
    public string Id { get; }
    public string Title { get; }
    public Reader? IssuedTo { get; private set; }
    public bool IsAvailable => IssuedTo is null;

    private BookCopy(string id, string title)
    {
        Id = id;
        Title = title;
    }

    public static BookCopy Create(string id, string title)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Ідентифікатор примірника обов'язковий", nameof(id));
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Назва книги не може бути порожньою", nameof(title));

        return new BookCopy(id.Trim(), title.Trim());
    }

    internal void IssueTo(Reader reader)
    {
        if (!IsAvailable)
            throw new InvalidOperationException($"Примірник {Id} вже виданий читачу {IssuedTo!.Name}");

        IssuedTo = reader;
    }

    internal void ReturnFrom(Reader reader)
    {
        if (!ReferenceEquals(IssuedTo, reader))
            throw new InvalidOperationException($"Примірник {Id} не виданий читачу {reader.Name}");

        IssuedTo = null;
    }
}
