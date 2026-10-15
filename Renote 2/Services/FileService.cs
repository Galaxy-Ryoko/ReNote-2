using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Microsoft.UI.Xaml;

namespace Renote_2.Services
{
    /// <summary>
    /// Service for file I/O operations including open, save, and encoding detection
    /// </summary>
    public class FileService
    {
        private const int MaxFileSize = 100 * 1024 * 1024; // 100 MB limit for performance

        /// <summary>
        /// Reads text content from a file path with specified encoding.
        /// </summary>
        public async Task<string> ReadFileAsync(string filePath, Encoding encoding)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return string.Empty;

            try
            {
                var fileInfo = new FileInfo(filePath);
                if (!fileInfo.Exists)
                    return string.Empty;

                if (fileInfo.Length > MaxFileSize)
                {
                    throw new InvalidOperationException($"File size ({fileInfo.Length} bytes) exceeds maximum limit ({MaxFileSize} bytes)");
                }

                await using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var reader = new StreamReader(stream, encoding);
                return await reader.ReadToEndAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to read file: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Writes text content to a file path with specified encoding.
        /// </summary>
        public async Task WriteFileAsync(string filePath, string content, Encoding encoding)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath));

            try
            {
                var directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory))
                    Directory.CreateDirectory(directory);

                await using var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
                using var writer = new StreamWriter(stream, encoding);
                await writer.WriteAsync(content);
                await writer.FlushAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to write file: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Detects the encoding of a file path by reading the BOM (Byte Order Mark)
        /// Falls back to UTF8 if no BOM is detected
        /// </summary>
        public async Task<Encoding> DetectEncodingAsync(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                return Encoding.UTF8;

            try
            {
                await using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                var buffer = new byte[4];
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

                if (bytesRead >= 3)
                {
                    // Check for UTF-8 BOM
                    if (buffer[0] == 0xEF && buffer[1] == 0xBB && buffer[2] == 0xBF)
                        return Encoding.UTF8;
                }

                if (bytesRead >= 4)
                {
                    // Check for UTF-32 LE BOM
                    if (buffer[0] == 0xFF && buffer[1] == 0xFE && buffer[2] == 0x00 && buffer[3] == 0x00)
                        return Encoding.UTF32; // LE
                }

                if (bytesRead >= 2)
                {
                    // Check for UTF-16 LE BOM
                    if (buffer[0] == 0xFF && buffer[1] == 0xFE)
                        return Encoding.Unicode; // UTF-16 LE

                    // Check for UTF-16 BE BOM
                    if (buffer[0] == 0xFE && buffer[1] == 0xFF)
                        return Encoding.BigEndianUnicode; // UTF-16 BE
                }

                // Check for UTF-8 without BOM by sampling content
                if (bytesRead > 0)
                {
                    // For now, default to UTF8 if no BOM found
                    return Encoding.UTF8;
                }
            }
            catch
            {
                // On any error, default to UTF8
                return Encoding.UTF8;
            }

            return Encoding.UTF8;
        }

        /// <summary>
        /// Opens a file picker for user to select a text file
        /// </summary>
        public async Task<(StorageFile? file, Encoding encoding)> OpenFileAsync(Window? owner = null)
        {
            var picker = new FileOpenPicker();
            InitializePicker(picker, owner);

            picker.FileTypeFilter.Add(".txt");
            picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;

            var file = await picker.PickSingleFileAsync();
            if (file == null)
                return (null, Encoding.UTF8);

            // Detect encoding
            var encoding = await DetectEncodingAsync(file);
            return (file, encoding);
        }

        /// <summary>
        /// Opens a save file picker for user to select save location
        /// </summary>
        public async Task<StorageFile?> SaveAsFileAsync(Window? owner = null)
        {
            var picker = new FileSavePicker();
            InitializePicker(picker, owner);

            picker.FileTypeChoices.Add("Text Document", new List<string> { ".txt" });
            picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            picker.SuggestedFileName = "document.txt";

            return await picker.PickSaveFileAsync();
        }

        /// <summary>
        /// Reads text content from a file with specified encoding.
        /// </summary>
        public Task<string> ReadFileAsync(StorageFile file, Encoding encoding)
        {
            if (file == null)
                return Task.FromResult(string.Empty);

            return ReadFileAsync(file.Path, encoding);
        }

        /// <summary>
        /// Writes text content to a file with specified encoding.
        /// </summary>
        public Task WriteFileAsync(StorageFile file, string content, Encoding encoding)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));

            return WriteFileAsync(file.Path, content, encoding);
        }

        /// <summary>
        /// Detects the encoding of a file by reading the BOM (Byte Order Mark)
        /// Falls back to UTF8 if no BOM is detected
        /// </summary>
        public Task<Encoding> DetectEncodingAsync(StorageFile file)
        {
            if (file == null)
                return Task.FromResult(Encoding.UTF8);

            return DetectEncodingAsync(file.Path);
        }

        private static void InitializePicker(object picker, Window? owner)
        {
            var window = owner ?? (Application.Current as App)?.Window;
            if (window == null)
                return;

            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
        }
    }
}
