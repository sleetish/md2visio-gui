using md2visio.mermaid.cmn;
using System.Text.RegularExpressions;

namespace md2visio.mermaid.sequence
{
    internal class SeqSttKeyword : SynState
    {
        public override SynState NextState()
        {
            if (!IsKeyword(Ctx)) throw new SynException($"unknown keyword '{Buffer}'", Ctx);

            Save(Buffer).ClearBuffer();
            
            string keyword = Fragment;
            
            // Determine next state based on keyword type
            switch (keyword)
            {
                case "sequenceDiagram":
                    return Forward<SeqSttChar>();
                    
                case "participant":
                    return Forward<SeqSttChar>();
                    
                case "activate":
                case "deactivate":
                    return Forward<SeqSttChar>();
                    
                case "note":
                    // TODO: Implement note syntax parsing
                    return Forward<SeqSttChar>();
                    
                default:
                    return Forward<SeqSttChar>();
            }
        }

        public static bool IsKeyword(SynContext ctx)
        {
            return Regex.IsMatch(ctx.Cache.ToString(),
                "^(sequenceDiagram|participant|activate|deactivate|note|loop|alt|else|opt|par|critical|break|end|autonumber)$");
        }
    }
}