using md2visio.mermaid.cmn;

namespace md2visio.mermaid.er
{
    /// <summary>
    /// ER图单词状态类
    /// 处理实体名称等标识符
    /// </summary>
    internal class ErSttWord : SynState
    {
        public override SynState NextState()
        {
            // 已在 ErSttChar 中通过 Create<ErSttWord>().Save() 保存
            // 这里直接转发
            return Forward<ErSttChar>();
        }
    }
}
