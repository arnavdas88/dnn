namespace Genix.MachineLearning.Imaging.Test
{
    using System;
    using Genix.Imaging;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ImageExtensionsTest
    {
        [TestMethod]
        public void ToImageTest_Rank2()
        {
            Tensor tensor = new Tensor(null, new int[] { 53, 67 });
            tensor.Randomize();

            Image image = tensor.ToImage();
            Assert.AreEqual(53, image.Width);
            Assert.AreEqual(67, image.Height);
            Assert.AreEqual(8, image.BitsPerPixel);
            Assert.AreEqual(200, image.HorizontalResolution);
            Assert.AreEqual(200, image.VerticalResolution);

            tensor.MinMax(out float min, out float max);
            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    uint expected = 255u - (uint)((tensor[x, y] - min) / (max - min) * 255).Round().Clip(0, 255);
                    Assert.AreEqual(expected, image.GetPixel(x, y));
                }
            }
        }

        [TestMethod]
        public void ToImageTest_1channel()
        {
            foreach (string format in new string[] { Shape.BWHC, Shape.BHWC, Shape.BCHW })
            {
                Shape shape = new Shape(format, 1, 53, 67, 1);
                Tensor tensor = new Tensor(null, shape);
                tensor.Randomize();

                Image image = tensor.ToImage();
                Assert.AreEqual(53, shape.GetAxis(Axis.X));
                Assert.AreEqual(67, shape.GetAxis(Axis.Y));
                Assert.AreEqual(8, image.BitsPerPixel);
                Assert.AreEqual(200, image.HorizontalResolution);
                Assert.AreEqual(200, image.VerticalResolution);

                tensor.MinMax(out float min, out float max);
                for (int x = 0; x < image.Width; x++)
                {
                    for (int y = 0; y < image.Height; y++)
                    {
                        uint expected = 255u - (uint)((tensor[shape.Position(0, x, y, 0)] - min) / (max - min) * 255).Round().Clip(0, 255);
                        Assert.AreEqual(expected, image.GetPixel(x, y));
                    }
                }
            }
        }

        [TestMethod]
        public void ToImageTest_3channels()
        {
            foreach (string format in new string[] { Shape.BWHC, Shape.BHWC, Shape.BCHW })
            {
                Shape shape = new Shape(format, 1, 53, 67, 3);
                Tensor tensor = new Tensor(null, shape);
                tensor.Randomize();

                Image image = tensor.ToImage();
                Assert.AreEqual(53, shape.GetAxis(Axis.X));
                Assert.AreEqual(67, shape.GetAxis(Axis.Y));
                Assert.AreEqual(24, image.BitsPerPixel);
                Assert.AreEqual(200, image.HorizontalResolution);
                Assert.AreEqual(200, image.VerticalResolution);

                tensor.MinMax(out float min, out float max);
                for (int x = 0; x < image.Width; x++)
                {
                    for (int y = 0; y < image.Height; y++)
                    {
                        uint expected1 = 255u - (uint)((tensor[shape.Position(0, x, y, 0)] - min) / (max - min) * 255).Round().Clip(0, 255);
                        uint expected2 = 255u - (uint)((tensor[shape.Position(0, x, y, 1)] - min) / (max - min) * 255).Round().Clip(0, 255);
                        uint expected3 = 255u - (uint)((tensor[shape.Position(0, x, y, 2)] - min) / (max - min) * 255).Round().Clip(0, 255);
                        Assert.AreEqual(expected1 | (expected2 << 8) | (expected3 << 16), image.GetPixel(x, y));
                    }
                }
            }
        }

        [TestMethod]
        public void FromImageTest_1bpp()
        {
            foreach (string format in new string[] { Shape.BWHC, Shape.BHWC, Shape.BCHW })
            {
                Image image = new Image(53, 67, 1, 200, 200);
                image.Randomize();

                Tensor tensor = ImageExtensions.FromImage(image, null, format, image.Width, image.Height);

                Assert.AreEqual(format, tensor.Shape.Format);
                Assert.AreEqual(1, tensor.Shape.GetAxis(Axis.B));
                Assert.AreEqual(53, tensor.Shape.GetAxis(Axis.X));
                Assert.AreEqual(53, tensor.Shape.GetAxis(Axis.X));
                Assert.AreEqual(1, tensor.Shape.GetAxis(Axis.C));

                for (int x = 0; x < image.Width; x++)
                {
                    for (int y = 0; y < image.Height; y++)
                    {
                        float expected = image.GetPixel(x, y);
                        Assert.AreEqual(expected, tensor[tensor.Shape.Position(0, x, y, 0)], 1e-6f);
                    }
                }
            }
        }

        [TestMethod]
        public void FromImageTest_2to16bpp()
        {
            foreach (int bpp in new int[] { 2, 4, 8, 16 })
            {
                foreach (string format in new string[] { Shape.BWHC, Shape.BHWC, Shape.BCHW })
                {
                    Image image = new Image(53, 67, 8, 200, 200);
                    image.Randomize();

                    Tensor tensor = ImageExtensions.FromImage(image, null, format, image.Width, image.Height);

                    Assert.AreEqual(format, tensor.Shape.Format);
                    Assert.AreEqual(1, tensor.Shape.GetAxis(Axis.B));
                    Assert.AreEqual(53, tensor.Shape.GetAxis(Axis.X));
                    Assert.AreEqual(53, tensor.Shape.GetAxis(Axis.X));
                    Assert.AreEqual(1, tensor.Shape.GetAxis(Axis.C));

                    for (int x = 0; x < image.Width; x++)
                    {
                        for (int y = 0; y < image.Height; y++)
                        {
                            float expected = 1.0f - ((float)image.GetPixel(x, y) / image.MaxColor);
                            Assert.AreEqual(expected, tensor[tensor.Shape.Position(0, x, y, 0)], 1e-6f);
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void FromImageTest_24to32bpp()
        {
            foreach (int bpp in new int[] { 24, 32 })
            {
                foreach (string format in new string[] { Shape.BWHC, Shape.BHWC, Shape.BCHW })
                {
                    Image image = new Image(53, 67, bpp, 200, 200);
                    image.Randomize();

                    Tensor tensor = ImageExtensions.FromImage(image, null, format, image.Width, image.Height);

                    Assert.AreEqual(format, tensor.Shape.Format);
                    Assert.AreEqual(1, tensor.Shape.GetAxis(Axis.B));
                    Assert.AreEqual(53, tensor.Shape.GetAxis(Axis.X));
                    Assert.AreEqual(53, tensor.Shape.GetAxis(Axis.X));
                    Assert.AreEqual(3, tensor.Shape.GetAxis(Axis.C));

                    for (int x = 0; x < image.Width; x++)
                    {
                        for (int y = 0; y < image.Height; y++)
                        {
                            Color color = Color.FromArgb(image.GetPixel(x, y));

                            float expected1 = (255.0f - color.B) / 255.0f;
                            Assert.AreEqual(expected1, tensor[tensor.Shape.Position(0, x, y, 0)], 1e-6f);
                            float expected2 = (255.0f - color.G) / 255.0f;
                            Assert.AreEqual(expected2, tensor[tensor.Shape.Position(0, x, y, 1)], 1e-6f);
                            float expected3 = (255.0f - color.R) / 255.0f;
                            Assert.AreEqual(expected3, tensor[tensor.Shape.Position(0, x, y, 2)], 1e-6f);
                        }
                    }
                }
            }
        }
    }
}
