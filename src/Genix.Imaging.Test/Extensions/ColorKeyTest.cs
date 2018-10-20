namespace Genix.Imaging.Test.Extensions
{
    using System;
    using Genix.Core;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ColorKeyTest
    {
        private readonly UlongRandomGenerator random = new UlongRandomGenerator();

        [TestMethod]
        public void ColorKeyMethodTest()
        {
            foreach (int bitsPerPixel in new[] { 8, 24, 32 })
            {
                Image src1 = new Image(231, 50, bitsPerPixel, 200, 200);
                Image src2 = new Image(231, 50, bitsPerPixel, 200, 200);

                foreach (uint color in new uint[] { 0, src1.MaxColor, src1.MaxColor / 2 })
                {
                    src1.Randomize(this.random);
                    src2.Randomize(this.random);

                    // ensure some pixels have the key color
                    for (int i = 0; i < 20; i++)
                    {
                        ulong pos = this.random.Generate();
                        src1.SetPixel((int)((pos >> 32) & 0x7fff_fffful) % src1.Width, (int)(pos & 0x7fff_fffful) % src1.Height, color);
                    }

                    Image dst = src1.ColorKey(null, src2, color);

                    for (int x = 0; x < src1.Width; x++)
                    {
                        for (int y = 0; y < src1.Height; y++)
                        {
                            uint src1color = src1.GetPixel(x, y);
                            uint src2color = src2.GetPixel(x, y);
                            uint dstcolor = dst.GetPixel(x, y);
                            Assert.AreEqual(src1color == color ? src2color : src1color, dstcolor);
                        }
                    }
                }
            }
        }
    }
}
