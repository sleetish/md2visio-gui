namespace md2visio.struc.sequence
{
    internal enum SeqNotePosition
    {
        LeftOf,
        RightOf,
        Over
    }

    internal class SeqNote
    {
        public List<string> ParticipantIds { get; set; } = new List<string>();
        public string Text { get; set; } = string.Empty;
        public SeqNotePosition Position { get; set; } = SeqNotePosition.Over;
        public double Y { get; set; }
        public double LabelHeight { get; set; }
    }
}
