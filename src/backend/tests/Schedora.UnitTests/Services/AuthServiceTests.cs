using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Schedora.Application.Services;
using Schedora.Domain.Entities;
using Schedora.Domain.Interfaces;
using Schedora.Domain.Services;
using Schedora.UnitTests.Mocks;
using Schedora.UnitTests.Fakers.Entities;
using Xunit;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Threading.Tasks;
using Schedora.Application.Requests;
using Schedora.Application.Responses;
using Schedora.Domain.Exceptions;
using SocialScheduler.Domain.Constants;

namespace Schedora.UnitTests.Services;

public class AuthServiceTests
{
    private static Mock<UserManager<User>> MockUserManager()
    {
        var store = new Mock<IUserStore<User>>();

        return new Mock<UserManager<User>>(
            store.Object,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null
        );
    }

    private IAuthService _service;
    private Mock<ILogger<AuthService>> _logger;
    private Mock<IMapper> _mapper;
    private Mock<ITokenService> _tokenService;
    private UnitOfWorkMock _uowMock;
    private Mock<IUnitOfWork> _uow;
    private Mock<IPasswordCryptographyService> _cryptographyService;
    private Mock<IEmailService> _emailService;
    private Mock<UserManager<User>> _userManager;
    private Mock<IActivityLogService> _activityLogService;
    private Mock<IConfiguration> _configuration;

    public AuthServiceTests()
    {
        _logger = new Mock<ILogger<AuthService>>();
        _mapper = new Mock<IMapper>();
        _tokenService = new Mock<ITokenService>();
        _uowMock = new UnitOfWorkMock();
        _uow = _uowMock.GetMock();
        _cryptographyService = new Mock<IPasswordCryptographyService>();
        _emailService = new Mock<IEmailService>();
        _activityLogService = new Mock<IActivityLogService>();
        _configuration = new Mock<IConfiguration>();

        _userManager = MockUserManager();

        _service = new AuthService(
            _logger.Object,
            _mapper.Object,
            _tokenService.Object,
            _uow.Object,
            _cryptographyService.Object,
            _emailService.Object,
            _userManager.Object,
            _activityLogService.Object,
            _configuration.Object
        );
    }

    #region RegisterUser Tests

