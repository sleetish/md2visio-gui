namespace md2visio.mermaid.cmn
{
    /// <summary>
    /// Skips unsupported mermaid diagram content until closing backticks.
    /// This allows graceful handling of unimplemented diagram types.
    /// </summary>
    internal class SttUnsupported : SynState
    {
        public override SynState NextState()
        {
            // Save the unsupported diagram type keyword
            Save(Buffer).ClearBuffer();

            // Skip all content until we hit the closing mermaid block
            while (true)
            {
                if (SttMermaidClose.IsMermaidClose(Ctx))
                {
                    return Forward<SttMermaidClose>();
                }

                if (Ctx.Peek() == null) return EndOfFile;

                Ctx.Slide();
            }
        }
    }
}
