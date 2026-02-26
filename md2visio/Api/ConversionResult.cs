namespace md2visio.Api
{
    /// <summary>
    /// Conversion Result
    /// </summary>
    public sealed class ConversionResult
    {
        /// <summary>
        /// Whether successful
        /// </summary>
        public bool Success { get; }

        /// <summary>
        /// Generated output file paths
        /// </summary>
        public string[] OutputFiles { get; }

        /// <summary>
        /// Error message (if failed)
        /// </summary>
        public string? ErrorMessage { get; }

        /// <summary>
        /// Exception details (for debugging)
        /// </summary>
        public Exception? Exception { get; }

        private ConversionResult(bool success, string[] outputFiles, string? errorMessage, Exception? exception)
        {
            Success = success;
            OutputFiles = outputFiles;
            ErrorMessage = errorMessage;
            Exception = exception;
        }

        /// <summary>
        /// Create successful result
        /// </summary>
        public static ConversionResult Succeeded(params string[] outputFiles)
        {
            return new ConversionResult(true, outputFiles ?? Array.Empty<string>(), null, null);
        }

        /// <summary>
        /// Create failed result
        /// </summary>
        public static ConversionResult Failed(string errorMessage, Exception? exception = null)
        {
            return new ConversionResult(false, Array.Empty<string>(), errorMessage, exception);
        }
    }
}
