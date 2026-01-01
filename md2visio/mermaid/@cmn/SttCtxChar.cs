using md2visio.mermaid.graph.@internal;
using md2visio.mermaid.pie;

namespace md2visio.mermaid.cmn
{
    internal class SttCtxChar : SynState
    {
        Dictionary<string, Type> typeMap = TypeMap.CharMap;

        public override SynState NextState()
        {
            // Only search for figure type within the current mermaid block
            // (after the most recent SttMermaidStart)
            (bool success, string graph) = FindFigureTypeInCurrentBlock();
            if (success)
            {
                // Check if the type is implemented in CharMap
                if (typeMap.TryGetValue(graph, out var charType))
                    return Forward(charType);
                // Skip unsupported diagram type
                return Forward<SttUnsupported>();
            }
            return Forward<SttIntro>();
        }

        /// <summary>
        /// Find figure type only within the current mermaid block
        /// </summary>
        (bool Success, string Fragment) FindFigureTypeInCurrentBlock()
        {
            var stateList = Ctx.StateList;
            for (int i = stateList.Count - 1; i >= 0; i--)
            {
                var state = stateList[i];
                // Stop searching when we hit the start of current mermaid block
                if (state is SttMermaidStart)
                    return (false, string.Empty);

                string frag = state.Fragment;
                if (System.Text.RegularExpressions.Regex.IsMatch(frag, $"^({SttFigureType.Supported})$"))
                    return (true, frag);
            }
            return (false, string.Empty);
        }
    }
}
