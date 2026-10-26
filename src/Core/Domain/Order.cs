namespace Core.Domain;

public sealed class Order
{
    public string Id { get; }
    public OrderStatus Status { get; private set; }

    private Order(string id)
    {
        Id = id;
        Status = OrderStatus.Draft;
    }

    public static Order Create(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Ідентифікатор замовлення обов'язковий", nameof(id));

        return new Order(id.Trim());
    }

    public void TransitionTo(OrderStatus targetStatus)
    {
        Status = (Status, targetStatus) switch
        {
            (OrderStatus.Draft, OrderStatus.Confirmed) => OrderStatus.Confirmed,
            (OrderStatus.Draft, OrderStatus.Cancelled) => OrderStatus.Cancelled,
            (OrderStatus.Confirmed, OrderStatus.Cancelled) => OrderStatus.Cancelled,
            _ => throw new InvalidOperationException(
                $"Недопустимий перехід замовлення {Id}: {Status} → {targetStatus}")
        };
    }
}
