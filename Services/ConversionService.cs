using md2visio.main;
using md2visio.struc.figure;
using System.Diagnostics;

namespace md2visio.GUI.Services
{
    /// <summary>
    /// Mermaid到Visio转换服务
    /// </summary>
    public class ConversionService
    {
        public event EventHandler<ConversionProgressEventArgs>? ProgressChanged;
        public event EventHandler<ConversionLogEventArgs>? LogMessage;

        /// <summary>
        /// 转换MD文件到Visio
        /// </summary>
        /// <param name="inputFile">输入的MD文件路径</param>
        /// <param name="outputDir">输出目录</param>
        /// <param name="showVisio">是否显示Visio窗口</param>
        /// <param name="silentOverwrite">是否静默覆盖</param>
        /// <returns>转换结果</returns>
        public async Task<ConversionResult> ConvertAsync(string inputFile, string outputDir, bool showVisio = false, bool silentOverwrite = false)
        {
            return await Task.Run(() => Convert(inputFile, outputDir, showVisio, silentOverwrite));
        }

        /// <summary>
        /// 同步转换方法
        /// </summary>
        private ConversionResult Convert(string inputFile, string outputDir, bool showVisio, bool silentOverwrite)
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
                ReportProgress(20, "Preparing conversion environment...");

                // 构建参数
                var args = new List<string>
                {
                    "/I", $"\"{inputFile}\"",
                    "/O", $"\"{outputDir}\""
                };

                if (showVisio) args.Add("/V");
                if (silentOverwrite) args.Add("/Y");

                ReportProgress(40, "Executing conversion...");
                ReportLog($"Conversion parameters: {string.Join(" ", args)}");

                // Call AppConfig for conversion
                var config = new AppConfig();
                if (!config.LoadArguments(args.ToArray()))
                {
                    return ConversionResult.Error("Failed to parse parameters");
                }

                ReportProgress(60, "Parsing Mermaid content...");

                // Execute conversion
                config.Main();

                ReportProgress(80, "Generating Visio files...");

                // Find generated files
                var outputFiles = Directory.GetFiles(outputDir, "*.vsdx");
                
                ReportProgress(100, "Conversion completed!");

                if (outputFiles.Length > 0)
                {
                    ReportLog($"Successfully generated {outputFiles.Length} files:");
                    foreach (var file in outputFiles)
                    {
                        ReportLog($"  - {Path.GetFileName(file)}");
                    }
                    return ConversionResult.Success(outputFiles);
                }
                else
                {
                    return ConversionResult.Error("Conversion completed but no output files were found");
                }
            }
            catch (NotImplementedException ex)
            {
                ReportLog($"Feature not implemented: {ex.Message}");
                return ConversionResult.Error($"This diagram type is not yet supported: {ex.Message}");
            }
            catch (Exception ex)
            {
                ReportLog($"Error during conversion: {ex.Message}");
                return ConversionResult.Error($"Conversion failed: {ex.Message}");
            }
        }

        /// <summary>
        /// 检测MD文件中的Mermaid图表类型
        /// </summary>
        /// <param name="filePath">MD文件路径</param>
        /// <returns>检测到的图表类型列表</returns>
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
                        // 检测图表类型
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
                ReportLog($"Error detecting file type: {ex.Message}");
            }
            
            return types;
        }

        private void ReportProgress(int percentage, string message)
        {
            ProgressChanged?.Invoke(this, new ConversionProgressEventArgs(percentage, message));
        }

        private void ReportLog(string message)
        {
            LogMessage?.Invoke(this, new ConversionLogEventArgs(DateTime.Now, message));
        }
    }

    /// <summary>
    /// 转换结果
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