using System.Runtime.InteropServices;
using Microsoft.Win32;
using Visio = Microsoft.Office.Interop.Visio;

namespace md2visio.vsdx.@base
{
    /// <summary>
    /// Visio COM 会话实现
    /// 管理单个 Visio Application 实例的完整生命周期
    /// </summary>
    public sealed class VisioSession : IVisioSession
    {
        private Visio.Application? _app;
        private bool _disposed;
        private readonly object _lock = new();

        /// <summary>
        /// 是否显示 Visio 窗口
        /// </summary>
        public bool Visible { get; }

        /// <summary>
        /// Visio 应用程序实例
        /// </summary>
        public Visio.Application Application
        {
            get
            {
                ObjectDisposedException.ThrowIf(_disposed, this);
                return _app ?? throw new InvalidOperationException("Visio Application not initialized");
            }
        }

        /// <summary>
        /// 创建 Visio 会话
        /// </summary>
        /// <param name="visible">是否显示 Visio 窗口</param>
        public VisioSession(bool visible = false)
        {
            Visible = visible;
            EnsureVisioApp();
        }

        /// <summary>
        /// 确保 Visio 应用程序可用
        /// </summary>
        private void EnsureVisioApp()
        {
            lock (_lock)
            {
                try
                {
                    if (_app != null)
                    {
                        // 测试 COM 对象是否有效
                        _ = _app.Version;
                        return;
                    }
                }
                catch (COMException ex)
                {
                    Console.WriteLine($"COM exception, recreating Visio application: {ex.Message}");
                    _app = null;
                }
                catch (InvalidComObjectException ex)
                {
                    Console.WriteLine($"COM object released, recreating: {ex.Message}");
                    _app = null;
                }

                try
                {
                    Console.WriteLine("Creating Visio application...");
                    _app = new Visio.Application();
                    _app.Visible = Visible;
                    Console.WriteLine($"Visio application created successfully, version: {_app.Version}");
                }
                catch (COMException ex)
                {
                    throw new ApplicationException(
                        $"Cannot create Visio application. Please ensure:\n" +
                        $"1. Microsoft Visio is correctly installed\n" +
                        $"2. Current user has permission to access Visio\n" +
                        $"3. Visio is not locked by another process\n" +
                        $"Error details: {ex.Message}", ex);
                }
                catch (Exception ex)
                {
                    throw new ApplicationException(
                        $"Unknown error occurred while creating Visio application: {ex.Message}", ex);
                }
            }
        }

        /// <summary>
        /// 创建新的空白文档
        /// </summary>
        public Visio.Document CreateDocument()
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            return Application.Documents.Add("");
        }

        /// <summary>
        /// 打开模板文档
        /// </summary>
        public Visio.Document OpenStencil(string path)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            return Application.Documents.OpenEx(path, (short)Visio.VisOpenSaveArgs.visOpenDocked);
        }

        /// <summary>
        /// 保存文档到指定路径
        /// </summary>
        public void SaveDocument(Visio.Document doc, string path, bool overwrite = true)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            if (!overwrite && File.Exists(path))
            {
                doc.Saved = true;
                return;
            }

            doc.SaveAsEx(path, 0);
        }

        /// <summary>
        /// 关闭文档
        /// </summary>
        public void CloseDocument(Visio.Document doc)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            if (!Visible)
            {
                doc.Close();
            }
            else
            {
                // 显示模式下保持文档打开，仅标记为已保存
                doc.Saved = true;
            }
        }

        /// <summary>
        /// 释放 Visio COM 资源
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;

            lock (_lock)
            {
                if (_disposed) return;

                try
                {
                    if (_app != null && !Visible)
                    {
                        // 非显示模式：退出 Visio 应用
                        _app.Quit();
                    }
                }
                catch (COMException)
                {
                    // Visio 可能已被用户手动关闭，忽略异常
                }
                finally
                {
                    _app = null;
                    _disposed = true;
                }
            }
        }

        /// <summary>
        /// 获取 Visio 内容目录路径
        /// </summary>
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
