using md2visio.Api;
using md2visio.mermaid.cmn;
using md2visio.mermaid.er;
using md2visio.struc.figure;
using md2visio.vsdx.@base;

namespace md2visio.struc.er
{
    /// <summary>
    /// ER图构建器
    /// 从状态序列构建 ErDiagram 数据结构
    /// </summary>
    internal class ErBuilder : FigureBuilder
    {
        readonly ErDiagram diagram = new();
        ErEntity? currentEntity = null;
        string? pendingEntityId = null;
        string? pendingRelationSymbol = null;
        ErCardinality pendingLeftCard = ErCardinality.ExactlyOne;
        ErCardinality pendingRightCard = ErCardinality.ExactlyOne;
        bool pendingIsIdentifying = true;

        public ErBuilder(SttIterator iter, ConversionContext context, IVisioSession session)
            : base(iter, context, session) { }

        public override void Build(string outputFile)
        {
            while (iter.HasNext())
            {
                SynState cur = iter.Next();

                if (cur is SttMermaidStart) { }
                else if (cur is SttMermaidClose)
                {
                    diagram.ToVisio(outputFile, _context, _session);
                    break;
                }
                else if (cur is ErSttKeyword) BuildKeyword();
                else if (cur is ErSttKeywordParam) { }
                else if (cur is ErSttWord) BuildWord();
                else if (cur is ErSttEntityBody) BuildEntityBody();
                else if (cur is ErSttRelation) BuildRelation();
                else if (cur is ErSttLabel) BuildLabel();
                else if (cur is SttComment) diagram.Config.LoadUserDirectiveFromComment(cur.Fragment);
                else if (cur is SttFrontMatter) diagram.Config.LoadUserFrontMatter(cur.Fragment);
            }
        }

        void BuildKeyword()
        {
            string kw = iter.Current.Fragment;

            switch (kw)
            {
                case "erDiagram":
                    // 标记为已处理，避免被 FigureBuilderFactory 重复检测
                    iter.Current.Fragment = "_erDiagram_processed";
                    break;

                case "title":
                    // 可选: 解析标题
                    break;

                case "direction":
                    // 可选: 解析方向
                    break;
            }
        }

        void BuildWord()
        {
            string word = iter.Current.Fragment.Trim();
            if (string.IsNullOrWhiteSpace(word)) return;

            // 如果有待处理的关系符号，这个词是目标实体
            if (!string.IsNullOrEmpty(pendingRelationSymbol))
            {
                // 创建关系
                if (!string.IsNullOrEmpty(pendingEntityId))
                {
                    var relation = new ErRelation
                    {
                        FromEntity = pendingEntityId,
                        ToEntity = word,
                        LeftCardinality = pendingLeftCard,
                        RightCardinality = pendingRightCard,
                        IsIdentifying = pendingIsIdentifying,
                        Label = ""
                    };
                    diagram.AddRelation(relation);

                    // 确保两个实体都存在
                    diagram.GetOrCreateEntity(pendingEntityId);
                    diagram.GetOrCreateEntity(word);
                }

                // 清除待处理状态，但保留 pendingEntityId 以便处理标签
                pendingRelationSymbol = null;
                pendingEntityId = word; // 更新为目标实体，以便可能的后续关系
            }
            else
            {
                // 这是一个新的实体名
                pendingEntityId = word;
                currentEntity = diagram.GetOrCreateEntity(word);
            }
        }

        void BuildEntityBody()
        {
            if (currentEntity == null && !string.IsNullOrEmpty(pendingEntityId))
            {
                currentEntity = diagram.GetOrCreateEntity(pendingEntityId);
            }

            if (currentEntity == null) return;

            string body = iter.Current.GetPart("body");
            var attributes = ErSttEntityBody.ParseAttributes(body);

            foreach (var (type, name, keys, comment) in attributes)
            {
                currentEntity.AddAttribute(type, name, keys, comment);
            }

            currentEntity = null;
        }

        void BuildRelation()
        {
            string relationSymbol = iter.Current.GetPart("relation");
            
            var (left, right, isIdentifying) = ErRelation.ParseRelationSymbol(relationSymbol);
            
            pendingRelationSymbol = relationSymbol;
            pendingLeftCard = left;
            pendingRightCard = right;
            pendingIsIdentifying = isIdentifying;
        }

        void BuildLabel()
        {
            string label = iter.Current.GetPart("label");

            // 将标签应用到最后一个关系
            if (diagram.Relations.Count > 0)
            {
                var lastRelation = diagram.Relations[^1];
                lastRelation.Label = label;
            }
        }
    }
}
