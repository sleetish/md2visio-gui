using md2visio.Api;
using md2visio.struc.figure;
using md2visio.vsdx.@tool;
using Microsoft.Office.Interop.Visio;

namespace md2visio.vsdx.@base
{
    internal abstract class VFigureDrawer<T> : VShapeDrawer where T : Figure
    {
        protected T figure = Empty.Get<T>();
        protected Config config;
        protected readonly ConversionContext _context;

        public VFigureDrawer(T figure, Application visioApp, ConversionContext context) : base(visioApp)
        {
            this.figure = figure;
            this._context = context;
            config = figure.Config;
        }

        public abstract void Draw();

        public void SetFillForegnd(Shape shape, string configPath)
        {
            if (config.GetString(configPath, out string sColor))
            {
                if (sColor.Trim().ToLower() == "transparent")
                {
                    SetShapeSheet(shape, "FillPattern", "0");
                    return;
                }
                SetFillForegnd(shape, VColor.Create(sColor));
            }
        }

        public void SetFillForegnd(Shape shape, VColor color)
        {
            SetShapeSheet(shape, "FillPattern", "1");
            SetShapeSheet(shape, "FillForegnd", $"THEMEGUARD({color.RGB()})");
        }

        public void SetLineColor(Shape shape, string configPath)
        {
            if (config.GetString(configPath, out string color))
            {
                SetShapeSheet(shape, "LineColor", $"THEMEGUARD({VColor.Create(color).RGB()})");
            }
        }

        public void SetTextColor(Shape shape, string configPath)
        {
            if (config.GetString(configPath, out string color))
            {
                shape.CellsU["Char.Color"].FormulaU = $"THEMEGUARD({VColor.Create(color).RGB()})";
            }
        }

        public List<string> GetStringList(string prefix, int maxCount = 13)
        {
            return figure.Config.GetStringList(prefix, maxCount);
        }

        /// <summary>
        /// Add delay during drawing to let user see the process
        /// </summary>
        protected void PauseForViewing(int milliseconds = 200)
        {
            if (_context.Visible)
            {
                System.Threading.Thread.Sleep(milliseconds);
            }
        }

        /// <summary>
        /// Ensure Visio application is visible when drawing
        /// </summary>
        protected void EnsureVisible()
        {
            if (_context.Visible && visioApp != null)
            {
                visioApp.Visible = true;
                visioApp.ActiveWindow?.Activate();
            }
        }
    }
}
