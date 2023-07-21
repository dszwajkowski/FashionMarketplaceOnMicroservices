using FluentValidation;
using IdentityService.Configuration;

namespace IdentityService.Auth;

public static class CreateUser
{
    public record Request(
        string Username, 
        string Email, 
        string Password,
        string FirstName,
        string SecondName,
        string PhoneNumber);

    internal class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty()
                .MinimumLength(4)
                .MaximumLength(15);
            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress();
            RuleFor(x => x.Password)
                .NotEmpty()
                .MinimumLength(IdentityConfiguration.MinimumPasswordLength); // todo move other constants
            RuleFor(x => x.FirstName)
                .NotEmpty()
                .MinimumLength(2)
                .MaximumLength(50);
            RuleFor(x => x.SecondName)
                .NotEmpty()
                .MinimumLength(2)
                .MaximumLength(50);
            RuleFor(x => x.PhoneNumber)
                .MinimumLength(5)
                .MaximumLength(15);
        }
    }
}
