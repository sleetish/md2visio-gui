using md2visio.mermaid.cmn;
using System.Text.RegularExpressions;

namespace md2visio.mermaid.er
{
    /// <summary>
    /// ER Diagram Relation Parsing State
    /// Parses relation symbols like: ||--o{, }|..|{, ||--|{
    /// </summary>
    internal class ErSttRelation : SynState
    {
        // Complete relation symbol pattern
        // Left cardinality: ||, |o, o|, }|, }o, o{, |{
        // Line type: -- (solid/identifying), .. (dashed/non-identifying)
        // Right cardinality: ||, o|, |{, o{, }|, }o
        // Fix: Use more explicit alternation pattern
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

                // Consume matched characters
                for (int i = 0; i < relationSymbol.Length; i++)
                {
                    Ctx.Take();
                }

                // Save relation symbol components
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
        /// Parse left cardinality
        /// </summary>
        public static string ParseLeftCardinality(string symbol)
        {
            var match = regRelation.Match(symbol);
            return match.Success ? match.Groups["left"].Value : "";
        }

        /// <summary>
        /// Parse right cardinality
        /// </summary>
        public static string ParseRightCardinality(string symbol)
        {
            var match = regRelation.Match(symbol);
            return match.Success ? match.Groups["right"].Value : "";
        }

        /// <summary>
        /// Check if identifying relationship (solid line)
        /// </summary>
        public static bool IsIdentifying(string symbol)
        {
            return symbol.Contains("--");
        }
    }
}
