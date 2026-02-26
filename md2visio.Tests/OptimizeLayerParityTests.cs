using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using md2visio.vsdx;
using md2visio.struc.classdiag;
using System.Reflection;
using System.Runtime.Serialization;

namespace md2visio.Tests
{
    public class OptimizeLayerParityTests
    {
        private VDrawerCls CreateDrawer()
        {
            return (VDrawerCls)FormatterServices.GetUninitializedObject(typeof(VDrawerCls));
        }

        [Fact]
        public void OptimizeLayerOrdering_ReordersLayersCorrectly()
        {
            var drawer = CreateDrawer();

            var clsA = new ClsClass { ID = "A" };
            var clsB = new ClsClass { ID = "B" };
            var clsC = new ClsClass { ID = "C" };
            var clsD = new ClsClass { ID = "D" };

            // Layer 0: A, B
            // Layer 1: C, D
            var layers = new Dictionary<int, List<ClsClass>>
            {
                { 0, new List<ClsClass> { clsA, clsB } },
                { 1, new List<ClsClass> { clsC, clsD } }
            };

            // Edges: A->D, B->C
            var graph = new Dictionary<string, List<VDrawerCls.WeightedEdge>>
            {
                ["A"] = new List<VDrawerCls.WeightedEdge> { new VDrawerCls.WeightedEdge { FromClass = "A", ToClass = "D" } },
                ["B"] = new List<VDrawerCls.WeightedEdge> { new VDrawerCls.WeightedEdge { FromClass = "B", ToClass = "C" } }
            };

            // Act
            drawer.OptimizeLayerOrdering(layers, graph);

            // Assert
            // After downward sweep:
            // Layer 1 nodes barycenters:
            // C: connected to B (pos 1) -> barycenter 1
            // D: connected to A (pos 0) -> barycenter 0
            // Sorted: D (0), C (1)
            Assert.Equal("D", layers[1][0].ID);
            Assert.Equal("C", layers[1][1].ID);

            // Layer 0 should also be checked after upward sweep
            // C is at pos 1, D is at pos 0
            // A connected to D (pos 0) -> barycenter 0
            // B connected to C (pos 1) -> barycenter 1
            // Sorted: A (0), B (1)
            Assert.Equal("A", layers[0][0].ID);
            Assert.Equal("B", layers[0][1].ID);
        }
    }
}
