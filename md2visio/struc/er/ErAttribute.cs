namespace md2visio.struc.er
{
    /// <summary>
    /// ER图属性类
    /// 表示实体的一个属性
    /// </summary>
    internal class ErAttribute
    {
        /// <summary>
        /// 属性类型 (如 string, int, date)
        /// </summary>
        public string Type { get; set; } = "";

        /// <summary>
        /// 属性名称
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// 键类型 (PK, FK, UK 或组合如 "PK, FK")
        /// </summary>
        public string Keys { get; set; } = "";

        /// <summary>
        /// 注释
        /// </summary>
        public string Comment { get; set; } = "";

        /// <summary>
        /// 是否是主键
        /// </summary>
        public bool IsPrimaryKey => Keys.Contains("PK");

        /// <summary>
        /// 是否是外键
        /// </summary>
        public bool IsForeignKey => Keys.Contains("FK");

        /// <summary>
        /// 是否是唯一键
        /// </summary>
        public bool IsUniqueKey => Keys.Contains("UK");

        /// <summary>
        /// 生成显示字符串
        /// </summary>
        public string ToDisplayString()
        {
            var parts = new List<string> { Type, Name };

            if (!string.IsNullOrEmpty(Keys))
            {
                parts.Add(Keys);
            }

            if (!string.IsNullOrEmpty(Comment))
            {
                parts.Add($"\"{Comment}\"");
            }

            return string.Join(" ", parts);
        }
    }
}
