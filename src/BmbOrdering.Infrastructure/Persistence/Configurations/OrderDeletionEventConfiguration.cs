using BmbOrdering.Domain.Customers;
using BmbOrdering.Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BmbOrdering.Infrastructure.Persistence.Configurations;

public sealed class OrderDeletionEventConfiguration :
    IEntityTypeConfiguration<OrderDeletionEvent>
{
    public void Configure(
        EntityTypeBuilder<OrderDeletionEvent> builder)
    {
        builder.ToTable("OrderDeletionEvents");

        builder.HasKey(deletionEvent => deletionEvent.Id);

        builder.Property(deletionEvent => deletionEvent.Id)
            .ValueGeneratedNever();

        builder.Property(deletionEvent => deletionEvent.OrderId)
            .IsRequired();

        builder.HasIndex(deletionEvent => deletionEvent.OrderId)
            .IsUnique()
            .HasDatabaseName("UX_OrderDeletionEvents_OrderId");

        builder.HasOne<Order>()
            .WithMany()
            .HasForeignKey(deletionEvent => deletionEvent.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(deletionEvent => deletionEvent.CustomerId)
            .IsRequired();

        builder.HasOne<Customer>()
            .WithMany()
            .HasForeignKey(deletionEvent => deletionEvent.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(
                deletionEvent => deletionEvent.OrderCreatedAtUtc)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Property(deletionEvent => deletionEvent.DeletedAtUtc)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Property(
                deletionEvent => deletionEvent.QualifiesForBanCount)
            .IsRequired();

        builder.HasIndex(
                deletionEvent => new
                {
                    deletionEvent.CustomerId,
                    deletionEvent.QualifiesForBanCount,
                    deletionEvent.DeletedAtUtc
                })
            .HasDatabaseName(
                "IX_OrderDeletionEvents_Customer_Qualifies_DeletedAt");
    }
}