﻿// -----------------------------------------------------------------------
// <copyright file="IPP.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Imaging
{
    using System;
    using System.Diagnostics;

    /// <content>
    /// Provides support methods for calling Intel IPP library functions.
    /// </content>
    internal class IPP
    {
        public static void Execute(Func<int> action)
        {
            int retCode = action();
            IPP.ProcessErrorCode(retCode);
        }

        private static void ProcessErrorCode(int code)
        {
            if (code > 0)
            {
                switch (code)
                {
                    case 33: /* ippStsDstSizeLessExpected */
                        Debug.WriteLine("DC: Size of the destination buffer is less than the expected one.");
                        break;
                }

                return;
            }

            switch (code)
            {
                case 0:
                    break;

                case -17:
                    throw new ArgumentException("Invalid value for the FFT order parameter.");
                case -12:
                    throw new ArgumentException("Scale bounds are out of range.");
                case -11:
                    throw new ArgumentException("Argument is out of range, or point is outside the image.");
                case -10:
                    throw new ArgumentException("An attempt to divide by zero.");
                case -9:
                    throw new ArgumentException("Memory allocated for the operation is not enough.");
                case -8:
                    throw new ArgumentException("Null pointer error.");
                case -7:
                    throw new ArgumentException("Incorrect values for bounds: the lower bound is greater than the upper bound.");
                case -6:
                    throw new ArgumentException("Incorrect value for data size.");
                case -5:
                    throw new ArgumentException("Incorrect arg/param of the function.");
                case -4:
                    throw new ArgumentException("Not enough memory for the operation.");
                case -2:
                    throw new ArgumentException("Unknown/unspecified error");

                default:
                    throw new InvalidOperationException();
            }
        }
    }
}
