using BmbOrdering.Domain.Customers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BmbOrdering.Infrastructure.Persistence.Configurations;

public sealed class CustomerConfiguration :
    IEntityTypeConfiguration<Customer>
{
    public void Configure(
        EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");

        builder.HasKey(customer => customer.Id);

        builder.Property(customer => customer.Id)
            .ValueGeneratedNever();

        builder.Property(customer => customer.FullName)
            .HasMaxLength(Customer.FullNameMaxLength)
            .IsRequired();

        builder.Property(customer => customer.Email)
            .HasMaxLength(Customer.EmailMaxLength)
            .IsRequired();

        builder.Property(customer => customer.NormalizedEmail)
            .HasMaxLength(Customer.EmailMaxLength)
            .IsRequired();

        builder.HasIndex(customer => customer.NormalizedEmail)
            .IsUnique()
            .HasDatabaseName("UX_Customers_NormalizedEmail");

        builder.Property(customer => customer.PasswordHash)
            .IsRequired();

        builder.Property(customer => customer.CreatedAtUtc)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Property(customer => customer.BannedUntilUtc)
            .HasColumnType("datetime2");

        builder.Property<byte[]>("RowVersion")
            .IsRowVersion()
            .IsConcurrencyToken()
            .IsRequired();
    }
}