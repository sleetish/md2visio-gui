namespace md2visio.Api
{
    /// <summary>
    /// 转换上下文（每次转换的运行时上下文）
    /// 用于替代 AppConfig.Instance 全局状态
    /// </summary>
    public sealed class ConversionContext
    {
        /// <summary>
        /// 转换请求参数
        /// </summary>
        public ConversionRequest Options { get; }

        /// <summary>
        /// 日志接收器
        /// </summary>
        public ILogSink Logger { get; }

        #region 快捷属性（减少调用点修改）

        /// <summary>
        /// 是否启用调试模式
        /// </summary>
        public bool Debug => Options.Debug;

        /// <summary>
        /// 是否显示 Visio 窗口
        /// </summary>
        public bool Visible => Options.ShowVisio;

        /// <summary>
        /// 输入文件路径
        /// </summary>
        public string InputFile => Options.InputPath;

        /// <summary>
        /// 输出路径
        /// </summary>
        public string OutputPath => Options.OutputPath;

        /// <summary>
        /// 是否静默覆盖
        /// </summary>
        public bool Quiet => Options.SilentOverwrite;

        #endregion

        /// <summary>
        /// 记录最后一次错误信息（用于跨层反馈）
        /// </summary>
        public string? LastError { get; private set; }

        public ConversionContext(ConversionRequest options, ILogSink? logger = null)
        {
            Options = options ?? throw new ArgumentNullException(nameof(options));
            Logger = logger ?? NullLogSink.Instance;
        }

        /// <summary>
        /// 输出调试日志（仅在 Debug 模式下）
        /// </summary>
        public void Log(string message)
        {
            if (Debug)
            {
                Logger.Debug(message);
            }
        }

        /// <summary>
        /// 输出信息日志
        /// </summary>
        public void LogInfo(string message)
        {
            Logger.Info(message);
        }

        /// <summary>
        /// 输出警告日志
        /// </summary>
        public void LogWarning(string message)
        {
            Logger.Warning(message);
        }

        /// <summary>
        /// 输出错误日志
        /// </summary>
        public void LogError(string message)
        {
            Logger.Error(message);
        }

        /// <summary>
        /// 记录错误并保存到上下文，供上层终止转换
        /// </summary>
        public void SetError(string message)
        {
            LastError = message;
            Logger.Error(message);
        }
    }
}
