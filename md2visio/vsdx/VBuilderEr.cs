using md2visio.Api;
using md2visio.struc.er;
using md2visio.vsdx.@base;

namespace md2visio.vsdx
{
    /// <summary>
    /// ER Diagram Visio Builder
    /// </summary>
    internal class VBuilderEr : VFigureBuilder<ErDiagram>
    {
        public VBuilderEr(ErDiagram figure, ConversionContext context, IVisioSession session)
            : base(figure, context, session) { }

        protected override void ExecuteBuild()
        {
            using var drawer = new VDrawerEr(figure, _session.Application, _context);
            drawer.Draw();
        }
    }
}
