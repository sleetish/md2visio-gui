using md2visio.Api;
using md2visio.mermaid.cmn;
using md2visio.struc.figure;
using md2visio.struc.graph;
using md2visio.vsdx.@base;
using Microsoft.Office.Interop.Visio;

namespace md2visio.vsdx
{
    internal enum RelativePos
    {
        FRONT, TAIL
    }

    internal class VDrawerG : VFigureDrawer<Graph>
    {
        const double GraphMinTextRate = 0.4;
        LinkedList<GNode> drawnList = new LinkedList<GNode>();
        HashSet<GNode> drawnSet = new HashSet<GNode>();

        public VDrawerG(Graph figure, Application visioApp, ConversionContext context)
            : base(figure, visioApp, context) { }

        public override void Draw()
        {
            EnsureVisible(); // 确保Visio可见
            PauseForViewing(300); // 给用户时间看到初始状态
            
            DrawNodes(figure);
            PauseForViewing(500); // 节点绘制完成后暂停
            
            DrawEdges(figure);
            PauseForViewing(300); // 边绘制完成后暂停
        }        

        void DrawNodes(Graph graph)
        {
            foreach (GSubgraph subGraph in graph.Subgraphs)
            {
                DrawNodes(subGraph);
            }

            // align grouped nodes
            LinkedList<GNode> nodes2Draw = new LinkedList<GNode>();
            (GNode? linkedNode, RelativePos rpos) = graph.NodesHavingInput(nodes2Draw);
            while (linkedNode != null)
            {
                if (linkedNode.VisioShape == null) break;
                (linkedNode, rpos) = graph.NodesHavingInput(nodes2Draw);
            }

            foreach (GNode node in graph.AlignGroupedNodes()) 
            {
                if(!nodes2Draw.Contains(node)) nodes2Draw.AddLast(node);
            }

            // draw nodes
            if (nodes2Draw.Count > 0)
            {
                DrawLinkedNodes(nodes2Draw, graph.GrowthDirect);
                Relocate(nodes2Draw.ToList(), graph.GrowthDirect);
            }

            // border - 直接判断当前graph是否为subgraph
            if (graph is GSubgraph subgraph)
            {
                DrawSubgraphBorder(subgraph);
                Relocate(subgraph);
            }        
        }

        List<GNode> SortNodesBFS(LinkedList<GNode> nodes)
        {
            var nodeSet = new HashSet<GNode>(nodes);
            var sorted = new List<GNode>();
            var visited = new HashSet<GNode>();
            var queue = new Queue<GNode>();

            foreach (var startNode in nodes)
            {
                if (!nodeSet.Contains(startNode) || visited.Contains(startNode)) continue;

                visited.Add(startNode);
                queue.Enqueue(startNode);

                while (queue.Count > 0)
                {
                    var n = queue.Dequeue();
                    sorted.Add(n);

                    foreach (var child in n.OutputNodes())
                    {
                        if (nodeSet.Contains(child) && !visited.Contains(child))
                        {
                            visited.Add(child);
                            queue.Enqueue(child);
                        }
                    }
                }
            }
            return sorted;
        }

