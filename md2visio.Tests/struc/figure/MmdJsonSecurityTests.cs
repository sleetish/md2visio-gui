using System;
using System.Text;
using Xunit;
using md2visio.struc.figure;

namespace md2visio.Tests.struc.figure
{
    public class MmdJsonSecurityTests
    {
        [Fact]
        public void MmdJsonObj_PreventsStackOverflow()
        {
            var sb = new StringBuilder();
            for (int i = 0; i < 60; i++) sb.Append("{\"a\":");
            sb.Append("1");
            for (int i = 0; i < 60; i++) sb.Append("}");

            var exception = Record.Exception(() => new MmdJsonObj(sb.ToString()));
            Assert.NotNull(exception);
            Assert.IsType<InvalidOperationException>(exception);
        }
    }
}
