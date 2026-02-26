using md2visio.Api;
using md2visio.struc.sequence;
using md2visio.vsdx.@base;
using md2visio.vsdx.tool;
using Microsoft.Office.Interop.Visio;
using System.Drawing;
using System.Globalization;

namespace md2visio.vsdx
{
    internal class VDrawerSeq : VFigureDrawer<Sequence>
    {
        // Layout parameters - these values are "layout units" (mm * LayoutScale)
        // Converted to mm with unit suffix when writing to Visio
        private const double LayoutScale = 15.0;
        private const double TextPaddingMm = 2.0;
        private const double DefaultFragmentLabelHeight = 250;

        private double participantSpacing = 1500; // Participant spacing
        private double messageSpacing = 375;      // Message spacing
        private double participantWidth = 900;    // Participant width
        private double participantHeight = 450;   // Participant height
        private double activationWidth = 120;     // Activation box width
        private double selfCallWidth = 300;       // Self-call width
        private double selfCallHeight = 225;      // Self-call height
        private double selfCallTextOffset = 120;  // Self-call text offset
        private double fragmentPaddingTop;        // Fragment top padding
        private double fragmentPaddingBottom;     // Fragment bottom padding

        // Y-coordinate reference points
        private double topY = 3750;              // Top participant Y position
        private double bottomY;                  // Bottom participant Y position (dynamic)
        private double diagramStartY;            // Diagram content start Y position (dynamic)

        // ====== Coordinate unit conversion helpers ======
        // Visio internal unit is inches, must append "mm" suffix to interpret as millimeters

        /// <summary>
        /// Format number as Visio formula compatible string (using InvariantCulture to avoid comma issues)
        /// </summary>
        private static string Num(double value)
            => value.ToString("0.###############", CultureInfo.InvariantCulture);

        /// <summary>
        /// Convert layout unit to Visio formula with mm suffix
        /// Layout unit = actual mm * LayoutScale, so divide by LayoutScale to restore
        /// </summary>
        private static string Mm(double layoutUnits)
            => $"{Num(layoutUnits / LayoutScale)} mm";

        /// <summary>
        /// Convert layout unit to Visio internal unit (inches)
        /// 1 inch = 25.4 mm, Layout unit = mm * LayoutScale
        /// </summary>
        private static double Inches(double layoutUnits)
            => layoutUnits / (LayoutScale * 25.4);

        /// <summary>
        /// Convert relative Y to absolute Y
        /// message.Y and activation.CenterY are relative, need to add diagramStartY offset
        /// </summary>
        private double AbsY(double relativeY) => diagramStartY + relativeY;

        /// <summary>
        /// Move shape to specified layout position (with mm unit)
        /// </summary>
        private void PlaceAt(Shape shape, double xLayoutUnits, double yLayoutUnits)
            => MoveTo(shape, Mm(xLayoutUnits), Mm(yLayoutUnits));

        /// <summary>
        /// Debug log: Output actual shape position
        /// </summary>
        private void DebugLogPin(string label, Shape shape)
        {
            if (!_context.Debug) return;
            try
            {
                _context.Log($"[DEBUG] VDrawerSeq: {label} Pin=({shape.CellsU["PinX"].ResultStr["mm"]}, {shape.CellsU["PinY"].ResultStr["mm"]})");
            }
            catch { }
        }

        public VDrawerSeq(Sequence figure, Application visioApp, ConversionContext context)
            : base(figure, visioApp, context)
        {
            LoadConfiguration();
        }

