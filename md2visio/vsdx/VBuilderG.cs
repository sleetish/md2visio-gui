using md2visio.Api;
using md2visio.struc.graph;
using md2visio.vsdx.@base;

namespace md2visio.vsdx
{
    internal class VBuilderG : VFigureBuilder<Graph>
    {
        public VBuilderG(Graph figure, ConversionContext context, IVisioSession session)
            : base(figure, context, session) { }

        override protected void ExecuteBuild()
        {
            using var drawer = new VDrawerG(figure, _session.Application, _context);
            drawer.Draw();
        }
    }
}
