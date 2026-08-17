using BmbOrdering.Domain.Common;

namespace BmbOrdering.Domain.Orders;

public sealed class Order
{
    public const int OrderNumberMaxLength = 30;

    private readonly List<OrderItem> _items = new();

    private Order()
    {
        OrderNumber = string.Empty;
    }

    private Order(
        Guid id,
        Guid customerId,
        string orderNumber,
        DateTime createdAtUtc)
    {
        Id = id;
        CustomerId = customerId;
        OrderNumber = orderNumber;
        Status = OrderStatus.Created;
        CreatedAtUtc = createdAtUtc;
    }

    public Guid Id { get; private set; }

    public Guid CustomerId { get; private set; }

    public string OrderNumber { get; private set; }

    public OrderStatus Status { get; private set; }

    public decimal TotalAmount { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime? DeletedAtUtc { get; private set; }

    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    public bool IsDeleted => Status == OrderStatus.Deleted;

    public static Order Create(
        Guid customerId,
        string orderNumber,
        IEnumerable<OrderItemDetails> itemDetails,
        DateTime createdAtUtc)
    {
        if (customerId == Guid.Empty)
        {
            throw new DomainException("Customer ID is required.");
        }

        if (string.IsNullOrWhiteSpace(orderNumber))
        {
            throw new DomainException("Order number is required.");
        }

        orderNumber = orderNumber.Trim();

        if (orderNumber.Length > OrderNumberMaxLength)
        {
            throw new DomainException(
                "Order number cannot exceed 30 characters.");
        }

        EnsureUtc(createdAtUtc, nameof(createdAtUtc));

        if (itemDetails is null)
        {
            throw new DomainException("Order items are required.");
        }

        var details = itemDetails.ToList();

        if (details.Count == 0)
        {
            throw new DomainException(
                "An order must contain at least one item.");
        }

        var order = new Order(
            Guid.NewGuid(),
            customerId,
            orderNumber,
            createdAtUtc);

        foreach (var item in details)
        {
            order._items.Add(
                OrderItem.Create(
                    order.Id,
                    item.ProductName,
                    item.Quantity,
                    item.UnitPrice));
        }

        order.TotalAmount = order._items.Sum(item => item.LineTotal);

        return order;
    }

    public void Delete(DateTime deletedAtUtc)
    {
        EnsureUtc(deletedAtUtc, nameof(deletedAtUtc));

        if (IsDeleted)
        {
            throw new DomainException("Order is already deleted.");
        }

        if (deletedAtUtc < CreatedAtUtc)
        {
            throw new DomainException(
                "Order deletion time cannot be before its creation time.");
        }

        Status = OrderStatus.Deleted;
        DeletedAtUtc = deletedAtUtc;
    }

    public bool WasDeletedOnCreationDate()
    {
        return DeletedAtUtc.HasValue &&
               DeletedAtUtc.Value.Date == CreatedAtUtc.Date;
    }

    private static void EnsureUtc(DateTime value, string parameterName)
    {
        if (value.Kind != DateTimeKind.Utc)
        {
            throw new DomainException(
                $"{parameterName} must use the UTC date and time kind.");
        }
    }
}