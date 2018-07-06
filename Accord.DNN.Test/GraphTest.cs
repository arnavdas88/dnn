namespace Accord.DNN.Test
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Layers;

    [TestClass]
    public class GraphTest
    {
        [TestMethod]
        public void SerializeTest1()
        {
            NetworkGraph graph = new NetworkGraph();

            Layer layer1 = new FullyConnectedLayer(new[] { 1, 2, 3, 1 }, 5, MatrixLayout.ColumnMajor, null);
            Layer layer2 = new FullyConnectedLayer(new[] { 2, 3, 4, 1 }, 6, MatrixLayout.ColumnMajor, null);

            NetworkEdge edge1 = new NetworkEdge(layer1, layer2);
            NetworkEdge edge2 = new NetworkEdge(layer1, layer2);

            graph.AddEdges(new NetworkEdge[] { edge1, edge2 });

            string s1 = graph.SaveToString();

            NetworkGraph graph2 = NetworkGraph.FromString(s1);

            string s2 = graph2.SaveToString();

            Assert.AreEqual(s1, s2);
        }
    }
}
