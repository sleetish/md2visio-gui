using md2visio.Api;
using md2visio.Tests.Mocks;

namespace md2visio.Tests.Api
{
    public class ConversionContextTests
    {
        [Fact]
        public void Constructor_WithValidRequest_SetsProperties()
        {
            // Arrange
            var request = ConversionRequest.Create("input.md", "output.vsdx")
                .WithShowVisio()
                .WithDebug();

            // Act
            var context = new ConversionContext(request);

            // Assert
            Assert.Same(request, context.Options);
            Assert.True(context.Debug);
            Assert.True(context.Visible);
            Assert.Equal("input.md", context.InputFile);
            Assert.Equal("output.vsdx", context.OutputPath);
        }

        [Fact]
        public void Constructor_WithNullRequest_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new ConversionContext(null!));
        }

        [Fact]
        public void Constructor_WithNullLogger_UsesNullLogSink()
        {
            var request = ConversionRequest.Create("input.md", "output.vsdx");
            var context = new ConversionContext(request, null);

            // Should not throw when logging
            context.Log("test message");
            context.LogInfo("info message");
        }

        [Fact]
        public void Log_WhenDebugTrue_LogsMessage()
        {
            // Arrange
            var mockLogger = new MockLogSink();
            var request = ConversionRequest.Create("input.md", "output.vsdx").WithDebug();
            var context = new ConversionContext(request, mockLogger);

            // Act
            context.Log("Test debug message");

            // Assert
            Assert.True(mockLogger.HasDebugMessages);
            Assert.Contains("Test debug message", mockLogger.GetMessages("DEBUG"));
        }

        [Fact]
        public void Log_WhenDebugFalse_DoesNotLogMessage()
        {
            // Arrange
            var mockLogger = new MockLogSink();
            var request = ConversionRequest.Create("input.md", "output.vsdx"); // Debug = false
            var context = new ConversionContext(request, mockLogger);

            // Act
            context.Log("Test debug message");

            // Assert
            Assert.False(mockLogger.HasDebugMessages);
        }

        [Fact]
        public void LogInfo_AlwaysLogsMessage()
        {
            // Arrange
            var mockLogger = new MockLogSink();
            var request = ConversionRequest.Create("input.md", "output.vsdx");
            var context = new ConversionContext(request, mockLogger);

            // Act
            context.LogInfo("Test info message");

            // Assert
            Assert.True(mockLogger.HasInfoMessages);
        }

        [Fact]
        public void LogWarning_AlwaysLogsMessage()
        {
            // Arrange
            var mockLogger = new MockLogSink();
            var request = ConversionRequest.Create("input.md", "output.vsdx");
            var context = new ConversionContext(request, mockLogger);

            // Act
            context.LogWarning("Test warning message");

            // Assert
            Assert.True(mockLogger.HasWarningMessages);
        }

        [Fact]
        public void LogError_AlwaysLogsMessage()
        {
            // Arrange
            var mockLogger = new MockLogSink();
            var request = ConversionRequest.Create("input.md", "output.vsdx");
            var context = new ConversionContext(request, mockLogger);

            // Act
            context.LogError("Test error message");

            // Assert
            Assert.True(mockLogger.HasErrorMessages);
        }

        [Fact]
        public void AddGeneratedFile_AddsFileToList()
        {
            // Arrange
            var request = ConversionRequest.Create("input.md", "output_dir");
            var context = new ConversionContext(request);
            var filePath = "output_dir/file1.vsdx";

            // Act
            context.AddGeneratedFile(filePath);

            // Assert
            Assert.Single(context.GeneratedFiles);
            Assert.Equal(filePath, context.GeneratedFiles[0]);
        }

        [Fact]
        public void AddGeneratedFile_DoesNotAddDuplicateFile()
        {
            // Arrange
            var request = ConversionRequest.Create("input.md", "output_dir");
            var context = new ConversionContext(request);
            var filePath = "output_dir/file1.vsdx";

            // Act
            context.AddGeneratedFile(filePath);
            context.AddGeneratedFile(filePath);

            // Assert
            Assert.Single(context.GeneratedFiles);
        }
    }
}
