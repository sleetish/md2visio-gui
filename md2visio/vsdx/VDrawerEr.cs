using md2visio.Api;
using md2visio.struc.er;
using md2visio.vsdx.@base;
using Microsoft.Office.Interop.Visio;
using System.Text;

namespace md2visio.vsdx
{
    /// <summary>
    /// ER图 Visio 绘制器
    /// 使用 Crow's Foot 表示法绘制实体关系图
    /// </summary>
    internal class VDrawerEr : VFigureDrawer<ErDiagram>
    {
        // 实体尺寸常量
        const double ENTITY_WIDTH = 2.2;
        const double ENTITY_MIN_HEIGHT = 0.8;
        const double ATTRIBUTE_HEIGHT = 0.2;
        const double HEADER_HEIGHT = 0.35;
        const double SPACING_H = 1.5;
        const double SPACING_V = 1.0;

        readonly List<ErEntity> drawnEntities = new();

        public VDrawerEr(ErDiagram figure, Application visioApp, ConversionContext context)
            : base(figure, visioApp, context) { }

        public override void Draw()
        {
            EnsureVisible();
            PauseForViewing(300);

            // 1. 绘制所有实体
            DrawEntities();
            PauseForViewing(500);

            // 2. 布局实体
            LayoutNodes();
            PauseForViewing(500);

            // 3. 绘制关系
            DrawRelations();
            PauseForViewing(300);
        }

        #region Draw Entities

        void DrawEntities()
        {
            foreach (var entity in figure.Entities.Values)
            {
                DrawEntity(entity);
                drawnEntities.Add(entity);
                PauseForViewing(150);
            }
        }

        void DrawEntity(ErEntity entity)
        {
            double height = GetEntityHeight(entity);

            // 在原点绘制，之后通过 LayoutNodes 重新定位
            Shape mainShape = visioPage.DrawRectangle(0, 0, ENTITY_WIDTH, height);

            entity.VisioShape = mainShape;

            // 设置样式
            mainShape.CellsU["LineWeight"].FormulaU = "1 pt";
            SetFillForegnd(mainShape, "config.themeVariables.primaryColor");
            SetLineColor(mainShape, "config.themeVariables.primaryBorderColor");

            // 构建文本内容
            StringBuilder textContent = new();

            // 实体名称 (标题)
            textContent.AppendLine(entity.GetDisplayName());

            // 属性列表
            if (entity.Attributes.Count > 0)
            {
                textContent.AppendLine("─────────────");
                foreach (var attr in entity.Attributes)
                {
                    // string keyIndicator = "";
                    // if (attr.IsPrimaryKey) keyIndicator = "🔑 ";
                    // else if (attr.IsForeignKey) keyIndicator = "🔗 ";
                    // textContent.AppendLine($"{keyIndicator}{attr.Type} {attr.Name}");

                    textContent.AppendLine($"{attr.Type} {attr.Name}");
                }
            }

            mainShape.Text = textContent.ToString().TrimEnd();
            mainShape.CellsU["VerticalAlign"].FormulaU = "0"; // 顶部对齐
            mainShape.CellsU["Para.HorzAlign"].FormulaU = "0"; // 左对齐
            mainShape.CellsU["Char.Size"].FormulaU = "9 pt";

            SetTextColor(mainShape, "config.themeVariables.primaryTextColor");
        }

        double GetEntityHeight(ErEntity entity)
        {
            double height = HEADER_HEIGHT;

            if (entity.Attributes.Count > 0)
            {
                height += entity.Attributes.Count * ATTRIBUTE_HEIGHT + 0.15; // 分隔线间距
            }

            return Math.Max(ENTITY_MIN_HEIGHT, height);
        }

        #endregion

        #region Layout

