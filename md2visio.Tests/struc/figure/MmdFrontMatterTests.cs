using md2visio.struc.figure;
using Xunit;
using System;
using System.Text;
using YamlDotNet.Core;

namespace md2visio.Tests.struc.figure
{
    public class MmdFrontMatterTests
    {
        [Fact]
        public void LoadYaml_WithSimpleYaml_Works()
        {
            var fm = new MmdFrontMatter();
            fm.LoadYaml("key: value");
            Assert.Equal("value", fm["key"]);
        }

        [Fact]
        public void LoadYaml_WithYamlBomb_ShouldThrowYamlException()
        {
            // Construct a YAML bomb (Billion Laughs)
            var sb = new StringBuilder();
            sb.AppendLine("a: &a [\"lol\",\"lol\",\"lol\",\"lol\",\"lol\",\"lol\",\"lol\",\"lol\",\"lol\"]");
            sb.AppendLine("b: &b [*a,*a,*a,*a,*a,*a,*a,*a,*a]");
            sb.AppendLine("c: &c [*b,*b,*b,*b,*b,*b,*b,*b,*b]");
            sb.AppendLine("d: &d [*c,*c,*c,*c,*c,*c,*c,*c,*c]");
            sb.AppendLine("e: &e [*d,*d,*d,*d,*d,*d,*d,*d,*d]");
            sb.AppendLine("f: &f [*e,*e,*e,*e,*e,*e,*e,*e,*e]");
            sb.AppendLine("g: &g [*f,*f,*f,*f,*f,*f,*f,*f,*f]");
            sb.AppendLine("h: &h [*g,*g,*g,*g,*g,*g,*g,*g,*g]");
            sb.AppendLine("i: &i [*h,*h,*h,*h,*h,*h,*h,*h,*h]"); // 9^9 elements

            string yaml = sb.ToString();

            var fm = new MmdFrontMatter();

            // We assert that loading this YAML throws a YamlException because we will
            // configure the deserializer to disallow aliases or limit recursion.
            // Before the fix, this would NOT throw, and subsequent usage (like ToString) would crash.

            // For now, let's just see if it throws.
             Assert.Throws<YamlException>(() => fm.LoadYaml(yaml));
        }
    }
}
