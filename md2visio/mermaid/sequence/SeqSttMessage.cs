using md2visio.mermaid.cmn;
using System.Text.RegularExpressions;

namespace md2visio.mermaid.sequence
{
    internal class SeqSttMessage : SynState
    {
        private string from = string.Empty;
        private string to = string.Empty;
        private string arrowType = string.Empty;
        private string label = string.Empty;

        public override SynState NextState()
        {
            // 解析消息语法: from->>to: message
            // 支持: ->> -->> -> --> 以及 inline 激活 +/- 
            string messageText = Buffer.ToString();
            
            if (!TryParseMessage(messageText))
            {
                throw new SynException($"Invalid message format: '{messageText}'", Ctx);
            }

            Save(messageText).ClearBuffer();
            return Forward<SeqSttChar>();
        }

        private bool TryParseMessage(string messageText)
        {
            // 正则表达式匹配消息格式: participant1->>[+-]?participant2: message text
            var pattern = @"^\s*(\w+)\s*(-->?>>?|-->>?|->>?|->)\s*([+-]?)\s*(\w+)\s*:\s*(.*)$";
            var match = Regex.Match(messageText, pattern);
            
            if (match.Success)
            {
                from = match.Groups[1].Value.Trim();
                arrowType = match.Groups[2].Value.Trim();
                to = match.Groups[4].Value.Trim();
                label = match.Groups[5].Value.Trim();
                return true;
            }
            
            return false;
        }

        public string GetFrom() => from;
        public string GetTo() => to;
        public string GetArrowType() => arrowType;
        public string GetLabel() => label;

        public static bool IsMessage(SynContext ctx)
        {
            string text = ctx.Cache.ToString().Trim();
            // 检查是否包含消息箭头
            return Regex.IsMatch(text, @"\w+\s*(-->?>>?|-->>?|->>?|->)\s*[+-]?\s*\w+\s*:");
        }
    }
}
