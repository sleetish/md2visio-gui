using Microsoft.Office.Interop.Visio;

namespace md2visio.struc.er
{
    /// <summary>
    /// ER图实体类
    /// 表示一个数据库表或实体
    /// </summary>
    internal class ErEntity
    {
        /// <summary>
        /// 实体ID (用于内部引用)
        /// </summary>
        public string ID { get; set; } = "";

        /// <summary>
        /// 显示名称 (别名，如有)
        /// </summary>
        public string DisplayName { get; set; } = "";

        /// <summary>
        /// 实体属性列表
        /// </summary>
        public List<ErAttribute> Attributes { get; } = new();

        /// <summary>
        /// 对应的 Visio 形状
        /// </summary>
        public Shape? VisioShape { get; set; }

        /// <summary>
        /// 获取显示用的名称
        /// </summary>
        public string GetDisplayName()
        {
            return string.IsNullOrEmpty(DisplayName) ? ID : DisplayName;
        }

        /// <summary>
        /// 添加属性
        /// </summary>
        public void AddAttribute(ErAttribute attribute)
        {
            Attributes.Add(attribute);
        }

        /// <summary>
        /// 添加属性
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