        void DrawLinkedNodes(LinkedList<GNode> nodes, GrowthDirection direct)
        {
            if (nodes.Count == 0) return;

            var sortedNodes = SortNodesBFS(nodes);

            // 1. 创建所有形状
            foreach (GNode node in sortedNodes)
            {
                if (node is GBorderNode) continue;

                Shape shape = CreateShape(node);
                SetFillForegnd(shape, "config.themeVariables.primaryColor");
                SetLineColor(shape, "config.themeVariables.primaryBorderColor");
                SetTextColor(shape, "config.themeVariables.primaryTextColor");
                shape.CellsU["LineWeight"].FormulaU = "0.75 pt";
                PostProcessShape(node, shape);
            }

            // 2. 计算子树宽度（后序遍历）
            var subtreeCrossSizes = new Dictionary<GNode, double>();
            bool isVertical = direct.H == 0;

            double GetCrossSize(GNode n) => n.VisioShape != null
                ? (isVertical ? Width(n.VisioShape) : Height(n.VisioShape)) : 0;
            double GetMainSize(GNode n) => n.VisioShape != null
                ? (isVertical ? Height(n.VisioShape) : Width(n.VisioShape)) : 0;

            // 反向遍历BFS列表实现后序
            for (int i = sortedNodes.Count - 1; i >= 0; i--)
            {
                var node = sortedNodes[i];
                if (node.VisioShape == null) continue;

                var children = node.OutputNodes()
                    .Where(c => sortedNodes.Contains(c) && c.VisioShape != null)
                    .ToList();

                double selfSize = GetCrossSize(node);

                if (children.Count == 0)
                {
                    subtreeCrossSizes[node] = selfSize;
                }
                else
                {
                    double childrenSize = children.Sum(c => subtreeCrossSizes.GetValueOrDefault(c, GetCrossSize(c)))
                                        + (children.Count - 1) * GNode.SPACE;
                    subtreeCrossSizes[node] = Math.Max(selfSize, childrenSize);
                }
            }

            // 3. 布局（预序遍历）
            var processed = new HashSet<GNode>();
            var roots = sortedNodes.Where(n => !n.InputNodes().Any(p => sortedNodes.Contains(p))).ToList();
            double currentRootCross = 0;

            void PlaceTree(GNode node, double crossCenter, double mainPos)
            {
                if (processed.Contains(node) || node.VisioShape == null) return;
                processed.Add(node);

                // 定位节点
                double finalX, finalY;
                if (isVertical)
                {
                    finalX = crossCenter;
                    finalY = mainPos;
                }
                else
                {
                    finalX = mainPos;
                    finalY = crossCenter;
                }
                MoveTo(node.VisioShape, finalX, finalY);
                drawnList.AddLast(node);
                drawnSet.Add(node);
                PauseForViewing(100);

                // 处理子节点
                var children = node.OutputNodes()
                    .Where(c => sortedNodes.Contains(c) && c.VisioShape != null && !processed.Contains(c))
                    .ToList();

                if (children.Count == 0) return;

                double childrenTotalCross = children.Sum(c => subtreeCrossSizes.GetValueOrDefault(c, GetCrossSize(c)))
                                          + (children.Count - 1) * GNode.SPACE;

                double startChildCross = crossCenter - childrenTotalCross / 2;

                foreach (var child in children)
                {
                    double childCrossW = subtreeCrossSizes.GetValueOrDefault(child, GetCrossSize(child));
                    double childCenter = startChildCross + childCrossW / 2;

                    double selfMain = GetMainSize(node);
                    double childMain = GetMainSize(child);
                    double dist = selfMain / 2 + GNode.SPACE + childMain / 2;

                    // 按生长方向计算下一层位置
                    double nextMain = mainPos + (isVertical ? direct.V : direct.H) * dist;

                    PlaceTree(child, childCenter, nextMain);
                    startChildCross += childCrossW + GNode.SPACE;
                }
            }

            foreach (var root in roots)
            {
                if (root.VisioShape == null) continue;
                double rootSize = subtreeCrossSizes.GetValueOrDefault(root, GetCrossSize(root));
                PlaceTree(root, currentRootCross + rootSize / 2, 0);
                currentRootCross += rootSize + GNode.SPACE;
            }
        }

        void DrawEdges(Graph graph)
        {
            List<GEdge> drawnEdges = new List<GEdge>();
            foreach (INode node in graph.NodeDict.Values)
            {
                if (node.VisioShape == null) continue;
                foreach (GEdge edge in node.OutputEdges)
                {
                    if (drawnEdges.Contains(edge) || edge.To.VisioShape == null) continue;

                    Shape shape = CreateEdge(edge);
                    node.VisioShape.AutoConnect(edge.To.VisioShape, VisAutoConnectDir.visAutoConnectDirNone, shape);
                    shape.Delete();
                    drawnEdges.Add(edge);
                    PauseForViewing(100); // 每条边绘制后暂停
                }
            }
        }

