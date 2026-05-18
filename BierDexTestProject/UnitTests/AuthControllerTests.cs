using API.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BierDex.Tests.UnitTests;

[TestFixture]
public class AuthControllerTests
{
    private Mock<UserManager<IdentityUser>> _mockUserManager;
    private Mock<SignInManager<IdentityUser>> _mockSignInManager;
    private Mock<IEmailSender> _mockEmailSender;
    private AuthController _controller;
    private IdentityUser _testUser;

    [SetUp]
    public void Setup()
    {
        // 1. Setup Mock UserManager
        var userStoreMock = new Mock<IUserStore<IdentityUser>>();
        _mockUserManager = new Mock<UserManager<IdentityUser>>(
            userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        // 2. Setup Mock SignInManager
        var contextAccessorMock = new Mock<IHttpContextAccessor>();
        var userClaimsPrincipalFactoryMock = new Mock<IUserClaimsPrincipalFactory<IdentityUser>>();
        _mockSignInManager = new Mock<SignInManager<IdentityUser>>(
            _mockUserManager.Object,
            contextAccessorMock.Object,
            userClaimsPrincipalFactoryMock.Object,
            null!, null!, null!, null!);

        // 3. Setup Mock EmailSender
        _mockEmailSender = new Mock<IEmailSender>();

        _testUser = new IdentityUser { Id = "user-123", UserName = "Ronald", Email = "ronald@bierdex.nl" };

        // 4. Instantieer Controller
        _controller = new AuthController(_mockSignInManager.Object, _mockUserManager.Object, _mockEmailSender.Object);

        // 5. Mock HttpContext voor de ingelogde User claims principal
        var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, "user-123") };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };
    }

    [Test]
    public async Task GetStatus_UserAuthenticated_ReturnsUserInfoAndRoles()
    {
        // Arrange
        _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(_testUser);
        _mockUserManager.Setup(um => um.GetRolesAsync(_testUser)).ReturnsAsync(new List<string> { "User" });

        // Act
        var result = await _controller.GetStatus();

        // Assert
        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);

        dynamic data = okResult.Value!;
        Assert.That(data.GetType().GetProperty("isAuthenticated")?.GetValue(data), Is.True);
        Assert.That(data.GetType().GetProperty("name")?.GetValue(data), Is.EqualTo("Ronald"));
    }

    [Test]
    public async Task GetStatus_UserNotFound_ReturnsUnauthorized()
    {
        // Arrange
        _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync((IdentityUser)null!);

        // Act
        var result = await _controller.GetStatus();

        // Assert
        Assert.That(result, Is.InstanceOf<UnauthorizedResult>());
    }

    [Test]
    public async Task EmailLogin_ValidCredentials_ReturnsOk()
    {
        // Arrange
        var request = new EmailLoginRequest("ronald@bierdex.nl", "Wachtwoord123!");
        _mockUserManager.Setup(um => um.FindByEmailAsync(request.Email)).ReturnsAsync(_testUser);

        _mockSignInManager.Setup(sm => sm.PasswordSignInAsync(_testUser.UserName!, request.Password, request.RememberMe, false))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

        // Act
        var result = await _controller.EmailLogin(request);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
    }

    [Test]
    public async Task EmailLogin_InvalidUser_ReturnsUnauthorized()
    {
        // Arrange
        var request = new EmailLoginRequest("onbekend@bierdex.nl", "FoutWachtwoord");
        _mockUserManager.Setup(um => um.FindByEmailAsync(request.Email)).ReturnsAsync((IdentityUser)null!);

        // Act
        var result = await _controller.EmailLogin(request);

        // Assert
        var unauthorizedResult = result as UnauthorizedObjectResult;
        Assert.That(unauthorizedResult, Is.Not.Null);
        Assert.That(unauthorizedResult.Value, Is.EqualTo("Ongeldige inloggegevens."));
    }

    [Test]
    public async Task ChangeUsername_ValidRequest_ReturnsOk()
    {
        // Arrange
        var request = new ChangeUsernameRequest("NieuweNaam");
        _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(_testUser);
        _mockUserManager.Setup(um => um.SetUserNameAsync(_testUser, request.NewUsername)).ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _controller.ChangeUsername(request);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        _mockSignInManager.Verify(sm => sm.RefreshSignInAsync(_testUser), Times.Once);
    }

    [Test]
    public async Task CreateUser_AdminCreatesValidUser_GeneratesTokenAndSendsEmail()
    {
        // Arrange
        var dto = new UserRegisterDto("nieuwbrouwer@bierdex.nl", "BrouwerX", "Supplier", null);

        _mockUserManager.Setup(um => um.CreateAsync(It.IsAny<IdentityUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        _mockUserManager.Setup(um => um.GeneratePasswordResetTokenAsync(It.IsAny<IdentityUser>()))
            .ReturnsAsync("MockedResetToken12345");

        // Act
        var result = await _controller.CreateUser(dto);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);

        // Gecorrigeerd: Nette C# lambda om te controleren of de body het token bevat
        _mockEmailSender.Verify(es => es.SendEmailAsync(
            "nieuwbrouwer@bierdex.nl",
            "Reset Password",
            It.Is<string>(body => body.Contains("MockedResetToken12345"))),
            Times.Once);
    }
}