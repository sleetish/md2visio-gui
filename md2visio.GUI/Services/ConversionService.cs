using md2visio.Api;

namespace md2visio.GUI.Services
{
    /// <summary>
    /// Mermaid to Visio Conversion Service - Using new API layer
    /// </summary>
    public class ConversionService : IDisposable
    {
        public event EventHandler<ConversionProgressEventArgs>? ProgressChanged;
        public event EventHandler<ConversionLogEventArgs>? LogMessage;

        private IMd2VisioConverter? _converter;
        private bool _disposed = false;
        private readonly object _lock = new object();

        /// <summary>
        /// Convert MD file to Visio (Async)
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
        /// Synchronous conversion method
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
                ReportProgress(0, "Starting conversion...");
                ReportLog($"Input file: {inputFile}");
                ReportLog($"Output directory: {outputDir}");

                // Validate input file
                if (!File.Exists(inputFile))
                    return ConversionResult.Error($"Input file does not exist: {inputFile}");

                if (!Path.GetExtension(inputFile).Equals(".md", StringComparison.OrdinalIgnoreCase))
                    return ConversionResult.Error("Input file must be in .md format");

                // Create output directory
                Directory.CreateDirectory(outputDir);
                ReportProgress(10, "Preparing conversion environment...");

                // Build output path
                string outputPath = BuildOutputPath(outputDir, fileName);
                ReportLog($"Output path: {outputPath}");

                // Create conversion request
                var request = new md2visio.Api.ConversionRequest(
                    inputPath: inputFile,
                    outputPath: outputPath,
                    showVisio: showVisio,
                    silentOverwrite: silentOverwrite,
                    debug: false // 🔒 Security: Disable debug mode by default to prevent info leakage
                );

                // Create progress reporter
                var progress = new Progress<md2visio.Api.ConversionProgress>(p =>
                {
                    int guiProgress = MapProgress(p.Phase, p.Percentage);
                    ReportProgress(guiProgress, p.Message);
                });

                // Create log sink
                var logSink = new GuiLogSink(this);

                // Get or create converter
                lock (_lock)
                {
                    if (showVisio)
                    {
                        // Show mode: Reuse converter to keep Visio window
                        _converter ??= new md2visio.Api.Md2VisioConverter();
                    }
                    else
                    {
                        // Non-show mode: Create new converter each time
                        _converter?.Dispose();
                        _converter = new md2visio.Api.Md2VisioConverter();
                    }
                }

                // Execute conversion
                ReportLog("Executing core conversion logic...");
                var apiResult = _converter.Convert(request, progress, logSink);
                ReportLog("Core conversion logic completed");

                // Immediately release resources in non-show mode
                if (!showVisio)
                {
                    lock (_lock)
                    {
                        _converter?.Dispose();
                        _converter = null;
                    }
                }

                // Conversion result
                if (apiResult.Success)
                {
                    ReportProgress(100, "Conversion completed!");
                    ReportLog($"Successfully generated {apiResult.OutputFiles.Length} files:");
                    foreach (var file in apiResult.OutputFiles)
                    {
                        ReportLog($"  - {Path.GetFileName(file)}");
                    }
                    return ConversionResult.Success(apiResult.OutputFiles);
                }
                else
                {
                    ReportLog($"Conversion failed: {apiResult.ErrorMessage}");
                    return ConversionResult.Error(apiResult.ErrorMessage ?? "Unknown error");
                }
            }
            catch (NotImplementedException ex)
            {
                ReportLog($"Feature not implemented: {ex.Message}");
                return ConversionResult.Error($"This diagram type is not yet supported: {ex.Message}");
            }
            catch (System.Runtime.InteropServices.COMException ex)
            {
                ReportLog($"COM Exception: {ex.Message} (HRESULT: 0x{ex.HResult:X8})");
                return ConversionResult.Error($"COM component exception, possible causes:\n" +
                    "1. Microsoft Visio not correctly installed or registered\n" +
                    "2. Visio process locked or insufficient permissions\n" +
                    "3. System COM component corrupted\n" +
                    $"Detailed error: {ex.Message}");
            }
            catch (Exception ex)
            {
                ReportLog($"Conversion error: {ex.Message}");
                ReportLog($"Exception type: {ex.GetType().Name}");
                if (ex.InnerException != null)
                {
                    ReportLog($"Inner exception: {ex.InnerException.Message}");
                }
                return ConversionResult.Error($"Conversion failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Build output path
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
        /// Map API progress to GUI progress
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
        /// Detect Mermaid diagram types in MD file
        /// </summary>
        public List<string> DetectMermaidTypes(string filePath)
        {
            var types = new HashSet<string>();

            try
            {
                var lines = File.ReadLines(filePath);

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
                        int spaceIndex = trimmed.IndexOf(' ');
                        string type = spaceIndex > -1 ? trimmed.Substring(0, spaceIndex) : trimmed;
                        types.Add(type.ToLower());
                    }
                }
            }
            catch (Exception ex)
            {
                ReportLog($"Error detecting file type: {ex.Message}");
            }

            return types.ToList();
        }

        /// <summary>
        /// Check if Visio is available
        /// </summary>
        public ConversionResult CheckVisioAvailability()
        {
            Microsoft.Office.Interop.Visio.Application? visioApp = null;
            try
            {
                ReportLog("Checking Visio environment...");

                visioApp = new Microsoft.Office.Interop.Visio.Application();
                var version = visioApp.Version;
                ReportLog($"Visio available, version: {version}");
                return ConversionResult.Success(new string[] { $"Visio Version: {version}" });
            }
            catch (System.Runtime.InteropServices.COMException ex)
            {
                ReportLog($"Visio unavailable: {ex.Message}");
                return ConversionResult.Error($"Visio environment check failed:\n" +
                    "1. Please confirm Microsoft Visio is correctly installed\n" +
                    "2. Check if Visio is correctly registered\n" +
                    "3. Try starting Visio manually to test\n" +
                    $"Error details: {ex.Message}");
            }
            catch (Exception ex)
            {
                ReportLog($"Environment check exception: {ex.Message}");
                return ConversionResult.Error($"Environment check failed: {ex.Message}");
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
        /// GUI Log Sink - Forwards API logs to GUI events
        /// </summary>
        private class GuiLogSink : md2visio.Api.ILogSink
        {
            private readonly ConversionService _service;
            private static readonly string[] LevelPrefixes = new[] { "[DEBUG]", "[WARN]", "[ERROR]", "[INFO]" };

            public GuiLogSink(ConversionService service)
            {
                _service = service;
            }

            public void Info(string message) => _service.ReportLog(message);
            public void Debug(string message) => _service.ReportLog(WithPrefix("[DEBUG]", message));
            public void Warning(string message) => _service.ReportLog(WithPrefix("[WARN]", message));
            public void Error(string message) => _service.ReportLog(WithPrefix("[ERROR]", message));

            private static string WithPrefix(string prefix, string message)
            {
                if (string.IsNullOrWhiteSpace(message))
                {
                    return prefix;
                }

                foreach (var levelPrefix in LevelPrefixes)
                {
                    if (message.StartsWith(levelPrefix, StringComparison.OrdinalIgnoreCase))
                    {
                        return message;
                    }
                }

                return $"{prefix} {message}";
            }
        }
    }

    /// <summary>
    /// Conversion Result (Backward compatible)
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
    /// Conversion Progress Event Args
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
    /// Conversion Log Event Args
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
