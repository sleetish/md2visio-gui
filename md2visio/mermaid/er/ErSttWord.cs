using md2visio.mermaid.cmn;

namespace md2visio.mermaid.er
{
    /// <summary>
    /// ER Diagram Word State
    /// Handles identifiers like entity names
    /// </summary>
    internal class ErSttWord : SynState
    {
        public override SynState NextState()
        {
            // Saved in ErSttChar via Create<ErSttWord>().Save()
            // Forward directly here
            return Forward<ErSttChar>();
        }
    }
}
