using Microsoft.Office.Interop.Visio;

namespace md2visio.struc.er
{
    /// <summary>
    /// ER Entity Class
    /// Represents a database table or entity
    /// </summary>
    internal class ErEntity
    {
        /// <summary>
        /// Entity ID (for internal reference)
        /// </summary>
        public string ID { get; set; } = "";

        /// <summary>
        /// Display Name (Alias, if any)
        /// </summary>
        public string DisplayName { get; set; } = "";

        /// <summary>
        /// List of entity attributes
        /// </summary>
        public List<ErAttribute> Attributes { get; } = new();

        /// <summary>
        /// Corresponding Visio Shape
        /// </summary>
        public Shape? VisioShape { get; set; }

        /// <summary>
        /// Get display name
        /// </summary>
        public string GetDisplayName()
        {
            return string.IsNullOrEmpty(DisplayName) ? ID : DisplayName;
        }

        /// <summary>
        /// Add attribute
        /// </summary>
        public void AddAttribute(ErAttribute attribute)
        {
            Attributes.Add(attribute);
        }

        /// <summary>
        /// Add attribute
        /// </summary>
        public void AddAttribute(string type, string name, string keys = "", string comment = "")
        {
            Attributes.Add(new ErAttribute
            {
                Type = type,
                Name = name,
                Keys = keys,
                Comment = comment
            });
        }
    }
}
