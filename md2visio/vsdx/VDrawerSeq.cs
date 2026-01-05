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
        // 布局参数 - 这些值是"布局单位"(mm * LayoutScale)
        // 写入 Visio 时统一换算成 mm 并带单位后缀
        private const double LayoutScale = 15.0;
        private const double TextPaddingMm = 2.0;
        private const double DefaultFragmentLabelHeight = 250;

        private double participantSpacing = 1500; // 参与者间距
        private double messageSpacing = 375;      // 消息间距
        private double participantWidth = 900;    // 参与者宽度
        private double participantHeight = 450;   // 参与者高度
        private double activationWidth = 120;     // 激活框宽度
        private double selfCallWidth = 300;       // 自调用宽度
        private double selfCallHeight = 225;      // 自调用高度
        private double selfCallTextOffset = 120;  // 自调用文本偏移
        private double fragmentPaddingTop;        // 片段顶部留白
        private double fragmentPaddingBottom;     // 片段底部留白

        // Y坐标参考点
        private double topY = 3750;              // 顶部参与者Y位置
        private double bottomY;                  // 底部参与者Y位置 (动态计算)
        private double diagramStartY;            // 图表内容开始Y位置 (动态计算)

        // ====== 坐标单位转换辅助方法 ======
        // Visio 内部单位是英寸，必须带 "mm" 后缀才能正确解释为毫米

        /// <summary>
        /// 将数值格式化为 Visio 公式兼容的字符串（使用不变区域性，避免逗号问题）
        /// </summary>
        private static string Num(double value)
            => value.ToString("0.###############", CultureInfo.InvariantCulture);

        /// <summary>
        /// 将布局单位转换为带 mm 后缀的 Visio 公式
        /// 布局单位 = 实际毫米 * LayoutScale，所以需要除以 LayoutScale 还原
        /// </summary>
        private static string Mm(double layoutUnits)
            => $"{Num(layoutUnits / LayoutScale)} mm";

        /// <summary>
        /// 将布局单位转换为 Visio 内部单位（英寸）
        /// 1 inch = 25.4 mm, 布局单位 = mm * LayoutScale
        /// </summary>
        private static double Inches(double layoutUnits)
            => layoutUnits / (LayoutScale * 25.4);

        /// <summary>
        /// 将相对 Y 坐标转换为绝对 Y 坐标
        /// message.Y 和 activation.CenterY 是相对值，需要加上 diagramStartY 偏移
        /// </summary>
        private double AbsY(double relativeY) => diagramStartY + relativeY;

        /// <summary>
        /// 将形状移动到指定的布局坐标位置（带 mm 单位）
        /// </summary>
        private void PlaceAt(Shape shape, double xLayoutUnits, double yLayoutUnits)
            => MoveTo(shape, Mm(xLayoutUnits), Mm(yLayoutUnits));

        /// <summary>
        /// 调试日志：输出形状的实际位置
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
            // 添加调试信息
            if (_context.Debug)
            {
                _context.Log($"[DEBUG] VDrawerSeq: 开始绘制时序图，参与者数量: {figure.Participants.Count}，消息数量: {figure.Messages.Count}，激活框数量: {figure.Activations.Count}");
            }

            try
            {
                EnsureVisible();
                PauseForViewing(300);
                UpdateTextMetrics();

                // 1. 计算布局
                if (_context.Debug)
                {
                    _context.Log($"[DEBUG] VDrawerSeq: 开始计算布局");
                }
                CalculateLayout();

                // 2. 绘制参与者
                if (_context.Debug)
                {
                    _context.Log($"[DEBUG] VDrawerSeq: 开始绘制参与者");
                }
                DrawParticipants();
                PauseForViewing(500);

                // 3. 绘制生命线
                if (_context.Debug)
                {
                    _context.Log($"[DEBUG] VDrawerSeq: 开始绘制生命线");
                }
                DrawLifelines();
                PauseForViewing(300);

                // 4. 绘制激活框
                if (_context.Debug)
                {
                    _context.Log($"[DEBUG] VDrawerSeq: 开始绘制激活框");
                }
                DrawActivations();
                PauseForViewing(300);

                // 5. 绘制组合片段
                if (_context.Debug)
                {
                    _context.Log($"[DEBUG] VDrawerSeq: 开始绘制组合片段");
                }
                DrawFragments();
                PauseForViewing(300);

                // 6. 绘制消息
                if (_context.Debug)
                {
                    _context.Log($"[DEBUG] VDrawerSeq: 开始绘制消息");
                }
                DrawMessages();
                PauseForViewing(500);

                if (_context.Debug)
                {
                    _context.Log($"[DEBUG] VDrawerSeq: 时序图绘制完成");
                }
            }
            catch (Exception ex)
            {
                if (_context.Debug)
                {
                    _context.Log($"[DEBUG] VDrawerSeq: 绘制过程中发生异常: {ex.Message}");
                    _context.Log($"[DEBUG] VDrawerSeq: 异常类型: {ex.GetType().Name}");
                    if (ex.InnerException != null)
                    {
                        _context.Log($"[DEBUG] VDrawerSeq: 内部异常: {ex.InnerException.Message}");
                    }
                    _context.Log($"[DEBUG] VDrawerSeq: 异常堆栈: {ex.StackTrace}");
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
            // 计算参与者的水平位置 (放大15倍)
            double startX = 1500;
            for (int i = 0; i < figure.Participants.Count; i++)
            {
                figure.Participants[i].X = startX + i * participantSpacing;
            }

            // 计算垂直布局
            CalculateVerticalLayout();
        }

        private void CalculateVerticalLayout()
        {
            // 1. 计算消息起始Y位置：紧贴顶部参与者框下方
            double messageStartOffset = messageSpacing / 2;
            diagramStartY = topY - participantHeight / 2 - messageStartOffset;

            // 2. 找出内容的最低点（最小相对Y值）
            // Visio坐标系Y轴向上增长，所以最低内容点 = 最小Y值
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

            // 3. 边缘情况处理：无内容时设置最小高度
            if (figure.Messages.Count == 0 && figure.Activations.Count == 0 && figure.Fragments.Count == 0)
            {
                minRelativeY = -messageSpacing * 2; // 默认最小高度
            }

            // 4. 动态计算底部参与者位置
            double lastContentY = diagramStartY + minRelativeY;
            bottomY = lastContentY - messageStartOffset - participantHeight / 2;
        }

        private void DrawParticipants()
        {
            foreach (var participant in figure.Participants)
            {
                // 顶部参与者 - 先在(0,0)创建，再移动到正确位置
                participant.TopShape = visioPage.Drop(GetMaster("[]"), 0, 0);
                SetupParticipantShape(participant.TopShape, participant.DisplayName);
                PlaceAt(participant.TopShape, participant.X, topY);
                DebugLogPin($"Participant.Top '{participant.ID}'", participant.TopShape);

                // 底部参与者（镜像）
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

            // 设置样式
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
                // 获取实际参与者框高度（AdjustSize可能已调整）
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
            // 使用 DrawLine 绘制简单 1D 线，避免动态连接器吸附行为
            Shape line = visioPage.DrawLine(
                Inches(x), Inches(startY),
                Inches(x), Inches(endY));

            // 虚线样式
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
                double activationCenterY = AbsY(activation.CenterY);  // 应用 AbsY 偏移
                double activationHeight = Math.Abs(activation.Height);

                // 先在(0,0)创建，再移动到正确位置
                activation.ActivationShape = visioPage.Drop(GetMaster("[]"), 0, 0);
                PlaceAt(activation.ActivationShape, activationX, activationCenterY);
                DebugLogPin($"Activation '{activation.ParticipantId}' L{activation.NestingLevel}", activation.ActivationShape);

                activation.ActivationShape.CellsU["Width"].FormulaU = Mm(activationWidth);
                activation.ActivationShape.CellsU["Height"].FormulaU = Mm(activationHeight);

                // 激活框样式
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
            double y = AbsY(message.Y);  // 应用 AbsY 偏移

            // 判断消息方向：从左到右 或 从右到左
            bool isLeftToRight = fromX < toX;
            double leftX = Math.Min(fromX, toX);
            double rightX = Math.Max(fromX, toX);

            // 始终从左到右绘制线条，防止文字翻转
            Shape messageShape = visioPage.DrawLine(
                Inches(leftX), Inches(y),
                Inches(rightX), Inches(y));

            // 根据实际方向设置箭头位置
            SetupMessageArrowWithDirection(messageShape, message.ArrowType, isLeftToRight);

            // 设置消息文本
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
            // 设置线条样式
            if (arrowType.StartsWith("--"))
            {
                messageShape.CellsU["LinePattern"].FormulaU = "2"; // 虚线
            }
            else
            {
                messageShape.CellsU["LinePattern"].FormulaU = "1"; // 实线
            }

            messageShape.CellsU["LineWeight"].FormulaU = "1 pt";
            var messageColor = (VRGBColor)VRGBColor.Create("#333333");
            VShapeDrawer.SetLineColor(messageShape, messageColor);

            // 根据方向设置箭头位置
            // 线条始终从左到右绘制，所以：
            // - 左到右消息：箭头在 End（右端）
            // - 右到左消息：箭头在 Begin（左端）
            bool hasArrow = arrowType.EndsWith(">>") || arrowType.EndsWith(">");

            if (isLeftToRight)
            {
                // 左到右：箭头在右端 (End)
                messageShape.CellsU["EndArrow"].FormulaU = hasArrow ? "4" : "0";
                messageShape.CellsU["BeginArrow"].FormulaU = "0";
            }
            else
            {
                // 右到左：箭头在左端 (Begin)
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
            double startY = AbsY(message.Y);  // 应用 AbsY 偏移
            double endY = startY - selfCallHeight;
            double rightX = startX + selfCallWidth;

            // 使用 Inches() 转换布局单位到 Visio 内部单位
            double x1 = Inches(startX);
            double y1 = Inches(startY);
            double x2 = Inches(rightX);
            double y2 = Inches(endY);

            // 使用 DrawLine API 绘制三条线段组成 U 形自调用
            // 1. 水平线向右
            Shape line1 = visioPage.DrawLine(x1, y1, x2, y1);
            // 2. 垂直线向下
            Shape line2 = visioPage.DrawLine(x2, y1, x2, y2);
            // 3. 水平线向左（带箭头）
            Shape line3 = visioPage.DrawLine(x2, y2, x1, y2);

            // 设置线条样式
            var messageColor = (VRGBColor)VRGBColor.Create("#333333");
            foreach (var line in new[] { line1, line2, line3 })
            {
                VShapeDrawer.SetLineColor(line, messageColor);
                line.CellsU["LineWeight"].FormulaU = "1 pt";
                if (message.ArrowType.StartsWith("--"))
                {
                    line.CellsU["LinePattern"].FormulaU = "2"; // 虚线
                }
            }

            // 只在返回线上设置箭头
            if (message.ArrowType.EndsWith(">>") || message.ArrowType.EndsWith(">"))
            {
                line3.CellsU["EndArrow"].FormulaU = "4"; // 标准箭头
            }

            // 设置消息文本（使用独立文本框保持水平显示）
            if (!string.IsNullOrEmpty(message.Label))
            {
                var labelShape = DropText(message.Label, 0, 0);
                double labelX = rightX + selfCallTextOffset;
                double labelY = (startY + endY) / 2;
                PlaceAt(labelShape, labelX, labelY);
                DebugLogPin($"SelfCallText '{message.From}'", labelShape);
            }
            DebugLogPin($"SelfCall '{message.From}'", line3);

            return line3; // 返回带箭头的那条线
        }

    }
}
