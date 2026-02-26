using Microsoft.Office.Interop.Visio;

namespace md2visio.struc.er
{
    /// <summary>
    /// ER Relation Class
    /// Represents relationship between two entities
    /// </summary>
    internal class ErRelation
    {
        /// <summary>
        /// Source Entity ID
        /// </summary>
        public string FromEntity { get; set; } = "";

        /// <summary>
        /// Target Entity ID
        /// </summary>
        public string ToEntity { get; set; } = "";

        /// <summary>
        /// Source Cardinality
        /// </summary>
        public ErCardinality LeftCardinality { get; set; } = ErCardinality.ExactlyOne;

        /// <summary>
        /// Target Cardinality
        /// </summary>
        public ErCardinality RightCardinality { get; set; } = ErCardinality.ExactlyOne;

        /// <summary>
        /// Is identifying relationship (solid line), else non-identifying (dashed line)
        /// </summary>
        public bool IsIdentifying { get; set; } = true;

        /// <summary>
        /// Relation Label
        /// </summary>
        public string Label { get; set; } = "";

        /// <summary>
        /// Corresponding Visio Shape
        /// </summary>
        public Shape? VisioShape { get; set; }

        /// <summary>
        /// Parse Cardinality Symbol
        /// </summary>
        public static ErCardinality ParseCardinality(string symbol)
        {
            // Normalize symbol
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
        /// Parse Complete Relation Symbol
        /// </summary>
        public static (ErCardinality left, ErCardinality right, bool isIdentifying) ParseRelationSymbol(string symbol)
        {
            // Find middle line style (-- or ..)
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