        GNode DrawSubgraphBorder(GSubgraph subGraph)
        {
            GNode borderNode = DropSubgraphBorder(subGraph);
            drawnList.AddLast(borderNode);
            drawnSet.Add(borderNode);

            return borderNode;
        }

        VBoundary Relocate(GSubgraph subgraph)
        {
            return Relocate(subgraph.AllGroupedNodes, subgraph.Container.GrowthDirect);
        }

        VBoundary Relocate(List<GNode> nodes, GrowthDirection direct)
        {
            VBoundary nodesBound = NodesBoundary(nodes);
            VBoundary relativeBound = NodesBoundary(drawnList.Except(nodes).ToList());
            if (nodes.Count == 0 || relativeBound.Height == 0) return nodesBound;

            VBoundary newBound = new(true);
            bool drawAtTail = IsDrawAtTail(nodes, direct);
            if(direct.H != 0)
            {
                double moveH = 0, moveV = relativeBound.PinY-nodesBound.PinY;
                if (drawAtTail) moveH = relativeBound.Right + GNode.SPACE - nodesBound.Left;
                else moveH = relativeBound.Left - GNode.SPACE - nodesBound.Right;

                foreach (GNode node in nodes) 
                {
                    if (node.VisioShape == null) continue;
                    MoveTo(node.VisioShape, PinX(node.VisioShape)+moveH, PinY(node.VisioShape)+moveV);  
                    newBound.Expand(node.VisioShape);
                }                
            }
            if(direct.V != 0)
            {
                double moveV = 0, moveH = relativeBound.PinX - nodesBound.PinX;
                if (drawAtTail) moveV = relativeBound.Top + GNode.SPACE - nodesBound.Bottom;
                else moveV = relativeBound.Bottom - GNode.SPACE - nodesBound.Top;

                foreach (GNode node in nodes)
                {
                    if (node.VisioShape == null) continue;
                    MoveTo(node.VisioShape, PinX(node.VisioShape)+moveH, PinY(node.VisioShape)+moveV);
                    newBound.Expand(node.VisioShape);
                }
            }

            return newBound;
        }

        bool IsDrawAtTail(List<GNode> nodes, GrowthDirection direct)
        {
            int nOut = 0, nIn = 0;
            foreach (GNode from in nodes)
            {
                foreach (GNode to in drawnList)
                {
                    if(from.OutputNodes().Contains(to)) nOut++;
                    if(from.InputNodes().Contains(to)) nIn++;
                }
            }

            if (nOut == nIn) return direct.Positive;
            else return nIn > nOut;
        }

        VBoundary NodesBoundary(List<GNode> nodes)
        {
            VBoundary boundary = new(true);
            foreach (var node in nodes)
            {
                if (node.VisioShape != null)
                    boundary.Expand(node.VisioShape);
            }

            return boundary;
        }

        public static double ShapeSheetIU(GNode node, string propName)
        {
            if (node.VisioShape == null) return 0;
            return ShapeSheetIU(node.VisioShape, propName);
        }

        public GNode DropSubgraphBorder(GSubgraph gSubgraph)
        {
            if (gSubgraph.Parent == null) throw new SynException("expected parent of subgraph");

            GNode node = gSubgraph.BorderNode; 
            VBoundary bnd = SubgraphBoundary(gSubgraph);
            Shape shape = CreateShape(node);
            shape.CellsU["Width"].FormulaU = (bnd.Width + GNode.SPACE * 2).ToString();
            shape.CellsU["Height"].FormulaU = (bnd.Height + GNode.SPACE * 2).ToString();
            shape.CellsU["PinX"].FormulaU = bnd.PinX.ToString();
            shape.CellsU["PinY"].FormulaU = bnd.PinY.ToString();
            shape.CellsU["FillPattern"].FormulaU = "0";
            shape.CellsU["VerticalAlign"].FormulaU = "0";
            shape.Text = gSubgraph.Label;
            SetFillForegnd(shape, "config.themeVariables.secondaryColor");
            SetLineColor(shape, "config.themeVariables.secondaryBorderColor");
            SetTextColor(shape, "config.themeVariables.secondaryTextColor");
            shape.CellsU["LineWeight"].FormulaU = "0.75 pt";
            visioApp.DoCmd((short)VisUICmds.visCmdObjectSendToBack);
            gSubgraph.VisioShape = shape;

            return node;
        }

