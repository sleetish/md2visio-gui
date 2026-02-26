using md2visio.Api;
using md2visio.struc.figure;
using md2visio.vsdx;
using md2visio.vsdx.@base;

namespace md2visio.struc.er
{
    /// <summary>
    /// ER Diagram Main Data Structure
    /// </summary>
    internal class ErDiagram : Figure
    {
        readonly Dictionary<string, ErEntity> entityDict = new();
        readonly List<ErRelation> relations = new();

        public ErDiagram() { }

        /// <summary>
        /// All entities
        /// </summary>
        public Dictionary<string, ErEntity> Entities => entityDict;

        /// <summary>
        /// All relations
        /// </summary>
        public List<ErRelation> Relations => relations;

        /// <summary>
        /// Get or create entity
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
        /// Add relation
        /// </summary>
        public void AddRelation(ErRelation relation)
        {
            relations.Add(relation);
        }

        /// <summary>
        /// Convert to Visio file
        /// </summary>
        public override void ToVisio(string path, ConversionContext context, IVisioSession session)
        {
            new VBuilderEr(this, context, session).Build(path);
        }
    }
}
