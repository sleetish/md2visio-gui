using md2visio.Api;
using md2visio.struc.er;
using md2visio.vsdx.@base;
using Microsoft.Office.Interop.Visio;
using System.Text;

namespace md2visio.vsdx
{
    /// <summary>
    /// ER Diagram Visio Drawer
    /// Draws Entity-Relationship diagrams using Crow's Foot notation
    /// </summary>
    internal class VDrawerEr : VFigureDrawer<ErDiagram>
    {
        // Entity dimension constants
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

            // 1. Draw all entities
            DrawEntities();
            PauseForViewing(500);

            // 2. Layout entities
            LayoutNodes();
            PauseForViewing(500);

            // 3. Draw relations
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

            // Draw at origin, will be repositioned by LayoutNodes
            Shape mainShape = visioPage.DrawRectangle(0, 0, ENTITY_WIDTH, height);

            entity.VisioShape = mainShape;

            // Set styles
            mainShape.CellsU["LineWeight"].FormulaU = "1 pt";
            SetFillForegnd(mainShape, "config.themeVariables.primaryColor");
            SetLineColor(mainShape, "config.themeVariables.primaryBorderColor");

            // Build text content
            StringBuilder textContent = new();

            // Entity Name (Title)
            textContent.AppendLine(entity.GetDisplayName());

            // Attribute list
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
            mainShape.CellsU["VerticalAlign"].FormulaU = "0"; // Top alignment
            mainShape.CellsU["Para.HorzAlign"].FormulaU = "0"; // Left alignment
            mainShape.CellsU["Char.Size"].FormulaU = "9 pt";

            SetTextColor(mainShape, "config.themeVariables.primaryTextColor");
        }

        double GetEntityHeight(ErEntity entity)
        {
            double height = HEADER_HEIGHT;

            if (entity.Attributes.Count > 0)
            {
                height += entity.Attributes.Count * ATTRIBUTE_HEIGHT + 0.15; // Separator line spacing
            }

            return Math.Max(ENTITY_MIN_HEIGHT, height);
        }

        #endregion

        #region Layout

        void LayoutNodes()
        {
            var entities = figure.Entities.Values.ToList();
            if (entities.Count == 0) return;

            // Use simple hierarchical layout
            var nodeLayer = AssignLayers();
            var layers = OrganizeLayers(nodeLayer);

            if (layers.Count == 0) return;

            // Calculate and apply positions
            double startY = 10.0;
            double currentY = startY;
            var sortedLayerKeys = layers.Keys.OrderBy(k => k).ToList();

            foreach (var layerKey in sortedLayerKeys)
            {
                var layerEntities = layers[layerKey];

                // Calculate layer height
                double maxHeight = layerEntities.Max(e => e.VisioShape != null ? Height(e.VisioShape) : ENTITY_MIN_HEIGHT);

                // Calculate starting X position
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
        /// Assign layers to entities
        /// </summary>
        Dictionary<string, int> AssignLayers()
        {
            var nodeLayer = new Dictionary<string, int>();
            var allNodes = figure.Entities.Keys.ToHashSet();
            var inDegree = new Dictionary<string, int>();

            // Initialize in-degree
            foreach (var node in allNodes)
                inDegree[node] = 0;

            // Calculate in-degree (number of incoming edges)
            foreach (var rel in figure.Relations)
            {
                if (allNodes.Contains(rel.ToEntity))
                    inDegree[rel.ToEntity]++;
            }

            // Kahn's algorithm for layer assignment
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

            // Handle unassigned nodes
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
        /// Organize entities into layers
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

            // Check if self-association
            if (fromEntity == toEntity)
            {
                // Manually connect self-association
                Shape entityShape = fromEntity.VisioShape!;
                
                // Glue start and end points of the connector to different connection points on the same shape
                connector.CellsU["BeginX"].GlueTo(entityShape.CellsU["PinX"]);
                connector.CellsU["BeginY"].GlueTo(entityShape.CellsU["PinY"]);
                connector.CellsU["EndX"].GlueTo(entityShape.CellsU["PinX"]);
                connector.CellsU["EndY"].GlueTo(entityShape.CellsU["PinY"]);
                
                // Adjust connector path to form a loop
                double shapeWidth = Width(entityShape);
                double shapeHeight = Height(entityShape);
                double pinX = PinX(entityShape);
                double pinY = PinY(entityShape);
                
                // Set connector control points to create a self-loop
                connector.CellsU["BeginX"].FormulaU = $"{pinX + shapeWidth/2}";
                connector.CellsU["BeginY"].FormulaU = $"{pinY}";
                connector.CellsU["EndX"].FormulaU = $"{pinX}";
                connector.CellsU["EndY"].FormulaU = $"{pinY + shapeHeight/2}";
                return;
            }
            else
            {
                // Use AutoConnect to connect two different entities
                fromEntity.VisioShape!.AutoConnect(toEntity.VisioShape!, VisAutoConnectDir.visAutoConnectDirNone, connector);
            }
            
            connector.Delete();
        }

        Shape CreateConnector(ErRelation relation)
        {
            Master? master = GetMaster("-");
            Shape connector = visioPage.Drop(master, 0, 0);

            // Initialize no arrows at both ends
            connector.CellsU["BeginArrow"].FormulaU = "0";
            connector.CellsU["EndArrow"].FormulaU = "0";
            connector.CellsU["BeginArrowSize"].FormulaU = "2";
            connector.CellsU["EndArrowSize"].FormulaU = "2";

            // Set line pattern (solid or dashed)
            connector.CellsU["LinePattern"].FormulaU = relation.IsIdentifying ? "1" : "2";

            // Crow's Foot notation uses arrows
            // Start end (Left cardinality)
            connector.CellsU["BeginArrow"].FormulaU = GetCrowsFootArrow(relation.LeftCardinality);

            // Target end (Right cardinality)
            connector.CellsU["EndArrow"].FormulaU = GetCrowsFootArrow(relation.RightCardinality);

            connector.CellsU["LineWeight"].FormulaU = "0.75 pt";
            SetLineColor(connector, "config.themeVariables.lineColor");

            return connector;
        }

        /// <summary>
        /// Get Crow's Foot arrow style
        /// Visio arrow indices:
        /// 0 = None
        /// 1 = Simple arrow
        /// 4 = Hollow triangle
        /// 10 = Crow's foot (many) - Visio does not support combined optionality symbols
        /// 11 = Hollow diamond (used for approximate zero-or-one)
        /// 22 = Vertical bar (one)
        /// </summary>
        string GetCrowsFootArrow(ErCardinality cardinality)
        {
            return cardinality switch
            {
                ErCardinality.ExactlyOne => "22",    // Vertical bar - Exactly one
                ErCardinality.ZeroOrOne => "11",     // Hollow diamond - Zero or one (approximate)
                ErCardinality.OneOrMore => "10",     // Crow's foot - One or more
                ErCardinality.ZeroOrMore => "10",    // Crow's foot - Zero or more (same as OneOrMore)
                _ => "0"
            };
        }

        #endregion
    }
}
