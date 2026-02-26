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

        // Check if self-call
        public bool IsSelfCall => From == To;

        // Check if dashed line
        public bool IsDashed => ArrowType.StartsWith("--");

        // Check if synchronous message (ends with >>)
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
