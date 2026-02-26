using Xunit;
using md2visio.struc.figure;
using md2visio.mermaid.cmn;
using md2visio.Api;
using md2visio.Tests.Mocks;
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace md2visio.Tests.struc.figure
{
    public class FigureBuilderFactorySecurityTests
    {
        [Fact]
        public void BuildFigures_WithDebugEnabled_ShouldNotLogSensitiveFragments()
        {
            // Arrange
            string sensitiveData = "SensitiveSecretPassword";
            string tempFile = Path.GetTempFileName();
            string mdContent = $@"
```mermaid
graph TD;
    A-->{sensitiveData};
```";
            File.WriteAllText(tempFile, mdContent, Encoding.UTF8);

            try
            {
                // Setup Context and Iterator
                var synContext = new SynContext(tempFile);

                // Manually populate StateList
                var stateList = synContext.GetType().GetProperty("StateList")?.GetValue(synContext) as List<SynState>;
                if (stateList == null) throw new Exception("Could not access StateList");

                stateList.Clear();
                stateList.Add(new SttMermaidStart { Fragment = "```mermaid" });
                stateList.Add(new SttFigureType { Fragment = "graph" }); // This triggers "IsFigure" check
                stateList.Add(new SynStateMock { Fragment = "TD;" });
                stateList.Add(new SynStateMock { Fragment = "A-->" });
                stateList.Add(new SynStateMock { Fragment = sensitiveData }); // This is the sensitive part
                stateList.Add(new SttMermaidClose { Fragment = "```" });

                var iterator = new SttIterator(synContext);

                // Setup Mock Dependencies
                var mockSession = new MockVisioSession();
                var mockLogSink = new MockLogSink();

                // Enable Debug to trigger the vulnerable code path
                var request = new ConversionRequest(tempFile, "out.vsdx", debug: true);
                var context = new ConversionContext(request, mockLogSink);

                var factory = new FigureBuilderFactory(iterator, context, mockSession);

                // Act
                try
                {
                    factory.BuildFigures();
                }
                catch
                {
                    // Ignore exceptions from BuildFigure (since we don't have real Visio)
                }

                // Assert
                bool logContainsSensitiveData = mockLogSink.Messages.Any(m => m.Item2.Contains(sensitiveData));

                // Security Check: Sensitive data MUST NOT be in the logs
                Assert.False(logContainsSensitiveData, "Sensitive data was found in debug logs!");
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }

        // Helper class to mock SynState since it's abstract
        private class SynStateMock : SynState
        {
            public override SynState NextState() => throw new NotImplementedException();
        }
    }
}
