using md2visio.mermaid.cmn;
using System.Text.RegularExpressions;

namespace md2visio.mermaid.er
{
    /// <summary>
    /// ER图关系解析状态类
    /// 解析关系符号如: ||--o{, }|..|{, ||--|{
    /// </summary>
    internal class ErSttRelation : SynState
    {
        // 完整关系符号模式
        // 左基数: ||, |o, o|, }|, }o, o{, |{
        // 线型: -- (实线/识别), .. (虚线/非识别)
        // 右基数: ||, o|, |{, o{, }|, }o
        // 修复：使用更明确的交替模式
        static readonly Regex regRelation = new(
            @"^(?<left>\|\||o\||o\{|\|o|\}\||\}o|\|{|\}\{)(?<line>--|\.\.)" +
            @"(?<right>\|\||o\||o\{|\|o|\}\||\}o|\|{|\}\{)",
            RegexOptions.Compiled);

        public override SynState NextState()
        {
            string incoming = Ctx.Incoming.ToString();
            var match = regRelation.Match(incoming);

            if (match.Success)
            {
                string relationSymbol = match.Value;

                // 消费匹配的字符
                for (int i = 0; i < relationSymbol.Length; i++)
                {
                    Ctx.Take();
                }

                // 保存关系符号到 Parts
                AddCompo("relation", relationSymbol);
                AddCompo("left", match.Groups["left"].Value);
                AddCompo("line", match.Groups["line"].Value);
                AddCompo("right", match.Groups["right"].Value);

                Save(relationSymbol);
                ClearBuffer();
            }

            return Forward<ErSttChar>();
        }

        /// <summary>
        /// 解析左基数
        /// </summary>
        public static string ParseLeftCardinality(string symbol)
        {
            var match = regRelation.Match(symbol);
            return match.Success ? match.Groups["left"].Value : "";
        }

        /// <summary>
        /// 解析右基数
        /// </summary>
        public static string ParseRightCardinality(string symbol)
        {
            var match = regRelation.Match(symbol);
            return match.Success ? match.Groups["right"].Value : "";
        }

        /// <summary>
        /// 检查是否是识别关系 (实线)
        /// </summary>
        public static bool IsIdentifying(string symbol)
        {
            return symbol.Contains("--");
        }
    }
}
