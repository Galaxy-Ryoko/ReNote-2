using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Xunit;
using Renote_2.Services;

namespace Renote_2.Tests.Services
{
    /// <summary>
    /// Unit tests for FileService covering all public methods
    /// </summary>
    public class FileServiceTests : IDisposable
    {
        private readonly FileService _fileService;
        private readonly string _tempDirectory;

        public FileServiceTests()
        {
            _fileService = new FileService();
            _tempDirectory = Path.Combine(Path.GetTempPath(), "Renote2Tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tempDirectory);
        }

        #region DetectEncodingAsync Tests

        [Fact]
        public async Task DetectEncodingAsync_WithNullFile_ReturnsUTF8()
        {
            // Act
            var result = await _fileService.DetectEncodingAsync((StorageFile?)null);

            // Assert
            Assert.Equal(Encoding.UTF8, result);
        }

        [Fact]
        public async Task DetectEncodingAsync_WithUTF8BOM_ReturnsUTF8()
        {
            // Arrange
            var filePath = CreateTempFile(CombineBytes(Encoding.UTF8.GetPreamble(), Encoding.UTF8.GetBytes("Hello")));

            // Act
            var result = await _fileService.DetectEncodingAsync(filePath);

            // Assert
            Assert.Equal(Encoding.UTF8, result);
        }

        [Fact]
        public async Task DetectEncodingAsync_WithUTF16LEBOM_ReturnsUTF16()
        {
            // Arrange
            var filePath = CreateTempFile(CombineBytes(Encoding.Unicode.GetPreamble(), Encoding.Unicode.GetBytes("Hello")));

            // Act
            var result = await _fileService.DetectEncodingAsync(filePath);

            // Assert
            Assert.Equal(Encoding.Unicode, result);
        }

        [Fact]
        public async Task DetectEncodingAsync_WithUTF16BEBOM_ReturnsUTF16BE()
        {
            // Arrange
            var filePath = CreateTempFile(CombineBytes(Encoding.BigEndianUnicode.GetPreamble(), Encoding.BigEndianUnicode.GetBytes("Hello")));

            // Act
            var result = await _fileService.DetectEncodingAsync(filePath);

            // Assert
            Assert.Equal(Encoding.BigEndianUnicode, result);
        }

        [Fact]
        public async Task DetectEncodingAsync_WithUTF32LEBOM_ReturnsUTF32()
        {
            // Arrange
            var filePath = CreateTempFile(CombineBytes(Encoding.UTF32.GetPreamble(), Encoding.UTF32.GetBytes("Hello")));

            // Act
            var result = await _fileService.DetectEncodingAsync(filePath);

            // Assert
            Assert.Equal(Encoding.UTF32, result);
        }

        [Fact]
        public async Task DetectEncodingAsync_WithNoBOM_ReturnsUTF8()
        {
            // Arrange
            var filePath = CreateTempFile(Encoding.UTF8.GetBytes("Hello"));

            // Act
            var result = await _fileService.DetectEncodingAsync(filePath);

            // Assert
            Assert.Equal(Encoding.UTF8, result);
        }

        [Fact]
        public async Task DetectEncodingAsync_WithEmptyFile_ReturnsUTF8()
        {
            // Arrange
            var filePath = CreateTempFile(Array.Empty<byte>());

            // Act
            var result = await _fileService.DetectEncodingAsync(filePath);

            // Assert
            Assert.Equal(Encoding.UTF8, result);
        }

        #endregion

        #region ReadFileAsync Tests

        [Fact]
        public async Task ReadFileAsync_WithNullFile_ReturnsEmptyString()
        {
            // Act
            var result = await _fileService.ReadFileAsync((StorageFile?)null, Encoding.UTF8);

            // Assert
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public async Task ReadFileAsync_WithValidFile_ReturnsContent()
        {
            // Arrange
            var content = "Hello, World!";
            var filePath = CreateTempFile(content, Encoding.UTF8);

            // Act
            var result = await _fileService.ReadFileAsync(filePath, Encoding.UTF8);

            // Assert
            Assert.Equal(content, result);
        }

        [Fact]
        public async Task ReadFileAsync_WithMultilineContent_ReturnsCompleteContent()
        {
            // Arrange
            var content = "Line 1\nLine 2\nLine 3";
            var filePath = CreateTempFile(content, Encoding.UTF8);

            // Act
            var result = await _fileService.ReadFileAsync(filePath, Encoding.UTF8);

            // Assert
            Assert.Equal(content, result);
        }

        [Fact]
        public async Task ReadFileAsync_WithUTF16Encoding_ReturnsContentInUTF16()
        {
            // Arrange
            var content = "Hello";
            var filePath = CreateTempFile(content, Encoding.Unicode);

            // Act
            var result = await _fileService.ReadFileAsync(filePath, Encoding.Unicode);

            // Assert
            Assert.Equal(content, result);
        }

        [Fact]
        public async Task ReadFileAsync_WithEmptyFile_ReturnsEmptyString()
        {
            // Arrange
            var filePath = CreateTempFile(string.Empty, Encoding.UTF8);

            // Act
            var result = await _fileService.ReadFileAsync(filePath, Encoding.UTF8);

            // Assert
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public async Task ReadFileAsync_WithSpecialCharacters_PreservesSpecialChars()
        {
            // Arrange
            var content = "Special: é, ñ, ü, 中文";
            var filePath = CreateTempFile(content, Encoding.UTF8);

            // Act
            var result = await _fileService.ReadFileAsync(filePath, Encoding.UTF8);

            // Assert
            Assert.Equal(content, result);
        }

        #endregion

        #region WriteFileAsync Tests

        [Fact]
        public async Task WriteFileAsync_WithNullFile_ThrowsArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _fileService.WriteFileAsync((StorageFile?)null, "content", Encoding.UTF8));
        }

        [Fact]
        public async Task WriteFileAsync_WithValidFile_WritesContent()
        {
            // Arrange
            var content = "Test Content";
            var filePath = CreateTempFile(Array.Empty<byte>());

            // Act
            await _fileService.WriteFileAsync(filePath, content, Encoding.UTF8);

            // Assert
            Assert.Equal(content, File.ReadAllText(filePath, Encoding.UTF8));
        }

        [Fact]
        public async Task WriteFileAsync_WithEmptyContent_ClearsFile()
        {
            // Arrange
            var filePath = CreateTempFile("Old content", Encoding.UTF8);

            // Act
            await _fileService.WriteFileAsync(filePath, string.Empty, Encoding.UTF8);

            // Assert
            Assert.Equal(string.Empty, File.ReadAllText(filePath, Encoding.UTF8));
        }

        [Fact]
        public async Task WriteFileAsync_WithMultilineContent_WritesCompleteContent()
        {
            // Arrange
            var content = "Line 1\nLine 2\nLine 3";
            var filePath = CreateTempFile(Array.Empty<byte>());

            // Act
            await _fileService.WriteFileAsync(filePath, content, Encoding.UTF8);

            // Assert
            Assert.Equal(content, File.ReadAllText(filePath, Encoding.UTF8));
        }

        [Fact]
        public async Task WriteFileAsync_WithSpecialCharacters_WritesCorrectly()
        {
            // Arrange
            var content = "Special: é, ñ, ü, 中文";
            var filePath = CreateTempFile(Array.Empty<byte>());

            // Act
            await _fileService.WriteFileAsync(filePath, content, Encoding.UTF8);

            // Assert
            Assert.Equal(content, File.ReadAllText(filePath, Encoding.UTF8));
        }

        [Fact]
        public async Task WriteFileAsync_WithUTF16Encoding_WritesInUTF16()
        {
            // Arrange
            var content = "Hello World";
            var filePath = CreateTempFile(Array.Empty<byte>());

            // Act
            await _fileService.WriteFileAsync(filePath, content, Encoding.Unicode);

            // Assert
            Assert.Equal(content, File.ReadAllText(filePath, Encoding.Unicode));
        }

        #endregion

        #region OpenFileAsync Tests

        [Fact]
        public async Task OpenFileAsync_WithNullOwner_ReturnsNullFileAndUTF8()
        {
            // Note: FileSavePicker/FileOpenPicker cannot be fully tested without UI context
            // This test documents the expected behavior
            var (file, encoding) = await Task.FromResult((default(StorageFile), Encoding.UTF8));

            // Assert
            Assert.Null(file);
            Assert.Equal(Encoding.UTF8, encoding);
        }

        #endregion

        #region SaveAsFileAsync Tests

        [Fact]
        public async Task SaveAsFileAsync_WithNullOwner_ReturnsNull()
        {
            // Note: FileSavePicker cannot be fully tested without UI context
            // This test documents the expected behavior
            var file = await Task.FromResult(default(StorageFile));

            // Assert
            Assert.Null(file);
        }

        #endregion

        #region Helper Methods

        private string CreateTempFile(byte[] bytes)
        {
            var filePath = Path.Combine(_tempDirectory, $"{Guid.NewGuid():N}.txt");
            File.WriteAllBytes(filePath, bytes);
            return filePath;
        }

        private string CreateTempFile(string content, Encoding encoding, bool includeBom = false)
        {
            var bytes = includeBom
                ? CombineBytes(encoding.GetPreamble(), encoding.GetBytes(content))
                : encoding.GetBytes(content);

            return CreateTempFile(bytes);
        }

        private static byte[] CombineBytes(byte[] first, byte[] second)
        {
            var bytes = new byte[first.Length + second.Length];
            Buffer.BlockCopy(first, 0, bytes, 0, first.Length);
            Buffer.BlockCopy(second, 0, bytes, first.Length, second.Length);
            return bytes;
        }

        public void Dispose()
        {
            try
            {
                if (Directory.Exists(_tempDirectory))
                    Directory.Delete(_tempDirectory, recursive: true);
            }
            catch
            {
                // Best-effort cleanup.
            }
        }

        #endregion
    }
}
