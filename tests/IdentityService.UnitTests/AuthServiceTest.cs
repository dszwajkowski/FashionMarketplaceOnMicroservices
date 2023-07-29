using EventBus;
using FluentAssertions;
using IdentityService.Auth;
using IdentityService.Models;
using IdentityService.UnitTests.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using static IdentityService.Configuration.IdentityConfiguration;

namespace IdentityService.UnitTests;

public class AuthServiceTest
{
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly JwtSettings _jwtSettings;
    private readonly Mock<ILogger<AuthService>> _logger;
    private readonly Mock<IEventBus> _eventBus;
    private List<User> _users;

    public AuthServiceTest()
    {
        _users = GetFakeUsers();
        _userManagerMock = MockHelpers.MockUserManager(_users);

        var jwtSettings = new JwtSettings();
        jwtSettings.Secret = "secretfortestingpurposese8dfb0b7774c42c4a56fe8feb17d3f93";
        jwtSettings.ValidationParameters = GetTokenValidationParameters(jwtSettings.Secret);
        _jwtSettings = jwtSettings;

        _logger = new Mock<ILogger<AuthService>>();

        _eventBus = new Mock<IEventBus>();
        _eventBus.Setup(x => x.Publish(It.IsAny<IntegrationEvent>()))
            .Verifiable();
    }

    [Fact]
    public async Task RegisterUser_ReturnTrue_WhenUserCreated()
    {
        // Arrange
        var authService = new AuthService(_userManagerMock.Object, _jwtSettings, _logger.Object, _eventBus.Object);
        var createUserRequest = new CreateUser.Request("TestUser3", "testuser3@example.com",
            "Qwerty123", "John", "Smith", "123456789");

        // Act
        var result = await authService.RegisterAsync(createUserRequest, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeTrue();
        _users.Count.Should().Be(3);
    }

    [Fact]
    public async Task RegisterUser_ReturnValidationError_WhenRequestNotValid()
    {
        // Arrange
        var authService = new AuthService(_userManagerMock.Object, _jwtSettings, _logger.Object, _eventBus.Object);
        var createUserRequest = new CreateUser.Request("", "natvalidemail",
            "", "", "", "");

        // Act
        var result = await authService.RegisterAsync(createUserRequest, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.Validation);
        result.ErrorMessages.Should().NotBeNull();
        // there should be error message for each property
        result.ErrorMessages!.Count().Should().Be(6);
        result.Data.Should().BeNull();
        // user shouldn't be added
        _users.Count.Should().Be(2);
    }

    [Fact]
    public async Task RegisterUser_ReturnValidationError_WhenEmailTaken()
    {
        // Arrange
        var authService = new AuthService(_userManagerMock.Object, _jwtSettings, _logger.Object, _eventBus.Object);
        var createUserRequest = new CreateUser.Request("TestUser3", "testuser1@example.com",
            "Qwerty123", "John", "Smith", "123456789");

        // Act
        var result = await authService.RegisterAsync(createUserRequest, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.Validation);
        result.ErrorMessages.Should().NotBeNull();
        result.ErrorMessages!.Count().Should().Be(1);
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task RegisterUser_ReturnValidationError_WhenUserNameTaken()
    {
        // Arrange
        var authService = new AuthService(_userManagerMock.Object, _jwtSettings, _logger.Object, _eventBus.Object);
        var createUserRequest = new CreateUser.Request("TestUser3", "testuser1@example.com",
            "Qwerty123", "John", "Smith", "123456789");

        // Act
        var result = await authService.RegisterAsync(createUserRequest, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.Validation);
        result.ErrorMessages.Should().NotBeNull();
        result.ErrorMessages!.Count().Should().Be(1);
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task LoginUser_ReturnToken_WhenLoginSuccessfully()
    {
        // Arrange
        var authService = new AuthService(_userManagerMock.Object, _jwtSettings, _logger.Object, _eventBus.Object);
        var loginRequest = new LoginUser.Request("testuser1@example.com", "Qwerty123");

        // Act
        var result = await authService.LoginAsync(loginRequest, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task LoginUser_ReturnValidationError_WhenRequestNotValid()
    {
        // Arrange
        var authService = new AuthService(_userManagerMock.Object, _jwtSettings, _logger.Object, _eventBus.Object);
        var loginRequest = new LoginUser.Request("notvalidemail", "");

        // Act
        var result = await authService.LoginAsync(loginRequest, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.Validation);
        result.ErrorMessages.Should().NotBeNull();
        // there should be error message for each property
        result.ErrorMessages!.Count().Should().Be(2);
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task LoginUser_ReturnValidationError_WhenWrongEmail()
    {
        // Arrange
        var authService = new AuthService(_userManagerMock.Object, _jwtSettings, _logger.Object, _eventBus.Object);
        var loginRequest = new LoginUser.Request("thisemaildoesntexists@example.com", "Qwerty123");

        // Act
        var result = await authService.LoginAsync(loginRequest, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.Validation);
        result.ErrorMessages.Should().NotBeNull();
        result.ErrorMessages!.Count().Should().Be(1);
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task LoginUser_ReturnValidationError_WhenWrongPassword()
    {
        // Arrange
        var authService = new AuthService(_userManagerMock.Object, _jwtSettings, _logger.Object, _eventBus.Object);
        var loginRequest = new LoginUser.Request("testuser1@example.com", "notvalidpassword");

        // Act
        var result = await authService.LoginAsync(loginRequest, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.Validation);
        result.ErrorMessages.Should().NotBeNull();
        result.ErrorMessages!.Count().Should().Be(1);
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task ValidateToken_ReturnTrue_WhenTokenValid()
    {
        // Arrange
        var authService = new AuthService(_userManagerMock.Object, _jwtSettings, _logger.Object, _eventBus.Object);
        // get valid token
        var loginRequest = new LoginUser.Request("testuser1@example.com", "Qwerty123");
        var loginResult = await authService.LoginAsync(loginRequest, CancellationToken.None);
        var token = loginResult.Data?.Token ?? "notvalidtoken";

        // Act
        var result = await authService.ValidateTokenAsync(token);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateToken_ReturnFalse_WhenTokenInvalid()
    {
        // Arrange
        var authService = new AuthService(_userManagerMock.Object, _jwtSettings, _logger.Object, _eventBus.Object);
        const string invalidToken = "somenotvalidtoken";

        // Act
        var result = await authService.ValidateTokenAsync(invalidToken);

        // Assert
        result.Should().BeFalse();
    }

    private List<User> GetFakeUsers()
    {
        // use plain text password so we can easily mock password check
        var users = new List<User>
        {
            new User { Id = Guid.NewGuid().ToString(), UserName = "TestUser1", 
                Email = "testuser1@example.com", PasswordHash = "Qwerty123",
                FirstName = "John", SecondName = "Smith"},
            new User { Id = Guid.NewGuid().ToString(), UserName = "TestUser2", 
                Email = "testuser2@example.com", PasswordHash = "Qwerty123",
                FirstName = "John", SecondName = "Doe"},
        };

        return users;
    }
}