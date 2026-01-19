using md2visio.mermaid.cmn;
using System.Text;
using System.Text.RegularExpressions;

namespace md2visio.mermaid.er
{
    /// <summary>
    /// ER图实体体解析状态类
    /// 解析 { ... } 块内的属性定义
    /// 格式: type name [PK|FK|UK] ["comment"]
    /// </summary>
    internal class ErSttEntityBody : SynState
    {
        // 属性行模式: type name [keys] ["comment"]
        static readonly Regex regAttribute = new(
            @"^\s*(?<type>\S+)\s+(?<name>\S+)(?:\s+(?<keys>(?:PK|FK|UK)(?:\s*,\s*(?:PK|FK|UK))*))?(?:\s+""(?<comment>[^""]*)"")?\s*$",
            RegexOptions.Compiled);

        public override SynState NextState()
        {
            // 跳过开始的 {
            if (Ctx.Peek() == "{")
            {
                Ctx.Take();
            }

            StringBuilder bodyContent = new();
            int braceCount = 1;

            // 读取直到匹配的 }
            while (braceCount > 0)
            {
                string? ch = Ctx.Peek();
                if (ch == null) break;

                Ctx.Take();

                if (ch == "{") braceCount++;
                else if (ch == "}") braceCount--;

                if (braceCount > 0)
                {
                    bodyContent.Append(ch);
                }
            }

            AddCompo("body", bodyContent.ToString());
            Save(bodyContent.ToString());
            ClearBuffer();

            return Forward<ErSttChar>();
        }

        /// <summary>
        /// 解析实体体中的属性列表
        /// </summary>
        public static List<(string type, string name, string keys, string comment)> ParseAttributes(string body)
        {
            var attributes = new List<(string type, string name, string keys, string comment)>();

            var lines = body.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (string.IsNullOrEmpty(trimmed)) continue;

                var match = regAttribute.Match(trimmed);
                if (match.Success)
                {
                    attributes.Add((
                        match.Groups["type"].Value,
                        match.Groups["name"].Value,
                        match.Groups["keys"].Value,
                        match.Groups["comment"].Value
                    ));
                }
                else
                {
                    // 尝试简单解析 (只有 type name)
                    var parts = trimmed.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 2)
                    {
                        attributes.Add((parts[0], parts[1], "", ""));
                    }
                }
            }

            return attributes;
        }
    }
}
