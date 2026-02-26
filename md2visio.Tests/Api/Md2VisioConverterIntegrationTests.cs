using md2visio.Api;
using md2visio.vsdx.@base;
using System;
using System.IO;
using Xunit;
using Visio = Microsoft.Office.Interop.Visio;

namespace md2visio.Tests.Api
{
    public class Md2VisioConverterIntegrationTests
    {
        class MockVisioSession : IVisioSession
        {
            public Visio.Application Application => null!;
            public bool Visible => false;
            public Visio.Document CreateDocument() => null!;
            public Visio.Document OpenStencil(string path) => null!;
            public void SaveDocument(Visio.Document doc, string path, bool overwrite = true) { }
            public void CloseDocument(Visio.Document doc) { }
            public void Dispose() { }
        }

        [Fact]
        public void Convert_WhenFactoryThrowsNotImplemented_ReturnsUnsupportedDiagramTypeMessage()
        {
            // Arrange
            var mdFile = Path.GetTempFileName();
            var mdPath = Path.ChangeExtension(mdFile, ".md");
            var outputPath = Path.ChangeExtension(mdFile, ".vsdx");

            try
            {
                // Clean up the 0-byte file created by GetTempFileName immediately as we don't use it directly
                if (File.Exists(mdFile)) File.Delete(mdFile);

                File.WriteAllText(mdPath, "```mermaid\ngraph LR\nA-->B\n```");

                // Factory throws NotImplementedException immediately
                var converter = new Md2VisioConverter((visible) => throw new NotImplementedException("Simulated not implemented error"));

                var request = ConversionRequest.Create(mdPath, outputPath);

                // Act
                var result = converter.Convert(request);

                // Assert
                Assert.False(result.Success);
                // Verify that the error message contains the NEW terminology
                Assert.Contains("Unsupported diagram type", result.ErrorMessage);
                // And verifies the inner exception message is included
                Assert.Contains("Simulated not implemented error", result.ErrorMessage);
            }
            finally
            {
                if (File.Exists(mdPath)) File.Delete(mdPath);
                if (File.Exists(outputPath)) File.Delete(outputPath);
            }
        }
    }
}
