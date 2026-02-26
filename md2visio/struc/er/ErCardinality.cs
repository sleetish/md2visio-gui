namespace md2visio.struc.er
{
    /// <summary>
    /// ER Cardinality Enum
    /// Represents relationship cardinality between entities
    /// </summary>
    internal enum ErCardinality
    {
        /// <summary>
        /// Exactly One (||)
        /// </summary>
        ExactlyOne,

        /// <summary>
        /// Zero or One (|o, o|)
        /// </summary>
        ZeroOrOne,

        /// <summary>
        /// One or More (}|, |{)
        /// </summary>
        OneOrMore,

        /// <summary>
        /// Zero or More (}o, o{)
        /// </summary>
        ZeroOrMore
    }
}
