namespace Genix.MachineLearning.Test
{
    using System;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;

    [TestClass]
    public class TensorTest
    {
        private const float Eps = 1e-6f;
        ////private readonly RandomNumberGenerator<float> random = new RandomGenerator();

        [TestMethod]
        public void ConstructorTest1()
        {
            int[] axes = new[] { 2, 3, 4, 5 };
            Tensor tensor = new Tensor("test", axes);

            Assert.AreEqual("test", tensor.Name);
            CollectionAssert.AreEqual(new[] { 2, 3, 4, 5 }, tensor.Axes);
            CollectionAssert.AreEqual(new[] { 60, 20, 5, 1 }, tensor.Strides);
            Assert.AreEqual(2 * 3 * 4 * 5, tensor.Length);
            Assert.AreEqual(4, tensor.Rank);

            Assert.IsTrue(tensor.Weights.All(x => x == 0.0f));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CopyConstructorTest1()
        {
            Assert.IsNotNull(new Tensor((Tensor)null));
        }

        [TestMethod]
        public void CopyConstructorTest2()
        {
            Tensor tensor1 = new Tensor("test", new[] { 2, 10, 20, 5 });
            tensor1.Randomize();
            Tensor tensor2 = new Tensor(tensor1);

            Assert.AreEqual(tensor1.Name, tensor2.Name);
            Helpers.AreTensorsEqual(tensor1, tensor2);
        }

        [TestMethod]
        public void SerializeTest()
        {
            Tensor tensor1 = new Tensor(null, new[] { 1, 2, 3, 4 });
            tensor1.Randomize();
            string s1 = JsonConvert.SerializeObject(tensor1);
            Tensor tensor2 = JsonConvert.DeserializeObject<Tensor>(s1);
            string s2 = JsonConvert.SerializeObject(tensor2);
            Assert.AreEqual(s1, s2);
        }

        [TestMethod]
        public void PositionTest1()
        {
            Tensor tensor = new Tensor(null, new[] { 2, 3, 4, 5 });
            Assert.AreEqual((1 * 5) + (2 * 1), tensor.Shape.Position(new int[] { 0, 0, 1, 2 }));
            Assert.AreEqual((1 * 4 * 5) + (2 * 5) + (3 * 1), tensor.Shape.Position(new int[] { 0, 1, 2, 3 }));
            Assert.AreEqual((1 * 3 * 4 * 5) + (1 * 4 * 5) + (2 * 5) + (3 * 1), tensor.Shape.Position(new int[] { 1, 1, 2, 3 }));
        }

        [TestMethod]
        public void OnesTest1()
        {
            Shape shape = new Shape(new[] { 2, 10, 20, 5 });
            Tensor tensor = Tensor.Ones(null, shape);

            CollectionAssert.AreEqual(shape.Axes, tensor.Axes);
            Assert.IsTrue(tensor.Weights.All(x => x == 1.0f));
        }
    }
}
