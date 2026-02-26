using md2visio.Api;
using md2visio.struc.sequence;
using md2visio.vsdx.@base;

namespace md2visio.vsdx
{
    internal class VBuilderSeq : VFigureBuilder<Sequence>
    {
        public VBuilderSeq(Sequence figure, ConversionContext context, IVisioSession session)
            : base(figure, context, session) { }

        protected override void ExecuteBuild()
        {
            if (_context.Debug)
            {
                _context.Log($"[DEBUG] VBuilderSeq: Start executing build, VisioApp state: {(_session.Application != null ? "Created" : "Not Created")}");
            }

            try
            {
                using var drawer = new VDrawerSeq(figure, _session.Application, _context);
                drawer.Draw();

                if (_context.Debug)
                {
                    _context.Log($"[DEBUG] VBuilderSeq: VDrawerSeq.Draw() execution completed");
                }
            }
            catch (Exception ex)
            {
                if (_context.Debug)
                {
                    _context.Log($"[DEBUG] VBuilderSeq: VDrawerSeq.Draw() execution failed: {ex.Message}");
                    _context.Log($"[DEBUG] VBuilderSeq: Exception type: {ex.GetType().Name}");
                    if (ex.InnerException != null)
                    {
                        _context.Log($"[DEBUG] VBuilderSeq: Inner exception: {ex.InnerException.Message}");
                    }
                }
                throw;
            }
        }
    }
}
