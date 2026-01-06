using md2visio.Api;
using md2visio.struc.figure;
using md2visio.struc.packet;
using md2visio.vsdx.@base;

namespace md2visio.vsdx
{
    internal class VBuilderPac : VFigureBuilder<Packet>
    {
        public VBuilderPac(Packet figure, ConversionContext context, IVisioSession session)
            : base(figure, context, session) { }

        protected override void ExecuteBuild()
        {
            using var drawer = new VDrawerPac(figure, _session.Application, _context);
            drawer.SortedNodes = OrderInnerNodes();
            drawer.Draw();
        }

        List<INode> OrderInnerNodes()
        {
            List<INode> nodes = figure.InnerNodes.Values.ToList<INode>();
            IComparer<INode> comparer = new PacBitsComparer();
            nodes.Sort(comparer);

            return nodes;
        }
    }

    class PacBitsComparer : IComparer<INode>
    {
        public int Compare(INode? x, INode? y)
        {
            if(x == null || y == null) return 0;
            PacBlock bitsx = (PacBlock) x, 
                bitsy = (PacBlock) y;

            return bitsx.BitStart - bitsy.BitStart;
        }
    }
}