        void LayoutNodes()
        {
            var entities = figure.Entities.Values.ToList();
            if (entities.Count == 0) return;

            // 使用简单的分层布局
            var nodeLayer = AssignLayers();
            var layers = OrganizeLayers(nodeLayer);

            if (layers.Count == 0) return;

            // 计算并应用位置
            double startY = 10.0;
            double currentY = startY;
            var sortedLayerKeys = layers.Keys.OrderBy(k => k).ToList();

            foreach (var layerKey in sortedLayerKeys)
            {
                var layerEntities = layers[layerKey];

                // 计算层高度
                double maxHeight = layerEntities.Max(e => e.VisioShape != null ? Height(e.VisioShape) : ENTITY_MIN_HEIGHT);

                // 计算起始 X 位置
                double startX = 1.0;
                double currentX = startX;

                foreach (var entity in layerEntities)
                {
                    if (entity.VisioShape == null) continue;

                    double w = Width(entity.VisioShape);
                    double h = Height(entity.VisioShape);

                    MoveTo(entity.VisioShape, currentX + w / 2, currentY - h / 2);
                    PauseForViewing(80);

                    currentX += w + SPACING_H;
                }

                currentY -= maxHeight + SPACING_V;
            }
        }

        /// <summary>
        /// 为实体分配层级
        /// </summary>
        Dictionary<string, int> AssignLayers()
        {
            var nodeLayer = new Dictionary<string, int>();
            var allNodes = figure.Entities.Keys.ToHashSet();
            var inDegree = new Dictionary<string, int>();

            // 初始化入度
            foreach (var node in allNodes)
                inDegree[node] = 0;

            // 计算入度 (被指向的次数)
            foreach (var rel in figure.Relations)
            {
                if (allNodes.Contains(rel.ToEntity))
                    inDegree[rel.ToEntity]++;
            }

            // Kahn's 算法分配层级
            var queue = new Queue<string>();

            foreach (var node in allNodes)
            {
                if (inDegree[node] == 0)
                {
                    queue.Enqueue(node);
                    nodeLayer[node] = 0;
                }
            }

            while (queue.Count > 0)
            {
                string current = queue.Dequeue();
                int currentLayer = nodeLayer[current];

                foreach (var rel in figure.Relations.Where(r => r.FromEntity == current))
                {
                    string child = rel.ToEntity;
                    if (!allNodes.Contains(child)) continue;

                    int proposedLayer = currentLayer + 1;

                    if (!nodeLayer.ContainsKey(child))
                        nodeLayer[child] = proposedLayer;
                    else
                        nodeLayer[child] = Math.Max(nodeLayer[child], proposedLayer);

                    inDegree[child]--;
                    if (inDegree[child] == 0)
                        queue.Enqueue(child);
                }
            }

            // 处理未分配的节点
            foreach (var node in allNodes)
            {
                if (!nodeLayer.ContainsKey(node))
                {
                    nodeLayer[node] = 0;
                }
            }

            return nodeLayer;
        }

        /// <summary>
        /// 将实体组织到层中
        /// </summary>
        Dictionary<int, List<ErEntity>> OrganizeLayers(Dictionary<string, int> nodeLayer)
        {
            var layers = new Dictionary<int, List<ErEntity>>();

            foreach (var (nodeId, layer) in nodeLayer)
            {
                if (!figure.Entities.TryGetValue(nodeId, out var entity)) continue;
                if (entity.VisioShape == null) continue;

                if (!layers.ContainsKey(layer))
                    layers[layer] = new List<ErEntity>();
                layers[layer].Add(entity);
            }

            return layers;
        }

        #endregion

        #region Draw Relations

        void DrawRelations()
        {
            var drawnRelations = new HashSet<string>();

            foreach (var relation in figure.Relations)
            {
                string key = $"{relation.FromEntity}->{relation.ToEntity}:{relation.LeftCardinality}:{relation.RightCardinality}:{relation.IsIdentifying}:{relation.Label}";
                if (drawnRelations.Contains(key)) continue;

                if (!figure.Entities.TryGetValue(relation.FromEntity, out var fromEntity) ||
                    !figure.Entities.TryGetValue(relation.ToEntity, out var toEntity))
                    continue;

                if (fromEntity.VisioShape == null || toEntity.VisioShape == null)
                    continue;

                DrawRelation(relation, fromEntity, toEntity);
                drawnRelations.Add(key);
                PauseForViewing(100);
            }
        }

