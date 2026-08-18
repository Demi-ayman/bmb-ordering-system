using System.Net.Mail;
using BmbOrdering.Application.Common.Exceptions;
using BmbOrdering.Domain.Customers;

namespace BmbOrdering.Application.Authentication.Login;

public sealed class LoginCustomerValidator
{
    public void Validate(LoginCustomerCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var errors = new Dictionary<string, List<string>>();

        ValidateEmail(command.Email, errors);
        ValidatePassword(command.Password, errors);

        if (errors.Count > 0)
        {
            throw new ValidationException(
                errors.ToDictionary(
                    pair => pair.Key,
                    pair => pair.Value.ToArray()));
        }
    }

    private static void ValidateEmail(
        string email,
        IDictionary<string, List<string>> errors)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            AddError(
                errors,
                nameof(LoginCustomerCommand.Email),
                "Email is required.");

            return;
        }

        email = email.Trim();

        if (email.Length > Customer.EmailMaxLength)
        {
            AddError(
                errors,
                nameof(LoginCustomerCommand.Email),
                $"Email cannot exceed {Customer.EmailMaxLength} characters.");

            return;
        }

        try
        {
            var mailAddress = new MailAddress(email);

            if (!string.Equals(
                    mailAddress.Address,
                    email,
                    StringComparison.OrdinalIgnoreCase))
            {
                AddError(
                    errors,
                    nameof(LoginCustomerCommand.Email),
                    "Email format is invalid.");
            }
        }
        catch (FormatException)
        {
            AddError(
                errors,
                nameof(LoginCustomerCommand.Email),
                "Email format is invalid.");
        }
    }

    private static void ValidatePassword(
        string password,
        IDictionary<string, List<string>> errors)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            AddError(
                errors,
                nameof(LoginCustomerCommand.Password),
                "Password is required.");
        }
    }

    private static void AddError(
        IDictionary<string, List<string>> errors,
        string propertyName,
        string message)
    {
        if (!errors.TryGetValue(propertyName, out var propertyErrors))
        {
            propertyErrors = new List<string>();
            errors[propertyName] = propertyErrors;
        }

        propertyErrors.Add(message);
    }
}