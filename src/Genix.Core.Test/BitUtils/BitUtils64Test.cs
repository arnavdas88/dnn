﻿namespace Genix.Core.Test
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
                    BitUtils.SetBits(count, y, pos);

                    Assert.AreEqual(count, BitUtils.CountOneBits(Size, y, 0));

                    Assert.AreEqual(pos, BitUtils.BitScanOneForward(Size, y, 0));
                    Assert.AreEqual(pos + count - 1, BitUtils.BitScanOneReverse(Size, y, Size - 1));

                    Assert.AreEqual(pos + count == Size ? -1 : pos + count, BitUtils.BitScanZeroForward(Size - pos, y, pos));
                    Assert.AreEqual(pos == 0 ? -1 : pos - 1, BitUtils.BitScanZeroReverse(pos + count, y, pos + count - 1));
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
                    BitUtils.ResetBits(count, y, pos);

                    Assert.AreEqual(Size - count, BitUtils.CountOneBits(Size, y, 0));

                    Assert.AreEqual(pos, BitUtils.BitScanZeroForward(Size, y, 0));
                    Assert.AreEqual(pos + count - 1, BitUtils.BitScanZeroReverse(Size, y, Size - 1));

                    Assert.AreEqual(pos + count == Size ? -1 : pos + count, BitUtils.BitScanOneForward(Size, y, pos));
                    Assert.AreEqual(pos == 0 ? -1 : pos - 1, BitUtils.BitScanOneReverse(pos + count, y, pos + count - 1));
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
                        BitUtils.SetBits(1, x, posx + pos);

                        y[0] = y[1] = y[2] = 0;
                        BitUtils.Or(count, x, posx, y, posy);

                        Assert.AreEqual(posy + pos, BitUtils.BitScanOneForward(Size, y, 0));
                    }

                    // shift right
                    for (int posx = 0; posx < Size - count; posx++)
                    {
                        x[0] = x[1] = x[2] = 0;
                        BitUtils.SetBits(1, x, posx + pos);

                        int posy = Size - count;
                        y[0] = y[1] = y[2] = 0;
                        BitUtils.Or(count, x, posx, y, posy);

                        Assert.AreEqual(posy + pos, BitUtils.BitScanOneForward(Size, y, 0));
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
                        BitUtils.ResetBits(1, x, posx + pos);

                        y[0] = y[1] = y[2] = ulong.MaxValue;
                        BitUtils.And(count, x, posx, y, posy);

                        Assert.AreEqual(posy + pos, BitUtils.BitScanZeroForward(Size, y, 0));
                    }

                    // shift right
                    for (int posx = 0; posx < Size - count; posx++)
                    {
                        x[0] = x[1] = x[2] = ulong.MaxValue;
                        BitUtils.ResetBits(1, x, posx + pos);

                        int posy = Size - count;
                        y[0] = y[1] = y[2] = ulong.MaxValue;
                        BitUtils.And(count, x, posx, y, posy);

                        Assert.AreEqual(posy + pos, BitUtils.BitScanZeroForward(Size, y, 0));
                    }
                }
            }
        }

        [TestMethod]
        public void ShrTest()
        {
            const int RepeatCount = 5;
            UlongRandomGenerator random = new UlongRandomGenerator();
            ulong[] y = new ulong[3];
            ulong[] copy = new ulong[3];
            int size = y.Length * 64;

            for (int count = 0; count <= size; count++)
            {
                for (int pos = 0; pos < size - count; pos++)
                {
                    for (int shift = 0; shift <= count; shift++)
                    {
                        for (int i = 0; i < RepeatCount; i++)
                        {
                            random.Generate(y.Length, y);
                            Vectors.Copy(y.Length, y, 0, copy, 0);
                            BitUtils.Shr(count, shift, y, pos);

                            // right part that was not copied
                            if (pos > 0)
                            {
                                Assert.IsTrue(BitUtils.Equals(pos, y, 0, copy, 0));
                            }

                            // shifted part
                            if (shift < count)
                            {
                                Assert.IsTrue(BitUtils.Equals(count - shift, y, pos, copy, pos + shift));
                            }

                            // zeroed part
                            if (shift > 0)
                            {
                                Assert.AreEqual(0, BitUtils.CountOneBits(shift, y, pos + count - shift));
                            }

                            // left part that was not copied
                            if (pos + count < size)
                            {
                                Assert.IsTrue(BitUtils.Equals(size - (pos + count), y, pos + count, copy, pos + count));
                            }
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void ShlTest()
        {
            const int RepeatCount = 5;
            UlongRandomGenerator random = new UlongRandomGenerator();
            ulong[] y = new ulong[3];
            ulong[] copy = new ulong[3];
            int size = y.Length * 64;

            for (int count = 0; count <= size; count++)
            {
                for (int pos = 0; pos < size - count; pos++)
                {
                    for (int shift = 0; shift <= count; shift++)
                    {
                        for (int i = 0; i < RepeatCount; i++)
                        {
                            random.Generate(y.Length, y);
                            Vectors.Copy(y.Length, y, 0, copy, 0);
                            BitUtils.Shl(count, shift, y, pos);

                            // right part that was not copied
                            if (pos > 0)
                            {
                                Assert.IsTrue(BitUtils.Equals(pos, y, 0, copy, 0));
                            }

                            // shifted part
                            if (shift < count)
                            {
                                Assert.IsTrue(BitUtils.Equals(count - shift, y, pos + shift, copy, pos));
                            }

                            // zeroed part
                            if (shift > 0)
                            {
                                Assert.AreEqual(0, BitUtils.CountOneBits(shift, y, pos));
                            }

                            // left part that was not copied
                            if (pos + count < size)
                            {
                                Assert.IsTrue(BitUtils.Equals(size - (pos + count), y, pos + count, copy, pos + count));
                            }
                        }
                    }
                }
            }
        }
    }
}