        void DrawRelation(ErRelation relation, ErEntity fromEntity, ErEntity toEntity)
        {
            Shape connector = CreateConnector(relation);

            if (!string.IsNullOrEmpty(relation.Label))
            {
                connector.Text = relation.Label;
                connector.CellsU["Char.Size"].FormulaU = "8 pt";
            }

            // 检查是否为自关联
            if (fromEntity == toEntity)
            {
                // 手动连接自关联关系
                Shape entityShape = fromEntity.VisioShape!;
                
                // 连接连接器的起点和终点到同一个形状的不同连接点
                connector.CellsU["BeginX"].GlueTo(entityShape.CellsU["PinX"]);
                connector.CellsU["BeginY"].GlueTo(entityShape.CellsU["PinY"]);
                connector.CellsU["EndX"].GlueTo(entityShape.CellsU["PinX"]);
                connector.CellsU["EndY"].GlueTo(entityShape.CellsU["PinY"]);
                
                // 调整连接器路径以形成循环
                double shapeWidth = Width(entityShape);
                double shapeHeight = Height(entityShape);
                double pinX = PinX(entityShape);
                double pinY = PinY(entityShape);
                
                // 设置连接器的控制点以创建自循环
                connector.CellsU["BeginX"].FormulaU = $"{pinX + shapeWidth/2}";
                connector.CellsU["BeginY"].FormulaU = $"{pinY}";
                connector.CellsU["EndX"].FormulaU = $"{pinX}";
                connector.CellsU["EndY"].FormulaU = $"{pinY + shapeHeight/2}";
                return;
            }
            else
            {
                // 使用 AutoConnect 连接两个不同的实体
                fromEntity.VisioShape!.AutoConnect(toEntity.VisioShape!, VisAutoConnectDir.visAutoConnectDirNone, connector);
            }
            
            connector.Delete();
        }

        Shape CreateConnector(ErRelation relation)
        {
            Master? master = GetMaster("-");
            Shape connector = visioPage.Drop(master, 0, 0);

            // 初始化两端无箭头
            connector.CellsU["BeginArrow"].FormulaU = "0";
            connector.CellsU["EndArrow"].FormulaU = "0";
            connector.CellsU["BeginArrowSize"].FormulaU = "2";
            connector.CellsU["EndArrowSize"].FormulaU = "2";

            // 设置线型 (实线或虚线)
            connector.CellsU["LinePattern"].FormulaU = relation.IsIdentifying ? "1" : "2";

            // Crow's Foot 表示法使用箭头
            // 起始端 (左基数)
            connector.CellsU["BeginArrow"].FormulaU = GetCrowsFootArrow(relation.LeftCardinality);

            // 目标端 (右基数)
            connector.CellsU["EndArrow"].FormulaU = GetCrowsFootArrow(relation.RightCardinality);

            connector.CellsU["LineWeight"].FormulaU = "0.75 pt";
            SetLineColor(connector, "config.themeVariables.lineColor");

            return connector;
        }

        /// <summary>
        /// 获取 Crow's Foot 箭头样式
        /// Visio 箭头索引:
        /// 0 = 无
        /// 1 = 简单箭头
        /// 4 = 空心三角
        /// 10 = 鸦爪 (many) - Visio 不支持可选性组合符号
        /// 11 = 空心菱形 (用于近似 zero-or-one)
        /// 22 = 竖线 (one)
        /// </summary>
        string GetCrowsFootArrow(ErCardinality cardinality)
        {
            return cardinality switch
            {
                ErCardinality.ExactlyOne => "22",    // 竖线 - 恰好一个
                ErCardinality.ZeroOrOne => "11",     // 空心菱形 - 零或一个(近似)
                ErCardinality.OneOrMore => "10",     // 鸦爪 - 一个或多个
                ErCardinality.ZeroOrMore => "10",    // 鸦爪 - 零个或多个(与 OneOrMore 相同)
                _ => "0"
            };
        }

        #endregion
    }
}
