namespace md2visio.struc.sequence
{
    internal class SeqFragment
    {
        public string Type { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public double StartY { get; set; }
        public double EndY { get; set; }
        public double LabelHeight { get; set; }
        public List<SeqFragmentSection> Sections { get; set; } = new();
    }

    internal class SeqFragmentSection
    {
        public double Y { get; set; }
        public string Text { get; set; } = string.Empty;
        public double LabelHeight { get; set; }
    }
}
