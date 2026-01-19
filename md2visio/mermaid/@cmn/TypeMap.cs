using md2visio.mermaid.classdiag;
using md2visio.mermaid.er;
using md2visio.mermaid.graph;
using md2visio.mermaid.graph.@internal;
using md2visio.mermaid.journey;
using md2visio.mermaid.packet;
using md2visio.mermaid.pie;
using md2visio.mermaid.sequence;
using md2visio.mermaid.xy;
using md2visio.struc.classdiag;
using md2visio.struc.er;
using md2visio.struc.graph;
using md2visio.struc.journey;
using md2visio.struc.packet;
using md2visio.struc.pie;
using md2visio.struc.sequence;
using md2visio.struc.xy;

namespace md2visio.mermaid.cmn
{
    internal class TypeMap
    {
        public static readonly Dictionary<string, Type> KeywordMap = new()
        {
            { "graph", typeof(GSttKeyword) }, { "flowchart", typeof(GSttKeyword) },
            { "classDiagram", typeof(ClsSttKeyword) },
            { "journey", typeof(JoSttKeyword) },
            { "pie", typeof(PieSttKeyword) },
            { "packet-beta", typeof(PacSttKeyword) }, { "packet", typeof(PacSttKeyword) },
            { "sequenceDiagram", typeof(SeqSttKeyword) },
            { "xychart-beta", typeof(XySttKeyword) }, { "xychart", typeof(XySttKeyword) },
            { "erDiagram", typeof(ErSttKeyword) },
        };

        public static readonly Dictionary<string, Type> CharMap = new()
        {
            { "graph", typeof(GSttChar) }, { "flowchart", typeof(GSttChar) },
            { "classDiagram", typeof(ClsSttChar) },
            { "journey", typeof(JoSttChar) },
            { "pie", typeof(PieSttChar) },
            { "packet-beta", typeof(PaSttChar) }, { "packet", typeof(PaSttChar) },
            { "sequenceDiagram", typeof(SeqSttChar) },
            { "xychart-beta", typeof(XySttChar) }, { "xychart", typeof(XySttChar) },
            { "erDiagram", typeof(ErSttChar) },
        };

        public static readonly Dictionary<string, Type> BuilderMap = new()
        {
            { "graph", typeof(GBuilder) }, { "flowchart", typeof(GBuilder) },
            { "classDiagram", typeof(ClsBuilder) },
            { "journey", typeof(JoBuilder) },
            { "pie", typeof(PieBuilder) },
            { "packet-beta", typeof(PacBuilder) }, { "packet", typeof(PacBuilder) },
            { "sequenceDiagram", typeof(SeqBuilder) },
            { "xychart-beta", typeof(XyBuilder) }, { "xychart", typeof(XyBuilder) },
            { "erDiagram", typeof(ErBuilder) },
        };

        public static readonly Dictionary<string, string> ConfigMap = new()
        {
            {typeof(Graph).Name, "flowchart"},
            {typeof(ClassDiagram).Name, "class"},
            {typeof(Journey).Name, "journey"},
            {typeof(Packet).Name, "packet"},
            {typeof(Pie).Name, "pie"},
            {typeof(Sequence).Name, "sequence"},
            {typeof(XyChart).Name, "xyChart"},
            {typeof(ErDiagram).Name, "er"},
        };
    }
}
