using md2visio.Api;
using Microsoft.Win32;
using Visio = Microsoft.Office.Interop.Visio;

namespace md2visio.vsdx.@base
{
    internal abstract class VBuilder
    {
        // 注入的依赖
        protected readonly IVisioSession _session;
        protected readonly ConversionContext _context;

        protected Visio.Document visioDoc;
        protected Visio.Page visioPage;

        public VBuilder(ConversionContext context, IVisioSession session)
        {
            _context = context;
            _session = session;
            visioDoc = session.CreateDocument();
            visioPage = visioDoc.Pages[1];
        }

        public void SaveAndClose(string outputFile)
        {
            visioPage.ResizeToFitContents();

            // 如果显示 Visio 窗口，给用户时间查看结果
            if (_context.Visible && _session.Application != null)
            {
                _session.Application.Visible = true;
                System.Threading.Thread.Sleep(1000);
            }

            bool overwrite = _context.Quiet || !File.Exists(outputFile);

            if (overwrite)
            {
                if (!CanWriteOutputFile(outputFile, out string? reason))
                {
                    _context.SetError(reason ?? "输出文件不可写入。");
                    visioDoc.Saved = true;
                    _session.CloseDocument(visioDoc);
                    return;
                }
                _session.SaveDocument(visioDoc, outputFile);
            }
            else
            {
                visioDoc.Saved = true;
            }

            _session.CloseDocument(visioDoc);
        }

        bool CanWriteOutputFile(string outputFile, out string? reason)
        {
            reason = null;
            if (!File.Exists(outputFile)) return true;

            try
            {
                using var stream = new FileStream(
                    outputFile,
                    FileMode.Open,
                    FileAccess.ReadWrite,
                    FileShare.None);
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                reason = $"输出文件被占用或只读，无法写入：{outputFile}";
                return false;
            }
            catch (IOException)
            {
                reason = $"输出文件正在被其他程序占用，请关闭后重试：{outputFile}";
                return false;
            }
        }

        public static string? GetVisioContentDirectory()
        {
            int[] officeVersions = Enumerable.Range(11, 16).ToArray();

            foreach (int version in officeVersions)
            {
                string subKey = $@"Software\Microsoft\Office\{version}.0\Visio\InstallRoot";
#pragma warning disable CA1416, CS8604
                using RegistryKey? key = Registry.LocalMachine.OpenSubKey(subKey);
                object? value = key?.GetValue("Path");
                if (value != null)
                {
                    string contentDir = Path.Combine(value.ToString(), "Visio Content");
#pragma warning restore CA1416, CS8604
                    if (Directory.Exists(contentDir))
                    {
                        return contentDir;
                    }
                }
            }

            return null;
        }
    }
}
