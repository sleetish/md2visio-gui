using md2visio.Api;
using md2visio.mermaid.cmn;
using md2visio.mermaid.sequence;
using md2visio.struc.figure;
using md2visio.vsdx.@base;
using System.Text.RegularExpressions;

namespace md2visio.struc.sequence
{
    internal class SeqBuilder : FigureBuilder
    {
        private readonly Sequence sequence = new Sequence();
        private readonly Stack<SeqActivation> activationStack = new Stack<SeqActivation>();
        private readonly Stack<SeqFragment> fragmentStack = new Stack<SeqFragment>();
        private double currentY = 0;
        private int autoNumberCounter = 0;
        private const double LayoutScale = 15.0;
        private const double DefaultMessageSpacing = 375;
        private double messageSpacing = DefaultMessageSpacing;
        private double fragmentHeaderPadding;
        private double fragmentSectionPadding;
        private double fragmentEndPadding;

        public SeqBuilder(SttIterator iter, ConversionContext context, IVisioSession session)
            : base(iter, context, session)
        {
        }

        public override void Build(string outputFile)
        {
            if (_context.Debug)
            {
                _context.Log($"[DEBUG] SeqBuilder.Build: Start building, output file: {outputFile}");
            }

            currentY = 0;
            RefreshMessageSpacing();

            while (iter.HasNext())
            {
                SynState cur = iter.Next();

                if (_context.Debug)
                {
                    _context.Log($"[DEBUG] SeqBuilder: Processing state {cur.GetType().Name}, Fragment='{cur.Fragment}'");
                }

                if (cur is SttMermaidStart) { }
                else if (cur is SttMermaidClose)
                {
                    if (_context.Debug)
                    {
                        _context.Log($"[DEBUG] SeqBuilder: Parsing completed, Participants: {sequence.Participants.Count}, Messages: {sequence.Messages.Count}");
                        _context.Log($"[DEBUG] SeqBuilder: Activations: {sequence.Activations.Count}");

                        for (int i = 0; i < sequence.Participants.Count; i++)
                        {
                            var p = sequence.Participants[i];
                            _context.Log($"[DEBUG] SeqBuilder: Participant[{i}]: ID='{p.ID}', Label='{p.Label}', Alias='{p.Alias}'");
                        }

                        for (int i = 0; i < sequence.Messages.Count; i++)
                        {
                            var m = sequence.Messages[i];
                            _context.Log($"[DEBUG] SeqBuilder: Message[{i}]: From='{m.From}', To='{m.To}', Label='{m.Label}', ArrowType='{m.ArrowType}'");
                        }

                        _context.Log($"[DEBUG] SeqBuilder: Start calling sequence.ToVisio(\"{outputFile}\")");
                    }

                    try
                    {
                        sequence.ToVisio(outputFile, _context, _session);

                        if (_context.Debug)
                        {
                            _context.Log($"[DEBUG] SeqBuilder: sequence.ToVisio() call completed");
                        }
                    }
                    catch (Exception ex)
                    {
                        if (_context.Debug)
                        {
                            _context.Log($"[DEBUG] SeqBuilder: sequence.ToVisio() call failed: {ex.Message}");
                            _context.Log($"[DEBUG] SeqBuilder: Exception type: {ex.GetType().Name}");
                            if (ex.InnerException != null)
                            {
                                _context.Log($"[DEBUG] SeqBuilder: Inner exception: {ex.InnerException.Message}");
                            }
                            _context.Log($"[DEBUG] SeqBuilder: Stack trace: {ex.StackTrace}");
                        }
                        throw;
                    }
                    break;
                }
                else if (cur is SeqSttKeyword) { BuildKeyword(); }
                else if (cur is SttComment)
                {
                    sequence.Config.LoadUserDirectiveFromComment(cur.Fragment);
                    RefreshMessageSpacing();
                }
                else if (cur is SttFrontMatter)
                {
                    sequence.Config.LoadUserFrontMatter(cur.Fragment);
                    RefreshMessageSpacing();
                }
                else if (cur is SttFinishFlag) { }
                else
                {
                    string line = cur.Fragment.Trim();
                    if (line.Contains("->>") || line.Contains("-->>") || line.Contains("->") || line.Contains("-->"))
                    {
                        ParseMessageLine(line);
                    }
                }
            }
        }

