using Visio = Microsoft.Office.Interop.Visio;

namespace md2visio.vsdx.@base
{
    /// <summary>
    /// Visio COM Session Interface
    /// Used to manage Visio Application lifecycle
    /// </summary>
    public interface IVisioSession : IDisposable
    {
        /// <summary>
        /// Visio Application Instance
        /// </summary>
        Visio.Application Application { get; }

        /// <summary>
        /// Whether to show Visio window
        /// </summary>
        bool Visible { get; }

        /// <summary>
        /// Create a new blank document
        /// </summary>
        Visio.Document CreateDocument();

        /// <summary>
        /// Open stencil document
        /// </summary>
        Visio.Document OpenStencil(string path);

        /// <summary>
        /// Save document to specified path
        /// </summary>
        void SaveDocument(Visio.Document doc, string path, bool overwrite = true);

        /// <summary>
        /// Close document
        /// </summary>
        void CloseDocument(Visio.Document doc);
    }
}
