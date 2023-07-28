using EventBus;
using IdentityService.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using static IdentityService.Configuration.IdentityConfiguration;

namespace IdentityService.Auth;

internal class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<AuthService> _logger;
    private readonly IEventBus _eventBus;

    public AuthService(
        UserManager<User> userManager,
        JwtSettings jwtSettings,
        ILogger<AuthService> logger,
        IEventBus eventBus)
    {
        _userManager = userManager;
        _jwtSettings = jwtSettings;
        _logger = logger;
        _eventBus = eventBus;
    }

    public async Task<Result<LoginUser.Response>> LoginAsync(
        LoginUser.Request request,
        CancellationToken cancellationToken)
    {
        var validator = new LoginUser.RequestValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return new Result<LoginUser.Response>(ErrorType.Validation,
                validationResult.Errors.Select(x => x.ErrorMessage));
        }

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            // don't inform if user exists or not
            return new Result<LoginUser.Response>(ErrorType.Validation ,"Wrong e-mail or password.");
        }

        bool hasValidPassword = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!hasValidPassword)
        {
            _logger.LogWarning("Not valid password provided while trying to log to account {User}", request.Email);
            return new Result<LoginUser.Response>(ErrorType.Validation, "Wrong e-mail or password.");
        }

        return GenerateLoginResponse(user);
    }

    public async Task<Result<bool?>> RegisterAsync(
        CreateUser.Request request,
        CancellationToken cancellationToken)
    {
        var validator = new CreateUser.RequestValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errorResponse = new Result<bool?>(
                ErrorType.Validation,
                validationResult.Errors.Select(x => x.ErrorMessage));
            return errorResponse;
        }

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is not null)
        {
            return new Result<bool?>(ErrorType.Validation, $"E-mail already in use.");
        }

        user = await _userManager.FindByNameAsync(request.Username);
        if (user is not null)
        {
            return new Result<bool?>(ErrorType.Validation, $"Username already in use.");
        }

        var newUser = new User 
        { 
            UserName = request.Username, 
            Email = request.Email,
            FirstName = request.FirstName,
            SecondName = request.SecondName,
            PhoneNumber = request.PhoneNumber,
        };
        var result = await _userManager.CreateAsync(newUser, request.Password);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(x => x.Description);
            return new Result<bool?>(ErrorType.Validation, errors);
        }

        var userCreatedEvent = new CreateUser.CreatedUserEvent
        {
            Email = request.Email,
            Username = request.Username,
            FirstName = request.FirstName,
            SecondName = request.SecondName,
            PhoneNumber = request.PhoneNumber
        };
        // todo implement outbox pattern so we don't fail whole process when connection with event bus fails
        _eventBus.Publish(userCreatedEvent);

        return new Result<bool?>(true);
    }

    public async Task<bool> ValidateTokenAsync(string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            return false;
        }
        var tokenHandler = new JwtSecurityTokenHandler();
        var result = await tokenHandler.ValidateTokenAsync(token, _jwtSettings.ValidationParameters);
        return result.IsValid;
    }

    private Result<LoginUser.Response> GenerateLoginResponse(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtKey = Encoding.ASCII.GetBytes(_jwtSettings.Secret);

        var subjects = new ClaimsIdentity(new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.UserName!),
            new Claim(JwtRegisteredClaimNames.Email, user.Email!),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            // RFC7519 - GivenName: Given name(s) or first name(s)
            new Claim(JwtRegisteredClaimNames.GivenName, user.FirstName!),
            // RFC7519 - FamilyName: Surname(s) or last name(s)
            new Claim(JwtRegisteredClaimNames.FamilyName, user.SecondName!),
        });

        if (user.PhoneNumber is not null)
        {
            subjects.AddClaim(new Claim("phone", user.PhoneNumber.ToString()));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        { 
            Subject = subjects,
            Expires = DateTime.UtcNow.AddHours(2),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(jwtKey), 
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);

        return new Result<LoginUser.Response>(
            new LoginUser.Response 
            { 
                Token = tokenHandler.WriteToken(token),
                IssueDate = token.ValidFrom,
                ExpiryDate = token.ValidTo
            });
    }
}
