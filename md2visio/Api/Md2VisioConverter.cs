using md2visio.mermaid.cmn;
using md2visio.struc.figure;
using md2visio.vsdx.@base;
using System.Reflection;

namespace md2visio.Api
{
    /// <summary>
    /// Mermaid 到 Visio 转换器实现
    /// 包装现有转换逻辑，提供简洁的 API
    /// </summary>
    public sealed class Md2VisioConverter : IMd2VisioConverter
    {
        private IVisioSession? _session;
        private bool _disposed;
        private readonly object _lock = new object();

        /// <summary>
        /// 执行转换
        /// </summary>
        public ConversionResult Convert(
            ConversionRequest request,
            IProgress<ConversionProgress>? progress = null,
            ILogSink? logger = null)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(Md2VisioConverter));

            logger ??= NullLogSink.Instance;

            try
            {
                return ConvertInternal(request, progress, logger);
            }
            catch (NotImplementedException ex)
            {
                logger.Error($"不支持的图表类型: {ex.Message}");
                return ConversionResult.Failed($"不支持的图表类型: {ex.Message}", ex);
            }
            catch (System.Runtime.InteropServices.COMException ex)
            {
                logger.Error($"Visio COM 错误: {ex.Message}");
                return ConversionResult.Failed(
                    "Visio COM 错误，请确保 Microsoft Visio 已正确安装。",
                    ex);
            }
            catch (Exception ex)
            {
                var root = UnwrapException(ex);
                if (!ReferenceEquals(root, ex))
                {
                    logger.Error($"转换失败: {root.GetType().Name}: {root.Message}");
                    return ConversionResult.Failed($"转换失败: {root.GetType().Name}: {root.Message}", ex);
                }

                logger.Error($"转换失败: {ex.Message}");
                return ConversionResult.Failed($"转换失败: {ex.Message}", ex);
            }
        }

        private static Exception UnwrapException(Exception ex)
        {
            Exception current = ex;
            if (current is TargetInvocationException tie && tie.InnerException != null)
            {
                current = tie.InnerException;
            }

            while (current.InnerException != null)
            {
                current = current.InnerException;
            }

            return current;
        }

        private ConversionResult ConvertInternal(
            ConversionRequest request,
            IProgress<ConversionProgress>? progress,
            ILogSink logger)
        {
            // Step 1: 验证输入
            progress?.Report(new ConversionProgress(0, "验证输入...", ConversionPhase.Starting));
            logger.Info($"输入文件: {request.InputPath}");
            logger.Info($"输出路径: {request.OutputPath}");

            if (!File.Exists(request.InputPath))
            {
                return ConversionResult.Failed($"输入文件不存在: {request.InputPath}");
            }

            if (!Path.GetExtension(request.InputPath).Equals(".md", StringComparison.OrdinalIgnoreCase))
            {
                return ConversionResult.Failed("输入文件必须是 .md 格式");
            }

            // 确保输出目录存在
            string? outputDir = request.OutputPath.EndsWith(".vsdx", StringComparison.OrdinalIgnoreCase)
                ? Path.GetDirectoryName(request.OutputPath)
                : request.OutputPath;

            if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
                logger.Debug($"创建输出目录: {outputDir}");
            }

            // Step 2: 创建转换上下文和 Visio 会话
            progress?.Report(new ConversionProgress(20, "初始化 Visio...", ConversionPhase.Starting));
            logger.Info("初始化转换上下文...");

            var context = new ConversionContext(request, logger);
            _session = new VisioSession(request.ShowVisio);

            // Step 3: 解析 Mermaid 内容
            progress?.Report(new ConversionProgress(30, "解析 Mermaid 内容...", ConversionPhase.Parsing));
            logger.Info("解析 Mermaid 内容...");

            var synContext = new SynContext(request.InputPath);
            SttMermaidStart.Run(synContext);

            if (request.Debug)
            {
                logger.Debug(synContext.ToString());
            }

            // Step 4: 构建图表
            progress?.Report(new ConversionProgress(50, "构建图表结构...", ConversionPhase.Building));
            logger.Info("构建图表结构...");

            var factory = new FigureBuilderFactory(synContext.NewSttIterator(), context, _session);

            // Step 5: 渲染到 Visio
            progress?.Report(new ConversionProgress(70, "渲染到 Visio...", ConversionPhase.Rendering));
            logger.Info("渲染到 Visio 格式...");

            factory.Build(request.OutputPath);

            // Step 6: 如果不显示 Visio 则清理
            progress?.Report(new ConversionProgress(90, "保存输出...", ConversionPhase.Saving));

            if (!request.ShowVisio)
            {
                _session.Dispose();
                _session = null;
            }

            // Step 7: 收集输出文件并提供详细反馈
            progress?.Report(new ConversionProgress(100, "转换完成!", ConversionPhase.Completed));

            var outputFiles = CollectOutputFiles(request);

            if (outputFiles.Length > 0)
            {
                logger.Info($"生成 {outputFiles.Length} 个文件:");
                foreach (var file in outputFiles)
                {
                    logger.Info($"  - {Path.GetFileName(file)}");
                }
                return ConversionResult.Succeeded(outputFiles);
            }
            else
            {
                // 提供详细的错误原因
                if (factory.FiguresBuilt == 0)
                {
                    var supportedTypes = string.Join(", ", TypeMap.BuilderMap.Keys.Distinct().OrderBy(k => k));
                    if (factory.UnsupportedTypes.Count > 0)
                    {
                        var unsupported = string.Join(", ", factory.UnsupportedTypes);
                        return ConversionResult.Failed(
                            $"文件中包含不支持的图表类型: {unsupported}\n" +
                            $"当前支持: {supportedTypes}");
                    }
                    else
                    {
                        return ConversionResult.Failed(
                            "文件中未找到有效的 Mermaid 图表。\n" +
                            "请确保使用 ```mermaid ... ``` 格式包裹图表代码。");
                    }
                }

                logger.Warning("转换完成但未找到输出文件。");
                return ConversionResult.Failed("转换完成但未生成输出文件。请检查输出路径和权限设置。");
            }
        }

        /// <summary>
        /// 收集输出文件
        /// </summary>
        private string[] CollectOutputFiles(ConversionRequest request)
        {
            if (request.OutputPath.EndsWith(".vsdx", StringComparison.OrdinalIgnoreCase))
            {
                // 文件模式：检查指定文件
                return File.Exists(request.OutputPath)
                    ? new[] { request.OutputPath }
                    : Array.Empty<string>();
            }
            else
            {
                // 目录模式：查找所有 .vsdx 文件
                return Directory.Exists(request.OutputPath)
                    ? Directory.GetFiles(request.OutputPath, "*.vsdx")
                    : Array.Empty<string>();
            }
        }

        public void Dispose()
        {
            if (_disposed) return;

            lock (_lock)
            {
                if (_disposed) return;

                _session?.Dispose();
                _session = null;

                _disposed = true;
            }
        }
    }
}
