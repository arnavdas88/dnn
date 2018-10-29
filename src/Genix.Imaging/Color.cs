// -----------------------------------------------------------------------
// <copyright file="Color.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Imaging
{
    using System;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Represents an ARGB color.
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct Color
        : IEquatable<Color>
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
        [CLSCompliant(false)]
        public uint Argb;

        /// <summary>
        /// Initializes a new instance of the <see cref="Color"/> struct from a 32-bit ARGB value.
        /// </summary>
        /// <param name="argb">A value specifying the 32-bit ARGB value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Color(uint argb)
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
        /// Compares two <see cref="Color"/> objects.
        /// The result specifies whether the values of the two <see cref="Argb"/> property of the two <see cref="Color"/> objects are equal.
        /// </summary>
        /// <param name="left">The <see cref="Color"/> structure that is to the left of the equality operator.</param>
        /// <param name="right">The <see cref="Color"/> structure that is to the right of the equality operator.</param>
        /// <returns><b>true</b> if the <see cref="Argb"/> values of <paramref name="left"/> and <paramref name="right"/> are equal; otherwise, <b>false</b>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Color left, Color right) => left.Equals(right);

        /// <summary>
        /// Compares two <see cref="Color"/> objects.
        /// The result specifies whether the values of the two <see cref="Argb"/> property of the two <see cref="Color"/> objects are unequal.
        /// </summary>
        /// <param name="left">The <see cref="Color"/> structure that is to the left of the equality operator.</param>
        /// <param name="right">The <see cref="Color"/> structure that is to the right of the equality operator.</param>
        /// <returns><b>true</b> if the values of either <see cref="Argb"/> properties of <paramref name="left"/> and <paramref name="right"/> are unequal; otherwise, <b>false</b>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Color left, Color right) => !left.Equals(right);

        /// <summary>
        /// Creates a <see cref="Color"/> structure from a 32-bit ARGB value.
        /// </summary>
        /// <param name="argb">A value specifying the 32-bit ARGB value.</param>
        /// <returns>
        /// The <see cref="Color"/> structure that this method creates.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [CLSCompliant(false)]
        public static Color FromArgb(uint argb)
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

        /// <summary>
        /// Adds each channel of the second color to each channel of the first and returns the result.
        /// </summary>
        /// <param name="color1">The first color.</param>
        /// <param name="color2">The second color.</param>
        /// <returns>The result of addition. </returns>
        public static Color Add(Color color1, Color color2)
        {
            byte r = (byte)Math.Min(255, (int)color1.R + (int)color2.R);
            byte g = (byte)Math.Min(255, (int)color1.G + (int)color2.G);
            byte b = (byte)Math.Min(255, (int)color1.B + (int)color2.B);
            byte a = (byte)Math.Min(255, (int)color1.A + (int)color2.A);
            return Color.FromArgb(a, r, g, b);
        }

        /// <summary>
        /// Adds each channel of the second color to each channel of the first and scales the result.
        /// </summary>
        /// <param name="color1">The first color.</param>
        /// <param name="color2">The second color.</param>
        /// <param name="scaleFactor">The scaling factor.</param>
        /// <param name="mode">Specification for how to round value if it is midway between two other numbers.</param>
        /// <returns>The result of addition. </returns>
        public static Color Add(Color color1, Color color2, double scaleFactor, MidpointRounding mode)
        {
            byte r = (byte)(scaleFactor * ((int)color1.R + (int)color2.R)).Round(mode).Clip(0, 255);
            byte g = (byte)(scaleFactor * ((int)color1.G + (int)color2.G)).Round(mode).Clip(0, 255);
            byte b = (byte)(scaleFactor * ((int)color1.B + (int)color2.B)).Round(mode).Clip(0, 255);
            byte a = (byte)(scaleFactor * ((int)color1.A + (int)color2.A)).Round(mode).Clip(0, 255);
            return Color.FromArgb(a, r, g, b);
        }

        /// <summary>
        /// Subtracts each channel of the second color to each channel of the first and returns the result.
        /// </summary>
        /// <param name="color1">The first color.</param>
        /// <param name="color2">The second color.</param>
        /// <returns>The result of subtraction.</returns>
        public static Color Subtract(Color color1, Color color2)
        {
            byte r = (byte)Math.Max(0, (int)color1.R - (int)color2.R);
            byte g = (byte)Math.Max(0, (int)color1.G - (int)color2.G);
            byte b = (byte)Math.Max(0, (int)color1.B - (int)color2.B);
            byte a = (byte)Math.Max(0, (int)color1.A - (int)color2.A);
            return Color.FromArgb(a, r, g, b);
        }

        /// <summary>
        /// Subtracts each channel of the second color to each channel of the first and scales the result.
        /// </summary>
        /// <param name="color1">The first color.</param>
        /// <param name="color2">The second color.</param>
        /// <param name="scaleFactor">The scaling factor.</param>
        /// <param name="mode">Specification for how to round value if it is midway between two other numbers.</param>
        /// <returns>The result of subtraction.</returns>
        public static Color Subtract(Color color1, Color color2, double scaleFactor, MidpointRounding mode)
        {
            byte r = (byte)(scaleFactor * ((int)color1.R - (int)color2.R)).Round(mode).Clip(0, 255);
            byte g = (byte)(scaleFactor * ((int)color1.G - (int)color2.G)).Round(mode).Clip(0, 255);
            byte b = (byte)(scaleFactor * ((int)color1.B - (int)color2.B)).Round(mode).Clip(0, 255);
            byte a = (byte)(scaleFactor * ((int)color1.A - (int)color2.A)).Round(mode).Clip(0, 255);
            return Color.FromArgb(a, r, g, b);
        }

        /// <summary>
        /// Multiplies each channel of the first color by each channel of the second color and scales the result.
        /// </summary>
        /// <param name="color1">The first color.</param>
        /// <param name="color2">The second color.</param>
        /// <param name="scaleFactor">The scaling factor.</param>
        /// <param name="mode">Specification for how to round value if it is midway between two other numbers.</param>
        /// <returns>The result of multiplication.</returns>
        public static Color Multiply(Color color1, Color color2, double scaleFactor, MidpointRounding mode)
        {
            byte r = (byte)(scaleFactor * ((int)color1.R * (int)color2.R)).Round(mode).Clip(0, 255);
            byte g = (byte)(scaleFactor * ((int)color1.G * (int)color2.G)).Round(mode).Clip(0, 255);
            byte b = (byte)(scaleFactor * ((int)color1.B * (int)color2.B)).Round(mode).Clip(0, 255);
            byte a = (byte)(scaleFactor * ((int)color1.A * (int)color2.A)).Round(mode).Clip(0, 255);
            return Color.FromArgb(a, r, g, b);
        }

        /// <summary>
        /// Divides each channel of the first color by each channel of the second color and scales the result.
        /// </summary>
        /// <param name="color1">The first color.</param>
        /// <param name="color2">The second color.</param>
        /// <param name="scaleFactor">The scaling factor.</param>
        /// <param name="mode">Specification for how to round value if it is midway between two other numbers.</param>
        /// <returns>The result of multiplication.</returns>
        public static Color Divide(Color color1, Color color2, double scaleFactor, MidpointRounding mode)
        {
            byte r = color2.R == 0 ? (color1.R == 0 ? (byte)0 : (byte)255) : (byte)(scaleFactor * color1.R / color2.R).Round(mode).Clip(0, 255);
            byte g = color2.G == 0 ? (color1.G == 0 ? (byte)0 : (byte)255) : (byte)(scaleFactor * color1.G / color2.G).Round(mode).Clip(0, 255);
            byte b = color2.B == 0 ? (color1.B == 0 ? (byte)0 : (byte)255) : (byte)(scaleFactor * color1.B / color2.B).Round(mode).Clip(0, 255);
            byte a = color2.A == 0 ? (color1.A == 0 ? (byte)0 : (byte)255) : (byte)(scaleFactor * color1.A / color2.A).Round(mode).Clip(0, 255);
            return Color.FromArgb(a, r, g, b);
        }

        /// <summary>
        /// Scales each channel of the color by the specified scaling factor.
        /// </summary>
        /// <param name="color">The color to scale.</param>
        /// <param name="scaleFactor">The scaling factor.</param>
        /// <param name="mode">Specification for how to round value if it is midway between two other numbers.</param>
        /// <returns>The result of scaling. </returns>
        public static Color Scale(Color color, double scaleFactor, MidpointRounding mode)
        {
            byte r = (byte)Math.Min(255, (scaleFactor * color.R).Round(mode));
            byte g = (byte)Math.Min(255, (scaleFactor * color.G).Round(mode));
            byte b = (byte)Math.Min(255, (scaleFactor * color.B).Round(mode));
            byte a = (byte)Math.Min(255, (scaleFactor * color.A).Round(mode));
            return Color.FromArgb(a, r, g, b);
        }

        /// <summary>
        /// Computes the of each channel maximum of two colors.
        /// </summary>
        /// <param name="color1">The first color.</param>
        /// <param name="color2">The second color.</param>
        /// <returns>The maximum of two colors.</returns>
        public static Color Max(Color color1, Color color2)
        {
            byte r = Core.MinMax.Max(color1.R, color2.R);
            byte g = Core.MinMax.Max(color1.G, color2.G);
            byte b = Core.MinMax.Max(color1.B, color2.B);
            byte a = Core.MinMax.Max(color1.A, color2.A);
            return Color.FromArgb(a, r, g, b);
        }

        /// <summary>
        /// Computes the minimum of each channel of two colors.
        /// </summary>
        /// <param name="color1">The first color.</param>
        /// <param name="color2">The second color.</param>
        /// <returns>The minimum of two colors.</returns>
        public static Color Min(Color color1, Color color2)
        {
            byte r = Core.MinMax.Min(color1.R, color2.R);
            byte g = Core.MinMax.Min(color1.G, color2.G);
            byte b = Core.MinMax.Min(color1.B, color2.B);
            byte a = Core.MinMax.Min(color1.A, color2.A);
            return Color.FromArgb(a, r, g, b);
        }

        /// <summary>
        /// Computes the maximum of two ARGB colors of the specified depth.
        /// </summary>
        /// <param name="color1">The first color.</param>
        /// <param name="color2">The second color.</param>
        /// <param name="bitsPerPixel">The image color depth, in number of bits per pixel.</param>
        /// <returns>The maximum of two colors.</returns>
        [CLSCompliant(false)]
        public static uint Max(uint color1, uint color2, int bitsPerPixel)
        {
            return bitsPerPixel < 24 ? Math.Max(color1, color2) : Color.Max(Color.FromArgb(color1), Color.FromArgb(color2)).Argb;
        }

        /// <summary>
        /// Computes the minimum of two ARGB colors of the specified depth.
        /// </summary>
        /// <param name="color1">The first color.</param>
        /// <param name="color2">The second color.</param>
        /// <param name="bitsPerPixel">The image color depth, in number of bits per pixel.</param>
        /// <returns>The maximum of two colors.</returns>
        [CLSCompliant(false)]
        public static uint Min(uint color1, uint color2, int bitsPerPixel)
        {
            return bitsPerPixel < 24 ? Math.Min(color1, color2) : Color.Min(Color.FromArgb(color1), Color.FromArgb(color2)).Argb;
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Color other) => other.Argb == this.Argb;

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (!(obj is Color))
            {
                return false;
            }

            return this.Equals((Color)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode() => unchecked((int)this.Argb);

        /// <inheritdoc />
        public override string ToString() =>
            string.Format(
                CultureInfo.InvariantCulture,
                "A={0}, R={1}, G={2}, B={3}",
                this.A,
                this.R,
                this.G,
                this.B);
    }
}