    [Fact]
    public async Task RegisterUser_WithValidData_ShouldRegisterSuccessfully()
    {
        // Arrange
        var request = new UserRegisterRequest
        {
            Email = "test@example.com",
            Password = "Password123!",
            FirstName = "Test",
            LastName = "User"
        };
        var emailConfirmationUri = "https://example.com/confirm";
        var user = UserEntityFaker.Generate();
        var userResponse = new UserResponse { Email = request.Email };
        var hashedPassword = "hashedPassword123";
        var emailConfirmationToken = "confirmationToken";
        var encodedToken = "encodedToken";
        var emailMessage = "<html>Confirmation Email</html>";

        _uowMock.UserRepository.Setup(x => x.UserByEmailExists(request.Email))
            .ReturnsAsync(false);
        _uowMock.UserRepository.Setup(x => x.GetUsersNotActiveByEmail(request.Email))
            .ReturnsAsync(new List<User>());
        _mapper.Setup(x => x.Map<User>(request)).Returns(user);
        _cryptographyService.Setup(x => x.HashPassword(request.Password))
            .Returns(hashedPassword);
        _uowMock.GenericRepository.Setup(x => x.Add<User>(It.IsAny<User>()))
            .Returns(Task.CompletedTask);
        _uowMock.GenericRepository.Setup(x => x.Add<Subscription>(It.IsAny<Subscription>()))
            .Returns(Task.CompletedTask);
        _uow.Setup(x => x.Commit()).Returns(Task.CompletedTask);
        _userManager.Setup(x => x.GenerateEmailConfirmationTokenAsync(It.IsAny<User>()))
            .ReturnsAsync(emailConfirmationToken);
        _emailService.Setup(x => x.RenderEmailConfirmation(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(emailMessage);
        _emailService.Setup(x => x.SendEmail(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        _mapper.Setup(x => x.Map<UserResponse>(It.IsAny<User>()))
            .Returns(userResponse);

        // Act
        var result = await _service.RegisterUser(request, emailConfirmationUri);

        // Assert
        result.Should().NotBeNull();
        result.Email.Should().Be(request.Email);
        _uowMock.GenericRepository.Verify(x => x.Add<User>(It.IsAny<User>()), Times.Once);
        _uowMock.GenericRepository.Verify(x => x.Add<Subscription>(It.IsAny<Subscription>()), Times.Once);
        _uow.Verify(x => x.Commit(), Times.Exactly(2));
        _emailService.Verify(x => x.SendEmail(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task RegisterUser_WithInvalidEmail_ShouldThrowRequestException()
    {
        // Arrange
        var request = new UserRegisterRequest
        {
            Email = "invalid-email",
            Password = "Password123!"
        };
        var emailConfirmationUri = "https://example.com/confirm";

        // Act & Assert
        await Assert.ThrowsAsync<RequestException>(
            () => _service.RegisterUser(request, emailConfirmationUri));
    }

    [Fact]
    public async Task RegisterUser_WithExistingEmail_ShouldThrowConflictException()
    {
        // Arrange
        var request = new UserRegisterRequest
        {
            Email = "existing@example.com",
            Password = "Password123!"
        };
        var emailConfirmationUri = "https://example.com/confirm";

        _uowMock.UserRepository.Setup(x => x.UserByEmailExists(request.Email))
            .ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(
            () => _service.RegisterUser(request, emailConfirmationUri));
    }

    [Fact]
    public async Task RegisterUser_WithInactiveUsers_ShouldDeleteThemBeforeRegistering()
    {
        // Arrange
        var request = new UserRegisterRequest
        {
            Email = "test@example.com",
            Password = "Password123!"
        };
        var emailConfirmationUri = "https://example.com/confirm";
        var inactiveUsers = new List<User>
        {
            UserEntityFaker.Generate(),
            UserEntityFaker.Generate()
        };
        var user = UserEntityFaker.Generate();
        var userResponse = new UserResponse { Email = request.Email };
        var emailMessage = "<html>Email</html>";

        _uowMock.UserRepository.Setup(x => x.UserByEmailExists(request.Email))
            .ReturnsAsync(false);
        _uowMock.UserRepository.Setup(x => x.GetUsersNotActiveByEmail(request.Email))
            .ReturnsAsync(inactiveUsers);
        _mapper.Setup(x => x.Map<User>(request)).Returns(user);
        _cryptographyService.Setup(x => x.HashPassword(It.IsAny<string>()))
            .Returns("hashedPassword");
        _userManager.Setup(x => x.GenerateEmailConfirmationTokenAsync(It.IsAny<User>()))
            .ReturnsAsync("token");
        _emailService.Setup(x => x.RenderEmailConfirmation(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(emailMessage);
        _mapper.Setup(x => x.Map<UserResponse>(It.IsAny<User>()))
            .Returns(userResponse);

        // Act
        var result = await _service.RegisterUser(request, emailConfirmationUri);

        // Assert
        _uowMock.GenericRepository.Verify(
            x => x.DeleteRange<User>(It.Is<List<User>>(list => list.Count == 2)), Times.Once);
    }

    #endregion

    #region ConfirmEmail Tests

    [Fact]
    public async Task ConfirmEmail_WithValidToken_ShouldConfirmEmailSuccessfully()
    {
        // Arrange
        var email = "test@example.com";
        var token = "dGVzdFRva2Vu"; // Base64 encoded "testToken"
        var user = UserEntityFaker.Generate();
        user.Email = email;
        user.IsActive = false;
        user.EmailConfirmed = false;

        _uowMock.UserRepository.Setup(x => x.UserByEmailNotConfirmed(email))
            .ReturnsAsync(user);
        _userManager.Setup(x => x.ConfirmEmailAsync(user, It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        _activityLogService.Setup(x => x.LogAsync(
            It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>(), 
            It.IsAny<long>(), It.IsAny<object>(), It.IsAny<bool>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.ConfirmEmail(email, token);

        // Assert
        user.IsActive.Should().BeTrue();
        user.EmailConfirmed.Should().BeTrue();
        _uowMock.GenericRepository.Verify(x => x.Update<User>(user), Times.Once);
        _uow.Verify(x => x.Commit(), Times.Once);
        _activityLogService.Verify(x => x.LogAsync(
            It.IsAny<long>(), ActivityActions.EMAIL_VERIFIED, It.IsAny<string>(), 
            It.IsAny<long>(), It.IsAny<object>(), false), Times.Once);
    }

    [Fact]
    public async Task ConfirmEmail_WithNonExistentUser_ShouldThrowRequestException()
    {
        // Arrange
        var email = "nonexistent@example.com";
        var token = "dGVzdFRva2Vu";

        _uowMock.UserRepository.Setup(x => x.UserByEmailNotConfirmed(email))
            .ReturnsAsync((User)null);

        // Act & Assert
        await Assert.ThrowsAsync<RequestException>(
            () => _service.ConfirmEmail(email, token));
    }

    [Fact]
    public async Task ConfirmEmail_WithInvalidToken_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var email = "test@example.com";
        var token = "aW52YWxpZFRva2Vu"; // Base64 encoded "invalidToken"
        var user = UserEntityFaker.Generate();
        user.Email = email;

        var errors = new List<IdentityError>
        {
            new IdentityError { Description = "Invalid token" }
        };

        _uowMock.UserRepository.Setup(x => x.UserByEmailNotConfirmed(email))
            .ReturnsAsync(user);
        _userManager.Setup(x => x.ConfirmEmailAsync(user, It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(errors.ToArray()));

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(
            () => _service.ConfirmEmail(email, token));
    }

    #endregion

    #region LoginByApplication Tests

    [Fact]
    public async Task LoginByApplication_WithValidCredentials_ShouldReturnLoginResponse()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "Password123!"
        };
        var user = UserEntityFaker.Generate();
        user.Email = request.Email;
        var accessToken = "accessToken123";
        var expiration = DateTime.UtcNow.AddHours(1);
        var refreshToken = "refreshToken123";
        var refreshExpiration = DateTime.UtcNow.AddDays(7);

        _uowMock.UserRepository.Setup(x => x.UserByEmail(request.Email))
            .ReturnsAsync(user);
        _tokenService.Setup(x => x.GenerateToken(user.Id, user.UserName, It.IsAny<List<Claim>>()))
            .Returns((accessToken, expiration));
        _tokenService.Setup(x => x.GenerateRefreshToken())
            .Returns(refreshToken);
        _tokenService.Setup(x => x.GenerateRefreshTokenExpiration())
            .Returns(refreshExpiration);
        _activityLogService.Setup(x => x.LogAsync(
            It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>(),  It.IsAny<object>(), It.IsAny<bool>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.LoginByApplication(request);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().Be(accessToken);
        result.RefreshToken.Should().Be(refreshToken);
        result.AccessExpiresAt.Should().Be(expiration);
        result.RefreshExpiresAt.Should().Be(refreshExpiration);
        user.RefreshToken.Should().Be(refreshToken);
        user.RefreshTokenExpires.Should().Be(refreshExpiration);
        _uowMock.GenericRepository.Verify(x => x.Update<User>(user), Times.Once);
        _uow.Verify(x => x.Commit(), Times.Once);
        _activityLogService.Verify(x => x.LogAsync(
            user.Id, ActivityActions.USER_LOGIN, "user", user.Id, It.IsAny<string>(), It.IsAny<bool>()), Times.Once);
    }

    [Fact]
    public async Task LoginByApplication_WithNonExistentUser_ShouldThrowNotFoundException()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "nonexistent@example.com",
            Password = "Password123!"
        };

        _uowMock.UserRepository.Setup(x => x.UserByEmail(request.Email))
            .ReturnsAsync((User)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => _service.LoginByApplication(request));
    }

    #endregion

    #region RefreshToken Tests

    [Fact]
    public async Task RefreshToken_WithValidToken_ShouldReturnNewTokens()
    {
        // Arrange
        var refreshToken = "validRefreshToken";
        var user = UserEntityFaker.Generate();
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpires = DateTime.UtcNow.AddDays(7);
        var newRefreshToken = "newRefreshToken";
        var newRefreshExpiration = DateTime.UtcNow.AddDays(7);
        var newAccessToken = "newAccessToken";
        var newAccessExpiration = DateTime.UtcNow.AddHours(1);
        var claims = new List<Claim>();

        _tokenService.Setup(x => x.GetUserByToken())
            .ReturnsAsync(user);
        _tokenService.Setup(x => x.GenerateRefreshToken())
            .Returns(newRefreshToken);
        _tokenService.Setup(x => x.GenerateRefreshTokenExpiration())
            .Returns(newRefreshExpiration);
        _tokenService.Setup(x => x.GetTokenClaims())
            .Returns(claims);
        _tokenService.Setup(x => x.GenerateToken(user.Id, user.UserName, claims))
            .Returns((newAccessToken, newAccessExpiration));

        // Act
        var result = await _service.RefreshToken(refreshToken);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().Be(newAccessToken);
        result.RefreshToken.Should().Be(newRefreshToken);
        result.AccessExpiresAt.Should().Be(newAccessExpiration);
        result.RefreshExpiresAt.Should().Be(newRefreshExpiration);
        _uowMock.GenericRepository.Verify(x => x.Update<User>(user), Times.Once);
        _uow.Verify(x => x.Commit(), Times.Once);
    }

    [Fact]
    public async Task RefreshToken_WithNullRefreshToken_ShouldThrowRequestException()
    {
        // Arrange
        var refreshToken = "someToken";
        var user = UserEntityFaker.Generate();
        user.RefreshToken = null;

        _tokenService.Setup(x => x.GetUserByToken())
            .ReturnsAsync(user);

        // Act & Assert
        await Assert.ThrowsAsync<RequestException>(
            () => _service.RefreshToken(refreshToken));
    }

    [Fact]
    public async Task RefreshToken_WithMismatchedToken_ShouldThrowRequestException()
    {
        // Arrange
        var refreshToken = "providedToken";
        var user = UserEntityFaker.Generate();
        user.RefreshToken = "differentToken";

        _tokenService.Setup(x => x.GetUserByToken())
            .ReturnsAsync(user);

        // Act & Assert
        await Assert.ThrowsAsync<RequestException>(
            () => _service.RefreshToken(refreshToken));
    }

    #endregion

    #region ResetPasswordRequest Tests

    [Fact]
    public async Task ResetPasswordRequest_WithValidEmail_ShouldSendResetEmail()
    {
        // Arrange
        var email = "test@example.com";
        var resetEndpoint = "/reset-password";
        var user = UserEntityFaker.Generate();
        user.Email = email;
        var resetToken = "resetToken123";
        var encodedToken = "ZW5jb2RlZFRva2Vu";
        var frontendUri = "https://example.com/reset";
        var emailTemplate = "<html>Reset Password</html>";

        _uowMock.UserRepository.Setup(x => x.UserByEmail(email))
            .ReturnsAsync(user);
        _userManager.Setup(x => x.GeneratePasswordResetTokenAsync(user))
            .ReturnsAsync(resetToken);
        _configuration.Setup(x => x.GetSection("frontend:uri").Value)
            .Returns(frontendUri);
        _emailService.Setup(x => x.RenderResetPassword(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(emailTemplate);
        _emailService.Setup(x => x.SendEmail(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.ResetPasswordRequest(email, resetEndpoint);

        // Assert
        _userManager.Verify(x => x.GeneratePasswordResetTokenAsync(user), Times.Once);
        _emailService.Verify(x => x.SendEmail(
            user.Email, user.UserName, emailTemplate, It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task ResetPasswordRequest_WithNonExistentUser_ShouldThrowNotFoundException()
    {
        // Arrange
        var email = "nonexistent@example.com";
        var resetEndpoint = "/reset-password";

        _uowMock.UserRepository.Setup(x => x.UserByEmail(email))
            .ReturnsAsync((User)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => _service.ResetPasswordRequest(email, resetEndpoint));
    }

    #endregion

    #region ResetUserPassword Tests

    [Fact]
    public async Task ResetUserPassword_WithValidToken_ShouldResetPassword()
    {
        // Arrange
        var email = "test@example.com";
        var token = "dmFsaWRUb2tlbg=="; // Base64 encoded
        var newPassword = "NewPassword123!";
        var user = UserEntityFaker.Generate();
        user.Email = email;
        var hashedPassword = "hashedNewPassword";

        _uowMock.UserRepository.Setup(x => x.UserByEmail(email))
            .ReturnsAsync(user);
        _userManager.Setup(x => x.ResetPasswordAsync(user, It.IsAny<string>(), newPassword))
            .ReturnsAsync(IdentityResult.Success);
        _cryptographyService.Setup(x => x.HashPassword(It.IsAny<string>()))
            .Returns(hashedPassword);

        // Act
        await _service.ResetUserPassword(email, token, newPassword);

        // Assert
        _userManager.Verify(x => x.ResetPasswordAsync(user, It.IsAny<string>(), newPassword), Times.Once);
        user.PasswordHash.Should().Be(hashedPassword);
        _uowMock.GenericRepository.Verify(x => x.Update<User>(user), Times.Once);
        _uow.Verify(x => x.Commit(), Times.Once);
    }

    [Fact]
    public async Task ResetUserPassword_WithNonExistentUser_ShouldThrowNotFoundException()
    {
        // Arrange
        var email = "nonexistent@example.com";
        var token = "dG9rZW4=";
        var newPassword = "NewPassword123!";

        _uowMock.UserRepository.Setup(x => x.UserByEmail(email))
            .ReturnsAsync((User)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => _service.ResetUserPassword(email, token, newPassword));
    }

    [Fact]
    public async Task ResetUserPassword_WithInvalidToken_ShouldThrowRequestException()
    {
        // Arrange
        var email = "test@example.com";
        var token = "aW52YWxpZFRva2Vu";
        var newPassword = "NewPassword123!";
        var user = UserEntityFaker.Generate();
        user.Email = email;

        var errors = new List<IdentityError>
        {
            new IdentityError { Description = "Invalid token" }
        };

        _uowMock.UserRepository.Setup(x => x.UserByEmail(email))
            .ReturnsAsync(user);
        _userManager.Setup(x => x.ResetPasswordAsync(user, It.IsAny<string>(), newPassword))
            .ReturnsAsync(IdentityResult.Failed(errors.ToArray()));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RequestException>(
            () => _service.ResetUserPassword(email, token, newPassword));
        exception.Message.Should().Contain("Reset password failed");
    }

    #endregion

    #region RevokeToken Tests

    [Fact]
    public async Task RevokeToken_ShouldClearRefreshToken()
    {
        // Arrange
        var user = UserEntityFaker.Generate();
        user.RefreshToken = "someRefreshToken";
        user.RefreshTokenExpires = DateTime.UtcNow.AddDays(7);

        _tokenService.Setup(x => x.GetUserByToken())
            .ReturnsAsync(user);

        // Act
        await _service.RevokeToken();

        // Assert
        user.RefreshToken.Should().BeNull();
        user.RefreshTokenExpires.Should().BeNull();
        _uowMock.GenericRepository.Verify(x => x.Update<User>(user), Times.Once);
        _uow.Verify(x => x.Commit(), Times.Once);
    }

    #endregion
}