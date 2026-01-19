using md2visio.mermaid.cmn;
using System.Text.RegularExpressions;

namespace md2visio.mermaid.er
{
    /// <summary>
    /// ER图关键字状态类
    /// 处理 erDiagram, title, direction 等关键字
    /// </summary>
    internal class ErSttKeyword : SynState
    {
        static readonly Regex regKW = new(
            @"^(erDiagram|title|direction)$",
            RegexOptions.Compiled);

        public override SynState NextState()
        {
            string keyword = Buffer;
            Save(Buffer).ClearBuffer();

            // direction/title 关键字后跟参数
            if (keyword == "direction" || keyword == "title")
            {
                if (ErSttKeywordParam.HasParam(Ctx)) return Forward<ErSttKeywordParam>();
            }

            return Forward<ErSttChar>();
        }

        public static bool IsKeyword(SynContext ctx)
        {
            return regKW.IsMatch(ctx.Cache.ToString());
        }

        public static bool IsKeyword(string word)
        {
            return regKW.IsMatch(word);
        }
    }
}
