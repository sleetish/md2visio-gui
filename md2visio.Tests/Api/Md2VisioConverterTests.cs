using md2visio.Api;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            Assert.Contains("Input file does not exist", result.ErrorMessage);
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
                Assert.Equal("Input file must be in .md format", result.ErrorMessage);
            }
            finally
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
                if (File.Exists(txtFile)) File.Delete(txtFile);
            }
        }

        [Fact]
        public void CollectOutputFiles_ReturnsOnlyGeneratedFiles()
        {
            // Arrange
            var converter = new Md2VisioConverter();
            var request = ConversionRequest.Create("input.md", "output_dir");
            var context = new ConversionContext(request);

            var file1 = "output_dir/file1.vsdx";
            var file2 = "output_dir/file2.vsdx";

            context.AddGeneratedFile(file1);
            context.AddGeneratedFile(file2);

            // Act
            var result = converter.CollectOutputFiles(context);

            // Assert
            Assert.Equal(2, result.Length);
            Assert.Contains(file1, result);
            Assert.Contains(file2, result);
        }
    }
}
