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
            RuleLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.Username)
                .NotEmpty()
                .Length(4, 15);
            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress();
            RuleFor(x => x.Password)
                .NotEmpty()
                .MinimumLength(IdentityConfiguration.MinimumPasswordLength); // todo move other constants
            RuleFor(x => x.FirstName)
                .NotEmpty()
                .Length(2, 50);
            RuleFor(x => x.SecondName)
                .NotEmpty()
                .Length(2, 50);
            RuleFor(x => x.PhoneNumber)
                .Length(5, 15);
        }
    }
}
