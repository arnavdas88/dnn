namespace Genix.MachineLearning.Test
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;

    [TestClass]
    public class KernelTest
    {
        [TestMethod]
        public void PrivateConstructorTest0()
        {
            Kernel kernel = Helpers.CreateClassWithPrivateConstructor<Kernel>();

            Assert.AreEqual(0, kernel.Width);
            Assert.AreEqual(0, kernel.Height);
            Assert.AreEqual(0, kernel.StrideX);
            Assert.AreEqual(0, kernel.StrideY);
            Assert.AreEqual(0, kernel.PaddingX);
            Assert.AreEqual(0, kernel.PaddingY);
        }

        [TestMethod]
        public void ConstructorTest1()
        {
            Kernel kernel = new Kernel(1, 2, 3, 4, 5, 6);

            Assert.AreEqual(1, kernel.Width);
            Assert.AreEqual(2, kernel.Height);
            Assert.AreEqual(3, kernel.StrideX);
            Assert.AreEqual(4, kernel.StrideY);
            Assert.AreEqual(5, kernel.PaddingX);
            Assert.AreEqual(6, kernel.PaddingY);
        }

        [TestMethod]
        public void ConstructorTest2()
        {
            Kernel kernel = new Kernel(1, 2, 3, 4);

            Assert.AreEqual(1, kernel.Width);
            Assert.AreEqual(2, kernel.Height);
            Assert.AreEqual(3, kernel.StrideX);
            Assert.AreEqual(4, kernel.StrideY);
            Assert.AreEqual(0, kernel.PaddingX);
            Assert.AreEqual(0, kernel.PaddingY);
        }

        [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly", Justification = "Uses argument of the method being tested.")]
        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ConstructorTest3()
        {
            try
            {
                Assert.IsNotNull(new Kernel(0, 2, 3, 4));
            }
            catch (ArgumentOutOfRangeException e)
            {
                Assert.AreEqual(new ArgumentOutOfRangeException("width", 0, Properties.Resources.E_InvalidKernelSize).Message, e.Message);
                throw;
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly", Justification = "Uses argument of the method being tested.")]
        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ConstructorTest4()
        {
            try
            {
                Assert.IsNotNull(new Kernel(1, 0, 3, 4));
            }
            catch (ArgumentOutOfRangeException e)
            {
                Assert.AreEqual(new ArgumentOutOfRangeException("height", 0, Properties.Resources.E_InvalidKernelSize).Message, e.Message);
                throw;
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly", Justification = "Uses argument of the method being tested.")]
        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ConstructorTest5()
        {
            try
            {
                Assert.IsNotNull(new Kernel(1, 2, 0, 4));
            }
            catch (ArgumentOutOfRangeException e)
            {
                Assert.AreEqual(new ArgumentOutOfRangeException("strideX", 0, Properties.Resources.E_InvalidStride).Message, e.Message);
                throw;
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly", Justification = "Uses argument of the method being tested.")]
        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ConstructorTest6()
        {
            try
            {
                Assert.IsNotNull(new Kernel(1, 2, 3, 0));
            }
            catch (ArgumentOutOfRangeException e)
            {
                Assert.AreEqual(new ArgumentOutOfRangeException("strideY", 0, Properties.Resources.E_InvalidStride).Message, e.Message);
                throw;
            }
        }

        [TestMethod]
        public void CopyConstructorTest0()
        {
            Kernel kernel = new Kernel(new Kernel(1, 2, 3, 4, 5, 6));

            Assert.AreEqual(1, kernel.Width);
            Assert.AreEqual(2, kernel.Height);
            Assert.AreEqual(3, kernel.StrideX);
            Assert.AreEqual(4, kernel.StrideY);
            Assert.AreEqual(5, kernel.PaddingX);
            Assert.AreEqual(6, kernel.PaddingY);
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void CopyConstructorTest1()
        {
            Assert.IsNotNull(new Kernel(null));
        }

        [TestMethod]
        public void EqualsTest()
        {
            Kernel kernel = new Kernel(1, 2, 3, 4, 5, 6);

            Assert.IsFalse(kernel.Equals(null));
            Assert.IsTrue(kernel.Equals(kernel));
            Assert.IsFalse(kernel.Equals(new { x = 1 }));
            Assert.IsFalse(kernel.Equals(new Kernel(10, 2, 3, 4, 5, 6)));
            Assert.IsFalse(kernel.Equals(new Kernel(1, 20, 3, 4, 5, 6)));
            Assert.IsFalse(kernel.Equals(new Kernel(1, 2, 30, 4, 5, 6)));
            Assert.IsFalse(kernel.Equals(new Kernel(1, 2, 3, 40, 5, 6)));
            Assert.IsFalse(kernel.Equals(new Kernel(1, 2, 3, 4, 50, 6)));
            Assert.IsFalse(kernel.Equals(new Kernel(1, 2, 3, 4, 5, 60)));
        }

        [TestMethod]
        public void GetHashCodeTest()
        {
            Kernel kernel = new Kernel(1, 2, 3, 4, 5, 6);
            Assert.AreEqual(1 ^ 2 ^ 3 ^ 4 ^ 5 ^ 6, kernel.GetHashCode());
        }

        [TestMethod]
        public void ToStringTest()
        {
            Assert.AreEqual("2", new Kernel(2, 2, 1, 1).ToString());
            Assert.AreEqual("2x3", new Kernel(2, 3, 1, 1).ToString());

            Assert.AreEqual("2+2(S)", new Kernel(2, 2, 2, 2).ToString());
            Assert.AreEqual("2x3+2x3(S)", new Kernel(2, 3, 2, 3).ToString());

            Assert.AreEqual("2+2(S)+1(P)", new Kernel(2, 2, 2, 2, 1, 1).ToString());
            Assert.AreEqual("2x3+2x3(S)+1x2(P)", new Kernel(2, 3, 2, 3, 1, 2).ToString());
        }

        [TestMethod]
        public void CloneTest()
        {
            Kernel kernel1 = new Kernel(1, 2, 3, 4, 5, 6);
            Kernel kernel2 = kernel1.Clone() as Kernel;
            Assert.AreEqual(JsonConvert.SerializeObject(kernel1), JsonConvert.SerializeObject(kernel2));
        }

        [TestMethod]
        public void SerializeTest()
        {
            Kernel kernel1 = new Kernel(1, 2, 3, 4, 5, 6);
            string s1 = JsonConvert.SerializeObject(kernel1);
            Kernel kernel2 = JsonConvert.DeserializeObject<Kernel>(s1);
            string s2 = JsonConvert.SerializeObject(kernel2);
            Assert.AreEqual(s1, s2);
        }

        [TestMethod]
        public void CalculateOutputTest()
        {
            Kernel kernel = new Kernel(1, 2, 3, 4, 5, 6);
            Assert.AreEqual(38, kernel.CalculateOutputWidth(100));
            Assert.AreEqual(29, kernel.CalculateOutputHeight(100));
            Assert.AreEqual(-1, kernel.CalculateOutputWidth(-1));
            Assert.AreEqual(-1, kernel.CalculateOutputHeight(-1));
        }
    }
}
