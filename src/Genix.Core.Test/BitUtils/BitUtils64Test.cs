namespace Genix.Core.Test
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class BitUtils64Test
    {
        [TestMethod]
        public void SetBitsTest()
        {
            const int Size = 3 * 64;
            ulong[] y = new ulong[3];

            for (int count = 1; count < Size; count++)
            {
                for (int pos = 0; pos < Size - count; pos++)
                {
                    y[0] = y[1] = y[2] = 0;
                    BitUtils64.SetBits(count, y, pos);

                    Assert.AreEqual(count, BitUtils64.CountOneBits(Size, y, 0));

                    Assert.AreEqual(pos, BitUtils64.BitScanOneForward(Size, y, 0));
                    Assert.AreEqual(pos + count - 1, BitUtils64.BitScanOneReverse(Size, y, Size - 1));

                    Assert.AreEqual(pos + count == Size ? -1 : pos + count, BitUtils64.BitScanZeroForward(Size - pos, y, pos));
                    Assert.AreEqual(pos == 0 ? -1 : pos - 1, BitUtils64.BitScanZeroReverse(pos + count, y, pos + count - 1));
                }
            }
        }

        [TestMethod]
        public void ResetBitsTest()
        {
            const int Size = 3 * 64;
            ulong[] y = new ulong[3];

            for (int count = 1; count < Size; count++)
            {
                for (int pos = 0; pos < Size - count; pos++)
                {
                    y[0] = y[1] = y[2] = ulong.MaxValue;
                    BitUtils64.ResetBits(count, y, pos);

                    Assert.AreEqual(Size - count, BitUtils64.CountOneBits(Size, y, 0));

                    Assert.AreEqual(pos, BitUtils64.BitScanZeroForward(Size, y, 0));
                    Assert.AreEqual(pos + count - 1, BitUtils64.BitScanZeroReverse(Size, y, Size - 1));

                    Assert.AreEqual(pos + count == Size ? -1 : pos + count, BitUtils64.BitScanOneForward(Size, y, pos));
                    Assert.AreEqual(pos == 0 ? -1 : pos - 1, BitUtils64.BitScanOneReverse(pos + count, y, pos + count - 1));
                }
            }
        }

        [TestMethod]
        public void BitsOrTest()
        {
            const int Size = 3 * 64;
            ulong[] x = new ulong[3];
            ulong[] y = new ulong[3];

            for (int count = 1; count < Size; count++)
            {
                for (int pos = 0; pos < count; pos++)
                {
                    // shift left
                    for (int posy = 0; posy < Size - count; posy++)
                    {
                        int posx = Size - count;
                        x[0] = x[1] = x[2] = 0;
                        BitUtils64.SetBits(1, x, posx + pos);

                        y[0] = y[1] = y[2] = 0;
                        BitUtils64.OR(count, x, posx, y, posy);

                        Assert.AreEqual(posy + pos, BitUtils64.BitScanOneForward(Size, y, 0));
                    }

                    // shift right
                    for (int posx = 0; posx < Size - count; posx++)
                    {
                        x[0] = x[1] = x[2] = 0;
                        BitUtils64.SetBits(1, x, posx + pos);

                        int posy = Size - count;
                        y[0] = y[1] = y[2] = 0;
                        BitUtils64.OR(count, x, posx, y, posy);

                        Assert.AreEqual(posy + pos, BitUtils64.BitScanOneForward(Size, y, 0));
                    }
                }
            }
        }

        [TestMethod]
        public void BitsAndTest()
        {
            const int Size = 3 * 64;
            ulong[] x = new ulong[3];
            ulong[] y = new ulong[3];

            for (int count = 1; count < Size; count++)
            {
                for (int pos = 0; pos < count; pos++)
                {
                    // shift left
                    for (int posy = 0; posy < Size - count; posy++)
                    {
                        int posx = Size - count;
                        x[0] = x[1] = x[2] = ulong.MaxValue;
                        BitUtils64.ResetBits(1, x, posx + pos);

                        y[0] = y[1] = y[2] = ulong.MaxValue;
                        BitUtils64.BitsAND(count, x, posx, y, posy);

                        Assert.AreEqual(posy + pos, BitUtils64.BitScanZeroForward(Size, y, 0));
                    }

                    // shift right
                    for (int posx = 0; posx < Size - count; posx++)
                    {
                        x[0] = x[1] = x[2] = ulong.MaxValue;
                        BitUtils64.ResetBits(1, x, posx + pos);

                        int posy = Size - count;
                        y[0] = y[1] = y[2] = ulong.MaxValue;
                        BitUtils64.BitsAND(count, x, posx, y, posy);

                        Assert.AreEqual(posy + pos, BitUtils64.BitScanZeroForward(Size, y, 0));
                    }
                }
            }
        }
    }
}
