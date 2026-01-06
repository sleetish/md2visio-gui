using md2visio.Api;

namespace md2visio.GUI.Services
{
    /// <summary>
    /// Mermaid 到 Visio 转换服务 - 使用新的 API 层
    /// </summary>
    public class ConversionService : IDisposable
    {
        public event EventHandler<ConversionProgressEventArgs>? ProgressChanged;
        public event EventHandler<ConversionLogEventArgs>? LogMessage;

        private IMd2VisioConverter? _converter;
        private bool _disposed = false;
        private readonly object _lock = new object();

        /// <summary>
        /// 转换 MD 文件到 Visio（异步）
        /// </summary>
        public async Task<ConversionResult> ConvertAsync(
            string inputFile,
            string outputDir,
            string? fileName = null,
            bool showVisio = false,
            bool silentOverwrite = false)
        {
            return await RunOnStaThread(() => Convert(inputFile, outputDir, fileName, showVisio, silentOverwrite));
        }

        private Task<ConversionResult> RunOnStaThread(Func<ConversionResult> work)
        {
            var tcs = new TaskCompletionSource<ConversionResult>(TaskCreationOptions.RunContinuationsAsynchronously);

            var thread = new Thread(() =>
            {
                try
                {
                    tcs.SetResult(work());
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            })
            {
                IsBackground = true
            };

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();

            return tcs.Task;
        }

        /// <summary>
        /// 同步转换方法
        /// </summary>
        private ConversionResult Convert(
            string inputFile,
            string outputDir,
            string? fileName,
            bool showVisio,
            bool silentOverwrite)
        {
            try
            {
                ReportProgress(0, "开始转换...");
                ReportLog($"输入文件: {inputFile}");
                ReportLog($"输出目录: {outputDir}");

                // 验证输入文件
                if (!File.Exists(inputFile))
                    return ConversionResult.Error($"输入文件不存在: {inputFile}");

                if (!Path.GetExtension(inputFile).Equals(".md", StringComparison.OrdinalIgnoreCase))
                    return ConversionResult.Error("输入文件必须是 .md 格式");

                // 创建输出目录
                Directory.CreateDirectory(outputDir);
                ReportProgress(10, "准备转换环境...");

                // 构建输出路径
                string outputPath = BuildOutputPath(outputDir, fileName);
                ReportLog($"输出路径: {outputPath}");

                // 创建转换请求
                var request = new md2visio.Api.ConversionRequest(
                    inputPath: inputFile,
                    outputPath: outputPath,
                    showVisio: showVisio,
                    silentOverwrite: silentOverwrite,
                    debug: true  // 保持调试模式以获取详细日志
                );

                // 创建进度报告器
                var progress = new Progress<md2visio.Api.ConversionProgress>(p =>
                {
                    int guiProgress = MapProgress(p.Phase, p.Percentage);
                    ReportProgress(guiProgress, p.Message);
                });

                // 创建日志接收器
                var logSink = new GuiLogSink(this);

                // 获取或创建转换器
                lock (_lock)
                {
                    if (showVisio)
                    {
                        // 显示模式：复用转换器以保持 Visio 窗口
                        _converter ??= new md2visio.Api.Md2VisioConverter();
                    }
                    else
                    {
                        // 非显示模式：每次创建新的转换器
                        _converter?.Dispose();
                        _converter = new md2visio.Api.Md2VisioConverter();
                    }
                }

                // 执行转换
                ReportLog("开始执行核心转换逻辑...");
                var apiResult = _converter.Convert(request, progress, logSink);
                ReportLog("核心转换逻辑执行完成");

                // 非显示模式立即释放资源
                if (!showVisio)
                {
                    lock (_lock)
                    {
                        _converter?.Dispose();
                        _converter = null;
                    }
                }

                // 转换结果
                if (apiResult.Success)
                {
                    ReportProgress(100, "转换完成!");
                    ReportLog($"成功生成 {apiResult.OutputFiles.Length} 个文件:");
                    foreach (var file in apiResult.OutputFiles)
                    {
                        ReportLog($"  - {Path.GetFileName(file)}");
                    }
                    return ConversionResult.Success(apiResult.OutputFiles);
                }
                else
                {
                    ReportLog($"转换失败: {apiResult.ErrorMessage}");
                    return ConversionResult.Error(apiResult.ErrorMessage ?? "未知错误");
                }
            }
            catch (NotImplementedException ex)
            {
                ReportLog($"功能未实现: {ex.Message}");
                return ConversionResult.Error($"该图表类型暂未支持: {ex.Message}");
            }
            catch (System.Runtime.InteropServices.COMException ex)
            {
                ReportLog($"COM异常: {ex.Message} (HRESULT: 0x{ex.HResult:X8})");
                return ConversionResult.Error($"COM组件异常，可能的原因：\n" +
                    "1. Microsoft Visio未正确安装或注册\n" +
                    "2. Visio进程被锁定或权限不足\n" +
                    "3. 系统COM组件损坏\n" +
                    $"详细错误: {ex.Message}");
            }
            catch (Exception ex)
            {
                ReportLog($"转换出错: {ex.Message}");
                ReportLog($"异常类型: {ex.GetType().Name}");
                if (ex.InnerException != null)
                {
                    ReportLog($"内部异常: {ex.InnerException.Message}");
                }
                return ConversionResult.Error($"转换失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 构建输出路径
        /// </summary>
        private string BuildOutputPath(string outputDir, string? fileName)
        {
            if (!string.IsNullOrEmpty(fileName))
            {
                if (!fileName.EndsWith(".vsdx", StringComparison.OrdinalIgnoreCase))
                    fileName += ".vsdx";
                return Path.Combine(outputDir, fileName);
            }
            return outputDir;
        }

        /// <summary>
        /// 映射 API 进度到 GUI 进度
        /// </summary>
        private int MapProgress(md2visio.Api.ConversionPhase phase, int apiPercentage)
        {
            return phase switch
            {
                md2visio.Api.ConversionPhase.Starting => 10,
                md2visio.Api.ConversionPhase.Parsing => 30,
                md2visio.Api.ConversionPhase.Building => 50,
                md2visio.Api.ConversionPhase.Rendering => 70,
                md2visio.Api.ConversionPhase.Saving => 90,
                md2visio.Api.ConversionPhase.Completed => 100,
                _ => apiPercentage
            };
        }

        /// <summary>
        /// 检测 MD 文件中的 Mermaid 图表类型
        /// </summary>
        public List<string> DetectMermaidTypes(string filePath)
        {
            var types = new List<string>();

            try
            {
                var content = File.ReadAllText(filePath);
                var lines = content.Split('\n');

                bool inMermaidBlock = false;
                foreach (var line in lines)
                {
                    var trimmed = line.Trim();

                    if (trimmed.StartsWith("```mermaid"))
                    {
                        inMermaidBlock = true;
                        continue;
                    }

                    if (trimmed.StartsWith("```") && inMermaidBlock)
                    {
                        inMermaidBlock = false;
                        continue;
                    }

                    if (inMermaidBlock && !string.IsNullOrWhiteSpace(trimmed))
                    {
                        var words = trimmed.Split(' ');
                        if (words.Length > 0)
                        {
                            var type = words[0].ToLower();
                            if (!types.Contains(type))
                            {
                                types.Add(type);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ReportLog($"检测文件类型时出错: {ex.Message}");
            }

            return types;
        }

        /// <summary>
        /// 检查 Visio 是否可用
        /// </summary>
        public ConversionResult CheckVisioAvailability()
        {
            Microsoft.Office.Interop.Visio.Application? visioApp = null;
            try
            {
                ReportLog("正在检查Visio环境...");

                visioApp = new Microsoft.Office.Interop.Visio.Application();
                var version = visioApp.Version;
                ReportLog($"Visio可用，版本: {version}");
                return ConversionResult.Success(new string[] { $"Visio版本: {version}" });
            }
            catch (System.Runtime.InteropServices.COMException ex)
            {
                ReportLog($"Visio不可用: {ex.Message}");
                return ConversionResult.Error($"Visio环境检查失败：\n" +
                    "1. 请确认Microsoft Visio已正确安装\n" +
                    "2. 检查Visio是否已正确注册\n" +
                    "3. 尝试手动启动Visio测试\n" +
                    $"错误详情: {ex.Message}");
            }
            catch (Exception ex)
            {
                ReportLog($"环境检查异常: {ex.Message}");
                return ConversionResult.Error($"环境检查失败: {ex.Message}");
            }
            finally
            {
                if (visioApp != null)
                {
                    try
                    {
                        visioApp.Quit();
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(visioApp);
                    }
                    catch { }
                }
            }
        }

        private void ReportProgress(int percentage, string message)
        {
            ProgressChanged?.Invoke(this, new ConversionProgressEventArgs(percentage, message));
        }

        private void ReportLog(string message)
        {
            LogMessage?.Invoke(this, new ConversionLogEventArgs(DateTime.Now, message));
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                lock (_lock)
                {
                    _converter?.Dispose();
                    _converter = null;
                }
            }

            _disposed = true;
        }

        ~ConversionService()
        {
            Dispose(false);
        }

        /// <summary>
        /// GUI 日志接收器 - 将 API 日志转发到 GUI 事件
        /// </summary>
        private class GuiLogSink : md2visio.Api.ILogSink
        {
            private readonly ConversionService _service;

            public GuiLogSink(ConversionService service)
            {
                _service = service;
            }

            public void Info(string message) => _service.ReportLog(message);
            public void Debug(string message) => _service.ReportLog($"[DEBUG] {message}");
            public void Warning(string message) => _service.ReportLog($"[WARN] {message}");
            public void Error(string message) => _service.ReportLog($"[ERROR] {message}");
        }
    }

    /// <summary>
    /// 转换结果（保持向后兼容）
    /// </summary>
    public class ConversionResult
    {
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
        public string[]? OutputFiles { get; set; }

        public static ConversionResult Success(string[] outputFiles)
        {
            return new ConversionResult { IsSuccess = true, OutputFiles = outputFiles };
        }

        public static ConversionResult Error(string message)
        {
            return new ConversionResult { IsSuccess = false, ErrorMessage = message };
        }
    }

    /// <summary>
    /// 转换进度事件参数
    /// </summary>
    public class ConversionProgressEventArgs : EventArgs
    {
        public int Percentage { get; }
        public string Message { get; }

        public ConversionProgressEventArgs(int percentage, string message)
        {
            Percentage = percentage;
            Message = message;
        }
    }

    /// <summary>
    /// 转换日志事件参数
    /// </summary>
    public class ConversionLogEventArgs : EventArgs
    {
        public DateTime Timestamp { get; }
        public string Message { get; }

        public ConversionLogEventArgs(DateTime timestamp, string message)
        {
            Timestamp = timestamp;
            Message = message;
        }
    }
}
