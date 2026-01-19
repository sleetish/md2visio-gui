using Microsoft.Office.Interop.Visio;

namespace md2visio.struc.er
{
    /// <summary>
    /// ER图关系类
    /// 表示两个实体之间的关系
    /// </summary>
    internal class ErRelation
    {
        /// <summary>
        /// 起始实体ID
        /// </summary>
        public string FromEntity { get; set; } = "";

        /// <summary>
        /// 目标实体ID
        /// </summary>
        public string ToEntity { get; set; } = "";

        /// <summary>
        /// 起始端基数
        /// </summary>
        public ErCardinality LeftCardinality { get; set; } = ErCardinality.ExactlyOne;

        /// <summary>
        /// 目标端基数
        /// </summary>
        public ErCardinality RightCardinality { get; set; } = ErCardinality.ExactlyOne;

        /// <summary>
        /// 是否是识别关系 (实线)，否则是非识别关系 (虚线)
        /// </summary>
        public bool IsIdentifying { get; set; } = true;

        /// <summary>
        /// 关系标签
        /// </summary>
        public string Label { get; set; } = "";

        /// <summary>
        /// 对应的 Visio 形状
        /// </summary>
        public Shape? VisioShape { get; set; }

        /// <summary>
        /// 解析基数符号
        /// </summary>
        public static ErCardinality ParseCardinality(string symbol)
        {
            // 规范化符号
            symbol = symbol.Trim();

            return symbol switch
            {
                "||" => ErCardinality.ExactlyOne,
                "|o" or "o|" => ErCardinality.ZeroOrOne,
                "}|" or "|{" => ErCardinality.OneOrMore,
                "}o" or "o{" => ErCardinality.ZeroOrMore,
                _ => ErCardinality.ExactlyOne
            };
        }

        /// <summary>
        /// 解析完整的关系符号
        /// </summary>
        public static (ErCardinality left, ErCardinality right, bool isIdentifying) ParseRelationSymbol(string symbol)
        {
            // 查找中间的线型 (-- 或 ..)
            int dashPos = symbol.IndexOf("--");
            int dotPos = symbol.IndexOf("..");

            bool isIdentifying = dashPos >= 0;
            int splitPos = isIdentifying ? dashPos : dotPos;

            if (splitPos < 0)
            {
                return (ErCardinality.ExactlyOne, ErCardinality.ExactlyOne, true);
            }

            string leftPart = symbol.Substring(0, splitPos);
            string rightPart = symbol.Substring(splitPos + 2);

            return (
                ParseCardinality(leftPart),
                ParseCardinality(rightPart),
                isIdentifying
            );
        }
    }
}
