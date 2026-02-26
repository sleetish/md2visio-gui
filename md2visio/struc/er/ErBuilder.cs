using md2visio.Api;
using md2visio.mermaid.cmn;
using md2visio.mermaid.er;
using md2visio.struc.figure;
using md2visio.vsdx.@base;

namespace md2visio.struc.er
{
    /// <summary>
    /// ER Diagram Builder
    /// Builds ErDiagram data structure from state sequence
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
                    // Mark as processed to avoid repeated detection by FigureBuilderFactory
                    iter.Current.Fragment = "_erDiagram_processed";
                    break;

                case "title":
                    // Optional: Parse title
                    break;

                case "direction":
                    // Optional: Parse direction
                    break;
            }
        }

        void BuildWord()
        {
            string word = iter.Current.Fragment.Trim();
            if (string.IsNullOrWhiteSpace(word)) return;

            // If there is a pending relation symbol, this word is the target entity
            if (!string.IsNullOrEmpty(pendingRelationSymbol))
            {
                // Create relation
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

                    // Ensure both entities exist
                    diagram.GetOrCreateEntity(pendingEntityId);
                    diagram.GetOrCreateEntity(word);
                }

                // Clear pending state, but keep pendingEntityId for label processing
                pendingRelationSymbol = null;
                pendingEntityId = word; // Update to target entity for possible subsequent relations
            }
            else
            {
                // This is a new entity name
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

            // Apply label to the last relation
            if (diagram.Relations.Count > 0)
            {
                var lastRelation = diagram.Relations[^1];
                lastRelation.Label = label;
            }
        }
    }
}
