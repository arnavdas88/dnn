// -----------------------------------------------------------------------
// <copyright file="ArrayHelpers.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Accord.DNN
{
    using System;
    using System.Runtime.CompilerServices;

    internal static class ArrayHelpers
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[] InsertAt<T>(T[] array, int position, T value)
        {
            int rank = array?.Length ?? throw new ArgumentNullException(nameof(array));

            if (position < 0 || position > rank)
            {
                throw new ArgumentOutOfRangeException(nameof(position));
            }

            T[] newarray = new T[++rank];

            for (int i = 0; i < position; i++)
            {
                newarray[i] = array[i];
            }

            newarray[position] = value;

            for (int i = position + 1; i < rank; i++)
            {
                newarray[i] = array[i - 1];
            }

            return newarray;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[] RemoveAt<T>(T[] array, int position)
        {
            int rank = array?.Length ?? throw new ArgumentNullException(nameof(array));

            if (position < 0 || position >= rank)
            {
                throw new ArgumentOutOfRangeException(nameof(position));
            }

            T[] newarray = new T[--rank];

            for (int i = 0; i < position; i++)
            {
                newarray[i] = array[i];
            }

            for (int i = position + 1; i <= rank; i++)
            {
                newarray[i - 1] = array[i];
            }

            return newarray;
        }
    }
}
