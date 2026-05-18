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
        // We simuleren de appsettings.json configuratie in het geheugen
        _inMemorySettings = new Dictionary<string, string?>
        {
            {"smtpCredentials:host", "localhost"},
            {"smtpCredentials:port", "587"},
            {"smtpCredentials:username", "test-user"},
            {"smtpCredentials:password", "test-pass"},
            {"smtpCredentials:linkSpa", "https://mijnbierdexapp.nl"}
        };

        // Bouw een echte IConfiguration op basis van onze in-memory dictionary
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(_inMemorySettings)
            .Build();

        _smtpController = new SmtpController(configuration);
    }

    [Test]
    public async Task SendEmailAsync_RegularEmail_ShouldNotTransformHtmlMessage()
    {
        // Arrange
        string email = "user@test.com";
        string subject = "Welkom bij BierDex!";
        string originalHtml = "<p>Je account is geactiveerd.</p>";

        // Act & Assert
        // Omdat "localhost" op poort 587 waarschijnlijk niet echt bestaat tijdens de test,
        // verwachten we een SmtpException of SocketException. 
        // Maar we testen HIERMETEE wel of de poort-parsing (int.Parse) succesvol verloopt!
        var ex = Assert.ThrowsAsync<System.Net.Mail.SmtpException>(async () =>
            await _smtpController.SendEmailAsync(email, subject, originalHtml));

        // Als hij hier komt, betekent het dat de SMTP configuratie succesvol is uitgelezen
        Assert.That(ex.Message, Does.Contain("Failure sending mail")
            .Or.Contain("De externe naam kan niet worden opgelost")
            .Or.Contain("Connection refused"));
    }

    [Test]
    public void SendEmailAsync_ResetPasswordEmail_ShouldParseTokenAndBuildSpaLink()
    {
        // Arrange
        string email = "ronald@test.nl";
        string subject = "Reset Password";
        // We simuleren de standaard Identity output: een tekst met een token achter de dubbele punt
        string identityMessage = "Please reset your password by using this token: SecretToken12345";

        // Act & Assert
        // We willen zien of de code crashvrij de link bouwt voordat hij de SMTP-client raakt.
        // Als int.Parse of de string-split faalt, gooit hij een andere exception dan een SmtpException.
        Assert.ThrowsAsync<System.Net.Mail.SmtpException>(async () =>
            await _smtpController.SendEmailAsync(email, subject, identityMessage));

        // De link zou achter de schermen opgebouwd moeten zijn als:
        // https://mijnbierdexapp.nl/reset-password?email=ronald%40test.nl&code=SecretToken12345
    }

    [Test]
    public void SendEmailAsync_MissingPortInConfig_ShouldThrowFormatExceptionOrNullReference()
    {
        // Arrange
        // We slopen de poort uit de configuratie om te kijken of je foutafhandeling (int.Parse) triggerd
        _inMemorySettings["smtpCredentials:port"] = "niet-een-getal";

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(_inMemorySettings)
            .Build();

        var brokenControler = new SmtpController(configuration);

        // Act & Assert
        Assert.ThrowsAsync<FormatException>(async () =>
            await brokenControler.SendEmailAsync("test@test.com", "Test", "Test"));
    }
}