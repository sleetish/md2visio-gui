using md2visio.struc.figure;
using md2visio.struc.graph;
using Microsoft.Office.Interop.Visio;

namespace md2visio.struc.sequence
{
    internal class SeqParticipant : INode
    {
        public string ID { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Alias { get; set; } = string.Empty; // Supports "participant a as user" syntax
        public double X { get; set; } // Horizontal position
        
        // Visio shape objects
        public Shape? TopShape { get; set; }      // Top participant box
        public Shape? BottomShape { get; set; }   // Bottom participant box
        public Shape? LifelineShape { get; set; } // Lifeline

        public Shape? VisioShape { get; set; } // Required by INode interface

        public Container Container { get; set; } = Empty.Get<Container>();

        // Required by INode interface
        public List<GEdge> InputEdges { get; } = new List<GEdge>();
        public List<GEdge> OutputEdges { get; } = new List<GEdge>();

        public SeqParticipant()
        {
        }

        public SeqParticipant(string id, string label)
        {
            ID = id;
            Label = label;
        }

        public string DisplayName => !string.IsNullOrEmpty(Alias) ? Alias : Label;

        public List<INode> InputNodes()
        {
            return InputEdges.Select(e => e.From).Cast<INode>().ToList();
        }

        public List<INode> OutputNodes()
        {
            return OutputEdges.Select(e => e.To).Cast<INode>().ToList();
        }
    }
}