        private void BuildKeyword()
        {
            string keyword = iter.Current.Fragment;

            switch (keyword)
            {
                case "sequenceDiagram":
                    break;

                case "participant":
                    BuildParticipant();
                    break;

                case "activate":
                    BuildActivate();
                    break;

                case "deactivate":
                    BuildDeactivate();
                    break;

                case "note":
                    BuildNote();
                    break;

                case "autonumber":
                    sequence.ShowSequenceNumbers = true;
                    autoNumberCounter = 1;
                    break;

                case "alt":
                case "opt":
                case "loop":
                case "par":
                case "critical":
                case "break":
                    BuildFragmentStart(keyword);
                    break;

                case "else":
                    BuildFragmentElse();
                    break;

                case "end":
                    BuildFragmentEnd();
                    break;

                default:
                    break;
            }
        }

        private void BuildParticipant()
        {
            if (_context.Debug)
            {
                _context.Log($"[DEBUG] SeqBuilder.BuildParticipant: Start processing participant");
            }

            var participantInfo = new List<string>();

            while (iter.HasNext())
            {
                SynState peek = iter.PeekNext();
                if (peek == null) break;

                if (_context.Debug)
                {
                    _context.Log($"[DEBUG] SeqBuilder.BuildParticipant: Checking state {peek.GetType().Name}, Fragment='{peek.Fragment}'");
                }

                if (peek is SttFinishFlag) break;

                iter.Next();
                if (!string.IsNullOrWhiteSpace(peek.Fragment))
                {
                    participantInfo.Add(peek.Fragment);
                }
            }

            if (_context.Debug)
            {
                _context.Log($"[DEBUG] SeqBuilder.BuildParticipant: Collected fragments: [{string.Join(", ", participantInfo)}]");
            }

            if (participantInfo.Count >= 1)
            {
                string participantId = participantInfo[0];
                string participantAlias = string.Empty;

                if (participantInfo.Count >= 3 && participantInfo[1] == "as")
                {
                    participantAlias = participantInfo[2];
                }

                if (_context.Debug)
                {
                    _context.Log($"[DEBUG] SeqBuilder.BuildParticipant: ID='{participantId}', Alias='{participantAlias}'");
                }

                var participant = sequence.GetParticipant(participantId);
                if (!string.IsNullOrEmpty(participantAlias))
                {
                    participant.Alias = participantAlias;
                }
            }
        }

        private void BuildActivate(string participantId)
        {
            var activation = new SeqActivation(participantId, currentY);

            var existingActivations = activationStack.Where(a => a.ParticipantId == participantId).ToList();
            activation.NestingLevel = existingActivations.Count;

            sequence.Activations.Add(activation);
            activationStack.Push(activation);
        }

        private void BuildActivate()
        {
            SynState next = iter.PeekNext();
            if (next != null)
            {
                iter.Next();
                string participantId = next.Fragment;
                BuildActivate(participantId);
            }
        }

        private void BuildDeactivate(string participantId)
        {
            var activation = activationStack.LastOrDefault(a => a.ParticipantId == participantId);
            if (activation != null)
            {
                activation.SetEnd(currentY);
                var tempStack = new Stack<SeqActivation>();
                while (activationStack.Count > 0)
                {
                    var item = activationStack.Pop();
                    if (item != activation)
                    {
                        tempStack.Push(item);
                    }
                }
                while (tempStack.Count > 0)
                {
                    activationStack.Push(tempStack.Pop());
                }
            }
        }

        private void BuildDeactivate()
        {
            SynState next = iter.PeekNext();
            if (next != null)
            {
                iter.Next();
                string participantId = next.Fragment;
                BuildDeactivate(participantId);
            }
        }

        private void BuildFragmentStart(string type)
        {
            string text = CollectFragmentLabel();
            var fragment = new SeqFragment
            {
                Type = type,
                Text = text,
                StartY = currentY
            };
            sequence.Fragments.Add(fragment);
            fragmentStack.Push(fragment);
            currentY -= fragmentHeaderPadding;
        }

        private void BuildFragmentElse()
        {
            if (fragmentStack.Count > 0)
            {
                currentY -= fragmentSectionPadding;
                var fragment = fragmentStack.Peek();
                string text = CollectFragmentLabel();
                fragment.Sections.Add(new SeqFragmentSection
                {
                    Y = currentY,
                    Text = text
                });
            }
        }

        private void BuildFragmentEnd()
        {
            if (fragmentStack.Count > 0)
            {
                currentY -= fragmentEndPadding;
                var fragment = fragmentStack.Pop();
                fragment.EndY = currentY;
            }
        }

