using BierDex.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace BierDex.Tests.UnitTests;

[TestFixture]
public class ImageServiceTests
{
    private Mock<IWebHostEnvironment> _mockEnvironment;
    private ImageService _imageService;
    private string _testWebRootPath;

    [SetUp]
    public void Setup()
    {
        _mockEnvironment = new Mock<IWebHostEnvironment>();

        _testWebRootPath = Path.Combine(Path.GetTempPath(), "BierDexTestWebRoot_" + Guid.NewGuid());
        _mockEnvironment.Setup(m => m.WebRootPath).Returns(_testWebRootPath);

        _imageService = new ImageService(_mockEnvironment.Object);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_testWebRootPath))
        {
            Directory.Delete(_testWebRootPath, true);
        }
    }

    private IFormFile CreateMockFormFile(string fileName, long sizeInBytes, string content = "fake image data")
    {
        var stream = new MemoryStream();

        var writer = new StreamWriter(stream, Encoding.UTF8, leaveOpen: true);
        writer.Write(content);
        writer.Flush();

        if (sizeInBytes > stream.Length)
        {
            stream.SetLength(sizeInBytes);
        }

        stream.Position = 0;

        return new FormFile(stream, 0, stream.Length, "file", fileName);
    }

    [Test]
    public async Task UploadImageAsync_ValidFile_ShouldUploadSuccessfullyAndReturnPath()
    {
        var file = CreateMockFormFile("hertog_jan.png", 1024);
        string folderName = "beers";

        var resultPath = await _imageService.UploadImageAsync(file, folderName);

        Assert.That(resultPath, Does.StartWith($"/uploads/{folderName}/"));
        Assert.That(resultPath, Does.EndWith(".png"));

        var absoluteExpectedPath = Path.Combine(_testWebRootPath, resultPath.TrimStart('/'));
        Assert.That(File.Exists(absoluteExpectedPath), Is.True);
    }

    [Test]
    public void UploadImageAsync_FileIsNull_ShouldThrowArgumentException()
    {
        var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
            await _imageService.UploadImageAsync(null!, "beers"));

        Assert.That(ex.Message, Does.Contain("Bestand is leeg"));
    }

    [Test]
    public void UploadImageAsync_InvalidExtension_ShouldThrowInvalidOperationException()
    {
        var file = CreateMockFormFile("hacker_script.exe", 1024);

        var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _imageService.UploadImageAsync(file, "beers"));

        Assert.That(ex.Message, Does.Contain("Ongeldig bestandstype"));
    }

    [Test]
    public void UploadImageAsync_FileTooLarge_ShouldThrowInvalidOperationException()
    {
        long sixMegabytes = 6 * 1024 * 1024;
        var file = CreateMockFormFile("huge_beer_picture.jpg", sixMegabytes);

        var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _imageService.UploadImageAsync(file, "beers"));

        Assert.That(ex.Message, Does.Contain("Bestand is te groot"));
    }
}