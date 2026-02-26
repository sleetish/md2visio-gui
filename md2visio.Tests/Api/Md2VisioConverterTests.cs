using md2visio.Api;
using System.IO;
using Xunit;

namespace md2visio.Tests.Api
{
    public class Md2VisioConverterTests
    {
        [Fact]
        public void Convert_WithNonExistentFile_ReturnsFailedResult()
        {
            // Arrange
            var converter = new Md2VisioConverter();
            var request = ConversionRequest.Create("non-existent.md", "output.vsdx");

            // Act
            var result = converter.Convert(request);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("输入文件不存在", result.ErrorMessage);
        }

        [Fact]
        public void Convert_WithInvalidExtension_ReturnsFailedResult()
        {
            // Arrange
            var tempFile = Path.GetTempFileName();
            var txtFile = Path.ChangeExtension(tempFile, ".txt");
            try
            {
                File.WriteAllText(txtFile, "some content");
                var converter = new Md2VisioConverter();
                var request = ConversionRequest.Create(txtFile, "output.vsdx");

                // Act
                var result = converter.Convert(request);

                // Assert
                Assert.False(result.Success);
                Assert.Equal("输入文件必须是 .md 格式", result.ErrorMessage);
            }
            finally
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
                if (File.Exists(txtFile)) File.Delete(txtFile);
            }
        }
    }
}
