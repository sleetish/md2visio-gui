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
                _context.Log($"[DEBUG] VBuilderSeq: 开始执行构建，VisioApp状态: {(_session.Application != null ? "已创建" : "未创建")}");
            }

            try
            {
                using var drawer = new VDrawerSeq(figure, _session.Application, _context);
                drawer.Draw();

                if (_context.Debug)
                {
                    _context.Log($"[DEBUG] VBuilderSeq: VDrawerSeq.Draw() 执行完成");
                }
            }
            catch (Exception ex)
            {
                if (_context.Debug)
                {
                    _context.Log($"[DEBUG] VBuilderSeq: VDrawerSeq.Draw() 执行失败: {ex.Message}");
                    _context.Log($"[DEBUG] VBuilderSeq: 异常类型: {ex.GetType().Name}");
                    if (ex.InnerException != null)
                    {
                        _context.Log($"[DEBUG] VBuilderSeq: 内部异常: {ex.InnerException.Message}");
                    }
                }
                throw;
            }
        }
    }
}
