namespace Genix.DNN.Test
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Genix.DNN.Layers;

    [TestClass]
    public class LayerTest
    {
        [TestMethod]
        public void FromArchitectureReLUTest1()
        {
            const string Architecture = "RELU";
            Layer layer = NetworkGraphBuilder.CreateLayer(new[] { 1, 10, 10, 10 }, Architecture, null);

            Assert.IsInstanceOfType(layer, typeof(ReLULayer));
            Assert.AreEqual(Architecture, layer.Architecture);
        }

        [TestMethod]
        public void FromArchitectureTanhTest1()
        {
            string Architecture = "TH";
            Layer layer = NetworkGraphBuilder.CreateLayer(new[] { 1, 10, 10, 10 }, Architecture, null);

            Assert.IsInstanceOfType(layer, typeof(TanhLayer));
            Assert.AreEqual(Architecture, layer.Architecture);
        }

        [TestMethod]
        public void FromArchitectureSigmoidTest1()
        {
            const string Architecture = "SIG";
            Layer layer = NetworkGraphBuilder.CreateLayer(new[] { 1, 10, 10, 10 }, Architecture, null);

            Assert.IsInstanceOfType(layer, typeof(SigmoidLayer));
            Assert.AreEqual(Architecture, layer.Architecture);
        }

        [TestMethod]
        public void FromArchitectureMaxOutTest1()
        {
            const string Architecture = "MO2";
            Layer layer = NetworkGraphBuilder.CreateLayer(new[] { 1, 10, 10, 10 }, Architecture, null);

            Assert.IsInstanceOfType(layer, typeof(MaxOutLayer));
            Assert.AreEqual(2, ((MaxOutLayer)layer).GroupSize);
            Assert.AreEqual(Architecture, layer.Architecture);
        }

        [TestMethod]
        public void FromArchitectureSoftMaxTest1()
        {
            const string Architecture = "SM";
            Layer layer = NetworkGraphBuilder.CreateLayer(new[] { 1, 10, 10, 10 }, Architecture, null);

            Assert.IsInstanceOfType(layer, typeof(SoftMaxLayer));
            Assert.AreEqual(1000, ((SoftMaxLayer)layer).NumberOfClasses);
            Assert.AreEqual(Architecture, layer.Architecture);
        }

        [TestMethod]
        public void FromArchitectureDropoutTest1()
        {
            const string Architecture = "D0.5";
            Layer layer = NetworkGraphBuilder.CreateLayer(new[] { 1, 10, 10, 10 }, Architecture, null);

            Assert.IsInstanceOfType(layer, typeof(DropoutLayer));
            Assert.AreEqual(0.5f, ((DropoutLayer)layer).Probability);
            Assert.AreEqual(Architecture, layer.Architecture);
        }

        [TestMethod]
        public void FromArchitectureFullyConnectedTest1()
        {
            const string Architecture = "100N";
            Layer layer = NetworkGraphBuilder.CreateLayer(new[] { 1, 10, 10, 10 }, Architecture, null);

            Assert.IsInstanceOfType(layer, typeof(FullyConnectedLayer));
            Assert.AreEqual(100, ((FullyConnectedLayer)layer).NumberOfNeurons);
            Assert.AreEqual(Architecture, layer.Architecture);
        }

        [TestMethod]
        public void FromArchitectureConvolutionTest1()
        {
            const string Architecture = "40C2";
            Layer layer = NetworkGraphBuilder.CreateLayer(new[] { 1, 10, 10, 10 }, Architecture, null);

            Assert.IsInstanceOfType(layer, typeof(ConvolutionLayer));

            ConvolutionLayer convolutionLayer = layer as ConvolutionLayer;
            Assert.AreEqual(2, convolutionLayer.Kernel.Width);
            Assert.AreEqual(2, convolutionLayer.Kernel.Height);
            Assert.AreEqual(1, convolutionLayer.Kernel.StrideX);
            Assert.AreEqual(1, convolutionLayer.Kernel.StrideY);
            Assert.AreEqual(0, convolutionLayer.Kernel.PaddingX);
            Assert.AreEqual(0, convolutionLayer.Kernel.PaddingY);
            Assert.AreEqual(Architecture, layer.Architecture);
        }

        [TestMethod]
        public void FromArchitectureConvolutionTest2()
        {
            const string Architecture = "40C6x5+4x3(S)+2x1(P)";
            Layer layer = NetworkGraphBuilder.CreateLayer(new[] { 1, 10, 10, 10 }, Architecture, null);

            Assert.IsInstanceOfType(layer, typeof(ConvolutionLayer));

            ConvolutionLayer convolutionLayer = layer as ConvolutionLayer;
            Assert.AreEqual(6, convolutionLayer.Kernel.Width);
            Assert.AreEqual(5, convolutionLayer.Kernel.Height);
            Assert.AreEqual(4, convolutionLayer.Kernel.StrideX);
            Assert.AreEqual(3, convolutionLayer.Kernel.StrideY);
            Assert.AreEqual(2, convolutionLayer.Kernel.PaddingX);
            Assert.AreEqual(1, convolutionLayer.Kernel.PaddingY);
            Assert.AreEqual(Architecture, layer.Architecture);
        }

        [TestMethod]
        public void FromArchitectureInputTest2()
        {
            const string Architecture = "10x20x1";
            Layer layer = NetworkGraphBuilder.CreateLayer(new[] { 1, 10, 10, 10 }, Architecture, null);

            Assert.IsInstanceOfType(layer, typeof(InputLayer));
            CollectionAssert.AreEqual(new[] { -1, 10, 20, 1 }, (layer as InputLayer).Shape);
            Assert.AreEqual(Architecture, layer.Architecture);
        }
    }
}
