using md2visio.Api;
using md2visio.struc.classdiag;
using md2visio.vsdx.@base;
using Microsoft.Office.Interop.Visio;
using System.Text;

namespace md2visio.vsdx
{
    internal class VDrawerCls : VFigureDrawer<ClassDiagram>
    {
        const double CLASS_WIDTH = 2.0;
        const double CLASS_MIN_HEIGHT = 0.6;
        const double MEMBER_HEIGHT = 0.2;
        const double HEADER_HEIGHT = 0.35;
        const double SPACING_H = 1.2;
        const double SPACING_V = 0.8;

        readonly List<ClsClass> drawnClasses = new();

        public VDrawerCls(ClassDiagram figure, Application visioApp, ConversionContext context)
            : base(figure, visioApp, context) { }

        public override void Draw()
        {
            EnsureVisible();
            PauseForViewing(300);

            // 1. Draw all classes first (at origin)
            DrawClasses();
            PauseForViewing(500);

            // 2. Layout using BFS tree
            LayoutNodes();
            PauseForViewing(500);

            // 3. Draw relationships
            DrawRelations();
            PauseForViewing(300);

            DrawNamespaceBorders();
        }

        #region Sugiyama Layered Layout

        /// <summary>
        /// Weighted edge for layout algorithm
        /// </summary>
        class WeightedEdge
        {
            public string FromClass { get; set; } = "";
            public string ToClass { get; set; } = "";
            public ClsRelationType Type { get; set; }
            public int Weight { get; set; }
        }

        /// <summary>
        /// Get relation weight (smaller = stronger hierarchy constraint)
        /// </summary>
        static int GetRelationWeight(ClsRelationType type) => type switch
        {
            ClsRelationType.Inheritance => 1,
            ClsRelationType.Realization => 2,
            ClsRelationType.Composition => 3,
            ClsRelationType.Aggregation => 4,
            ClsRelationType.Association => 5,
            ClsRelationType.Link => 6,
            ClsRelationType.Dependency => 7,
            ClsRelationType.DashedLink => 8,
            _ => 9
        };

        /// <summary>
        /// Build weighted directed graph from all relationships
        /// </summary>
        Dictionary<string, List<WeightedEdge>> BuildWeightedGraph()
        {
            var graph = new Dictionary<string, List<WeightedEdge>>();

            foreach (var rel in figure.Relations)
            {
                int weight = GetRelationWeight(rel.Type);

                // Determine direction based on relation type and decoration
                string parent, child;

                if (rel.Type == ClsRelationType.Association || rel.Type == ClsRelationType.Dependency)
                {
                    // Arrow direction: A-->B means A uses/depends on B
                    // In layout: A (user) should be at higher layer, B (used) at lower layer
                    // IsDecorationOnFrom=true when symbol starts with < (e.g., <--)
                    // For -->, IsDecorationOnFrom=false: FromClass is parent, ToClass is child
                    // For <--, IsDecorationOnFrom=true: ToClass is parent, FromClass is child
                    parent = rel.IsDecorationOnFrom ? rel.ToClass : rel.FromClass;
                    child = rel.IsDecorationOnFrom ? rel.FromClass : rel.ToClass;
                }
                else
                {
                    // Inheritance/Composition/Aggregation: decoration side is "whole/parent"
                    parent = rel.IsDecorationOnFrom ? rel.FromClass : rel.ToClass;
                    child = rel.IsDecorationOnFrom ? rel.ToClass : rel.FromClass;
                }

                var edge = new WeightedEdge
                {
                    FromClass = parent,
                    ToClass = child,
                    Type = rel.Type,
                    Weight = weight
                };

                if (!graph.ContainsKey(parent))
                    graph[parent] = new List<WeightedEdge>();
                graph[parent].Add(edge);
            }

            return graph;
        }

