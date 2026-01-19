using md2visio.mermaid.cmn;
using System.Text.RegularExpressions;

namespace md2visio.mermaid.er
{
    /// <summary>
    /// ER图字符状态类
    /// 核心状态分发器，根据当前字符决定下一个状态
    /// </summary>
    internal class ErSttChar : SynState
    {
        // 关系符号的起始字符模式
        static readonly Regex regRelationStart = new(
            @"^(\|\||[|o}\|])",
            RegexOptions.Compiled);

        public override SynState NextState()
        {
            string? next = Ctx.Peek();
            if (next == null) return EndOfFile;

            // 注释处理
            if (next == "%") return Forward<SttPercent>();

            // 行结束
            if (next == "\n")
            {
                if (Buffer.Length > 0)
                {
                    Create<ErSttWord>().Save(Buffer);
                    ClearBuffer();
                }
                return Forward<SttFinishFlag>();
            }

            // Mermaid 代码块结束
            if (next == "`") return Forward<SttMermaidClose>();

            // 空白字符 - 可能是单词结束
            if (next == " " || next == "\t")
            {
                if (Buffer.Length > 0)
                {
                    // 检查是否是关键字
                    if (ErSttKeyword.IsKeyword(Buffer))
                    {
                        return Forward<ErSttKeyword>();
                    }
                    Create<ErSttWord>().Save(Buffer);
                    ClearBuffer();
                }
                return Take().Forward<ErSttChar>();
            }

            // 实体属性块开始
            if (next == "{")
            {
                if (Buffer.Length > 0)
                {
                    Create<ErSttWord>().Save(Buffer);
                    ClearBuffer();
                }
                return Forward<ErSttEntityBody>();
            }

            // 关系标签
            if (next == ":")
            {
                if (Buffer.Length > 0)
                {
                    Create<ErSttWord>().Save(Buffer);
                    ClearBuffer();
                }
                return Forward<ErSttLabel>();
            }

            // 检查是否是关系符号开始
            if (IsRelationStart())
            {
                if (Buffer.Length > 0)
                {
                    Create<ErSttWord>().Save(Buffer);
                    ClearBuffer();
                }
                return Forward<ErSttRelation>();
            }

            // 普通字符 - 累积到 Buffer
            return Take().Forward<ErSttChar>();
        }

        bool IsRelationStart()
        {
            string incoming = Ctx.Incoming.ToString();
            // 关系符号必须以完整的基数模式开始:
            // ||, |o, |{, }o, }|, }{ (后面跟 -- 或 ..)
            if (incoming.StartsWith("||") || incoming.StartsWith("|o") ||
                incoming.StartsWith("|{") || incoming.StartsWith("}|") || 
                incoming.StartsWith("}o") || incoming.StartsWith("}{"))
            {
                return true;
            }
            // o| 和 o{ 情况
            if (incoming.StartsWith("o|") || incoming.StartsWith("o{"))
            {
                return true;
            }
            return false;
        }
    }
}