        private void LoadConfiguration()
        {
            void ApplyLayoutValue(string keyPath, ref double target)
            {
                if (config.GetDouble(keyPath, out double valueMm))
                {
                    target = valueMm * LayoutScale;
                }
            }

            ApplyLayoutValue("config.sequence.participantSpacing", ref participantSpacing);
            ApplyLayoutValue("config.sequence.messageSpacing", ref messageSpacing);
            ApplyLayoutValue("config.sequence.participantWidth", ref participantWidth);
            ApplyLayoutValue("config.sequence.participantHeight", ref participantHeight);
            ApplyLayoutValue("config.sequence.activationWidth", ref activationWidth);
            ApplyLayoutValue("config.sequence.selfCallWidth", ref selfCallWidth);
            ApplyLayoutValue("config.sequence.selfCallHeight", ref selfCallHeight);
            ApplyLayoutValue("config.sequence.selfCallTextOffset", ref selfCallTextOffset);

            double defaultFragmentPadding = messageSpacing / 4;
            fragmentPaddingTop = defaultFragmentPadding;
            fragmentPaddingBottom = defaultFragmentPadding;
            ApplyLayoutValue("config.sequence.fragmentPaddingTop", ref fragmentPaddingTop);
            ApplyLayoutValue("config.sequence.fragmentPaddingBottom", ref fragmentPaddingBottom);
        }

        public override void Draw()
        {
            // Add debug info
            if (_context.Debug)
            {
                _context.Log($"[DEBUG] VDrawerSeq: Start drawing sequence diagram, Participants: {figure.Participants.Count}, Messages: {figure.Messages.Count}, Activations: {figure.Activations.Count}");
            }

            try
            {
                EnsureVisible();
                PauseForViewing(300);
                UpdateTextMetrics();

                // 1. Calculate Layout
                if (_context.Debug)
                {
                    _context.Log($"[DEBUG] VDrawerSeq: Calculating layout");
                }
                CalculateLayout();

                // 2. Draw Participants
                if (_context.Debug)
                {
                    _context.Log($"[DEBUG] VDrawerSeq: Drawing participants");
                }
                DrawParticipants();
                PauseForViewing(500);

                // 3. Draw Lifelines
                if (_context.Debug)
                {
                    _context.Log($"[DEBUG] VDrawerSeq: Drawing lifelines");
                }
                DrawLifelines();
                PauseForViewing(300);

                // 4. Draw Activations
                if (_context.Debug)
                {
                    _context.Log($"[DEBUG] VDrawerSeq: Drawing activations");
                }
                DrawActivations();
                PauseForViewing(300);

                // 5. Draw Fragments
                if (_context.Debug)
                {
                    _context.Log($"[DEBUG] VDrawerSeq: Drawing fragments");
                }
                DrawFragments();
                PauseForViewing(300);

                // 6. Draw Notes
                if (_context.Debug)
                {
                    _context.Log($"[DEBUG] VDrawerSeq: Drawing notes");
                }
                DrawNotes();
                PauseForViewing(300);

                // 7. Draw Messages
                if (_context.Debug)
                {
                    _context.Log($"[DEBUG] VDrawerSeq: Drawing messages");
                }
                DrawMessages();
                PauseForViewing(500);

                if (_context.Debug)
                {
                    _context.Log($"[DEBUG] VDrawerSeq: Sequence diagram drawing completed");
                }
            }
            catch (Exception ex)
            {
                if (_context.Debug)
                {
                    _context.Log($"[DEBUG] VDrawerSeq: Exception during drawing: {ex.Message}");
                    _context.Log($"[DEBUG] VDrawerSeq: Exception type: {ex.GetType().Name}");
                    if (ex.InnerException != null)
                    {
                        _context.Log($"[DEBUG] VDrawerSeq: Inner exception: {ex.InnerException.Message}");
                    }
                    _context.Log($"[DEBUG] VDrawerSeq: Stack trace: {ex.StackTrace}");
                }
                throw;
            }
        }

