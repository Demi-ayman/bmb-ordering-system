using System.Net.Mail;
using BmbOrdering.Application.Common.Exceptions;
using BmbOrdering.Domain.Customers;

namespace BmbOrdering.Application.Authentication.Register;

public sealed class RegisterCustomerValidator
{
    private const int PasswordMinimumLength = 8;
    private const int PasswordMaximumLength = 100;

    public void Validate(RegisterCustomerCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var errors = new Dictionary<string, List<string>>();

        ValidateFullName(command.FullName, errors);
        ValidateEmail(command.Email, errors);
        ValidatePassword(command.Password, errors);
        ValidatePasswordConfirmation(command, errors);

        if (errors.Count > 0)
        {
            throw new ValidationException(
                errors.ToDictionary(
                    pair => pair.Key,
                    pair => pair.Value.ToArray()));
        }
    }

    private static void ValidateFullName(
        string fullName,
        IDictionary<string, List<string>> errors)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            AddError(errors, nameof(RegisterCustomerCommand.FullName),
                "Full name is required.");

            return;
        }

        if (fullName.Trim().Length > Customer.FullNameMaxLength)
        {
            AddError(errors, nameof(RegisterCustomerCommand.FullName),
                $"Full name cannot exceed {Customer.FullNameMaxLength} characters.");
        }
    }

    private static void ValidateEmail(
        string email,
        IDictionary<string, List<string>> errors)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            AddError(errors, nameof(RegisterCustomerCommand.Email),
                "Email is required.");

            return;
        }

        email = email.Trim();

        if (email.Length > Customer.EmailMaxLength)
        {
            AddError(errors, nameof(RegisterCustomerCommand.Email),
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
                AddError(errors, nameof(RegisterCustomerCommand.Email),
                    "Email format is invalid.");
            }
        }
        catch (FormatException)
        {
            AddError(errors, nameof(RegisterCustomerCommand.Email),
                "Email format is invalid.");
        }
    }

    private static void ValidatePassword(
        string password,
        IDictionary<string, List<string>> errors)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            AddError(errors, nameof(RegisterCustomerCommand.Password),
                "Password is required.");

            return;
        }

        if (password.Length < PasswordMinimumLength)
        {
            AddError(errors, nameof(RegisterCustomerCommand.Password),
                $"Password must contain at least {PasswordMinimumLength} characters.");
        }

        if (password.Length > PasswordMaximumLength)
        {
            AddError(errors, nameof(RegisterCustomerCommand.Password),
                $"Password cannot exceed {PasswordMaximumLength} characters.");
        }

        if (!password.Any(char.IsUpper))
        {
            AddError(errors, nameof(RegisterCustomerCommand.Password),
                "Password must contain an uppercase letter.");
        }

        if (!password.Any(char.IsLower))
        {
            AddError(errors, nameof(RegisterCustomerCommand.Password),
                "Password must contain a lowercase letter.");
        }

        if (!password.Any(char.IsDigit))
        {
            AddError(errors, nameof(RegisterCustomerCommand.Password),
                "Password must contain a number.");
        }
    }

    private static void ValidatePasswordConfirmation(
        RegisterCustomerCommand command,
        IDictionary<string, List<string>> errors)
    {
        if (!string.Equals(
                command.Password,
                command.PasswordConfirmation,
                StringComparison.Ordinal))
        {
            AddError(
                errors,
                nameof(RegisterCustomerCommand.PasswordConfirmation),
                "Password confirmation does not match.");
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