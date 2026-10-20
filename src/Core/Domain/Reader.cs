namespace Core.Domain;

public sealed class Reader
{
    public const int MaximumOpenIssues = 5;

    private readonly List<BookCopy> _openIssues = [];

    public string Id { get; }
    public string Name { get; }
    public int OpenIssueCount => _openIssues.Count;

    private Reader(string id, string name)
    {
        Id = id;
        Name = name;
    }

    public static Reader Create(string id, string name)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Ідентифікатор читача обов'язковий", nameof(id));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Ім'я читача не може бути порожнім", nameof(name));

        return new Reader(id.Trim(), name.Trim());
    }

    public void Issue(BookCopy copy)
    {
        ArgumentNullException.ThrowIfNull(copy);
        if (OpenIssueCount >= MaximumOpenIssues)
            throw new InvalidOperationException(
                $"Читач {Name} вже має максимальну кількість відкритих видач ({MaximumOpenIssues})");

        copy.IssueTo(this);
        _openIssues.Add(copy);
    }

    public void Return(BookCopy copy)
    {
        ArgumentNullException.ThrowIfNull(copy);
        if (!_openIssues.Contains(copy))
            throw new InvalidOperationException($"Примірник {copy.Id} не виданий читачу {Name}");

        copy.ReturnFrom(this);
        _openIssues.Remove(copy);
    }

    public bool HasOpenIssue(BookCopy copy) => _openIssues.Contains(copy);
}
