namespace Core.Events;

// lab06 DoD 1 - тип даних події в Core: успадковує EventArgs і містить
// ідентифікатор товару, вид зміни, дельту кількості та залишок після зміни.
public sealed class CatalogChangedEventArgs(
    string id, CatalogChangeKind kind, int delta, int quantityAfter) : EventArgs
{
    public string Id { get; } = id;
    public CatalogChangeKind Kind { get; } = kind;
    public int Delta { get; } = delta;                 // на скільки змінилась кількість
    public int QuantityAfter { get; } = quantityAfter;
    public DateTimeOffset At { get; } = DateTimeOffset.Now;
}
