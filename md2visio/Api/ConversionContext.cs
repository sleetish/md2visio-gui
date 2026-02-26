namespace md2visio.Api
{
    /// <summary>
    /// Conversion Context (Runtime context for each conversion)
    /// Used to replace global state AppConfig.Instance
    /// </summary>
    public sealed class ConversionContext
    {
        /// <summary>
        /// Conversion Request Parameters
        /// </summary>
        public ConversionRequest Options { get; }

        /// <summary>
        /// Log Sink
        /// </summary>
        public ILogSink Logger { get; }

        #region Shortcut Properties (Reduce call site changes)

        /// <summary>
        /// Whether debug mode is enabled
        /// </summary>
        public bool Debug => Options.Debug;

        /// <summary>
        /// Whether to show Visio window
        /// </summary>
        public bool Visible => Options.ShowVisio;

        /// <summary>
        /// Input file path
        /// </summary>
        public string InputFile => Options.InputPath;

        /// <summary>
        /// Output path
        /// </summary>
        public string OutputPath => Options.OutputPath;

        /// <summary>
        /// Whether silent overwrite
        /// </summary>
        public bool Quiet => Options.SilentOverwrite;

        #endregion

        /// <summary>
        /// Record last error message (for cross-layer feedback)
        /// </summary>
        public string? LastError { get; private set; }

        private readonly List<string> _generatedFiles = new();

        /// <summary>
        /// Successfully generated files during this session
        /// </summary>
        public IReadOnlyList<string> GeneratedFiles => _generatedFiles;

        public ConversionContext(ConversionRequest options, ILogSink? logger = null)
        {
            Options = options ?? throw new ArgumentNullException(nameof(options));
            Logger = logger ?? NullLogSink.Instance;
        }

        /// <summary>
        /// Output debug log (only in Debug mode)
        /// </summary>
        public void Log(string message)
        {
            if (Debug)
            {
                Logger.Debug(message);
            }
        }

        /// <summary>
        /// Output info log
        /// </summary>
        public void LogInfo(string message)
        {
            Logger.Info(message);
        }

        /// <summary>
        /// Output warning log
        /// </summary>
        public void LogWarning(string message)
        {
            Logger.Warning(message);
        }

        /// <summary>
        /// Output error log
        /// </summary>
        public void LogError(string message)
        {
            Logger.Error(message);
        }

        /// <summary>
        /// Record error and save to context for upper layer to terminate conversion
        /// </summary>
        public void SetError(string message)
        {
            LastError = message;
            Logger.Error(message);
        }

        /// <summary>
        /// Record a successfully generated file
        /// </summary>
        public void AddGeneratedFile(string path)
        {
            if (!_generatedFiles.Contains(path))
            {
                _generatedFiles.Add(path);
            }
        }
    }
}
