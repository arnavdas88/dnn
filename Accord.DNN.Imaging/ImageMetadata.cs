// -----------------------------------------------------------------------
// <copyright file="ImageMetadata.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Accord.DNN.Imaging
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Provides support for reading and writing metadata to and from an image.
    /// </summary>
    public class ImageMetadata
    {
        private readonly List<PropertyItem> propertyItems = new List<PropertyItem>();

        internal ImageMetadata(IEnumerable<PropertyItem> propertyItems)
        {
            this.propertyItems.AddRange(propertyItems);
        }

        /// <summary>
        /// Gets the collection of <see cref="PropertyItem"/> objects.
        /// </summary>
        /// <value>
        /// The collection of <see cref="PropertyItem"/> objects.
        /// </value>
        public IList<PropertyItem> PropertyItems => this.propertyItems;

        /// <summary>
        /// Gets a value indicating whether the specified property item (piece of metadata) exists.
        /// </summary>
        /// <param name="propertyId">The identifier of the property item to check.</param>
        /// <returns>
        /// <b>true</b> if the property item exists; otherwise, <b>false</b>.
        /// </returns>
        public bool HasPropertyItem(int propertyId)
        {
            return this.propertyItems.Any(x => x.Id == propertyId);
        }

        /// <summary>
        /// Gets the value of the specified property item (piece of metadata).
        /// </summary>
        /// <param name="propertyId">The identifier of the property item to get.</param>
        /// <returns>
        /// The <see cref="Object"/> that contains property item value if the property item exists; otherwise, <b>null</b>.
        /// </returns>
        public object GetPropertyItem(int propertyId)
        {
            switch (propertyId)
            {
                /*case (int)TIFFField.ImageWidth:
                    return this.Bitmap._width;

                case (int)TIFFField.ImageLength:
                    return this.Bitmap._height;

                case (int)TIFFField.XResolution:
                    return this.Bitmap._xRes;

                case (int)TIFFField.YResolution:
                    return this.Bitmap._yRes;

                case (int)TIFFField.BitsPerSample:
                    return this.Bitmap._bitCount;*/

                default:
                    return this.propertyItems.FirstOrDefault(x => x.Id == propertyId)?.Value;
            }

            ////return null;
        }

        /// <summary>
        /// Sets the value of the specified property item (piece of metadata).
        /// </summary>
        /// <param name="propertyId">The identifier of the property item to set.</param>
        /// <param name="value">The property item value.</param>
        /// <returns>
        /// <b>true</b> if successfully set property value; otherwise, <b>false</b>.
        /// </returns>
        public bool SetPropertyItem(int propertyId, object value)
        {
            switch (propertyId)
            {
                /*case (int)TIFFField.ImageWidth:
                case (int)TIFFField.ImageLength:
                case (int)TIFFField.XResolution:
                case (int)TIFFField.YResolution:
                case (int)TIFFField.BitsPerSample:
                    break;*/

                default:
                    PropertyItem propertyItem = this.propertyItems.FirstOrDefault(x => x.Id == propertyId);
                    if (propertyItem != null)
                    {
                        propertyItem.Value = value;
                        return true;
                    }
                    else
                    {
                        this.propertyItems.Add(new PropertyItem(propertyId, value));
                        return true;
                    }
            }

            ////return false;
        }

        /// <summary>
        /// Removes the specified property item (piece of metadata).
        /// </summary>
        /// <param name="propertyId">The identifier of the property item to remove.</param>
        /// <exception cref="ArgumentException">
        /// The image does not contain the requested property item.
        /// </exception>
        public void RemovePropertyItem(int propertyId)
        {
            if (this.propertyItems.RemoveAll(x => x.Id == propertyId) == 0)
            {
                throw new ArgumentException(Properties.Resources.E_InvalidPropertyItem, nameof(propertyId));
            }
        }

        /// <summary>
        /// Removes the specified property items (pieces of metadata) that satisfy the conditions defined by the specified predicate.
        /// </summary>
        /// <param name="condition">A function that defines the conditions of the property items to remove.</param>
        public void RemovePropertyItems(Predicate<PropertyItem> condition)
        {
            this.propertyItems.RemoveAll(condition);
        }
    }
}