        /// <summary>
        /// Assign layers using longest path algorithm (Kahn's algorithm variant)
        /// </summary>
        Dictionary<string, int> AssignLayers(Dictionary<string, List<WeightedEdge>> graph)
        {
            var nodeLayer = new Dictionary<string, int>();
            var inDegree = new Dictionary<string, int>();
            var allNodes = figure.Classes.Keys.ToHashSet();

            // Initialize in-degree for all nodes
            foreach (var node in allNodes)
                inDegree[node] = 0;

            // Calculate in-degree (only consider edges with weight <= 7, include Dependency)
            foreach (var (_, edges) in graph)
            {
                foreach (var edge in edges.Where(e => e.Weight <= 7))
                {
                    if (allNodes.Contains(edge.ToClass))
                        inDegree[edge.ToClass]++;
                }
            }

            // Kahn's algorithm with longest path
            var queue = new Queue<string>();

            // Start with nodes having in-degree 0 (root nodes)
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

                if (!graph.ContainsKey(current)) continue;

                foreach (var edge in graph[current].Where(e => e.Weight <= 7))
                {
                    string child = edge.ToClass;
                    if (!allNodes.Contains(child)) continue;

                    // Longest path: child layer = max(current layer, parent layer + 1)
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

            // Handle nodes not yet assigned (in cycles or isolated)
            foreach (var node in allNodes)
            {
                if (!nodeLayer.ContainsKey(node))
                {
                    // Find max layer from incoming edges, or use 0
                    int maxIncoming = 0;
                    foreach (var (parent, edges) in graph)
                    {
                        if (edges.Any(e => e.ToClass == node) && nodeLayer.ContainsKey(parent))
                            maxIncoming = Math.Max(maxIncoming, nodeLayer[parent] + 1);
                    }
                    nodeLayer[node] = maxIncoming;
                }
            }

            return nodeLayer;
        }

        /// <summary>
        /// Organize nodes into layers
        /// </summary>
        Dictionary<int, List<ClsClass>> OrganizeLayers(Dictionary<string, int> nodeLayer)
        {
            var layers = new Dictionary<int, List<ClsClass>>();

            foreach (var (nodeId, layer) in nodeLayer)
            {
                if (!figure.Classes.TryGetValue(nodeId, out var cls)) continue;
                if (cls.VisioShape == null) continue;

                if (!layers.ContainsKey(layer))
                    layers[layer] = new List<ClsClass>();
                layers[layer].Add(cls);
            }

            return layers;
        }

        /// <summary>
        /// Optimize layer ordering using barycenter method to minimize edge crossings
        /// </summary>
        void OptimizeLayerOrdering(Dictionary<int, List<ClsClass>> layers, Dictionary<string, List<WeightedEdge>> graph)
        {
            var sortedLayerKeys = layers.Keys.OrderBy(k => k).ToList();
            if (sortedLayerKeys.Count <= 1) return;

            // Iterate 3 times for optimization
            for (int iter = 0; iter < 3; iter++)
            {
                // Downward sweep
                for (int i = 0; i < sortedLayerKeys.Count - 1; i++)
                {
                    int currentLayerKey = sortedLayerKeys[i];
                    int nextLayerKey = sortedLayerKeys[i + 1];

                    if (!layers.ContainsKey(nextLayerKey)) continue;

                    OptimizeLayer(layers[nextLayerKey], layers[currentLayerKey], graph, true);
                }

                // Upward sweep - use same graph, direction handled by isDownward flag
                for (int i = sortedLayerKeys.Count - 1; i > 0; i--)
                {
                    int currentLayerKey = sortedLayerKeys[i];
                    int prevLayerKey = sortedLayerKeys[i - 1];

                    if (!layers.ContainsKey(prevLayerKey)) continue;

                    OptimizeLayer(layers[prevLayerKey], layers[currentLayerKey], graph, false);
                }
            }
        }

        /// <summary>
        /// Optimize single layer using barycenter method
        /// </summary>
        void OptimizeLayer(List<ClsClass> currentLayer, List<ClsClass> adjacentLayer,
            Dictionary<string, List<string>> connectionGraph, bool isDownward)
        {
            // Build position map for adjacent layer
            var positions = new Dictionary<string, int>();
            for (int i = 0; i < adjacentLayer.Count; i++)
                positions[adjacentLayer[i].ID] = i;

            var barycenters = new List<(ClsClass cls, double barycenter)>();

            foreach (var cls in currentLayer)
            {
                var connectedPositions = new List<int>();

                if (isDownward)
                {
                    // For downward: check edges from adjacent layer to current
                    foreach (var adj in adjacentLayer)
                    {
                        if (connectionGraph.TryGetValue(adj.ID, out var edges))
                        {
                            if (edges.Contains(cls.ID) && positions.ContainsKey(adj.ID))
                                connectedPositions.Add(positions[adj.ID]);
                        }
                    }
                }
                else
                {
                    // For upward: check edges from current to adjacent
                    if (connectionGraph.TryGetValue(cls.ID, out var edges))
                    {
                        foreach (var target in edges)
                        {
                            if (positions.ContainsKey(target))
                                connectedPositions.Add(positions[target]);
                        }
                    }
                }

                double barycenter = connectedPositions.Any()
                    ? connectedPositions.Average()
                    : barycenters.Count;

                barycenters.Add((cls, barycenter));
            }

            // Sort by barycenter
            currentLayer.Clear();
            currentLayer.AddRange(barycenters.OrderBy(x => x.barycenter).Select(x => x.cls));
        }

        /// <summary>
        /// Wrapper for string-based graph
        /// </summary>
        void OptimizeLayer(List<ClsClass> currentLayer, List<ClsClass> adjacentLayer,
            Dictionary<string, List<WeightedEdge>> graph, bool isDownward)
        {
            // Convert WeightedEdge graph to simple string graph
            var simpleGraph = new Dictionary<string, List<string>>();
            foreach (var (parent, edges) in graph)
            {
                simpleGraph[parent] = edges.Select(e => e.ToClass).ToList();
            }

            OptimizeLayer(currentLayer, adjacentLayer, simpleGraph, isDownward);
        }

        /// <summary>
        /// Main layout method using Sugiyama algorithm
        /// </summary>
        void LayoutNodes()
        {
            var classes = figure.Classes.Values.ToList();
            if (classes.Count == 0) return;

            // Phase 1: Build weighted graph
            var weightedGraph = BuildWeightedGraph();

            // Phase 2: Assign layers
            var nodeLayer = AssignLayers(weightedGraph);
            var layers = OrganizeLayers(nodeLayer);

            if (layers.Count == 0) return;

            // Phase 3: Optimize layer ordering
            OptimizeLayerOrdering(layers, weightedGraph);

            // Phase 4: Calculate and apply positions
            double startY = 10.0;
            double currentY = startY;
            var sortedLayerKeys = layers.Keys.OrderBy(k => k).ToList();

            foreach (var layerKey in sortedLayerKeys)
            {
                var layerClasses = layers[layerKey];

                // Calculate layer height
                double maxHeight = layerClasses.Max(c => c.VisioShape != null ? Height(c.VisioShape) : CLASS_MIN_HEIGHT);

                // Calculate total width
                double totalWidth = layerClasses.Sum(c => c.VisioShape != null ? Width(c.VisioShape) : CLASS_WIDTH)
                                  + (layerClasses.Count - 1) * SPACING_H;

                // Center the layer horizontally
                double startX = 1.0;
                double currentX = startX;

                foreach (var cls in layerClasses)
                {
                    if (cls.VisioShape == null) continue;

                    double w = Width(cls.VisioShape);
                    double h = Height(cls.VisioShape);

                    // Position: center of shape
                    MoveTo(cls.VisioShape, currentX + w / 2, currentY - h / 2);
                    PauseForViewing(80);

                    currentX += w + SPACING_H;
                }

                // Move to next layer
                currentY -= maxHeight + SPACING_V;
            }
        }

        #endregion

        #region Draw Classes

        void DrawClasses()
        {
            foreach (var cls in figure.Classes.Values)
            {
                DrawClass(cls);
                drawnClasses.Add(cls);
                PauseForViewing(150);
            }
        }

        void DrawClass(ClsClass cls)
        {
            double height = GetClassHeight(cls);

            // Draw at origin first, will be repositioned by LayoutNodes
            Shape mainShape = visioPage.DrawRectangle(0, 0, CLASS_WIDTH, height);

            cls.VisioShape = mainShape;

            mainShape.CellsU["LineWeight"].FormulaU = "1 pt";
            SetFillForegnd(mainShape, "config.themeVariables.primaryColor");
            SetLineColor(mainShape, "config.themeVariables.primaryBorderColor");

            StringBuilder textContent = new();

            if (!string.IsNullOrEmpty(cls.Annotation))
            {
                textContent.AppendLine($"<<{cls.Annotation}>>");
            }

            textContent.AppendLine(cls.DisplayName);

            if (cls.Properties.Count > 0)
            {
                textContent.AppendLine("─────────────");
                foreach (var prop in cls.Properties)
                {
                    textContent.AppendLine(prop.ToDisplayString());
                }
            }

            if (cls.Methods.Count > 0)
            {
                textContent.AppendLine("─────────────");
                foreach (var method in cls.Methods)
                {
                    textContent.AppendLine(method.ToDisplayString());
                }
            }

            mainShape.Text = textContent.ToString().TrimEnd();
            mainShape.CellsU["VerticalAlign"].FormulaU = "0";
            mainShape.CellsU["Para.HorzAlign"].FormulaU = "0";
            mainShape.CellsU["Char.Size"].FormulaU = "9 pt";

            SetTextColor(mainShape, "config.themeVariables.primaryTextColor");
        }

        double GetClassHeight(ClsClass cls)
        {
            double height = HEADER_HEIGHT;

            if (!string.IsNullOrEmpty(cls.Annotation))
                height += 0.15;

            if (cls.Properties.Count > 0)
                height += cls.Properties.Count * MEMBER_HEIGHT + 0.1;

            if (cls.Methods.Count > 0)
                height += cls.Methods.Count * MEMBER_HEIGHT + 0.1;

            return Math.Max(CLASS_MIN_HEIGHT, height);
        }

        #endregion

        #region Draw Relations

        void DrawRelations()
        {
            var drawnRelations = new HashSet<string>();

            foreach (var relation in figure.Relations)
            {
                string key = $"{relation.FromClass}->{relation.ToClass}";
                if (drawnRelations.Contains(key)) continue;

                if (!figure.Classes.TryGetValue(relation.FromClass, out var fromClass) ||
                    !figure.Classes.TryGetValue(relation.ToClass, out var toClass))
                    continue;

                if (fromClass.VisioShape == null || toClass.VisioShape == null)
                    continue;

                DrawRelation(relation, fromClass, toClass);
                drawnRelations.Add(key);
                PauseForViewing(100);
            }
        }

        void DrawRelation(ClsRelation relation, ClsClass fromClass, ClsClass toClass)
        {
            Shape connector = CreateConnector(relation);

            if (!string.IsNullOrEmpty(relation.Label))
            {
                connector.Text = relation.Label;
                connector.CellsU["Char.Size"].FormulaU = "8 pt";
            }

            // Use AutoConnect + Delete pattern like VDrawerG (flowchart)
            fromClass.VisioShape!.AutoConnect(toClass.VisioShape!, VisAutoConnectDir.visAutoConnectDirNone, connector);
            connector.Delete();
        }

        Shape CreateConnector(ClsRelation relation)
        {
            Master? master = GetMaster("-");
            Shape connector = visioPage.Drop(master, 0, 0);
            bool decorationOnFrom = relation.IsDecorationOnFrom;

            // Initialize both ends to NO arrow (prevents double-arrow bug)
            connector.CellsU["BeginArrow"].FormulaU = "0";
            connector.CellsU["EndArrow"].FormulaU = "0";
            connector.CellsU["BeginArrowSize"].FormulaU = "2";
            connector.CellsU["EndArrowSize"].FormulaU = "2";

            switch (relation.Type)
            {
                case ClsRelationType.Inheritance:
                    connector.CellsU["LinePattern"].FormulaU = "1";
                    if (decorationOnFrom)
                        connector.CellsU["BeginArrow"].FormulaU = "4";
                    else
                        connector.CellsU["EndArrow"].FormulaU = "4";
                    break;

                case ClsRelationType.Realization:
                    connector.CellsU["LinePattern"].FormulaU = "2";
                    if (decorationOnFrom)
                        connector.CellsU["BeginArrow"].FormulaU = "4";
                    else
                        connector.CellsU["EndArrow"].FormulaU = "4";
                    break;

                case ClsRelationType.Composition:
                    connector.CellsU["LinePattern"].FormulaU = "1";
                    if (decorationOnFrom)
                        connector.CellsU["BeginArrow"].FormulaU = "12";
                    else
                        connector.CellsU["EndArrow"].FormulaU = "12";
                    break;

                case ClsRelationType.Aggregation:
                    connector.CellsU["LinePattern"].FormulaU = "1";
                    if (decorationOnFrom)
                        connector.CellsU["BeginArrow"].FormulaU = "11";
                    else
                        connector.CellsU["EndArrow"].FormulaU = "11";
                    break;

                case ClsRelationType.Association:
                    connector.CellsU["LinePattern"].FormulaU = "1";
                    if (decorationOnFrom)
                        connector.CellsU["BeginArrow"].FormulaU = "1";
                    else
                        connector.CellsU["EndArrow"].FormulaU = "1";
                    break;

                case ClsRelationType.Dependency:
                    connector.CellsU["LinePattern"].FormulaU = "2";
                    if (decorationOnFrom)
                        connector.CellsU["BeginArrow"].FormulaU = "1";
                    else
                        connector.CellsU["EndArrow"].FormulaU = "1";
                    break;

                case ClsRelationType.Link:
                    connector.CellsU["LinePattern"].FormulaU = "1";
                    break;

                case ClsRelationType.DashedLink:
                    connector.CellsU["LinePattern"].FormulaU = "2";
                    break;
            }

            connector.CellsU["LineWeight"].FormulaU = "0.75 pt";
            SetLineColor(connector, "config.themeVariables.lineColor");

            return connector;
        }

        #endregion

        #region Namespace Borders

        void DrawNamespaceBorders()
        {
            foreach (var ns in figure.Namespaces.Values)
            {
                if (ns.ClassIds.Count == 0) continue;

                var nsClasses = ns.ClassIds
                    .Where(id => figure.Classes.ContainsKey(id))
                    .Select(id => figure.Classes[id])
                    .Where(c => c.VisioShape != null)
                    .ToList();

                if (nsClasses.Count == 0) continue;

                VBoundary bound = new VBoundary(true);
                foreach (var cls in nsClasses)
                {
                    bound.Expand(cls.VisioShape!);
                }

                double padding = 0.2;
                Shape border = visioPage.DrawRectangle(
                    bound.Left - padding,
                    bound.Bottom - padding - 0.25,
                    bound.Right + padding,
                    bound.Top + padding + 0.25);

                border.CellsU["LinePattern"].FormulaU = "2";
                border.CellsU["FillPattern"].FormulaU = "0";
                border.CellsU["LineWeight"].FormulaU = "0.5 pt";
                border.Text = ns.Name;
                border.CellsU["VerticalAlign"].FormulaU = "0";
                border.CellsU["TxtPinY"].FormulaU = "Height - 0.15 in";

                SetLineColor(border, "config.themeVariables.secondaryBorderColor");
                SetTextColor(border, "config.themeVariables.secondaryTextColor");

                visioApp.DoCmd((short)VisUICmds.visCmdObjectSendToBack);

                ns.BorderShape = border;
            }
        }

        #endregion
    }
}
