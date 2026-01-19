using md2visio.mermaid.cmn;

namespace md2visio.mermaid.er
{
    /// <summary>
    /// ER图关键字参数状态类
    /// 处理 direction TB, direction LR 等参数
    /// </summary>
    internal class ErSttKeywordParam : SttKeywordParam
    {
        public override SynState NextState()
        {
            return Save(ExpectedGroups["param"].Value).Forward<ErSttChar>();
        }
    }
}
