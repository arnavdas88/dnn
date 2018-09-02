// -----------------------------------------------------------------------
// <copyright file="Color.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Imaging
{
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Represents an ARGB color.
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct Color
    {
        /// <summary>
        /// The blue component value of this <see cref="Color"/>.
        /// </summary>
        [FieldOffset(0)]
        public byte B;

        /// <summary>
        /// The green component value of this <see cref="Color"/>.
        /// </summary>
        [FieldOffset(1)]
        public byte G;

        /// <summary>
        /// The red component value of this <see cref="Color"/>.
        /// </summary>
        [FieldOffset(2)]
        public byte R;

        /// <summary>
        /// The alpha component value of this <see cref="Color"/>.
        /// </summary>
        [FieldOffset(3)]
        public byte A;

        /// <summary>
        /// The 32-bit ARGB value of this <see cref="Color"/>.
        /// </summary>
        [FieldOffset(0)]
        public int Argb;

        /// <summary>
        /// Initializes a new instance of the <see cref="Color"/> struct from a 32-bit ARGB value.
        /// </summary>
        /// <param name="argb">A value specifying the 32-bit ARGB value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Color(int argb)
            : this()
        {
            this.Argb = argb;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Color"/> struct from the four ARGB component (alpha, red, green, and blue) values.
        /// </summary>
        /// <param name="alpha">The alpha component. Valid values are 0 through 255.</param>
        /// <param name="red">The red component. Valid values are 0 through 255.</param>
        /// <param name="green">The green component. Valid values are 0 through 255.</param>
        /// <param name="blue">The blue component. Valid values are 0 through 255.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Color(byte alpha, byte red, byte green, byte blue)
            : this()
        {
            this.A = alpha;
            this.R = red;
            this.G = green;
            this.B = blue;
        }

        /// <summary>
        /// Creates a <see cref="Color"/> structure from a 32-bit ARGB value.
        /// </summary>
        /// <param name="argb">A value specifying the 32-bit ARGB value.</param>
        /// <returns>
        /// The <see cref="Color"/> structure that this method creates.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Color FromArgb(int argb)
        {
            return new Color(argb);
        }

        /// <summary>
        /// Creates a <see cref="Color"/> structure from the specified 8-bit color values (red, green, and blue). The alpha value is implicitly 255 (fully opaque).
        /// </summary>
        /// <param name="red">The red component. Valid values are 0 through 255.</param>
        /// <param name="green">The green component. Valid values are 0 through 255.</param>
        /// <param name="blue">The blue component. Valid values are 0 through 255.</param>
        /// <returns>
        /// The <see cref="Color"/> structure that this method creates.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Color FromArgb(byte red, byte green, byte blue)
        {
            return new Color(255, red, green, blue);
        }

        /// <summary>
        /// Creates a <see cref="Color"/> structure from the four ARGB component (alpha, red, green, and blue) values.
        /// </summary>
        /// <param name="alpha">The alpha component. Valid values are 0 through 255.</param>
        /// <param name="red">The red component. Valid values are 0 through 255.</param>
        /// <param name="green">The green component. Valid values are 0 through 255.</param>
        /// <param name="blue">The blue component. Valid values are 0 through 255.</param>
        /// <returns>
        /// The <see cref="Color"/> structure that this method creates.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Color FromArgb(byte alpha, byte red, byte green, byte blue)
        {
            return new Color(alpha, red, green, blue);
        }
    }
}
