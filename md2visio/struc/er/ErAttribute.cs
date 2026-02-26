namespace md2visio.struc.er
{
    /// <summary>
    /// ER Attribute Class
    /// Represents an attribute of an entity
    /// </summary>
    internal class ErAttribute
    {
        /// <summary>
        /// Attribute Type (e.g. string, int, date)
        /// </summary>
        public string Type { get; set; } = "";

        /// <summary>
        /// Attribute Name
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// Key Type (PK, FK, UK or combination like "PK, FK")
        /// </summary>
        public string Keys { get; set; } = "";

        /// <summary>
        /// Comment
        /// </summary>
        public string Comment { get; set; } = "";

        /// <summary>
        /// Is Primary Key
        /// </summary>
        public bool IsPrimaryKey => Keys.Contains("PK");

        /// <summary>
        /// Is Foreign Key
        /// </summary>
        public bool IsForeignKey => Keys.Contains("FK");

        /// <summary>
        /// Is Unique Key
        /// </summary>
        public bool IsUniqueKey => Keys.Contains("UK");

        /// <summary>
        /// Generate Display String
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
