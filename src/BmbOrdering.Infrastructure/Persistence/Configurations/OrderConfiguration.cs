using BmbOrdering.Domain.Customers;
using BmbOrdering.Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BmbOrdering.Infrastructure.Persistence.Configurations;

public sealed class OrderConfiguration :
    IEntityTypeConfiguration<Order>
{
    public void Configure(
        EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        builder.HasKey(order => order.Id);

        builder.Property(order => order.Id)
            .ValueGeneratedNever();

        builder.Property(order => order.CustomerId)
            .IsRequired();

        builder.HasOne<Customer>()
            .WithMany()
            .HasForeignKey(order => order.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(order => order.OrderNumber)
            .HasMaxLength(Order.OrderNumberMaxLength)
            .IsRequired();

        builder.HasIndex(order => order.OrderNumber)
            .IsUnique()
            .HasDatabaseName("UX_Orders_OrderNumber");

        builder.Property(order => order.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(order => order.TotalAmount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(order => order.CreatedAtUtc)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Property(order => order.DeletedAtUtc)
            .HasColumnType("datetime2");

        builder.HasIndex(
                order => new
                {
                    order.CustomerId,
                    order.CreatedAtUtc
                })
            .HasDatabaseName("IX_Orders_CustomerId_CreatedAtUtc");

        builder.HasMany(order => order.Items)
            .WithOne()
            .HasForeignKey(item => item.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(order => order.Items)
            .HasField("_items")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Ignore(order => order.IsDeleted);

        builder.HasQueryFilter(
            order => order.Status != OrderStatus.Deleted);

        builder.Property<byte[]>("RowVersion")
            .IsRowVersion()
            .IsConcurrencyToken()
            .IsRequired();
    }
}