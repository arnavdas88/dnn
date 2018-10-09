namespace Genix.Imaging.Test
{
    using System.Globalization;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class MorphologyTest
    {
        [TestMethod, TestCategory("Image")]
        public void DilateTest1()
        {
            const int Width = 150;
            const int Height = 20;
            for (int ix = 0; ix < Width; ix++)
            {
                for (int iy = 0; iy < Height; iy++)
                {
                    Image image = new Image(Width, Height, 1, 200, 200);
                    image.SetPixel(ix, iy, 1);

                    Image dilatedImage = image.Dilate(null, StructuringElement.Square(3), 1, BorderType.BorderConst, image.WhiteColor);

                    if ((ix == 0 || ix == Width - 1) && (iy == 0 || iy == Height - 1))
                    {
                        Assert.AreEqual(
                            4ul,
                            dilatedImage.Power(),
                            string.Format(CultureInfo.InvariantCulture, "{0} {1}", ix, iy));
                    }
                    else if ((ix == 0 || ix == Width - 1) || (iy == 0 || iy == Height - 1))
                    {
                        Assert.AreEqual(
                            6ul,
                            dilatedImage.Power(),
                            string.Format(CultureInfo.InvariantCulture, "{0} {1}", ix, iy));
                    }
                    else
                    {
                        Assert.AreEqual(
                            9ul,
                            dilatedImage.Power(),
                            string.Format(CultureInfo.InvariantCulture, "{0} {1}", ix, iy));
                    }
                }
            }
        }

        [TestMethod, TestCategory("Image")]
        public void ErodeTest1()
        {
            const int Width = 150;
            const int Height = 20;
            for (int ix = 0; ix < Width; ix++)
            {
                for (int iy = 0; iy < Height; iy++)
                {
                    Image image = new Image(Width, Height, 1, 200, 200);
                    image.SetBlack();
                    image.SetPixel(ix, iy, 0);

                    Image dilatedImage = image.Erode(null, StructuringElement.Square(3), 1, BorderType.BorderConst, uint.MaxValue);

                    if ((ix == 0 || ix == Width - 1) && (iy == 0 || iy == Height - 1))
                    {
                        Assert.AreEqual(
                            (ulong)((Width * Height) - 4),
                            dilatedImage.Power(),
                            string.Format(CultureInfo.InvariantCulture, "{0} {1}", ix, iy));
                    }
                    else if ((ix == 0 || ix == Width - 1) || (iy == 0 || iy == Height - 1))
                    {
                        Assert.AreEqual(
                            (ulong)((Width * Height) - 6),
                            dilatedImage.Power(),
                            string.Format(CultureInfo.InvariantCulture, "{0} {1}", ix, iy));
                    }
                    else
                    {
                        Assert.AreEqual(
                            (ulong)((Width * Height) - 9),
                            dilatedImage.Power(),
                            string.Format(CultureInfo.InvariantCulture, "{0} {1}", ix, iy));
                    }
                }
            }
        }
    }
}
