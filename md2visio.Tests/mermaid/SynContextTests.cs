using md2visio.mermaid.cmn;
using System.Text.RegularExpressions;
using Xunit;

namespace md2visio.Tests.mermaid
{
    public class SynContextTests
    {
        [Fact]
        public void Expect_ShouldWorkWithNormalInput()
        {
            var ctx = new SynContext();
            ctx.Restore("abc def");

            bool result = ctx.Expect("abc");

            Assert.True(result);
            Assert.Equal(" def", ctx.Incoming.ToString());
        }

        [Fact]
        public void Until_ShouldWorkWithNormalInput()
        {
            var ctx = new SynContext();
            ctx.Restore("prefix target suffix");

            bool result = ctx.Until("target");

            Assert.True(result);
            // Until consumes prefix + target. "target" is matched by regex so it should consume it.
            // Let's verify consumed length logic in code:
            // incoming.Remove(0, match.Index + match.Length);
            // match is against "^(?<head>.*?)(?<tail>{pattern})"
            // So it consumes head + tail.

            Assert.Equal(" suffix", ctx.Incoming.ToString());
        }

        [Fact]
        public void Test_ShouldWorkWithNormalInput()
        {
            var ctx = new SynContext();
            ctx.Restore("some pattern to match");

            bool result = ctx.Test("pattern");

            Assert.True(result);
        }
    }
}
