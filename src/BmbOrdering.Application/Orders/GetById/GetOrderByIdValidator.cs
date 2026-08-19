using BmbOrdering.Application.Common.Exceptions;

namespace BmbOrdering.Application.Orders.GetById;

public sealed class GetOrderByIdValidator
{
    public void Validate(GetOrderByIdQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (query.OrderId == Guid.Empty)
        {
            throw new ValidationException(
                new Dictionary<string, string[]>
                {
                    [nameof(GetOrderByIdQuery.OrderId)] =
                        new[] { "Order ID is required." }
                });
        }
    }
}