using md2visio.Api;
using md2visio.struc.classdiag;
using md2visio.vsdx.@base;

namespace md2visio.vsdx
{
    internal class VBuilderCls : VFigureBuilder<ClassDiagram>
    {
        public VBuilderCls(ClassDiagram figure, ConversionContext context, IVisioSession session)
            : base(figure, context, session) { }

        protected override void ExecuteBuild()
        {
            using var drawer = new VDrawerCls(figure, _session.Application, _context);
            drawer.Draw();
        }
    }
}
