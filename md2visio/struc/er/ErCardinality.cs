namespace md2visio.struc.er
{
    /// <summary>
    /// ER图基数枚举
    /// 表示实体之间的关系基数
    /// </summary>
    internal enum ErCardinality
    {
        /// <summary>
        /// 恰好一个 (||)
        /// </summary>
        ExactlyOne,

        /// <summary>
        /// 零个或一个 (|o, o|)
        /// </summary>
        ZeroOrOne,

        /// <summary>
        /// 一个或多个 (}|, |{)
        /// </summary>
        OneOrMore,

        /// <summary>
        /// 零个或多个 (}o, o{)
        /// </summary>
        ZeroOrMore
    }
}
