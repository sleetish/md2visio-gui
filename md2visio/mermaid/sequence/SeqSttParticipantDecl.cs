using md2visio.mermaid.cmn;
using System.Text.RegularExpressions;

namespace md2visio.mermaid.sequence
{
    internal class SeqSttParticipantDecl : SynState
    {
        private string participantId = string.Empty;
        private string participantAlias = string.Empty;

        public override SynState NextState()
        {
            // Parse participant declaration: participant a as User
            string declText = Buffer.ToString();
            
            if (!TryParseParticipantDecl(declText))
            {
                throw new SynException($"Invalid participant declaration: '{declText}'", Ctx);
            }

            Save(declText).ClearBuffer();
            return Forward<SeqSttChar>();
        }

        private bool TryParseParticipantDecl(string declText)
        {
            // Match "id as alias" or single "id"
            var patternWithAlias = @"^\s*(\w+)\s+as\s+(.+)$";
            var patternIdOnly = @"^\s*(\w+)\s*$";
            
            var matchWithAlias = Regex.Match(declText, patternWithAlias);
            if (matchWithAlias.Success)
            {
                participantId = matchWithAlias.Groups[1].Value.Trim();
                participantAlias = matchWithAlias.Groups[2].Value.Trim();
                return true;
            }
            
            var matchIdOnly = Regex.Match(declText, patternIdOnly);
            if (matchIdOnly.Success)
            {
                participantId = matchIdOnly.Groups[1].Value.Trim();
                participantAlias = string.Empty;
                return true;
            }
            
            return false;
        }

        public string GetParticipantId() => participantId;
        public string GetParticipantAlias() => participantAlias;
    }
}