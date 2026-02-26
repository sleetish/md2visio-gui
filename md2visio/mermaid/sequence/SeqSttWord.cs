using md2visio.mermaid.cmn;

namespace md2visio.mermaid.sequence
{
    internal class SeqSttWord : SttWordFlag
    {
        public override SynState NextState()
        {
            string word = Buffer.ToString().Trim();
            
            if (SeqSttKeyword.IsKeyword(Ctx))
            {
                return Forward<SeqSttKeyword>();
            }
            else if (word.Contains("->>") || word.Contains("-->>") || word.Contains("->") || word.Contains("-->"))
            {
                // Contains message arrow, treat as message
                Save(Buffer).ClearBuffer();
                return Forward<SeqSttChar>();
            }
            else
            {
                Save(Buffer).ClearBuffer();
                return Forward<SeqSttChar>();
            }
        }
    }
}