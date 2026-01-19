using md2visio.mermaid.cmn;
using System.Text;

namespace md2visio.mermaid.er
{
    /// <summary>
    /// ER图关系标签状态类
    /// 解析 : 后面的关系标签
    /// </summary>
    internal class ErSttLabel : SynState
    {
        public override SynState NextState()
        {
            // 跳过冒号
            if (Ctx.Peek() == ":")
            {
                Ctx.Take();
            }

            // 跳过空白
            while (Ctx.Peek() == " " || Ctx.Peek() == "\t")
            {
                Ctx.Take();
            }

            StringBuilder label = new();
            bool inQuotes = false;

            // 读取标签内容
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
