using BmbOrdering.Domain.Common;

namespace BmbOrdering.Domain.Orders;

public sealed class OrderItem
{
    public const int ProductNameMaxLength = 200;

    private OrderItem()
    {
        ProductName = string.Empty;
    }

    private OrderItem(
        Guid id,
        Guid orderId,
        string productName,
        int quantity,
        decimal unitPrice)
    {
        Id = id;
        OrderId = orderId;
        ProductName = productName;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }

    public Guid Id { get; private set; }

    public Guid OrderId { get; private set; }

    public string ProductName { get; private set; }

    public int Quantity { get; private set; }

    public decimal UnitPrice { get; private set; }

    public decimal LineTotal => Quantity * UnitPrice;

    internal static OrderItem Create(
        Guid orderId,
        string productName,
        int quantity,
        decimal unitPrice)
    {
        if (orderId == Guid.Empty)
        {
            throw new DomainException("Order ID is required.");
        }

        if (string.IsNullOrWhiteSpace(productName))
        {
            throw new DomainException("Product name is required.");
        }

        productName = productName.Trim();

        if (productName.Length > ProductNameMaxLength)
        {
            throw new DomainException(
                "Product name cannot exceed 200 characters.");
        }

        if (quantity <= 0)
        {
            throw new DomainException(
                "Order item quantity must be greater than zero.");
        }

        if (unitPrice < 0)
        {
            throw new DomainException(
                "Order item unit price cannot be negative.");
        }

        return new OrderItem(
            Guid.NewGuid(),
            orderId,
            productName,
            quantity,
            unitPrice);
    }
}