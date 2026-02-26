namespace md2visio.Api
{
    /// <summary>
    /// Conversion Progress Information
    /// </summary>
    public sealed class ConversionProgress
    {
        /// <summary>
        /// Progress percentage (0-100)
        /// </summary>
        public int Percentage { get; }

        /// <summary>
        /// Status message
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Current phase
        /// </summary>
        public ConversionPhase Phase { get; }

        public ConversionProgress(int percentage, string message, ConversionPhase phase)
        {
            Percentage = Math.Clamp(percentage, 0, 100);
            Message = message ?? string.Empty;
            Phase = phase;
        }
    }

    /// <summary>
    /// Conversion Phase Enum
    /// </summary>
    public enum ConversionPhase
    {
        /// <summary>Starting</summary>
        Starting,
        /// <summary>Parsing Mermaid Syntax</summary>
        Parsing,
        /// <summary>Building Diagram Data Structure</summary>
        Building,
        /// <summary>Rendering to Visio</summary>
        Rendering,
        /// <summary>Saving File</summary>
        Saving,
        /// <summary>Completed</summary>
        Completed
    }
}
