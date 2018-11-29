namespace Genix.DNN.Test
{
    using Genix.Core;
    using Genix.DNN.Layers;
    using Genix.Graph;
    using Genix.MachineLearning;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class GraphTest
    {
        [TestMethod]
        public void SerializeTest1()
        {
            NetworkGraph graph = new NetworkGraph();

            Layer layer1 = new FullyConnectedLayer(new Shape(new[] { 1, 2, 3, 1 }), 5, MatrixLayout.ColumnMajor, null);
            Layer layer2 = new FullyConnectedLayer(new Shape(new[] { 2, 3, 4, 1 }), 6, MatrixLayout.ColumnMajor, null);

            Edge<Layer> edge1 = new Edge<Layer>(layer1, layer2);
            Edge<Layer> edge2 = new Edge<Layer>(layer1, layer2);

            graph.AddEdges(new Edge<Layer>[] { edge1, edge2 });

            string s1 = graph.SaveToString();

            NetworkGraph graph2 = NetworkGraph.FromString(s1);

            string s2 = graph2.SaveToString();

            Assert.AreEqual(s1, s2);
        }
    }
}
