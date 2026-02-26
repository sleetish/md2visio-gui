using md2visio.mermaid.cmn;

namespace md2visio.mermaid.er
{
    /// <summary>
    /// ER Diagram Keyword Parameter State
    /// Handles parameters like direction TB, direction LR
    /// </summary>
    internal class ErSttKeywordParam : SttKeywordParam
    {
        public override SynState NextState()
        {
            return Save(ExpectedGroups["param"].Value).Forward<ErSttChar>();
        }
    }
}
