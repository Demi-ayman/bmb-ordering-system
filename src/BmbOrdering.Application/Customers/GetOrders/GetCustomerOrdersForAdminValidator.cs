using BmbOrdering.Application.Common.Exceptions;

namespace BmbOrdering.Application.Customers.GetOrders;

public sealed class GetCustomerOrdersForAdminValidator
{
    public void Validate(
        GetCustomerOrdersForAdminQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (query.CustomerId == Guid.Empty)
        {
            throw new ValidationException(
                new Dictionary<string, string[]>
                {
                    [nameof(GetCustomerOrdersForAdminQuery.CustomerId)] =
                        new[] { "Customer ID is required." }
                });
        }
    }
}
