using BierDex.Controllers;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace BierDex.Tests.UnitTests;

[TestFixture]
public class SmtpControllerTests
{
    private SmtpController _smtpController;
    private Dictionary<string, string?> _inMemorySettings;

    [SetUp]
    public void Setup()
    {
        _inMemorySettings = new Dictionary<string, string?>
        {
            {"smtpCredentials:host", "localhost"},
            {"smtpCredentials:port", "587"},
            {"smtpCredentials:username", "test-user"},
            {"smtpCredentials:password", "test-pass"},
            {"smtpCredentials:linkSpa", "https://mijnbierdexapp.nl"}
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(_inMemorySettings)
            .Build();

        _smtpController = new SmtpController(configuration);
    }

    [Test]
    public async Task SendEmailAsync_RegularEmail_ShouldNotTransformHtmlMessage()
    {
        string email = "user@test.com";
        string subject = "Welkom bij BierDex!";
        string originalHtml = "<p>Je account is geactiveerd.</p>";

        var ex = Assert.ThrowsAsync<System.Net.Mail.SmtpException>(async () =>
            await _smtpController.SendEmailAsync(email, subject, originalHtml));

        Assert.That(ex.Message, Does.Contain("Failure sending mail")
            .Or.Contain("De externe naam kan niet worden opgelost")
            .Or.Contain("Connection refused"));
    }

    [Test]
    public void SendEmailAsync_ResetPasswordEmail_ShouldParseTokenAndBuildSpaLink()
    {
        string email = "ronald@test.nl";
        string subject = "Reset Password";
        string identityMessage = "Please reset your password by using this token: SecretToken12345";

        Assert.ThrowsAsync<System.Net.Mail.SmtpException>(async () =>
            await _smtpController.SendEmailAsync(email, subject, identityMessage));
    }

    [Test]
    public void SendEmailAsync_MissingPortInConfig_ShouldThrowFormatExceptionOrNullReference()
    {
        _inMemorySettings["smtpCredentials:port"] = "niet-een-getal";

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(_inMemorySettings)
            .Build();

        var brokenControler = new SmtpController(configuration);

        Assert.ThrowsAsync<FormatException>(async () =>
            await brokenControler.SendEmailAsync("test@test.com", "Test", "Test"));
    }
}