        private void BuildNote()
        {
            string noteLine = CollectFragmentLabel();
            if (string.IsNullOrWhiteSpace(noteLine))
            {
                return;
            }

            var match = Regex.Match(noteLine, @"^(left of|right of|over)\s+([^:]+?)\s*:\s*(.*)$", RegexOptions.IgnoreCase);
            if (!match.Success)
            {
                return;
            }

            string positionText = match.Groups[1].Value.Trim().ToLowerInvariant();
            string participantsText = match.Groups[2].Value.Trim();
            string text = match.Groups[3].Value.Trim();

            var participantIds = participantsText
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(participant => participant.Trim())
                .Where(participant => participant.Length > 0)
                .ToList();

            if (participantIds.Count == 0)
            {
                return;
            }

            SeqNotePosition position = SeqNotePosition.Over;
            if (positionText == "left of") position = SeqNotePosition.LeftOf;
            else if (positionText == "right of") position = SeqNotePosition.RightOf;

            foreach (string participantId in participantIds)
            {
                sequence.GetParticipant(participantId);
            }

            sequence.Notes.Add(new SeqNote
            {
                Position = position,
                ParticipantIds = participantIds,
                Text = text,
                Y = currentY
            });

            currentY -= messageSpacing;
        }

        private string CollectFragmentLabel()
        {
            var parts = new List<string>();
            while (iter.HasNext())
            {
                SynState peek = iter.PeekNext();
                if (peek == null || peek is SttFinishFlag) break;
                iter.Next();
                if (!string.IsNullOrWhiteSpace(peek.Fragment))
                {
                    parts.Add(peek.Fragment);
                }
            }
            return string.Join(" ", parts);
        }

        private void ParseMessageLine(string messageText)
        {
            if (_context.Debug)
            {
                _context.Log($"[DEBUG] SeqBuilder: Parsing message line: {messageText}");
            }

            if (TryParseMessage(messageText, out string from, out string to, out string arrowType, out string activationOp, out string label))
            {
                if (_context.Debug)
                {
                    _context.Log($"[DEBUG] SeqBuilder: Parse success - From: {from}, To: {to}, Arrow: {arrowType}, Label: {label}");
                }
                var message = new SeqMessage(from, to, label, arrowType)
                {
                    Y = currentY
                };

                if (sequence.ShowSequenceNumbers)
                {
                    message.SequenceNumber = autoNumberCounter++;
                }

                sequence.Messages.Add(message);

                sequence.GetParticipant(from);
                sequence.GetParticipant(to);

                if (activationOp == "+")
                {
                    BuildActivate(to);
                }
                else if (activationOp == "-")
                {
                    BuildDeactivate(from);
                }

                currentY -= messageSpacing;
            }
            else
            {
                if (_context.Debug)
                {
                    _context.Log($"[DEBUG] SeqBuilder: Message parse failed: {messageText}");
                }
            }
        }

        private void RefreshMessageSpacing()
        {
            if (sequence.Config.GetDouble("config.sequence.messageSpacing", out double spacingMm))
            {
                messageSpacing = spacingMm * LayoutScale;
            }
            else
            {
                messageSpacing = DefaultMessageSpacing;
            }

            double defaultPadding = messageSpacing / 4;
            fragmentHeaderPadding = ResolvePadding("config.sequence.fragmentPaddingTop", defaultPadding);
            fragmentSectionPadding = ResolvePadding("config.sequence.fragmentSectionPadding", defaultPadding);
            fragmentEndPadding = ResolvePadding("config.sequence.fragmentPaddingBottom", defaultPadding);
        }

        private double ResolvePadding(string keyPath, double defaultPadding)
        {
            if (sequence.Config.GetDouble(keyPath, out double paddingMm))
            {
                return paddingMm * LayoutScale;
            }

            return defaultPadding;
        }

        private bool TryParseMessage(string messageText, out string from, out string to, out string arrowType, out string activationOp, out string label)
        {
            from = to = arrowType = activationOp = label = string.Empty;

            var pattern = @"^\s*(\w+)\s*(-->?>>?|-->>?|->>?|->)\s*([+-]?)\s*(\w+)\s*:\s*(.*)$";
            var match = System.Text.RegularExpressions.Regex.Match(messageText, pattern);

            if (match.Success)
            {
                from = match.Groups[1].Value.Trim();
                arrowType = match.Groups[2].Value.Trim();
                activationOp = match.Groups[3].Value.Trim();
                to = match.Groups[4].Value.Trim();
                label = match.Groups[5].Value.Trim();
                return true;
            }

            return false;
        }
    }
}
