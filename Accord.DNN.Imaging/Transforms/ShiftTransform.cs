// -----------------------------------------------------------------------
// <copyright file="TransformBase.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Accord.DNN.Imaging
{
    using System.Drawing;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a horizontal and vertical shift of an <see cref="Image"/>.
    /// </summary>
    public class ShiftTransform
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ShiftTransform"/> class.
        /// </summary>
        /// <param name="shiftX">The horizontal shift, in pixels.</param>
        /// <param name="shiftY">The vertical shift, in pixels.</param>
        public ShiftTransform(int shiftX, int shiftY)
        {
            this.ShiftX = shiftX;
            this.ShiftY = shiftY;
        }

        /// <summary>
        /// 
        /// </summary>
        [JsonConstructor]
        private ShiftTransform()
        {
        }

        /// <summary>
        /// Gets a horizontal shift, in pixels.
        /// </summary>
        /// <value>
        /// The horizontal shift, in pixels.
        /// </value>
        [JsonProperty("shiftX")]
        public int ShiftX { get; private set; }

        /// <summary>
        /// Gets a vertical shift, in pixels.
        /// </summary>
        /// <value>
        /// The vertical shift, in pixels.
        /// </value>
        [JsonProperty("shiftY")]
        public int ShiftY { get; private set; }

        /// <inheritdoc />
        public Point Convert(Point value)
        {
            return new Point(value.X - this.ShiftX, value.Y - this.ShiftY);
        }

        /// <inheritdoc />
        public Rectangle Convert(Rectangle value)
        {
            return new Rectangle(value.X - this.ShiftX, value.Y - this.ShiftY, value.Width, value.Height);
        }
    }
}
