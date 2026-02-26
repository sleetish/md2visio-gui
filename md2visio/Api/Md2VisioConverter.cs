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
                logger.Error($"Unsupported chart type: {ex.Message}");
                return ConversionResult.Failed($"Unsupported chart type: {ex.Message}", ex);
            }
            catch (System.Runtime.InteropServices.COMException ex)
            {
                logger.Error($"Visio COM error: {ex.Message}");
                return ConversionResult.Failed(
                    "Visio COM error, please ensure Microsoft Visio is correctly installed.",
                    ex);
            }
            catch (Exception ex)
            {
                var root = UnwrapException(ex);
                if (!ReferenceEquals(root, ex))
                {
                    logger.Error($"Conversion failed: {root.GetType().Name}: {root.Message}");
                    return ConversionResult.Failed($"Conversion failed: {root.GetType().Name}: {root.Message}", ex);
                }

                logger.Error($"Conversion failed: {ex.Message}");
                return ConversionResult.Failed($"Conversion failed: {ex.Message}", ex);
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
            // Step 1: Validate input
            progress?.Report(new ConversionProgress(0, "Validating input...", ConversionPhase.Starting));
            logger.Info($"Input file: {request.InputPath}");
            logger.Info($"Output path: {request.OutputPath}");

            if (!File.Exists(request.InputPath))
            {
                return ConversionResult.Failed($"Input file does not exist: {request.InputPath}");
            }

            if (!Path.GetExtension(request.InputPath).Equals(".md", StringComparison.OrdinalIgnoreCase))
            {
                return ConversionResult.Failed("Input file must be in .md format");
            }

            // Ensure output directory exists
            string? outputDir = request.OutputPath.EndsWith(".vsdx", StringComparison.OrdinalIgnoreCase)
                ? Path.GetDirectoryName(request.OutputPath)
                : request.OutputPath;

            if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
                logger.Debug($"Creating output directory: {outputDir}");
            }

            // Step 2: Create conversion context and Visio session
            progress?.Report(new ConversionProgress(20, "Initializing Visio...", ConversionPhase.Starting));
            logger.Info("Initializing conversion context...");

            var context = new ConversionContext(request, logger);
            _session = new VisioSession(request.ShowVisio);

            // Step 3: Parse Mermaid content
            progress?.Report(new ConversionProgress(30, "Parsing Mermaid content...", ConversionPhase.Parsing));
            logger.Info("Parsing Mermaid content...");

            var synContext = new SynContext(request.InputPath);
            SttMermaidStart.Run(synContext);

            if (request.Debug)
            {
                logger.Debug(synContext.ToString());
            }

            // Step 4: Build diagrams
            progress?.Report(new ConversionProgress(50, "Building diagram structure...", ConversionPhase.Building));
            logger.Info("Building diagram structure...");

            var factory = new FigureBuilderFactory(synContext.NewSttIterator(), context, _session);

            // Step 5: Render to Visio
            progress?.Report(new ConversionProgress(70, "Rendering to Visio...", ConversionPhase.Rendering));
            logger.Info("Rendering to Visio format...");

            factory.Build(request.OutputPath);

            if (!string.IsNullOrWhiteSpace(context.LastError))
            {
                if (!request.ShowVisio)
                {
                    _session?.Dispose();
                    _session = null;
                }
                return ConversionResult.Failed(context.LastError);
            }

            // Step 6: Cleanup if Visio is not shown
            progress?.Report(new ConversionProgress(90, "Saving output...", ConversionPhase.Saving));

            if (!request.ShowVisio)
            {
                _session.Dispose();
                _session = null;
            }

            // Step 7: Collect output files and provide detailed feedback
            progress?.Report(new ConversionProgress(100, "Conversion completed!", ConversionPhase.Completed));

            var outputFiles = CollectOutputFiles(request);

            if (outputFiles.Length > 0)
            {
                logger.Info($"Generated {outputFiles.Length} files:");
                foreach (var file in outputFiles)
                {
                    logger.Info($"  - {Path.GetFileName(file)}");
                }
                return ConversionResult.Succeeded(outputFiles);
            }
            else
            {
                // Provide detailed error reason
                if (factory.FiguresBuilt == 0)
                {
                    var supportedTypes = string.Join(", ", TypeMap.BuilderMap.Keys.Distinct().OrderBy(k => k));
                    if (factory.UnsupportedTypes.Count > 0)
                    {
                        var unsupported = string.Join(", ", factory.UnsupportedTypes);
                        return ConversionResult.Failed(
                            $"File contains unsupported diagram types: {unsupported}\n" +
                            $"Currently supported: {supportedTypes}");
                    }
                    else
                    {
                        return ConversionResult.Failed(
                            "No valid Mermaid diagrams found in the file.\n" +
                            "Please ensure diagrams are wrapped in ```mermaid ... ``` blocks.");
                    }
                }

                logger.Warning("Conversion completed but no output files were found.");
                return ConversionResult.Failed("Conversion completed but no output files were generated. Please check output path and permissions.");
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
