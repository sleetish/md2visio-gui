using md2visio.mermaid.cmn;
using System.Text.RegularExpressions;

namespace md2visio.mermaid.er
{
    /// <summary>
    /// ER Diagram Character State
    /// Core state dispatcher, determines next state based on current character
    /// </summary>
    internal class ErSttChar : SynState
    {
        // Relation symbol start pattern
        static readonly Regex regRelationStart = new(
            @"^(\|\||[|o}\|])",
            RegexOptions.Compiled);

        public override SynState NextState()
        {
            string? next = Ctx.Peek();
            if (next == null) return EndOfFile;

            // Comment handling
            if (next == "%") return Forward<SttPercent>();

            // End of line
            if (next == "\n")
            {
                if (Buffer.Length > 0)
                {
                    Create<ErSttWord>().Save(Buffer);
                    ClearBuffer();
                }
                return Forward<SttFinishFlag>();
            }

            // Mermaid block end
            if (next == "`") return Forward<SttMermaidClose>();

            // Whitespace - possibly end of word
            if (next == " " || next == "\t")
            {
                if (Buffer.Length > 0)
                {
                    // Check if keyword
                    if (ErSttKeyword.IsKeyword(Buffer))
                    {
                        return Forward<ErSttKeyword>();
                    }
                    Create<ErSttWord>().Save(Buffer);
                    ClearBuffer();
                }
                return Take().Forward<ErSttChar>();
            }

            // Entity attribute block start
            if (next == "{")
            {
                if (Buffer.Length > 0)
                {
                    Create<ErSttWord>().Save(Buffer);
                    ClearBuffer();
                }
                return Forward<ErSttEntityBody>();
            }

            // Relation label
            if (next == ":")
            {
                if (Buffer.Length > 0)
                {
                    Create<ErSttWord>().Save(Buffer);
                    ClearBuffer();
                }
                return Forward<ErSttLabel>();
            }

            // Check if relation symbol start
            if (IsRelationStart())
            {
                if (Buffer.Length > 0)
                {
                    Create<ErSttWord>().Save(Buffer);
                    ClearBuffer();
                }
                return Forward<ErSttRelation>();
            }

            // Normal character - accumulate to Buffer
            return Take().Forward<ErSttChar>();
        }

        bool IsRelationStart()
        {
            string incoming = Ctx.Incoming.ToString();
            // Relation symbol must start with full cardinality pattern:
            // ||, |o, |{, }o, }|, }{ (followed by -- or ..)
            if (incoming.StartsWith("||") || incoming.StartsWith("|o") ||
                incoming.StartsWith("|{") || incoming.StartsWith("}|") || 
                incoming.StartsWith("}o") || incoming.StartsWith("}{"))
            {
                return true;
            }
            // o| and o{ cases
            if (incoming.StartsWith("o|") || incoming.StartsWith("o{"))
            {
                return true;
            }
            return false;
        }
    }
}
