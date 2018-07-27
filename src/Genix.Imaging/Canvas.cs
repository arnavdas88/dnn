// -----------------------------------------------------------------------
// <copyright file="Canvas.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Imaging
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Windows;

    /// <summary>
    /// Represents an area that can be used for drawing.
    /// </summary>
    public class Canvas : IDisposable
    {
        private readonly int width;
        private readonly int height;
        private readonly int oldMapMode;
        private readonly int oldBkColor;
        private SafeHdcHandle hdc = null;
        private SafeGdiObjectHandle bitmap = null;
        private SafeGdiObjectHandle hfont = null;

        private bool disposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="Canvas"/> class.
        /// </summary>
        /// <param name="width">The canvas width.</param>
        /// <param name="height">The canvas height.</param>
        public Canvas(int width, int height)
        {
            try
            {
                // allocate enough to fit all characters
                this.width = width <= 0 ? 2000 : width;
                this.height = height;

                // get handle to device context.
                this.hdc = NativeMethods.CreateCompatibleDC(IntPtr.Zero);
                if (this.hdc.IsInvalid)
                {
                    throw new InvalidOperationException(Properties.Resources.E_InvalidCanvasOperation);
                }

                // create destination bitmap
                this.bitmap = NativeMethods.CreateCompatibleBitmap(this.hdc, this.width, this.height);
                if (this.bitmap.IsInvalid)
                {
                    throw new InvalidOperationException(Properties.Resources.E_InvalidCanvasOperation);
                }

                NativeMethods.SelectObject(this.hdc, this.bitmap);

                // select color
                this.oldMapMode = NativeMethods.SetBkMode(this.hdc, NativeMethods.TRANSPARENT);
                this.oldBkColor = NativeMethods.SetTextColor(this.hdc, ColorTranslator.ToWin32(Color.Black));

                // erase background
                this.Clear();
            }
            catch
            {
                this.ReleaseCanvas();
                throw;
            }
        }

        ~Canvas()
        {
            if (!this.disposed)
            {
                this.Dispose(false);

                this.disposed = true;
            }
        }

        /// <summary>
        /// Releases all resources used by this object.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method frees all unmanaged resources used by the object.
        /// The method invokes the protected <see cref="Dispose(Boolean)"/> method with the <c>disposing</c> parameter set to <b>true</b>.
        /// </para>
        /// <para>
        /// Call <b>Dispose</b> when you are finished using the object.
        /// The <b>Dispose</b> method leaves the object in an unusable state.
        /// After calling <b>Dispose</b>, you must release all references to the object so the garbage collector can reclaim the memory that the object was occupying.
        /// </para>
        /// </remarks>
        public void Dispose()
        {
            if (!this.disposed)
            {
                this.Dispose(true);

                this.disposed = true;
            }

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Clears the canvas.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", Justification = "Ignore return values during hdc restoration.")]
        public void Clear()
        {
            if (this.hdc != null)
            {
                int mapMode = NativeMethods.SetMapMode(this.hdc, NativeMethods.MM_TEXT);
                int backColor = NativeMethods.SetBkColor(this.hdc, ColorTranslator.ToWin32(Color.White));

                NativeMethods.Rect bounds = new NativeMethods.Rect(0, 0, this.width, this.height);
                NativeMethods.ExtTextOut(this.hdc, 0, 0, NativeMethods.ETO_OPAQUE, ref bounds, null, 0, null);

                NativeMethods.SetMapMode(this.hdc, mapMode);
                NativeMethods.SetBkColor(this.hdc, backColor);
            }
        }

        /// <summary>
        /// Select a font used to draw text on this <see cref="Canvas"/> class.
        /// </summary>
        /// <param name="fontName">The name of the font used to draw on the canvas.</param>
        /// <param name="fontSize">The size of the font used to draw on the canvas.</param>
        /// <param name="fontStyle">The style of the font used to draw on the canvas.</param>
        /// <param name="unit">The GraphicsUnit of the new font. </param>
        public void SelectFont(string fontName, float fontSize, System.Drawing.FontStyle fontStyle, GraphicsUnit unit)
        {
            if (this.hdc != null)
            {
                if (this.hfont != null)
                {
                    this.hfont.Dispose();
                    this.hfont = null;
                }

                // get a handle to the Font object.
                using (Font font = new Font(fontName, fontSize, fontStyle, unit))
                {
                    this.hfont = new SafeGdiObjectHandle(font.ToHfont(), true);
                    NativeMethods.SelectObject(this.hdc, this.hfont);
                }

                // select color
                NativeMethods.SetBkMode(this.hdc, NativeMethods.TRANSPARENT);
                NativeMethods.SetTextColor(this.hdc, ColorTranslator.ToWin32(Color.Black));
            }
        }

        /// <summary>
        /// Draws the specified text at the specified position using the specified alignment.
        /// </summary>
        /// <param name="text">The text to draw.</param>
        /// <param name="position">The position on the canvas, where to draw the text.</param>
        /// <param name="align">The text alignment.</param>
        /// <returns>The position of drawn text.</returns>
        public Rectangle DrawText(string text, Rectangle position, HorizontalAlignment align)
        {
            if (this.hdc != null)
            {
                int format = NativeMethods.DT_SINGLELINE | NativeMethods.DT_NOCLIP;
                if (align == HorizontalAlignment.Right)
                {
                    format |= NativeMethods.DT_RIGHT;
                }
                else if (align == HorizontalAlignment.Left)
                {
                    format |= NativeMethods.DT_LEFT;
                }
                else
                {
                    format |= NativeMethods.DT_CENTER;
                }

                // first pass - calculate the rectangle
                format |= NativeMethods.DT_CALCRECT;
                NativeMethods.Rect r = new NativeMethods.Rect(position.Left, position.Top, position.Right, position.Bottom);
                if (NativeMethods.DrawText(this.hdc, text, -1, ref r, format) == 0)
                {
                    throw new InvalidOperationException(Properties.Resources.E_InvalidCanvasOperation);
                }

                // second pass - draw the text (center vertically)
                format &= ~NativeMethods.DT_CALCRECT;
                if (position.Width == 0)
                {
                    int xoffset = position.Height / 4;
                    r.Left += xoffset;
                    r.Right += xoffset;
                }
                else
                {
                    r.Left = position.Left;
                    r.Right = position.Right;
                }

                int textHeight = r.Bottom - r.Top;
                int yoffset = (position.Height - textHeight) / 2;
                r.Top += yoffset;
                r.Bottom += yoffset;

                if (NativeMethods.DrawText(this.hdc, text, -1, ref r, format) == 0)
                {
                    throw new InvalidOperationException(Properties.Resources.E_InvalidCanvasOperation);
                }

                return Rectangle.FromLTRB(r.Left, r.Top, r.Right, r.Bottom);
            }
            else
            {
                return Rectangle.Empty;
            }
        }

        /// <summary>
        /// Converts this canvas to <see cref="Imaging.Image"/>.
        /// </summary>
        /// <param name="rect">The area to crop from the bitmap.</param>
        /// <returns>The <see cref="Imaging.Image"/> this method creates.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", Justification = "Need to comply with .NET interface requirements.")]
        public Genix.Imaging.Image ToImage(Rectangle rect)
        {
            if (this.bitmap == null)
            {
                return null;
            }

            using (Bitmap bitmap = System.Drawing.Image.FromHbitmap(this.bitmap.DangerousGetHandle()))
            {
                return rect.IsEmpty ? BitmapExtensions.FromBitmap(bitmap) : BitmapExtensions.FromBitmap(bitmap, rect);
            }
        }

        private void Dispose(bool disposing)
        {
            this.ReleaseCanvas();
        }

        private void ReleaseCanvas()
        {
            if (this.hdc != null)
            {
                NativeMethods.SetMapMode(this.hdc, this.oldMapMode);
                NativeMethods.SetBkColor(this.hdc, this.oldBkColor);

                this.hdc.Dispose();
                this.hdc = null;
            }

            if (this.bitmap != null)
            {
                this.bitmap.Dispose();
                this.bitmap = null;
            }

            if (this.hfont != null)
            {
                this.hfont.Dispose();
                this.hfont = null;
            }
        }

        private static class NativeMethods
        {
            // Mapping Modes
            public const int MM_TEXT = 1;
            public const int MM_LOMETRIC = 2;
            public const int MM_HIMETRIC = 3;
            public const int MM_LOENGLISH = 4;
            public const int MM_HIENGLISH = 5;
            public const int MM_TWIPS = 6;
            public const int MM_ISOTROPIC = 7;
            public const int MM_ANISOTROPIC = 8;

            // Draw text formats
            public const int DT_TOP = 0x00000000;
            public const int DT_LEFT = 0x00000000;
            public const int DT_CENTER = 0x00000001;
            public const int DT_RIGHT = 0x00000002;
            public const int DT_VCENTER = 0x00000004;
            public const int DT_BOTTOM = 0x00000008;
            public const int DT_WORDBREAK = 0x00000010;
            public const int DT_SINGLELINE = 0x00000020;
            public const int DT_EXPANDTABS = 0x00000040;
            public const int DT_TABSTOP = 0x00000080;
            public const int DT_NOCLIP = 0x00000100;
            public const int DT_EXTERNALLEADING = 0x00000200;
            public const int DT_CALCRECT = 0x00000400;
            public const int DT_NOPREFIX = 0x00000800;
            public const int DT_INTERNAL = 0x00001000;

            // Background modes
            public const int TRANSPARENT = 1;
            public const int OPAQUE = 2;

            // ExtTextOut formats
            public const int ETO_OPAQUE = 0x0002;
            public const int ETO_CLIPPED = 0x0004;
            public const int ETO_GLYPH_INDEX = 0x0010;
            public const int ETO_RTLREADING = 0x0080;
            public const int ETO_NUMERICSLOCAL = 0x0400;
            public const int ETO_NUMERICSLATIN = 0x0800;
            public const int ETO_IGNORELANGUAGE = 0x1000;
            public const int ETO_PDY = 0x2000;

            [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
            [SuppressUnmanagedCodeSecurity]
            public static extern SafeHdcHandle CreateCompatibleDC(IntPtr hdc);

            [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
            [SuppressUnmanagedCodeSecurity]
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool DeleteDC(IntPtr hdc);

            [DllImport("gdi32.dll", ExactSpelling = true)]
            [SuppressUnmanagedCodeSecurity]
            public static extern SafeGdiObjectHandle CreateCompatibleBitmap(SafeHdcHandle hdc, int width, int height);

            [DllImport("gdi32.dll", ExactSpelling = true)]
            [SuppressUnmanagedCodeSecurity]
            public static extern IntPtr SelectObject(SafeHdcHandle hdc, SafeHandle objectHandle);

            [DllImport("gdi32.dll", ExactSpelling = true)]
            [SuppressUnmanagedCodeSecurity]
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool DeleteObject(IntPtr objectHandle);

            [DllImport("gdi32.dll", ExactSpelling = true)]
            [SuppressUnmanagedCodeSecurity]
            public static extern int SetMapMode(SafeHdcHandle hdc, int mode);

            [DllImport("gdi32.dll", ExactSpelling = true)]
            [SuppressUnmanagedCodeSecurity]
            public static extern int SetTextColor(SafeHdcHandle hdc, int color);

            [DllImport("gdi32.dll", ExactSpelling = true)]
            [SuppressUnmanagedCodeSecurity]
            public static extern int SetBkColor(SafeHdcHandle hdc, int color);

            [DllImport("gdi32.dll", ExactSpelling = true)]
            [SuppressUnmanagedCodeSecurity]
            public static extern int SetBkMode(SafeHdcHandle hdc, int mode);

            [DllImport("user32.dll", CharSet = CharSet.Unicode)]
            [SuppressUnmanagedCodeSecurity]
            public static extern int DrawText(SafeHdcHandle hdc, string s, int count, ref Rect rect, int format);

            [DllImport("gdi32.dll", CharSet = CharSet.Unicode)]
            [SuppressUnmanagedCodeSecurity]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool ExtTextOut(SafeHdcHandle hdc, int x, int y, int options, ref Rect rect, string s, int count, int[] dx);

            public struct Rect
            {
                public int Left, Top, Right, Bottom;

                public Rect(int left, int top, int right, int bottom)
                {
                    this.Left = left;
                    this.Top = top;
                    this.Right = right;
                    this.Bottom = bottom;
                }

                public Rect(Rectangle r)
                {
                    this.Left = r.Left;
                    this.Top = r.Top;
                    this.Right = r.Right;
                    this.Bottom = r.Bottom;
                }

                public Rect(RectangleF r)
                {
                    this.Left = System.Convert.ToInt32(r.Left);
                    this.Top = System.Convert.ToInt32(r.Top);
                    this.Right = System.Convert.ToInt32(r.Right);
                    this.Bottom = System.Convert.ToInt32(r.Bottom);
                }
            }
        }

        /// <summary>
        /// Represents a wrapper class for the device content (HDC) resource.
        /// </summary>
        private sealed class SafeHdcHandle : SafeHandle
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="SafeHdcHandle"/> class.
            /// </summary>
            [SecurityPermission(SecurityAction.InheritanceDemand, UnmanagedCode = true)]
            [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
            public SafeHdcHandle()
                : this(IntPtr.Zero, true)
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="SafeHdcHandle"/> class.
            /// </summary>
            /// <param name="preexistingHandle">An object that represents the pre-existing handle to use.</param>
            [SecurityPermission(SecurityAction.InheritanceDemand, UnmanagedCode = true)]
            [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
            public SafeHdcHandle(IntPtr preexistingHandle)
                : this(preexistingHandle, true)
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="SafeHdcHandle"/> class.
            /// </summary>
            /// <param name="preexistingHandle">An object that represents the pre-existing handle to use.</param>
            /// <param name="ownsHandle"><b>true</b> to reliably release the handle during the finalization phase; <b>false</b> to prevent reliable release (not recommended).</param>
            [SecurityPermission(SecurityAction.InheritanceDemand, UnmanagedCode = true)]
            [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
            public SafeHdcHandle(IntPtr preexistingHandle, bool ownsHandle)
                : base(IntPtr.Zero, ownsHandle)
            {
                this.SetHandle(preexistingHandle);
            }

            /// <summary>
            /// Gets a value that indicates whether the handle is invalid.
            /// </summary>
            /// <value>
            /// <b>true</b> if the handle is not valid; otherwise, <b>false</b>.
            /// </value>
            public override bool IsInvalid
            {
                get { return this.handle == IntPtr.Zero; }
            }

            /// <summary>
            /// Executes the code required to free the handle.
            /// </summary>
            /// <returns><b>true</b> if the handle is released successfully; otherwise, in the event of a catastrophic failure, <b>false</b>.</returns>
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
            [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "This method must never fail.")]
            protected override bool ReleaseHandle()
            {
                // Here, we must obey all rules for constrained execution regions.
                // If ReleaseHandle failed, it can be reported via the "releaseHandleFailed" managed debugging assistant (MDA).
                // This MDA is disabled by default, but can be enabled in a debugger or during testing to diagnose handle corruption problems.
                // We do not throw an exception because most code could not recover from the problem.
                return NativeMethods.DeleteDC(this.handle);
            }
        }

        /// <summary>
        /// Represents a wrapper class for the Windows GDI object resource.
        /// </summary>
        private sealed class SafeGdiObjectHandle : SafeHandle
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="SafeGdiObjectHandle"/> class.
            /// </summary>
            [SecurityPermission(SecurityAction.InheritanceDemand, UnmanagedCode = true)]
            [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
            public SafeGdiObjectHandle()
                : this(IntPtr.Zero, true)
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="SafeGdiObjectHandle"/> class.
            /// </summary>
            /// <param name="preexistingHandle">An object that represents the pre-existing handle to use.</param>
            [SecurityPermission(SecurityAction.InheritanceDemand, UnmanagedCode = true)]
            [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
            public SafeGdiObjectHandle(IntPtr preexistingHandle)
                : this(preexistingHandle, true)
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="SafeGdiObjectHandle"/> class.
            /// </summary>
            /// <param name="preexistingHandle">An object that represents the pre-existing handle to use.</param>
            /// <param name="ownsHandle"><b>true</b> to reliably release the handle during the finalization phase; <b>false</b> to prevent reliable release (not recommended).</param>
            [SecurityPermission(SecurityAction.InheritanceDemand, UnmanagedCode = true)]
            [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
            public SafeGdiObjectHandle(IntPtr preexistingHandle, bool ownsHandle)
                : base(IntPtr.Zero, ownsHandle)
            {
                this.SetHandle(preexistingHandle);
            }

            /// <summary>
            /// Gets a value that indicates whether the handle is invalid.
            /// </summary>
            /// <value>
            /// <b>true</b> if the handle is not valid; otherwise, <b>false</b>.
            /// </value>
            public override bool IsInvalid
            {
                get { return this.handle == IntPtr.Zero; }
            }

            /// <summary>
            /// Executes the code required to free the handle.
            /// </summary>
            /// <returns><b>true</b> if the handle is released successfully; otherwise, in the event of a catastrophic failure, <b>false</b>.</returns>
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
            [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "This method must never fail.")]
            protected override bool ReleaseHandle()
            {
                // Here, we must obey all rules for constrained execution regions.
                // If ReleaseHandle failed, it can be reported via the "releaseHandleFailed" managed debugging assistant (MDA).
                // This MDA is disabled by default, but can be enabled in a debugger or during testing to diagnose handle corruption problems.
                // We do not throw an exception because most code could not recover from the problem.
                return NativeMethods.DeleteObject(this.handle);
            }
        }
    }
}