        private void UpdateTextMetrics()
        {
            foreach (var message in figure.Messages)
            {
                message.LabelHeight = MeasureLabelHeight(message.Label, 0);
            }

            foreach (var fragment in figure.Fragments)
            {
                double typeHeight = MeasureLabelHeight(fragment.Type, DefaultFragmentLabelHeight);
                double conditionHeight = string.IsNullOrWhiteSpace(fragment.Text)
                    ? 0
                    : MeasureLabelHeight(fragment.Text, DefaultFragmentLabelHeight);
                fragment.LabelHeight = Math.Max(typeHeight, conditionHeight);

                foreach (var section in fragment.Sections)
                {
                    section.LabelHeight = string.IsNullOrWhiteSpace(section.Text)
                        ? 0
                        : MeasureLabelHeight(section.Text, DefaultFragmentLabelHeight);
                }
            }

            foreach (var note in figure.Notes)
            {
                note.LabelHeight = MeasureLabelHeight(note.Text, 0);
            }
        }

        private double MeasureLabelHeight(string text, double minHeight)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return minHeight;
            }

            SizeF sizeMm = MeasureTextSizeMM(text);
            double height = (sizeMm.Height + TextPaddingMm) * LayoutScale;
            return Math.Max(minHeight, height);
        }

        private void CalculateLayout()
        {
            // Calculate horizontal positions of participants (scaled by 15)
            double startX = 1500;
            for (int i = 0; i < figure.Participants.Count; i++)
            {
                figure.Participants[i].X = startX + i * participantSpacing;
            }

            // Calculate vertical layout
            CalculateVerticalLayout();
        }

        private void CalculateVerticalLayout()
        {
            // 1. Calculate message start Y position: just below the top participant box
            double messageStartOffset = messageSpacing / 2;
            diagramStartY = topY - participantHeight / 2 - messageStartOffset;

            // 2. Find lowest point of content (minimum relative Y value)
            // Visio coordinate system Y-axis grows upwards, so lowest content point = minimum Y value
            double minRelativeY = 0;

            foreach (var message in figure.Messages)
            {
                double messageBottomY = message.IsSelfCall ? message.Y - selfCallHeight : message.Y;
                if (messageBottomY < minRelativeY)
                    minRelativeY = messageBottomY;
            }

            foreach (var activation in figure.Activations)
            {
                if (activation.EndY < minRelativeY)
                    minRelativeY = activation.EndY;
            }

            foreach (var fragment in figure.Fragments)
            {
                double labelHeight = fragment.LabelHeight > 0 ? fragment.LabelHeight : DefaultFragmentLabelHeight;
                double fragmentTop = fragment.StartY + fragmentPaddingTop;
                double fragmentHeaderBottom = fragmentTop - labelHeight;
                if (fragmentHeaderBottom < minRelativeY)
                    minRelativeY = fragmentHeaderBottom;

                double fragmentBottom = fragment.EndY - fragmentPaddingBottom;
                if (fragmentBottom < minRelativeY)
                    minRelativeY = fragmentBottom;

                foreach (var section in fragment.Sections)
                {
                    if (string.IsNullOrWhiteSpace(section.Text))
                        continue;

                    double sectionLabelHeight = section.LabelHeight > 0 ? section.LabelHeight : labelHeight;
                    double sectionLabelBottom = section.Y - sectionLabelHeight;
                    if (sectionLabelBottom < minRelativeY)
                        minRelativeY = sectionLabelBottom;
                }
            }

            foreach (var note in figure.Notes)
            {
                double noteHeight = note.LabelHeight > 0 ? note.LabelHeight : DefaultFragmentLabelHeight;
                double noteBottom = note.Y - noteHeight / 2;
                if (noteBottom < minRelativeY)
                    minRelativeY = noteBottom;
            }

            // 3. Edge case: set minimum height when no content
            if (figure.Messages.Count == 0 && figure.Activations.Count == 0 && figure.Fragments.Count == 0 && figure.Notes.Count == 0)
            {
                minRelativeY = -messageSpacing * 2; // Default minimum height
            }

            // 4. Dynamically calculate bottom participant position
            double lastContentY = diagramStartY + minRelativeY;
            bottomY = lastContentY - messageStartOffset - participantHeight / 2;
        }

        private void DrawParticipants()
        {
            foreach (var participant in figure.Participants)
            {
                // Top participant - create at (0,0), then move
                participant.TopShape = visioPage.Drop(GetMaster("[]"), 0, 0);
                SetupParticipantShape(participant.TopShape, participant.DisplayName);
                PlaceAt(participant.TopShape, participant.X, topY);
                DebugLogPin($"Participant.Top '{participant.ID}'", participant.TopShape);

                // Bottom participant (mirror)
                participant.BottomShape = visioPage.Drop(GetMaster("[]"), 0, 0);
                SetupParticipantShape(participant.BottomShape, participant.DisplayName);
                PlaceAt(participant.BottomShape, participant.X, bottomY);
                DebugLogPin($"Participant.Bottom '{participant.ID}'", participant.BottomShape);

                PauseForViewing(100);
            }
        }

        private void SetupParticipantShape(Shape shape, string text)
        {
            shape.Text = text;
            shape.CellsU["Width"].FormulaU = Mm(participantWidth);
            shape.CellsU["Height"].FormulaU = Mm(participantHeight);

            // Set style
            var bgColor = (VRGBColor)VRGBColor.Create("#E1F5FE");
            var borderColor = (VRGBColor)VRGBColor.Create("#0277BD");
            VShapeDrawer.SetFillForegnd(shape, bgColor);
            VShapeDrawer.SetLineColor(shape, borderColor);
            shape.CellsU["LineWeight"].FormulaU = "1 pt";

            AdjustSize(shape);
        }

        private void DrawLifelines()
        {
            foreach (var participant in figure.Participants)
            {
                // Get actual participant box height (AdjustSize may have changed it)
                double topH = participant.TopShape?.CellsU["Height"].Result[(short)VisUnitCodes.visMillimeters] * LayoutScale ?? participantHeight;
                double botH = participant.BottomShape?.CellsU["Height"].Result[(short)VisUnitCodes.visMillimeters] * LayoutScale ?? participantHeight;

                participant.LifelineShape = CreateVerticalDashedLine(
                    participant.X,
                    topY - topH / 2,
                    bottomY + botH / 2
                );

                PauseForViewing(50);
            }
        }

        private Shape CreateVerticalDashedLine(double x, double startY, double endY)
        {
            // Use DrawLine to draw simple 1D line, avoiding dynamic connector snapping behavior
            Shape line = visioPage.DrawLine(
                Inches(x), Inches(startY),
                Inches(x), Inches(endY));

            // Dashed style
            line.CellsU["LinePattern"].FormulaU = "2";
            line.CellsU["LineWeight"].FormulaU = "0.5 pt";
            var lineColor = (VRGBColor)VRGBColor.Create("#666666");
            VShapeDrawer.SetLineColor(line, lineColor);

            return line;
        }

        private void DrawActivations()
        {
            foreach (var activation in figure.Activations)
            {
                var participant = figure.Participants.FirstOrDefault(p => p.ID == activation.ParticipantId);
                if (participant == null) continue;

                double activationX = participant.X + (activation.NestingLevel * (activationWidth + 2));
                double activationCenterY = AbsY(activation.CenterY);  // Apply AbsY offset
                double activationHeight = Math.Abs(activation.Height);

                // Create at (0,0), then move
                activation.ActivationShape = visioPage.Drop(GetMaster("[]"), 0, 0);
                PlaceAt(activation.ActivationShape, activationX, activationCenterY);
                DebugLogPin($"Activation '{activation.ParticipantId}' L{activation.NestingLevel}", activation.ActivationShape);

                activation.ActivationShape.CellsU["Width"].FormulaU = Mm(activationWidth);
                activation.ActivationShape.CellsU["Height"].FormulaU = Mm(activationHeight);

                // Activation box style
                var activationBgColor = (VRGBColor)VRGBColor.Create("#FFFFCC");
                var activationBorderColor = (VRGBColor)VRGBColor.Create("#FFA000");
                VShapeDrawer.SetFillForegnd(activation.ActivationShape, activationBgColor);
                VShapeDrawer.SetLineColor(activation.ActivationShape, activationBorderColor);
                activation.ActivationShape.CellsU["LineWeight"].FormulaU = "1 pt";

                PauseForViewing(100);
            }
        }

        private void DrawFragments()
        {
            if (figure.Fragments.Count == 0) return;

            double fallbackMinX = figure.Participants.Min(p => p.X) - participantWidth / 2 - 100;
            double fallbackMaxX = figure.Participants.Max(p => p.X) + participantWidth / 2 + 100;

            foreach (var fragment in figure.Fragments)
            {
                double minX = double.MaxValue;
                double maxX = double.MinValue;
                bool hasBounds = false;

                void AddBounds(double left, double right)
                {
                    if (left > right)
                    {
                        (left, right) = (right, left);
                    }
                    if (!hasBounds)
                    {
                        minX = left;
                        maxX = right;
                        hasBounds = true;
                        return;
                    }
                    if (left < minX) minX = left;
                    if (right > maxX) maxX = right;
                }

                foreach (var message in figure.Messages)
                {
                    if (message.Y > fragment.StartY || message.Y < fragment.EndY)
                    {
                        continue;
                    }

                    var fromParticipant = figure.Participants.FirstOrDefault(p => p.ID == message.From);
                    if (fromParticipant != null)
                    {
                        double left = fromParticipant.X - participantWidth / 2;
                        double right = fromParticipant.X + participantWidth / 2;
                        if (message.IsSelfCall)
                        {
                            right = Math.Max(right, fromParticipant.X + selfCallWidth);
                        }
                        AddBounds(left, right);
                    }

                    var toParticipant = figure.Participants.FirstOrDefault(p => p.ID == message.To);
                    if (toParticipant != null)
                    {
                        AddBounds(toParticipant.X - participantWidth / 2, toParticipant.X + participantWidth / 2);
                    }
                }

                foreach (var activation in figure.Activations)
                {
                    if (activation.StartY < fragment.EndY || activation.EndY > fragment.StartY)
                    {
                        continue;
                    }

                    var participant = figure.Participants.FirstOrDefault(p => p.ID == activation.ParticipantId);
                    if (participant == null)
                    {
                        continue;
                    }

                    double activationCenterX = participant.X + (activation.NestingLevel * (activationWidth + 2));
                    AddBounds(activationCenterX - activationWidth / 2, activationCenterX + activationWidth / 2);
                }

                foreach (var note in figure.Notes)
                {
                    if (note.Y > fragment.StartY || note.Y < fragment.EndY)
                    {
                        continue;
                    }

                    if (TryGetNoteBounds(note, out double left, out double right))
                    {
                        AddBounds(left, right);
                    }
                }

                if (!hasBounds)
                {
                    minX = fallbackMinX;
                    maxX = fallbackMaxX;
                }
                else
                {
                    minX -= 100;
                    maxX += 100;
                }

                double top = AbsY(fragment.StartY) + fragmentPaddingTop;
                double bottom = AbsY(fragment.EndY) - fragmentPaddingBottom;

                var frameShape = visioPage.DrawRectangle(
                    Inches(minX), Inches(bottom),
                    Inches(maxX), Inches(top));

                frameShape.CellsU["LinePattern"].FormulaU = "1";
                frameShape.CellsU["LineWeight"].FormulaU = "1 pt";
                frameShape.CellsU["FillPattern"].FormulaU = "0";
                var frameColor = (VRGBColor)VRGBColor.Create("#888888");
                VShapeDrawer.SetLineColor(frameShape, frameColor);

                double labelWidth = 600;
                double labelHeight = fragment.LabelHeight > 0 ? fragment.LabelHeight : DefaultFragmentLabelHeight;
                var labelShape = visioPage.DrawRectangle(
                    Inches(minX), Inches(top - labelHeight),
                    Inches(minX + labelWidth), Inches(top));

                labelShape.Text = fragment.Type;
                labelShape.CellsU["FillPattern"].FormulaU = "1";
                var labelBgColor = (VRGBColor)VRGBColor.Create("#DDDDDD");
                VShapeDrawer.SetFillForegnd(labelShape, labelBgColor);
                labelShape.CellsU["LineWeight"].FormulaU = "1 pt";
                VShapeDrawer.SetLineColor(labelShape, frameColor);

                if (!string.IsNullOrEmpty(fragment.Text))
                {
                    var conditionShape = visioPage.DrawRectangle(
                        Inches(minX + labelWidth + 50), Inches(top - labelHeight),
                        Inches(minX + labelWidth + 800), Inches(top));
                    conditionShape.Text = $"[{fragment.Text}]";
                    conditionShape.CellsU["FillPattern"].FormulaU = "0";
                    conditionShape.CellsU["LinePattern"].FormulaU = "0";
                }

                foreach (var section in fragment.Sections)
                {
                    double sectionY = AbsY(section.Y);
                    var sectionLine = visioPage.DrawLine(
                        Inches(minX), Inches(sectionY),
                        Inches(maxX), Inches(sectionY));
                    sectionLine.CellsU["LinePattern"].FormulaU = "2";
                    sectionLine.CellsU["LineWeight"].FormulaU = "0.5 pt";
                    VShapeDrawer.SetLineColor(sectionLine, frameColor);

                    if (!string.IsNullOrEmpty(section.Text))
                    {
                        double sectionLabelHeight = section.LabelHeight > 0 ? section.LabelHeight : labelHeight;
                        var sectionLabel = visioPage.DrawRectangle(
                            Inches(minX + 50), Inches(sectionY - sectionLabelHeight),
                            Inches(minX + 800), Inches(sectionY));
                        sectionLabel.Text = $"[{section.Text}]";
                        sectionLabel.CellsU["FillPattern"].FormulaU = "0";
                        sectionLabel.CellsU["LinePattern"].FormulaU = "0";
                    }
                }

                PauseForViewing(100);
            }
        }

        private void DrawNotes()
        {
            if (figure.Notes.Count == 0) return;

            foreach (var note in figure.Notes)
            {
                if (!TryGetNoteBounds(note, out double left, out double right))
                {
                    continue;
                }

                double height = note.LabelHeight > 0 ? note.LabelHeight : DefaultFragmentLabelHeight;
                double centerY = AbsY(note.Y);
                double top = centerY + height / 2;
                double bottom = centerY - height / 2;

                var noteShape = visioPage.DrawRectangle(
                    Inches(left), Inches(bottom),
                    Inches(right), Inches(top));

                noteShape.Text = note.Text;
                noteShape.CellsU["FillPattern"].FormulaU = "1";
                var noteBgColor = (VRGBColor)VRGBColor.Create("#FFF8C5");
                VShapeDrawer.SetFillForegnd(noteShape, noteBgColor);
                noteShape.CellsU["LineWeight"].FormulaU = "1 pt";
                var noteBorderColor = (VRGBColor)VRGBColor.Create("#888888");
                VShapeDrawer.SetLineColor(noteShape, noteBorderColor);
            }
        }

        private bool TryGetNoteBounds(SeqNote note, out double left, out double right)
        {
            left = right = 0;
            if (note.ParticipantIds.Count == 0)
            {
                return false;
            }

            var participants = note.ParticipantIds
                .Select(id => figure.Participants.FirstOrDefault(p => p.ID == id))
                .Where(p => p != null)
                .ToList();

            if (participants.Count == 0)
            {
                return false;
            }

            double width = participantWidth;
            if (!string.IsNullOrWhiteSpace(note.Text))
            {
                SizeF sizeMm = MeasureTextSizeMM(note.Text);
                width = Math.Max(width, (sizeMm.Width + TextPaddingMm) * LayoutScale);
            }

            double minParticipantX = participants.Min(p => p!.X);
            double maxParticipantX = participants.Max(p => p!.X);

            switch (note.Position)
            {
                case SeqNotePosition.LeftOf:
                    right = minParticipantX - participantWidth / 2;
                    left = right - width;
                    return true;
                case SeqNotePosition.RightOf:
                    left = maxParticipantX + participantWidth / 2;
                    right = left + width;
                    return true;
                default:
                    double spanLeft = minParticipantX - participantWidth / 2;
                    double spanRight = maxParticipantX + participantWidth / 2;
                    double spanWidth = spanRight - spanLeft;
                    double noteWidth = Math.Max(width, spanWidth);
                    double center = (spanLeft + spanRight) / 2;
                    left = center - noteWidth / 2;
                    right = center + noteWidth / 2;
                    return true;
            }
        }

        private void DrawMessages()
        {
            foreach (var message in figure.Messages)
            {
                if (message.IsSelfCall)
                {
                    message.MessageShape = DrawSelfCallMessage(message);
                }
                else
                {
                    message.MessageShape = DrawRegularMessage(message);
                }
                
                PauseForViewing(150);
            }
        }

        private Shape DrawRegularMessage(SeqMessage message)
        {
            var fromParticipant = figure.Participants.FirstOrDefault(p => p.ID == message.From);
            var toParticipant = figure.Participants.FirstOrDefault(p => p.ID == message.To);

            if (fromParticipant == null || toParticipant == null)
                throw new InvalidOperationException($"Participant not found for message: {message}");

            double fromX = GetMessageStartX(fromParticipant, message.Y);
            double toX = GetMessageEndX(toParticipant, message.Y);
            double y = AbsY(message.Y);  // Apply AbsY offset

            // Determine message direction: Left-to-Right or Right-to-Left
            bool isLeftToRight = fromX < toX;
            double leftX = Math.Min(fromX, toX);
            double rightX = Math.Max(fromX, toX);

            // Always draw line from left to right to prevent text flipping
            Shape messageShape = visioPage.DrawLine(
                Inches(leftX), Inches(y),
                Inches(rightX), Inches(y));

            // Set arrow position based on actual direction
            SetupMessageArrowWithDirection(messageShape, message.ArrowType, isLeftToRight);

            // Set message text
            messageShape.Text = message.Label;
            DebugLogPin($"Message '{message.From}->{message.To}'", messageShape);

            return messageShape;
        }

        private void DrawSequenceNumber(int number, double x, double y, bool isLeftToRight)
        {
            double size = 200;
            double circleX = isLeftToRight ? x - size - 50 : x + size + 50;

            var circle = visioPage.DrawOval(
                Inches(circleX - size / 2), Inches(y - size / 2),
                Inches(circleX + size / 2), Inches(y + size / 2));

            circle.Text = number.ToString();
            circle.CellsU["Char.Size"].FormulaU = "8 pt";

            var circleBgColor = (VRGBColor)VRGBColor.Create("#CCCCCC");
            VShapeDrawer.SetFillForegnd(circle, circleBgColor);
            circle.CellsU["LineWeight"].FormulaU = "0.5 pt";
            var circleLineColor = (VRGBColor)VRGBColor.Create("#666666");
            VShapeDrawer.SetLineColor(circle, circleLineColor);
        }

        private double GetMessageStartX(SeqParticipant participant, double messageY)
        {
            var activation = figure.Activations.FirstOrDefault(a =>
                a.ParticipantId == participant.ID && a.IsActive(messageY));

            if (activation != null)
            {
                double activationX = participant.X + activation.NestingLevel * (activationWidth + 2);
                return activationX + activationWidth / 2;
            }

            return participant.X;
        }

        private double GetMessageEndX(SeqParticipant participant, double messageY)
        {
            var activation = figure.Activations.FirstOrDefault(a =>
                a.ParticipantId == participant.ID && a.IsActive(messageY));

            if (activation != null)
            {
                double activationX = participant.X + activation.NestingLevel * (activationWidth + 2);
                return activationX - activationWidth / 2;
            }

            return participant.X;
        }

        private void SetupMessageArrow(Shape messageShape, string arrowType)
        {
            SetupMessageArrowWithDirection(messageShape, arrowType, true);
        }

        private void SetupMessageArrowWithDirection(Shape messageShape, string arrowType, bool isLeftToRight)
        {
            // Set line style
            if (arrowType.StartsWith("--"))
            {
                messageShape.CellsU["LinePattern"].FormulaU = "2"; // Dashed
            }
            else
            {
                messageShape.CellsU["LinePattern"].FormulaU = "1"; // Solid
            }

            messageShape.CellsU["LineWeight"].FormulaU = "1 pt";
            var messageColor = (VRGBColor)VRGBColor.Create("#333333");
            VShapeDrawer.SetLineColor(messageShape, messageColor);

            // Set arrow position based on direction
            // Line is always drawn left-to-right, so:
            // - Left-to-Right message: Arrow at End (Right)
            // - Right-to-Left message: Arrow at Begin (Left)
            bool hasArrow = arrowType.EndsWith(">>") || arrowType.EndsWith(">");

            if (isLeftToRight)
            {
                // Left-to-Right: Arrow at right end (End)
                messageShape.CellsU["EndArrow"].FormulaU = hasArrow ? "4" : "0";
                messageShape.CellsU["BeginArrow"].FormulaU = "0";
            }
            else
            {
                // Right-to-Left: Arrow at left end (Begin)
                messageShape.CellsU["BeginArrow"].FormulaU = hasArrow ? "4" : "0";
                messageShape.CellsU["EndArrow"].FormulaU = "0";
            }
        }

        private Shape DrawSelfCallMessage(SeqMessage message)
        {
            var participant = figure.Participants.FirstOrDefault(p => p.ID == message.From);
            if (participant == null)
                throw new InvalidOperationException($"Participant not found for self-call: {message}");

            double startX = GetMessageStartX(participant, message.Y);
            double startY = AbsY(message.Y);  // Apply AbsY offset
            double endY = startY - selfCallHeight;
            double rightX = startX + selfCallWidth;

            // Use Inches() to convert layout units to Visio internal units
            double x1 = Inches(startX);
            double y1 = Inches(startY);
            double x2 = Inches(rightX);
            double y2 = Inches(endY);

            // Use DrawLine API to draw three segments forming a U-shape self-call
            // 1. Horizontal line to right
            Shape line1 = visioPage.DrawLine(x1, y1, x2, y1);
            // 2. Vertical line down
            Shape line2 = visioPage.DrawLine(x2, y1, x2, y2);
            // 3. Horizontal line to left (with arrow)
            Shape line3 = visioPage.DrawLine(x2, y2, x1, y2);

            // Set line styles
            var messageColor = (VRGBColor)VRGBColor.Create("#333333");
            foreach (var line in new[] { line1, line2, line3 })
            {
                VShapeDrawer.SetLineColor(line, messageColor);
                line.CellsU["LineWeight"].FormulaU = "1 pt";
                if (message.ArrowType.StartsWith("--"))
                {
                    line.CellsU["LinePattern"].FormulaU = "2"; // Dashed
                }
            }

            // Set arrow only on return line
            if (message.ArrowType.EndsWith(">>") || message.ArrowType.EndsWith(">"))
            {
                line3.CellsU["EndArrow"].FormulaU = "4"; // Standard arrow
            }

            // Set message text (use independent text box to keep it horizontal)
            if (!string.IsNullOrEmpty(message.Label))
            {
                var labelShape = DropText(message.Label, 0, 0);
                double labelX = rightX + selfCallTextOffset;
                double labelY = (startY + endY) / 2;
                PlaceAt(labelShape, labelX, labelY);
                DebugLogPin($"SelfCallText '{message.From}'", labelShape);
            }
            DebugLogPin($"SelfCall '{message.From}'", line3);

            return line3; // Return the line with arrow
        }

    }
}
