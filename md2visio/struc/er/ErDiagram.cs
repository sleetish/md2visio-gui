using md2visio.Api;
using md2visio.struc.figure;
using md2visio.vsdx;
using md2visio.vsdx.@base;

namespace md2visio.struc.er
{
    /// <summary>
    /// ER图数据结构主类
    /// </summary>
    internal class ErDiagram : Figure
    {
        readonly Dictionary<string, ErEntity> entityDict = new();
        readonly List<ErRelation> relations = new();

        public ErDiagram() { }

        /// <summary>
        /// 所有实体
        /// </summary>
        public Dictionary<string, ErEntity> Entities => entityDict;

        /// <summary>
        /// 所有关系
        /// </summary>
        public List<ErRelation> Relations => relations;

        /// <summary>
        /// 获取或创建实体
        /// </summary>
        public ErEntity GetOrCreateEntity(string id)
        {
            if (!entityDict.ContainsKey(id))
            {
                entityDict[id] = new ErEntity { ID = id, DisplayName = id };
            }
            return entityDict[id];
        }

        /// <summary>
        /// 添加关系
        /// </summary>
        public void AddRelation(ErRelation relation)
        {
            relations.Add(relation);
        }

        /// <summary>
        /// 转换为 Visio 文件
        /// </summary>
        public override void ToVisio(string path, ConversionContext context, IVisioSession session)
        {
            new VBuilderEr(this, context, session).Build(path);
        }
    }
}