        VBoundary SubgraphBoundary(GSubgraph gSubgraph)
        {
            VBoundary boundary = new VBoundary(true);
            foreach (INode node in gSubgraph.AlignGroupedNodes()) 
            {
                if (node.Container == gSubgraph && node.VisioShape != null) 
                    boundary.Expand(node.VisioShape);
            }
            if (gSubgraph.VisioShape != null) 
                boundary.Expand(gSubgraph.VisioShape);

            foreach (GSubgraph sub in gSubgraph.Subgraphs)
            {
                VBoundary subBoundary = SubgraphBoundary(sub);
                boundary.Expand(subBoundary);
            }

            return boundary;
        }

        public Shape CreateShape(GNode node)
        {
            Shape shape = visioPage.Drop(GetMaster($"{node.ShapeStart}{node.ShapeClose}"), 0, 0);
            shape.Text = node.Label;
            AdjustSize(shape, new System.Drawing.SizeF(0, 0), GraphMinTextRate);
            node.VisioShape = shape; 
            return shape;
        }

        void PostProcessShape(GNode node, Shape shape)
        {
            switch (node.NodeShape.Shape)
            {
                case "text":
                    shape.CellsU["LinePattern"].FormulaU = "=0";
                    shape.CellsU["FillPattern"].FormulaU = "=0";
                    break;
                case "h-cyl":
                    Rotate(shape, Math.PI / 2);
                    shape.CellsU["TxtAngle"].FormulaU = "0 rad";
                    break;
            }
        }

        public Shape CreateEdge(GEdge edge)
        {
            Master? master = GetMaster("-");
            Shape shape = visioPage.Drop(master, 0, 0);
            SetupEdgeShape(edge, shape);

            return shape;
        }

        void SetupEdgeShape(GEdge edge, Shape shape)
        {
            shape.Text = edge.Text;
            // line type
            switch (edge.LineType)
            {
                case "-": shape.CellsU["LineWeight"].FormulaU = "=0.75 pt"; break;
                case "=": shape.CellsU["LineWeight"].FormulaU = "=0.75 pt"; break;
                case ".": shape.CellsU["LinePattern"].FormulaU = "=2"; break;
                case "~": shape.CellsU["LinePattern"].FormulaU = "=0"; break;
                default: shape.CellsU["LineWeight"].FormulaU = "=0.75 pt"; break;
            }

            // start tag
            // x/o/-/<
            switch (edge.StartTag)
            {
                case "x": shape.CellsU["BeginArrow"].FormulaU = "=24"; break;
                case "o": shape.CellsU["BeginArrow"].FormulaU = "=10"; break;
                case "-": shape.CellsU["BeginArrow"].FormulaU = "=0"; break;
                case "<": shape.CellsU["BeginArrow"].FormulaU = "=4"; break;
                default: shape.CellsU["BeginArrow"].FormulaU = "=0"; break;
            }

            // end tag
            // x/o/-/>
            switch (edge.EndTag)
            {
                case "x": shape.CellsU["EndArrow"].FormulaU = "=24"; break;
                case "o": shape.CellsU["EndArrow"].FormulaU = "=10"; break;
                case "-": shape.CellsU["EndArrow"].FormulaU = "=0"; break;
                case ">": shape.CellsU["EndArrow"].FormulaU = "=4"; break;
                default: shape.CellsU["EndArrow"].FormulaU = "=0"; break;
            }
            
            // 设置连接线颜色
            SetLineColor(shape, "config.themeVariables.defaultLinkColor");
        }


    }
}
