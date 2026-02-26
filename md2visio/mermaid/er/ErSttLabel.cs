using md2visio.mermaid.cmn;
using System.Text;

namespace md2visio.mermaid.er
{
    /// <summary>
    /// ER Diagram Relation Label State
    /// Parses relation label after :
    /// </summary>
    internal class ErSttLabel : SynState
    {
        public override SynState NextState()
        {
            // Skip colon
            if (Ctx.Peek() == ":")
            {
                Ctx.Take();
            }

            // Skip whitespace
            while (Ctx.Peek() == " " || Ctx.Peek() == "\t")
            {
                Ctx.Take();
            }

            StringBuilder label = new();
            bool inQuotes = false;

            // Read label content
            while (true)
            {
                string? ch = Ctx.Peek();
                if (ch == null) break;

                if (ch == "\"")
                {
                    Ctx.Take();
                    inQuotes = !inQuotes;
                    continue;
                }

                if (!inQuotes && (ch == "\n" || ch == "`"))
                {
                    break;
                }

                Ctx.Take();
                label.Append(ch);
            }

            string labelText = label.ToString().Trim();
            AddCompo("label", labelText);
            Save(labelText);
            ClearBuffer();

            return Forward<ErSttChar>();
        }
    }
}
