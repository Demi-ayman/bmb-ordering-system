using BmbOrdering.Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BmbOrdering.Infrastructure.Persistence.Configurations;

public sealed class OrderItemConfiguration :
    IEntityTypeConfiguration<OrderItem>
{
    public void Configure(
        EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItems");

        builder.HasKey(item => item.Id);

        builder.Property(item => item.Id)
            .ValueGeneratedNever();

        builder.Property(item => item.OrderId)
            .IsRequired();

        builder.Property(item => item.ProductName)
            .HasMaxLength(OrderItem.ProductNameMaxLength)
            .IsRequired();

        builder.Property(item => item.Quantity)
            .IsRequired();

        builder.Property(item => item.UnitPrice)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Ignore(item => item.LineTotal);

        builder.HasIndex(item => item.OrderId)
            .HasDatabaseName("IX_OrderItems_OrderId");
    }
}