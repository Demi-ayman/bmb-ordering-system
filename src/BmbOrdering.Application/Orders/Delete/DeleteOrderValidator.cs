using BmbOrdering.Application.Common.Exceptions;

namespace BmbOrdering.Application.Orders.Delete;

public sealed class DeleteOrderValidator
{
	public void Validate(DeleteOrderCommand command)
	{
		ArgumentNullException.ThrowIfNull(command);

		if (command.OrderId == Guid.Empty)
		{
			throw new ValidationException(
				new Dictionary<string, string[]>
				{
					[nameof(DeleteOrderCommand.OrderId)] =
						new[] { "Order ID is required." }
				});
		}
	}
}