using Microsoft.Office.Interop.Visio;

namespace md2visio.struc.sequence
{
    internal class SeqMessage
    {
        public string From { get; set; } = string.Empty;
        public string To { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string ArrowType { get; set; } = "->"; // ->> -->> -> -->
        public double Y { get; set; }
        public int? SequenceNumber { get; set; }
        public Shape? MessageShape { get; set; }
        public double LabelHeight { get; set; }

        // 判断是否为自调用
        public bool IsSelfCall => From == To;

        // 判断是否为虚线
        public bool IsDashed => ArrowType.StartsWith("--");

        // 判断是否为同步消息(带>>)
        public bool IsSynchronous => ArrowType.EndsWith(">>");

        public SeqMessage()
        {
        }

        public SeqMessage(string from, string to, string label, string arrowType)
        {
            From = from;
            To = to;
            Label = label;
            ArrowType = arrowType;
        }

        public override string ToString()
        {
            return $"{From}{ArrowType}{To}: {Label}";
        }
    }
}
