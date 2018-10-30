﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a T4 template.
//     Generated on: 10/30/2018 12:19:13 PM
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated. Re-run the T4 template to update this file.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Genix.Core.Test
{
    using System;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    public partial class VectorsTest
    {
        [TestMethod]
        public void NegTest_sbyte()
        {
            const int offx = 5;
            const int offy = 10;

            foreach (int length in new[] { 24, 128 })
            {
                sbyte[] x0 = new sbyte[length];
                sbyte[] y0 = new sbyte[length];

                for (int i = 0; i < length; i++)
                {
                    x0[i] = (sbyte)i;
                    y0[i] = (sbyte)(i + 2);
                }

                // not-in-place
                sbyte[] y = y0.ToArray();
                int count = length - Math.Max(offx, offy) - 2;
                Vectors.Neg(count, x0, offx, y, offy);
                GenixAssert.AreArraysEqual(y0.Select((a, i) => i.Between(offy, offy + count - 1) ? (sbyte)-x0[i - offy + offx] : a).ToArray(), y);

                // in-place
                sbyte[] x = x0.ToArray();
                count = length - offx - 2;
                Vectors.Neg(count, x, offx);
                GenixAssert.AreArraysEqual(x0.Select((a, i) => i.Between(offx, offx + count - 1) ? (sbyte)-a : a).ToArray(), x);
            }
        }
        [TestMethod]
        public void NegTest_short()
        {
            const int offx = 5;
            const int offy = 10;

            foreach (int length in new[] { 24, 128 })
            {
                short[] x0 = new short[length];
                short[] y0 = new short[length];

                for (int i = 0; i < length; i++)
                {
                    x0[i] = (short)i;
                    y0[i] = (short)(i + 2);
                }

                // not-in-place
                short[] y = y0.ToArray();
                int count = length - Math.Max(offx, offy) - 2;
                Vectors.Neg(count, x0, offx, y, offy);
                GenixAssert.AreArraysEqual(y0.Select((a, i) => i.Between(offy, offy + count - 1) ? (short)-x0[i - offy + offx] : a).ToArray(), y);

                // in-place
                short[] x = x0.ToArray();
                count = length - offx - 2;
                Vectors.Neg(count, x, offx);
                GenixAssert.AreArraysEqual(x0.Select((a, i) => i.Between(offx, offx + count - 1) ? (short)-a : a).ToArray(), x);
            }
        }
        [TestMethod]
        public void NegTest_int()
        {
            const int offx = 5;
            const int offy = 10;

            foreach (int length in new[] { 24, 128 })
            {
                int[] x0 = new int[length];
                int[] y0 = new int[length];

                for (int i = 0; i < length; i++)
                {
                    x0[i] = (int)i;
                    y0[i] = (int)(i + 2);
                }

                // not-in-place
                int[] y = y0.ToArray();
                int count = length - Math.Max(offx, offy) - 2;
                Vectors.Neg(count, x0, offx, y, offy);
                GenixAssert.AreArraysEqual(y0.Select((a, i) => i.Between(offy, offy + count - 1) ? (int)-x0[i - offy + offx] : a).ToArray(), y);

                // in-place
                int[] x = x0.ToArray();
                count = length - offx - 2;
                Vectors.Neg(count, x, offx);
                GenixAssert.AreArraysEqual(x0.Select((a, i) => i.Between(offx, offx + count - 1) ? (int)-a : a).ToArray(), x);
            }
        }
        [TestMethod]
        public void NegTest_long()
        {
            const int offx = 5;
            const int offy = 10;

            foreach (int length in new[] { 24, 128 })
            {
                long[] x0 = new long[length];
                long[] y0 = new long[length];

                for (int i = 0; i < length; i++)
                {
                    x0[i] = (long)i;
                    y0[i] = (long)(i + 2);
                }

                // not-in-place
                long[] y = y0.ToArray();
                int count = length - Math.Max(offx, offy) - 2;
                Vectors.Neg(count, x0, offx, y, offy);
                GenixAssert.AreArraysEqual(y0.Select((a, i) => i.Between(offy, offy + count - 1) ? (long)-x0[i - offy + offx] : a).ToArray(), y);

                // in-place
                long[] x = x0.ToArray();
                count = length - offx - 2;
                Vectors.Neg(count, x, offx);
                GenixAssert.AreArraysEqual(x0.Select((a, i) => i.Between(offx, offx + count - 1) ? (long)-a : a).ToArray(), x);
            }
        }
        [TestMethod]
        public void NegTest_float()
        {
            const int offx = 5;
            const int offy = 10;

            foreach (int length in new[] { 24, 128 })
            {
                float[] x0 = new float[length];
                float[] y0 = new float[length];

                for (int i = 0; i < length; i++)
                {
                    x0[i] = (float)i;
                    y0[i] = (float)(i + 2);
                }

                // not-in-place
                float[] y = y0.ToArray();
                int count = length - Math.Max(offx, offy) - 2;
                Vectors.Neg(count, x0, offx, y, offy);
                GenixAssert.AreArraysEqual(y0.Select((a, i) => i.Between(offy, offy + count - 1) ? (float)-x0[i - offy + offx] : a).ToArray(), y);

                // in-place
                float[] x = x0.ToArray();
                count = length - offx - 2;
                Vectors.Neg(count, x, offx);
                GenixAssert.AreArraysEqual(x0.Select((a, i) => i.Between(offx, offx + count - 1) ? (float)-a : a).ToArray(), x);
            }
        }
        [TestMethod]
        public void NegTest_double()
        {
            const int offx = 5;
            const int offy = 10;

            foreach (int length in new[] { 24, 128 })
            {
                double[] x0 = new double[length];
                double[] y0 = new double[length];

                for (int i = 0; i < length; i++)
                {
                    x0[i] = (double)i;
                    y0[i] = (double)(i + 2);
                }

                // not-in-place
                double[] y = y0.ToArray();
                int count = length - Math.Max(offx, offy) - 2;
                Vectors.Neg(count, x0, offx, y, offy);
                GenixAssert.AreArraysEqual(y0.Select((a, i) => i.Between(offy, offy + count - 1) ? (double)-x0[i - offy + offx] : a).ToArray(), y);

                // in-place
                double[] x = x0.ToArray();
                count = length - offx - 2;
                Vectors.Neg(count, x, offx);
                GenixAssert.AreArraysEqual(x0.Select((a, i) => i.Between(offx, offx + count - 1) ? (double)-a : a).ToArray(), x);
            }
        }
    }
}